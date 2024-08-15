using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Summer;
using UnityEngine;

namespace GameClient.Entities
{
    //客户端实体，和服务器保持一致，其实就是对于Proto.NetEntity的进一次封装
    /// <summary>
    /// 对于网络中数据结构类Proto.NetEntity转成了Entity类
    /// </summary>
    /*
     // 实体信息
message NetEntity {
	int32 id = 1;
	Vec3 position = 2;
	Vec3 direction = 3;
	int32 speed = 4;
}
     */
    public class Entity
    {
        public EntityState State;     //状态
        private int _speed;              //移动速度
        private Vector3 _position;    //位置
        private Vector3 _direction;   //方向
        private NetEntity _netObj;         //网络对象
        private long _lastUpdate;       //最后一次更新位置的时间戳

        /// <summary>
        /// 其实就是NetEntity中的Id
        /// </summary>
        public int entityId => _netObj.Id;
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                //将Vector3转成Vec3赋值给了NetEntity.position
                _netObj.Position = V3.ToVec3(value);
                //记录更新位置的时间
                _lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }
        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                //这里将Vector3转成了网络中的Vec3
                _netObj.Direction = V3.ToVec3(value);
            }
        }
        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                _netObj.Speed = value;
            }
        }
        /// <summary>
        /// 距离上次位置更新的间隔（秒）
        /// </summary>
        public float PositionTime
        {
            get
            {
                return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastUpdate) * 0.001f;
            }
        }

        //构造函数
        public Entity(NetEntity entity)
        {
            _netObj = new NetEntity();
            _netObj.Id = entity.Id;
            this.EntityData = entity;
        }

        public Proto.NetEntity EntityData
        {
            get { return _netObj; }
            set
            {
                //对于EntityData本质上就是对于_netObj的赋值
                Position = V3.ToVector3(value.Position);
                Direction = V3.ToVector3(value.Direction);
                Speed = value.Speed;
            }
        }


    }
}
