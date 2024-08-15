using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proto;
using Summer.Network;
using System;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using GameClient.Mgr;
using GameClient.Entities;
using Serilog;
using GameClient;
using GameClient.Battle;

public class NetStart : MonoBehaviour
{

    public List<GameObject> keepAlive;

    [Header("服务器信息")]
    public string host = "127.0.0.1";
    public int port = 32510;

    public Text ycText;

    [Header("登录参数")]
    public InputField usernameInput;
    public InputField passwordInput;

    private GameObject hero; //当前的角色

    // Start is called before the first frame update
    void Start()
    {
        //6号Layer无视碰撞，可以把角色，NPC，怪物，全都放到6号图层
        Physics.IgnoreLayerCollision(6, 6, true);
        //绑定服务器
        NetClient.ConnectToServer(host, port);
        //让list的物品不被移除
        foreach (GameObject go in keepAlive)
        {
            DontDestroyOnLoad(go);
        }



        #region 监听消息的函数
        MessageRouter.Instance.AddFuntionToDic<SpaceEnterResponse>(_SpaceEnterResponse);
        MessageRouter.Instance.AddFuntionToDic<SpaceCharactersEnterResponse>(_SpaceCharactersEnterResponse);
        MessageRouter.Instance.AddFuntionToDic<SpaceEntitySyncResponse>(_SpaceEntitySyncResponse);
        MessageRouter.Instance.AddFuntionToDic<HeartBeatResponse>(_HeartBeatResponse);
        MessageRouter.Instance.AddFuntionToDic<SpaceCharacterLeaveResponse>(_SpaceCharacterLeaveResponse);
        //施法通知
        MessageRouter.Instance.AddFuntionToDic<SpellResponse>(_SpellResponse);
        //单位收到伤害，用来做特效展示，crit，miss，damage
        MessageRouter.Instance.AddFuntionToDic<DamageResponse>(_DamageResponse);
        //单位属性发生了变化（HP,MP,HPMAX,MPMAX,STATE,LEVEL,NAME...）
        MessageRouter.Instance.AddFuntionToDic<PropertyUpdateResponse>(_PropertyUpdateResponse);
        //监听聊天信息
        MessageRouter.Instance.AddFuntionToDic<ChatResponse>(_ChatResponse);
        //监听背包消息
        MessageRouter.Instance.AddFuntionToDic<InventoryResponse>(_InventoryResponse);
        #endregion




        //心跳包任务，每秒1次
        StartCoroutine(SendHeartMessage());

        //跳转场景
        SceneManager.LoadScene("LoginScene");
        GameApp.MakeEventSystem();

        //这段代码是用于获取当前执行程序的目录路径
        string exeDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        Debug.Log(exeDirectory);

        //读取数据
        DataManager.Instance.Init();

        //注册In事件
        Kaiyun.Event.RegisterIn("EnterGame", this, "EnterGame");
        Kaiyun.Event.RegisterIn("ItemPlacement", this, "ItemPlacement");
        Kaiyun.Event.RegisterIn("ItemDiscard", this, "ItemDiscard");
        Kaiyun.Event.RegisterIn("UseItem", this, "UseItem");

        //注册Out事件
        Kaiyun.Event.RegisterOut("OnDisconnected", this, "OnDisconnected");
    }

    #region 背包物品消息发送
    //使用物品
    public void UseItem(int slotIndex)
    {
        ItemUseRequest request = new ItemUseRequest
        {
            EntityId = GameApp.Character.entityId,
            SlotIndex = slotIndex
        };
        NetClient.Send(request);
    }

    //丢弃物品
    public void ItemDiscard(int slotIndex, int amount)
    {
        ItemDiscardRequest request = new ItemDiscardRequest
        {
            EntityId = GameApp.Character.entityId,
            SlotIndex = slotIndex,
            Count = amount
        };
        NetClient.Send(request);
    }

    //物品放置
    public void ItemPlacement(int originSlot, int targetSlot)
    {
        ItemPlacementRequest request = new ItemPlacementRequest
        {
            EntityId = GameApp.Character.entityId,
            OriginIndex = originSlot,
            TargetIndex = targetSlot,
        };
        NetClient.Send(request);
    }
    #endregion

