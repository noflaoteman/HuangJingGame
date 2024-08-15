using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace GameClient.Entities
{
    public class Monster : Actor
    {
        public Monster(NetActor info) : base(info)
        {

        }

        public override void OnStateChanged(UnitState old_value, UnitState new_value)
        {
            base.OnStateChanged(old_value, new_value);
            if(IsDeath)
            {
                FoundActorTool.StartCoroutine(_HideElement());
            }
        }

        IEnumerator _HideElement()
        {
            yield return new WaitForSeconds(3.0f);
            //如果单位已经死透，将其隐藏
            if (IsDeath && !renderObj.IsDestroyed())
            {
                renderObj?.SetActive(false);
            }

        }
    }
}
