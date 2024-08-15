using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient
{
    /// <summary>
    /// 计算地面坐标，压入Unity主线程的封装
    /// </summary>
    public class GameTools
    {
        /// <summary>
        /// 传入一个点计算该位置的垂直地面的坐标
        /// </summary>
        /// <param name="position">传入的位置</param>
        /// <returns></returns>
        public static Vector3 CalculateGroundPosition(Vector3 position, float up = 1000, int ignoreLayer = 6)
        {
            Vector3 upPs1000m = position + new Vector3(0, 1000f, 0);
            // Raycast downwards to find the ground
            RaycastHit hitInfo;
            int layerMask = ~(1 << ignoreLayer); // Ignore layer 6
            if (Physics.Raycast(upPs1000m, Vector3.down, out hitInfo, Mathf.Infinity, layerMask))
            {
                return hitInfo.point;
            }
            else
            {
                // If no ground is found, return the original position
                return position;
            }
        }

        /// <summary>
        /// 传入一个委托，把这个委托压入主线程中，相当于传入一个函数压入主线程
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnMainThread(Action action)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(action);
        }

    }
}
