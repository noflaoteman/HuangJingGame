
using GameClient;
using GameClient.Entities;
using Newtonsoft.Json;
using Proto;
using Serilog;
using Summer;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameObjectManager : MonoBehaviour
{
    public static GameObjectManager Instance;
    // <EntityId,GameObject>
    private static Dictionary<int, GameObject> dict = new Dictionary<int, GameObject>();

    private void Start()
    {
        Instance = this;
        //角色进入事件
        Kaiyun.Event.RegisterOut("CharacterEnter", this, "CharacterEnter");
        //角色离开事件
        Kaiyun.Event.RegisterOut("CharacterLeave", this, "CharacterLeave");
        //同步事件
        Kaiyun.Event.RegisterOut("EntitySync", this, "EntitySync");
    }

    private void OnDestroy()
    {
        Kaiyun.Event.UnregisterOut("CharacterEnter", this, "CharacterEnter");
        Kaiyun.Event.UnregisterOut("CharacterLeave", this, "CharacterLeave");
        Kaiyun.Event.UnregisterOut("EntitySync", this, "EntitySync");
    }

    public void EntitySync(NetEntitySync entitySync)
    {
        //得到字典中记录的Gameobject
        int entityId = entitySync.Entity.Id;
        GameObject gameObject = dict.GetValueOrDefault(entityId, null);
        if (gameObject == null) return;

        //对entitysync中的Position进行计算
        Vector3 pos = V3.Of(entitySync.Entity.Position) / 1000f;
        if (pos.y == 0)
        {
            pos = GameTools.CalculateGroundPosition(pos, 20);
            entitySync.Entity.Position = V3.ToVec3(pos * 1000);
        }

        //对于Gameobject中的gameEntity进行赋值数据
        GameEntity gameEntity = gameObject.GetComponent<GameEntity>();
        gameEntity.SetData(entitySync.Entity);

        //如果力量合法那么就移动，这一句不是很理解！
        //既然UPadate里面都持续的移动了，
        if (entitySync.Force)
        {
            Vector3 target = V3.Of(entitySync.Entity.Position) * 0.001f;
            gameEntity.Move(target);
        }
    }

    public void CharacterLeave(int entityId)
    {
        if (dict.ContainsKey(entityId))
        {
            GameObject obj = dict[entityId];
            if (obj != null && !obj.IsDestroyed())
            {
                //移除场景上的物体
                Destroy(obj);
            }
            //移除字典中记录的物体
            dict.Remove(entityId);
        }
    }



    //物品进入的逻辑
    private void ItemEnter(NetActor Nactor)
    {
        //计算降临位置
        Vector3 initPos = V3.Of(Nactor.Entity.Position) / 1000f;
        if (initPos.y == 0)
        {
            initPos = GameTools.CalculateGroundPosition(initPos);
        }
        ItemDefine itemDef = DataManager.Instance.Items[Nactor.ItemInfo.ItemId];
        //得到Acotr数据对他的RenderObj进行设置
        Actor actor = FoundActorTool.GetUnit(Nactor.Entity.Id);
        GameObject prefab = Resources.Load<GameObject>(itemDef.Model);
        GameObject go = Instantiate(prefab, initPos, Quaternion.identity, this.transform);
        //gameObject.layer = 6; //加入Actor图层
        go.transform.position = initPos;
        actor.renderObj = go;
        dict.Add(Nactor.Entity.Id, go);
    }
    //本质上就是得到Nactor中的数据给Unity中的数据赋值
    public void CharacterEnter(NetActor Nactor)
    {
        //如果是物品
        if (Nactor.Type == EntityType.Item)
        {
            this.ItemEnter(Nactor);
            return;
        }
        //有可能是Character，也可能是Monster
        if (!dict.ContainsKey(Nactor.Entity.Id))
        {
            Debug.Log("角色加入：" + Nactor);
            //得到是不是自己
            bool isMine = (Nactor.Entity.Id == GameApp.Character.entityId);

            //计算Y的坐标
            Vector3 initPos = V3.Of(Nactor.Entity.Position) / 1000f;
            if (initPos.y == 0)
            {
                initPos = GameTools.CalculateGroundPosition(initPos);
                Debug.Log("pos:");
                Debug.Log(initPos);
            }

            //得到EntityManager中的actor数据
            Actor actor = FoundActorTool.GetUnit(Nactor.Entity.Id);

            //根据数据实例化物体
            UnitDefine def = DataManager.Instance.Units[Nactor.Tid];
            GameObject prefab = Resources.Load<GameObject>(def.Resource);
            GameObject gameObject = Instantiate(prefab, initPos, Quaternion.identity, this.transform);
            gameObject.layer = 6; //加入Actor图层
            actor.renderObj = gameObject;

            //对GameEntity中的数据进行赋值
            GameEntity gameEntity = gameObject.GetComponent<GameEntity>();
            gameEntity.isMine = isMine;
            gameEntity.entityName = Nactor.Name;
            gameEntity.SetData(Nactor.Entity);
            if (isMine)
            {
                gameObject.AddComponent<HeroController>();
            }

            //设置物体的名字和图层相关
            if (Nactor.Type == EntityType.Character)
            {
                gameObject.name = "Character_" + Nactor.Entity.Id;
            }
            if (Nactor.Type == EntityType.Monster)
            {
                gameObject.name = "Monster_" + Nactor.Entity.Id;
            }
            if (Nactor.Type == EntityType.Gate)
            {
                gameObject.name = "Gate_" + Nactor.Entity.Id;
                gameObject.layer = 0;
            }

            //最终把这个物体加入字典进行统一管理
            dict.Add(Nactor.Entity.Id, gameObject);

        }


    }


}
