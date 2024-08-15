using GameClient.Mgr;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameClient.Entities
{
    /// <summary>
    /// 客户端实体类，绑定了NetActor，UnitDefine，SkillManager，UnitState，renderObj
    /// </summary>
    public class Actor : Entity
    {
        public NetActor Info;//关联一个网络中的角色消息类
        public UnitDefine Define;//关联配置文件的一个职业数据结构类
        public SkillManager SkillMgr;//关联一个技能管理器

        public UnitState UnitState; //单位状态
        public bool IsDeath => UnitState == UnitState.Dead; //角色是否死亡

        //实体对应的游戏对象
        public GameObject renderObj;

        #region 对于NetActor的数据进行封装
        public long Gold => Info.Gold;
        public long Exp => Info.Exp;
        public int Level => Info.Level;
        public float Hp => Info.Hp;
        public float Mp => Info.Mp;
        public float HpMax => Info.Hpmax;
        public float MpMax => Info.Mpmax;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        public Actor(NetActor info) : base(info.Entity)
        {
            this.Info = info;
            if (info.Type != EntityType.Item)
            {
                this.Define = DataManager.Instance.Units[info.Tid];
                this.SkillMgr = new SkillManager(this);
            }

        }

        /// <summary>
        /// 接收伤害的逻辑
        /// </summary>
        /// <param name="damage">伤害的消息类</param>
        internal void recvDamage(Damage damage)
        {
            Vector3 _txtPos = renderObj.transform.position;

            //关于漂字UI显示的逻辑
            //是否闪避
            if (damage.IsMiss)
            {
                DynamicTextManager.CreateText(_txtPos, "Miss", DynamicTextManager.missData);
            }
            else
            {
                //伤害飘字
                DynamicTextManager.CreateText(_txtPos, damage.Amount.ToString("0"));
                //是否暴击
                if (damage.IsCrit)
                {
                    UIManager.ShakeScreen();
                    DynamicTextManager.CreateText(_txtPos, "Crit!", DynamicTextManager.critData);
                }
            }

            //伤害特效
            Actor actorAtker = FoundActorTool.GetUnit(damage.AttackerId);
            if (actorAtker == null) return;
            Battle.Skill skill = actorAtker.SkillMgr.GetSkill(damage.SkillId);
            ParticleSystem ps = Resources.Load<ParticleSystem>(skill.Def.HitArt);
            if (ps != null)
            {
                Vector3 pos = renderObj.transform.position + Vector3.up;
                Quaternion dir = renderObj.transform.rotation;
                ParticleSystem newPs = GameObject.Instantiate(ps, pos, dir);
                GameObject.Destroy(newPs.gameObject, newPs.main.duration);
            }
            else
            {
                Debug.LogWarning("Failed to load particle system!");
            }
        }

        public virtual void OnHpChanged(float old_hp, float new_hp)
        {
            this.Info.Hp = new_hp;
        }

        public virtual void OnMpChanged(float old_value, float new_value)
        {
            this.Info.Mp = new_value;
        }

        public virtual void OnStateChanged(UnitState old_value, UnitState new_value)
        {
            this.UnitState = new_value;
            //如果死掉了，那么播放死亡动画
            if (IsDeath)
            {
                if (renderObj == null) return;
                HeroAnimations ani = renderObj?.GetComponent<HeroAnimations>();
                ani.PlayDie();
            }
            //没有死掉那么就激活
            else
            {
                renderObj?.SetActive(true);
            }
        }

        internal void OnGoldsChanged(long oldValue, long value)
        {
            Info.Gold = value;
        }

        internal void OnExpChanged(long longValue1, long value)
        {
            Info.Exp = value;
        }

        internal void OnLevelChanged(int intValue1, int value)
        {
            Info.Level = value;
        }

        internal void OnHPMaxChanged(float value)
        {
            Info.Hpmax = value;
        }
        internal void OnMPMaxChanged(float value)
        {
            Info.Mpmax = value;
        }
    }
}
