using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;


namespace Kaiyun
{
    /// <summary>
    /// 事件系统
    /// </summary>
    /// <version>1.2</version>
    /// <date>2023-08-02</date>
    public class Event
    {

        private static Dictionary<string, List<GsHandler>> eventInDict;
        private static Dictionary<string, List<GsHandler>> eventOutDict;

        private static Queue<FireTask> outQueue;

        public delegate void EventAction(params object[] args);

        private class FireTask
        {
            public string name; //事件名称
            public object[] args;
            public FireTask(string name, object[] args)
            {
                this.name = name;
                this.args = args;
            }
        }
        private class GsHandler
        {
            public object target;
            public string methodName;
            public EventAction action;
            public GsHandler(object target, string methodName)
            {
                this.target = target;
                this.methodName = methodName;
                MethodInfo method = target.GetType().GetMethod(methodName);
                if (method != null)
                {
                    action = args => method.Invoke(target, args);
                }

            }
        }

        static Event()
        {
            eventInDict = new Dictionary<string, List<GsHandler>>();
            eventOutDict = new Dictionary<string, List<GsHandler>>();
            outQueue = new Queue<FireTask>();
        }


        public static void RegisterIn(string eventName, object target, string methodName)
        {
            lock (eventInDict)
            {
                if (!eventInDict.ContainsKey(eventName))
                {
                    eventInDict[eventName] = new List<GsHandler>();
                }
                eventInDict[eventName].Add(new GsHandler(target, methodName));
            }

        }

        public static void RegisterOut(string eventName, object target, string methodName)
        {
            lock (eventOutDict)
            {
                if (!eventOutDict.ContainsKey(eventName))
                {
                    eventOutDict[eventName] = new List<GsHandler>();
                }
                eventOutDict[eventName].Add(new GsHandler(target, methodName));
            }

        }

        public static void FireIn(string eventName, params object[] parameters)
        {
            lock (eventInDict)
            {
                if (eventInDict.ContainsKey(eventName))
                {
                    List<GsHandler> list = eventInDict[eventName];
                    foreach (GsHandler handler in list)
                    {
                        handler.action?.Invoke(parameters);
                    }
                }
            }

        }

        public static void FireOut(string eventName, params object[] parameters)
        {
            lock (eventOutDict)
            {
                if (eventOutDict.ContainsKey(eventName))
                {
                    outQueue.Enqueue(new FireTask(eventName, parameters));
                }
            }
        }

        public static void UnregisterIn(string eventName, object target, string methodName)
        {
            lock (eventInDict)
            {
                List<GsHandler> list = eventInDict.GetValueOrDefault(eventName, null);
                list?.RemoveAll(h => h.target == target && h.methodName.Equals(methodName));
            }
        }

        public static void UnregisterOut(string eventName, object target, string methodName)
        {
            lock (eventOutDict)
            {
                List<GsHandler> list = eventOutDict.GetValueOrDefault(eventName, null);
                list?.RemoveAll(h => h.target == target && h.methodName.Equals(methodName));
            }
        }


        public static void UnregisterIn(string eventName)
        {
            lock (eventInDict)
            {
                eventInDict.Clear();
            }
        }
        public static void UnregisterOut(string eventName)
        {
            lock (eventOutDict)
            {
                eventOutDict.Clear();
            }
        }


        /// <summary>
        /// 在主线程Update调用
        /// </summary>
        public static void Tick()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == 1)
            {
                // 当前代码在主线程中运行
                // Debug.Log("主线程");
                while (outQueue.Count > 0)
                {
                    FireTask item = outQueue.Dequeue();
                    List<GsHandler> list = eventOutDict.GetValueOrDefault(item.name, null);
                    foreach (GsHandler handler in list)
                    {
                        handler.action?.Invoke(item.args);
                    }
                }
            }

        }


    }

}
