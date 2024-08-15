using GameClient.Battle;
using GameClient.Entities;
using Proto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace GameClient
{
    public class GameApp
    {
        // 全局角色
        public static Character Character;
        // 选择的目标，按下tab，鼠标点击，放技能，目标死亡时赋值
        public static Actor Target;
        // 当前正在施放的技能
        public static Skill CurrSkill;
        // 是否正输入文字
        public static bool IsInputting
        {
            get => SimpleChatBox.Instance.inputField.isFocused;
        }

        /// <summary>
        /// 进入对应的场景
        /// </summary>
        /// <param name="spaceId"></param>
        public static void LoadSpace(int spaceId)
        {
            //通过传入的spaceid从配置文件中找出场景信息
            SpaceDefine spaceDefine = DataManager.Instance.Spaces[spaceId];
            //根据配置文件记录的场景名字加载场景
            SceneManager.LoadScene(spaceDefine.Resource);
            MakeEventSystem();
        }
        public static void MakeEventSystem()
        {
            // 查找场景中的EventSystem对象
            GameObject eventSystem = GameObject.FindObjectOfType<EventSystem>()?.gameObject;
            if (eventSystem == null)
            {
                // 如果没有找到EventSystem对象，就创建一个新的
                GameObject newEventSystem = new GameObject("EventSystem");
                newEventSystem.AddComponent<EventSystem>();
                newEventSystem.AddComponent<StandaloneInputModule>();

                Debug.Log("场景中未发现 EventSystem，已自动创建。");
            }
            else
            {
                Debug.Log("场景中已存在 EventSystem。");
            }
        }

        //放技能，按下快捷键T会触发
        public static void SelectTarget()
        {
            Log.Information("选择目标");
            Target = FoundActorTool.RangeUnit(Character.Position, 12000)
                .Where(actor => actor is not ItemEntity)
                .OrderBy(actor => Vector3.Distance(Character.Position, actor.Position))
                .FirstOrDefault(actor => actor.entityId != Character.entityId && !actor.IsDeath);
        }

        /// <summary>
        /// 放技能
        /// </summary>
        /// <param name="skill">技能类型</param>
        public static void Spell(Skill skill)
        {
            if (skill.IsUnitTarget && Target == null)
            {
                SelectTarget();
                //若选择角色后为空则不释放技能
                if (Target == null)
                {
                    Log.Information("无效的技能目标");
                    return;
                }
            }

            //对于消息进行设置后发送
            SpellRequest req = new SpellRequest() { Info = new() };
            req.Info.CasterId = Character.entityId;
            req.Info.SkillId = skill.Def.ID;
            //如果是一个单位技能
            if (skill.IsUnitTarget)
            {
                req.Info.TargetId = Target.entityId;
            }
            //是点的攻击
            else if (skill.IsPointTarget)
            {
                req.Info.TargetLoc = V3.ToVec3(Target.Position);
            }
            NetClient.Send(req);
        }

    }
}
