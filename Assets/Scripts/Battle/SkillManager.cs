
using GameClient.Battle;
using GameClient.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient.Mgr
{
    /// <summary>
    /// 技能管理器，每个Actor都有独立的技能管理器
    /// </summary>
    public class SkillManager
    {
        //归属的角色
        private Actor actor;
        //技能列表
        public List<Skill> skillList = new();

        public SkillManager(Actor actor)
        {
            this.actor = actor;
            this.InitSkills();
        }

        public void InitSkills()
        {
            //把actor.NetAcotr记录的技能数据赋值到了该类中的skillList中
            foreach (Proto.SkillInfo skInfo in actor.Info.Skills)
            {
                //对skill进行数据赋值
                Skill skill = new Skill(actor, skInfo.Id);
                skillList.Add(skill);
                Log.Information("角色[{0}]加载技能[{1}-{2}]", actor.Define.Name, skill.Def.ID, skill.Def.Name);
            }
        }

        public void OnUpdate(float delta)
        {
            foreach (Skill skill in skillList)
            {
                skill.OnUpdate(delta);
            }
        }

        /// <summary>
        /// 通过skillid从技能管理器中找到技能
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        internal Skill GetSkill(int skillId)
        {
            return skillList.FirstOrDefault(skill => skill.Def.ID == skillId);
        }
    }

}

