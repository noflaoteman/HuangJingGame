using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameClient.Entities;

namespace GameClient
{
    /// <summary>
    /// Server-Client-Object
    /// </summary>
    public abstract class SCObject
    {

        protected object realObj;
        public SCObject(object realObj)
        {
            this.realObj = realObj;
        }
        public object RealObj => realObj;
        public Vector3 Position => GetPosition();
        public Vector3 Direction => GetDirection();
        public int Id => GetId();

        protected virtual int GetId() => 0;
        protected virtual Vector3 GetPosition() => Vector3.zero;
        protected virtual Vector3 GetDirection() => Vector3.zero;

    }

    // 定义SCEntity类，继承自SCObject
    public class SCEntity : SCObject
    {
        private Entity entityObject { get => (Entity)realObj; }
        public SCEntity(Entity entityObject) : base(entityObject) { }


        protected override int GetId()
        {
            return entityObject.entityId;
        }
        protected override Vector3 GetPosition()
        {
            return entityObject.Position;
        }
        protected override Vector3 GetDirection()
        {
            return entityObject.Direction;
        }

    }

    public class SCPosition : SCObject
    {
        public SCPosition(Vector3 realObj) : base(realObj)
        {
        }

        protected override Vector3 GetPosition()
        {
            return (Vector3)realObj;
        }
    }
}
