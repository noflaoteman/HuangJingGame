using GameClient.Entities;
using GameClient.Mgr;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{
    public class FoundActorTool
    {
        /// <summary>
        /// 通过entityId获取一个Actor
        /// </summary>
        /// <param name="entityId">传入的entityId</param>
        /// <returns>返回一个Actor</returns>
        public static Actor GetUnit(int entityId)
        {
            return EntityManager.Instance.GetEntity<Actor>(entityId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">传入的圆心</param>
        /// <param name="range">传入的半径</param>
        /// <returns></returns>
        internal static List<Actor> RangeUnit(Vector3 position, int range)
        {
            Predicate<Actor> match = (actor) =>
            {
                float distance = Vector3.Distance(position, actor.Position);
                Log.Information("选择：distance={0}", distance);
                return distance <= range;
            };
            return EntityManager.Instance.GetEntities<Actor>(match);
        }

        /// <summary>
        /// 封装的一个开启协程的函数
        /// </summary>
        /// <param name="routine"></param>
        public static void StartCoroutine(IEnumerator routine)
        {
            UIManager.Instance.StartCoroutine(routine);
        }
    }


}