    #region Response函数相关
    private void _InventoryResponse(Connection conn, InventoryResponse msg)
    {
        Character chr = GameApp.Character;
        if (chr == null || chr.entityId != msg.EntityId) return;
        if (msg.KnapsackInfo != null)
        {
            //加载背包信息
            chr.Knapsack.Reload(msg.KnapsackInfo);
            Kaiyun.Event.FireOut("OnKnapsackReloaded");
        }
    }

    //进入新的场景
    private void _SpaceEnterResponse(Connection conn, SpaceEnterResponse msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (GameApp.Character == null || GameApp.Character.Info.SpaceId != msg.Character.SpaceId)
            {
                //需要加载新场景
                EntityManager.Instance.Clear();
                GameApp.LoadSpace(msg.Character.SpaceId);
                //把其他单位加入游戏
                foreach (NetActor item in msg.List)
                {
                    EntityManager.Instance.OnEntityEnter(item);
                }
                //把主角加入游戏
                EntityManager.Instance.OnEntityEnter(msg.Character);
                //得到主角
                GameApp.Character = EntityManager.Instance.GetEntity<Character>(msg.Character.Entity.Id);
            }
        });
    }

    //接收聊天消息
    private void _ChatResponse(Connection conn, ChatResponse msg)
    {
        Character chr = FoundActorTool.GetUnit(msg.SenderId) as Character;
        string text = $"[玩家]{chr.Info.Name}：{msg.TextValue}";
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            SimpleChatBox.Instance.CreateText(text);
        });
    }

    private void _PropertyUpdateResponse(Connection conn, PropertyUpdateResponse msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            foreach (PropertyUpdate item in msg.List)
            {
                Actor actor = FoundActorTool.GetUnit(item.EntityId);
                switch (item.Property)
                {
                    case PropertyUpdate.Types.Property.Hp:
                        actor.OnHpChanged(item.OldValue.FloatValue, item.NewValue.FloatValue);
                        break;
                    case PropertyUpdate.Types.Property.Mp:
                        actor.OnMpChanged(item.OldValue.FloatValue, item.NewValue.FloatValue);
                        break;
                    case PropertyUpdate.Types.Property.State:
                        actor.OnStateChanged(item.OldValue.StateValue, item.NewValue.StateValue);
                        break;
                    case PropertyUpdate.Types.Property.Hpmax:
                        actor.OnHPMaxChanged(item.NewValue.FloatValue);
                        break;
                    case PropertyUpdate.Types.Property.Mpmax:
                        actor.OnMPMaxChanged(item.NewValue.FloatValue);
                        break;
                    case PropertyUpdate.Types.Property.Golds: //金币
                        actor.OnGoldsChanged(item.OldValue.LongValue, item.NewValue.LongValue);
                        break;
                    case PropertyUpdate.Types.Property.Exp: //经验
                        actor.OnExpChanged(item.OldValue.LongValue, item.NewValue.LongValue);
                        break;
                    case PropertyUpdate.Types.Property.Level: //等级
                        actor.OnLevelChanged(item.OldValue.IntValue, item.NewValue.IntValue);
                        break;
                }
            }
        });
    }

    private void _DamageResponse(Connection conn, DamageResponse msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            foreach (Damage item in msg.List)
            {
                Actor attacker = FoundActorTool.GetUnit(item.AttackerId);
                Actor target = FoundActorTool.GetUnit(item.TargetId);
                target.recvDamage(item);
            }
        });
    }

    //收到来自服务器的施法通知
    private void _SpellResponse(Connection conn, SpellResponse msg)
    {
        foreach (CastInfo castInfo in msg.CastList)
        {
            Log.Information("施法信息：{0}", castInfo);
            Actor caster = FoundActorTool.GetUnit(castInfo.CasterId);
            try
            {
                Skill skill = caster.SkillMgr.GetSkill(castInfo.SkillId);
                if (skill.IsUnitTarget)
                {
                    Actor target = FoundActorTool.GetUnit(castInfo.TargetId);
                    skill.Use(new SCEntity(target));
                }
                if (skill.IsNoneTarget)
                {
                    skill.Use(new SCEntity(caster));
                }
            }
            catch (Exception ex)
            {
                Log.Information("施法异常：{0}", ex.Message);
            }

        }
    }

    //来自于服务器的心跳响应
    private void _HeartBeatResponse(Connection conn, HeartBeatResponse msg)
    {

        TimeSpan t = DateTime.Now - lastBeatTime;
        //Debug.Log("来自于服务器的心跳响应:ms="+t.TotalMilliseconds);
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            int ms = Math.Max(1, (int)Math.Round(t.TotalMilliseconds));
            ycText.text = $"网络延迟：{ms}ms";
        });
    }


    /// <summary>
    /// 有角色离开地图
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="msg"></param>
    private void _SpaceCharacterLeaveResponse(Connection conn, SpaceCharacterLeaveResponse msg)
    {
        EntityManager.Instance.RemoveEntity(msg.EntityId);
    }

    //收到角色的同步信息
    private void _SpaceEntitySyncResponse(Connection conn, SpaceEntitySyncResponse msg)
    {
        EntityManager.Instance.OnEntitySync(msg.EntitySync);
    }

    //加入游戏的响应结果（Entity肯定是自己）
    private void _GameEnterResponse(Connection conn, GameEnterResponse msg)
    {
        Debug.Log("加入游戏的响应结果:" + msg.Success);
        //如果加入游戏成功那么才会执行逻辑
        if (msg.Success)
        {
            Debug.Log("角色信息:" + msg);
            NetActor netActor = msg.Character;
            netActor.Entity = msg.Entity;

            GameApp.LoadSpace(netActor.SpaceId);
            EntityManager.Instance.OnEntityEnter(netActor);
            GameApp.Character = EntityManager.Instance.GetEntity<Character>(msg.Entity.Id);
        }
    }


    //当有角色进入地图时候的通知（肯定不是自己）
    private void _SpaceCharactersEnterResponse(Connection conn, SpaceCharactersEnterResponse msg)
    {
        foreach (NetActor info in msg.CharacterList)
        {
            Debug.Log("角色加入：地图=" + msg.SpaceId + ",entityId=" + info.Entity.Id);
            EntityManager.Instance.OnEntityEnter(info);
        }
    }

    #endregion

    #region Send相关
    //提交聊天消息
    public void ChatSubmit(string text)
    {
        ChatRequest req = new ChatRequest();
        req.TextValue = text;
        NetClient.Send(req);
        SimpleChatBox.Instance.inputField.text = "";
    }

    //发送心跳消息
    private HeartBeatRequest beatRequest = new HeartBeatRequest();
    DateTime lastBeatTime = DateTime.MinValue;
    IEnumerator SendHeartMessage()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            NetClient.Send(beatRequest);
            lastBeatTime = DateTime.Now;
        }
    }

    //捡起物品消息
    private void Pickup()
    {
        PickupItemRequest request = new PickupItemRequest();
        NetClient.Send(request);
    }

    //加入游戏事件注册
    public void EnterGame(int roleId)
    {
        if (hero != null)
        {
            return;
        }
        GameEnterRequest request = new GameEnterRequest();
        request.CharacterId = roleId;
        NetClient.Send(request);
    }

    public void Login()
    {

    }

    #endregion

    void Update()
    {
        Kaiyun.Event.Tick();

        //给 GameApp.Target赋值
        if (Input.GetMouseButtonDown(0))
        {
            //从鼠标点击位置发出一条射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;  //存储射线投射结果的数据
            LayerMask actorLayer = LayerMask.GetMask("Actor");
            //检测射线是否与特定图层的物体相交
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, actorLayer))
            {
                //获取被点击的物体
                GameObject clickedObject = hitInfo.collider.gameObject;
                Debug.Log("选择目标: " + clickedObject.name);
                //记录被点击的角色
                int entityId = clickedObject.GetComponent<GameEntity>().entityId;
                GameApp.Target = EntityManager.Instance.GetEntity<Actor>(entityId);

            }
        }

        //拾取物品
        if (Input.GetKeyDown(KeyCode.C))
        {
            Pickup();
        }

    }

    private void FixedUpdate()
    {
        EntityManager.Instance.OnUpdate(Time.fixedDeltaTime);
    }

    private void OnDestroy()
    {
        //取消注册事件
        Kaiyun.Event.UnregisterIn("EnterGame", this, "EnterGame");
    }

    void OnApplicationQuit()
    {
        NetClient.Close();
    }

    //网络断开事件注册
    public void OnDisconnected()
    {
        MyDialog.ShowMessage("网络断开", "网络连接已中断，请重新进入游戏。", () =>
        {
            Application.Quit();
        });
    }

}
