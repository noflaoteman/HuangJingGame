using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using Common;
using Serilog;

namespace Summer.Network
{
    /// <summary>
    /// MessageRouter里面的内部消息类
    /// </summary>
    class Msg
    {
        public Connection msgConn;
        public Google.Protobuf.IMessage message;
    }

    /// <summary>
    /// 把Connection中收到的消息放进来，MessageRouter给你转发出去
    /// </summary>
    public class MessageRouter : Singleton<MessageRouter>
    {

        int ThreadCount = 1;    //工作线程数
        int WorkingCount = 0;    //正在工作的线程数
        bool _running = false;   //是否正在运行状态

        public bool Running { get { return _running; } }

        //用于管理线程
        AutoResetEvent threadEvent = new AutoResetEvent(true);
        // 消息队列，所有发来的消息都暂存在这里
        private Queue<Msg> MsgQueue = new Queue<Msg>();


        // 消息处理器(委托)，T是IMessage类型
        public delegate void MsgDelegate<T>(Connection sender, T msg);


        // string存储的是IMessage的名字，delegate存储的是函数，存储所有的订阅函数
        private Dictionary<string, Delegate> delegateDic = new Dictionary<string, Delegate>();



        //订阅，传入一个这样的函数（Connection，Msg）这里这个T其实就是IMessage类/子类
        //把这个函数存入delegateDic中的委托中
        /// <summary>
        /// 把函数加入delegateDic中存储，有消息来那么就执行
        /// </summary>
        /// <typeparam name="T">消息的类型</typeparam>
        /// <param name="funtion">你要传入的函数</param>
        public void AddFuntionToDic<T>(MsgDelegate<T> funtion) where T : Google.Protobuf.IMessage
        {
            string name = typeof(T).FullName;
            //如果没有这个类型的话
            if (!delegateDic.ContainsKey(name))
            {
                delegateDic[name] = null;
            }
            //父类转成子类之后再加上当前的这个函数
            delegateDic[name] = (MsgDelegate<T>)delegateDic[name] + funtion;
            Log.Debug(name + ":" + delegateDic[name].GetInvocationList().Length);
        }
        //退订，从delegateDic中移除该函数
        public void Off<T>(MsgDelegate<T> funtion) where T : Google.Protobuf.IMessage
        {
            string name = typeof(T).FullName;
            if (!delegateDic.ContainsKey(name))
            {
                delegateDic[name] = null;
            }
            delegateDic[name] = (MsgDelegate<T>)delegateDic[name] - funtion;
        }
        //触发，执行delegateDic中的委托
        private void Fire<T>(Connection conn, T msg)
        {
            string name = typeof(T).FullName;
            if (delegateDic.ContainsKey(name))
            {
                //类型转换其实就是父类转子类
                MsgDelegate<T> handler = (MsgDelegate<T>)delegateDic[name];
                try
                {
                    handler?.Invoke(conn, msg);
                }
                catch (Exception e)
                {
                    Log.Error("MessageRouter.Fire error:" + e.StackTrace);
                }

            }
        }


        /// <summary>
        /// 添加新的消息到队列中
        /// </summary>
        /// <param name="conn">消息发送者</param>
        /// <param name="message">消息对象</param>
        public void AddMessage(Connection conn, Google.Protobuf.IMessage message)
        {
            lock (MsgQueue)
            {
                MsgQueue.Enqueue(new Msg() { msgConn = conn, message = message });
            }
            //唤醒1个worker
            threadEvent.Set();
        }

        /// <summary>
        /// 初始化分发器
        /// </summary>
        /// <param name="_ThreadCount">线程数</param>
        public void Start(int _ThreadCount)
        {
            if (_running) return;
            _running = true;
            ThreadCount = Math.Min(Math.Max(_ThreadCount, 1), 200);
            ThreadPool.SetMinThreads(ThreadCount + 20, ThreadCount + 20);
            for (int i = 0; i < ThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageWork));
            }
            //如果工作线程没有达到数量，那么主线程就一直卡死
            while (WorkingCount < ThreadCount)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 停止分发器
        /// </summary>
        public void Stop()
        {
            //停止运行，那么WorkingCount就会减少
            _running = false;
            //清空消息队列
            MsgQueue.Clear();
            while (WorkingCount > 0)
            {
                threadEvent.Set();
            }
            Thread.Sleep(100);
        }

        #region 线程池函数
        /// <summary>
        /// 线程池函数
        /// </summary>
        /// <param name="state">忘记了这个值的作用</param>
        private void MessageWork(object? state)
        {
            Log.Information("worker thread start");
            try
            {
                //线程安全的自增
                WorkingCount = Interlocked.Increment(ref WorkingCount);
                while (_running)
                {
                    if (MsgQueue.Count == 0)
                    {
                        //线程等待
                        threadEvent.WaitOne();
                        continue;
                    }
                    //从消息队列取出一个元素
                    Msg msg = null;
                    lock (MsgQueue)
                    {
                        if (MsgQueue.Count == 0) continue;
                        msg = MsgQueue.Dequeue();
                    }
                    Google.Protobuf.IMessage package = msg.message;
                    if (package != null)
                    {
                        executeMessage(msg.msgConn, package);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            /*finally：
            finally 块中的代码总是会被执行，无论是否发生异常。
            通常用于执行一些无论是否发生异常都需要进行的清理工作，比如资源释放、关闭文件等。
             */
            finally
            {
                //线程安全的自减
                WorkingCount = Interlocked.Decrement(ref WorkingCount);
            }
            Log.Information("worker thread end");
        }

        //本质就是执行Fire函数
        private void executeMessage(Connection conn, Google.Protobuf.IMessage message)
        {
            //触发该类的Fire函数
            MethodInfo fireMethod = this.GetType().GetMethod("Fire", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo met = fireMethod.MakeGenericMethod(message.GetType());
            met.Invoke(this, new object[] { conn, message });

            Type msgtype = message.GetType();

            //因为消息里面可能还有一个消息
            foreach (PropertyInfo property in msgtype.GetProperties())
            {
                //过滤属性
                if ("Parser" == property.Name || "Descriptor" == property.Name) continue;
                object value = property.GetValue(message);
                if (value != null)
                {
                    if (typeof(Google.Protobuf.IMessage).IsAssignableFrom(value.GetType()))
                    {
                        //继续递归
                        executeMessage(conn, (Google.Protobuf.IMessage)value);
                    }
                }
            }
        }
        #endregion

    }

}
