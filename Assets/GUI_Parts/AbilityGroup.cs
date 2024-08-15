
using GameClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityGroup : MonoBehaviour
{


    public List<AbilityBar> bars = new List<AbilityBar>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameApp.Character == null) return;
        List<GameClient.Battle.Skill> list = GameApp.Character.SkillMgr.skillList;
        for (int i = 0; i < bars.Count; i++)
        {
            AbilityBar bar = bars[i];
            if (i < list.Count)
            {
                GameClient.Battle.Skill sk = list[i];
                bar.icon = sk.Icon;
                bar.name = sk.Def.Name;
                bar.description = sk.Def.Description;
                bar.maxCooldown = sk.Def.CD;
                bar.cooldown = sk.Cd;
                bar.skill = sk;
            }
            else
            {
                bar.icon = null;
                bar.name = "";
                bar.description = "";
                bar.maxCooldown = 1;
                bar.cooldown = 0;
                bar.skill = null;
            }

        }
    }
}
