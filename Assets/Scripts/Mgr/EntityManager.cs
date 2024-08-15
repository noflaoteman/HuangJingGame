using GameClient.Entities;
using Proto;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameClient.Mgr
{
    /// <summary>
    /// 本质就是提供了一个单例Entity的数据，提供了“增删改查”；
    /// </summary>
    public class EntityManager : Singleton<EntityManager>
    {
        public EntityManager() { }
        //线程安全的字典，父类装子类
        private ConcurrentDictionary<int, Entity> _dict = new ConcurrentDictionary<int, Entity>();

        //添加实体进入字典
        public void AddEntity(Entity entity)
        {
            UnityEngine.Debug.Log("AddEntity：" + entity.entityId);
            _dict[entity.entityId] = entity;
        }

        //根据id移除字典中的实体
        public void RemoveEntity(int entityId)
        {
            UnityEngine.Debug.Log("RemoveEntity：" + entityId);
            _dict.Remove(entityId, out Entity entity);
            Kaiyun.Event.FireOut("CharacterLeave", entityId);
        }

        /// <summary>
        /// 根据netActor的类型添加不同种类实体到字典中
        /// </summary>
        /// <param name="netActor">网络中的角色数据</param>
        public void OnEntityEnter(NetActor netActor)
        {
            if (netActor.Type == EntityType.Character)
            {
                AddEntity(new Character(netActor));
            }
            if (netActor.Type == EntityType.Monster)
            {
                AddEntity(new Monster(netActor));
            }
            if (netActor.Type == EntityType.Item)
            {
                AddEntity(new ItemEntity(netActor));
            }
            if (netActor.Type == EntityType.Gate)
            {
                AddEntity(new Gate(netActor));
            }
            //本质就是执行GameObjectManager中的CharacterEnter函数
            Kaiyun.Event.FireOut("CharacterEnter", netActor);
        }

        //获取同步信息
        public void OnEntitySync(NetEntitySync entitySync)
        {
            //通过收到的NetEntitySync数据在字典中查找看看是否有配对的
            Entity entity = _dict.GetValueOrDefault(entitySync.Entity.Id);
            if (entity != null)
            {
                //如果有那么执行逻辑
                entity.State = entitySync.State;
                entity.EntityData = entitySync.Entity;
                Kaiyun.Event.FireOut("EntitySync", entitySync);
            }

        }

        /// <summary>
        /// 从字典中拿到entity
        /// </summary>
        /// <typeparam name="T">想要拿到的类型</typeparam>
        /// <param name="entityId">实体的id</param>
        /// <returns>你想要的实体</returns>
        public T GetEntity<T>(int entityId) where T : Entity
        {
            return (T)_dict.GetValueOrDefault(entityId);
        }

        /// <summary>
        /// 这句应该是返回出去实体的列表
        /// </summary>
        /// <typeparam name="T"><想要拿到的类型/typeparam>
        /// <param name="match">TM我也不知道什么用这个</param>
        /// <returns>返回一个实体的列表</returns>
        public List<T> GetEntities<T>(Predicate<T> match)
        {
            return _dict.Values.OfType<T>().Where(e => match.Invoke(e)).ToList();
        }


        public void Clear()
        {
            foreach (Entity entity in _dict.Values)
            {
                if (entity is Actor actor)
                {
                    //清楚GameobjectManager的数据
                    GameObjectManager.Instance.CharacterLeave(actor.entityId);
                }
            }
            //清楚自己字典的数据
            _dict.Clear();
        }

        /// <summary>
        /// 此方法由Unity主线程帧调用，是执行技能的吗？
        /// </summary>
        /// <param name="delta"></param>
        public void OnUpdate(float delta)
        {
            foreach (Entity entity in _dict.Values)
            {
                Actor actor = entity as Actor;
                actor.SkillMgr?.OnUpdate(delta);
            }
        }
    }
}
