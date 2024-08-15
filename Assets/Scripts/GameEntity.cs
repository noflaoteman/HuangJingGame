using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
using GameClient.Entities;
using GameClient;
using Serilog;

public class GameEntity : MonoBehaviour
{
    public int entityId;
    public Vector3 position;    //位置
    public Vector3 direction;   //方向
    public float speed;         //移动速度
    public bool isMine;         //是否自己控制的角色
    public string entityName = "闪电五连鞭";
    public NetEntity netEntity;
    public EntityState entityState;
    public Actor actor { get; private set; }

    private CharacterController characterController;

    public float fallSpeed = 0f; //下落速度
    public float fallSpeedMax = 30f; //最大下落速度

    // Start is called before the first frame update
    void Start()
    {
        actor = FoundActorTool.GetUnit(entityId);
        Log.Information("加载角色：{0}", actor);

        this.gameObject.SetActive(!actor.IsDeath);

        characterController = GetComponent<CharacterController>();
        //开启协程，每秒10次，向服务器上传hero的属性
        StartCoroutine(SyncRequest());
    }

    SpaceEntitySyncRequest _sync = new SpaceEntitySyncRequest()
    {
        EntitySync = new NetEntitySync()
        {
            Entity = new NetEntity()
            {
                Position = new Vec3(),
                Direction = new Vec3(),
            }
        }
    };

    //执行死循环向服务器发送同步请求
    IEnumerator SyncRequest()
    {
        EntityState _lastState = EntityState.None;
        while (true)
        {
            //当主角位置发生变化才执行代码
            if (isMine && transform.hasChanged && !actor.IsDeath)
            {
                _sync.EntitySync.Entity.Id = entityId;
                if (_lastState != entityState)
                {
                    _sync.EntitySync.State = entityState;
                    _lastState = entityState;
                }
                //设置Actor的位置和方向
                this.actor.Position = this.position * 1000;
                this.actor.Direction = this.direction * 1000;
                SetValueTo(actor.Position, _sync.EntitySync.Entity.Position);
                SetValueTo(actor.Direction, _sync.EntitySync.Entity.Direction);

                NetClient.Send(_sync);
                transform.hasChanged = false;
                _sync.EntitySync.State = EntityState.None;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    //其实就是把entityName绘制在人物头顶上
    private void OnGUI()
    {
        if (!IsInView(gameObject)) return;

        //角色的高度
        float height = 1.8f;
        //获取玩家摄像机
        Camera playerCamera = Camera.main;
        //计算角色头顶的世界坐标
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
        //三维世界坐标转为二维屏幕坐标
        Vector2 uiPos = playerCamera.WorldToScreenPoint(pos);
        //计算角色头顶的真实2D坐标
        uiPos = new Vector2(uiPos.x, Screen.height - uiPos.y);
        //计算文字需要占用的尺寸
        Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(entityName));
        //设置画笔颜色
        GUI.color = Color.yellow;
        //计算显示的矩形区域
        Rect rect = new Rect(uiPos.x - (nameSize.x / 2), uiPos.y - nameSize.y, nameSize.x, nameSize.y);
        //绘制文字
        GUI.Label(rect, entityName);
    }
    public bool IsInView(GameObject target)
    {
        Vector3 worldPos = target.transform.position;
        Transform camTransform = Camera.main.transform;
        if (Vector3.Distance(camTransform.position, worldPos) > 60f)
            return false;
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos);
        Vector3 dir = (worldPos - camTransform.position).normalized;
        //判断物体是否在相机前面
        float dot = Vector3.Dot(camTransform.forward, dir);

        if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
            return false;
    }


    // Update is called once per frame
    void Update()
    {
        //由GameEntity向Transform赋值
        if (!isMine)
        {
            Move(Vector3.Lerp(transform.position, position, Time.deltaTime * 5f));

            //四元数
            Quaternion targetRotation = Quaternion.Euler(direction);
            this.transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation,
                Time.deltaTime * 10f);
        }
        //由Transform赋值给GameEntity
        else
        {
            //玩家控制的角色
            this.position = transform.position;
            this.direction = transform.rotation.eulerAngles;

        }

        //模拟重力
        if (!characterController.isGrounded)
        {
            //计算重力增量
            fallSpeed += 9.8f * Time.deltaTime;
            if (fallSpeed > fallSpeedMax)
            {
                fallSpeed = fallSpeedMax;
            }
            characterController.Move(new Vector3(0, -fallSpeed * Time.deltaTime, 0));
        }
        else
        {
            characterController.Move(new Vector3(0, -0.01f, 0));
            fallSpeed = 0f;
        }

    }



    /// <summary>
    /// 外部穿进来一个NetEntity数据对GameEntity的数据进行赋值
    /// </summary>
    /// <param name="nEntity"></param>
    public void SetData(NetEntity nEntity, bool instantMove = false)
    {
        this.entityId = nEntity.Id;
        this.position = ToVector3(nEntity.Position);
        this.direction = ToVector3(nEntity.Direction);
        this.speed = nEntity.Speed * 0.001f;
        if (instantMove)
        {
            this.transform.rotation = Quaternion.Euler(direction);
            Move(position);
        }
    }
    //立刻移动到指定位置
    public void Move(Vector3 target)
    {
        CharacterController ctr = GetComponent<CharacterController>();
        ctr.Move(target - ctr.transform.position);
    }

    /// <summary>
    /// 将Unity的三维向量乘1000倍转为网络int类型
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private Vec3 ToVec3(Vector3 v)
    {
        v *= 1000;
        return new Vec3() { X = (int)v.x, Y = (int)v.y, Z = (int)v.z };
    }
    private Vector3 ToVector3(Vec3 v)
    {
        return new Vector3() { x = v.X, y = v.Y, z = v.Z } * 0.001f;
    }
    private void SetValueTo(Vector3 a, Vec3 b)
    {
        b.X = (int)a.x;
        b.Y = (int)a.y;
        b.Z = (int)a.z;
    }
}
