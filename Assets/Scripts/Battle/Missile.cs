using GameClient.Battle;
using GameClient;
using Serilog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class Missile : MonoBehaviour
{

    //所属技能
    public Skill Skill { get; private set; }
    //追击目标
    public GameObject Target { get; private set; }
    //初始位置
    public Vector3 InitPos { get; private set; }

    private GameObject MissileObject;

    //对导弹的数据进行赋值
    public void Init(Skill skill, Vector3 initPos, GameObject target)
    {
        this.Skill = skill;
        this.Target = target;
        this.InitPos = initPos;
        transform.position = initPos;
        Log.Information("Missile initPos:{0}", initPos);

        GameObject missilePrefab = Resources.Load<GameObject>(Skill.Def.Missile);
        if (missilePrefab != null)
        {
            MissileObject = Instantiate(missilePrefab, Vector3.zero, Quaternion.identity, transform);

        }


    }


    void Start()
    {
        //transform.localScale = Vector3.one * 0.1f;
    }


    private void FixedUpdate()
    {
        OnUpdate(Time.fixedDeltaTime);
    }


    public void OnUpdate(float dtime)
    {
        Vector3 pos = transform.position;
        Vector3 targetPos = this.Target.transform.position;
        Vector3 dir = (targetPos - pos).normalized;
        //得到每一帧移动的距离
        float distance = Skill.Def.MissileSpeed * 0.001f * dtime;
        //导弹追到了目标（距离很近了）
        if (distance >= Vector3.Distance(pos, targetPos))
        {
            transform.position = targetPos;
            Destroy(this.gameObject, 0.6f);
        }
        //还没有追上目标
        else
        {
            transform.position += dir * distance;
        }
        //设置导弹预设体在父对象的正上方1m处
        MissileObject.transform.localPosition = Vector3.up;
    }


}
