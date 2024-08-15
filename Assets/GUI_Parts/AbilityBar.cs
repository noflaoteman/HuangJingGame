
using GameClient;
using GameClient.Battle;
using Proto;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityBar : MonoBehaviour, IPointerClickHandler
{

    public static List<AbilityBar> AbilityGroup = new List<AbilityBar>();

    public Sprite icon;             // 图标
    public string name;             // 名称
    public string description;      // 简介
    public float cooldown;          // 冷却读秒
    public float maxCooldown;       // 总冷却时间

    private Image iconImg;
    private Image cdImg;    //冷却图层
    private Text text;      //显示秒数

    public Skill skill;     //技能对象

    // Start is called before the first frame update
    void Start()
    {
        iconImg = transform.Find("Icon").GetComponent<Image>();
        cdImg = transform.Find("Cooldown").GetComponent<Image>();
        text = transform.Find("Text").GetComponent<Text>();
    }

    private void TakeSkill()
    {
        if (skill == null) return;
        Log.Information("技能点击：[{0}]{1}", skill.Def.ID, skill.Def.Name);

        GameApp.Spell(skill);

    }

    // Update is called once per frame
    void Update()
    {
        iconImg.sprite = icon;
        iconImg.enabled = icon != null;
        cdImg.fillAmount = cooldown / maxCooldown;

        text.enabled = cooldown > 0;
        if (text.enabled)
        {
            if (cooldown > 1.0f)
            {
                text.text = cooldown.ToString("F0"); // 大于1秒时不显示小数
            }
            else
            {
                text.text = cooldown.ToString("F1"); // 显示1位小数
            }

        }


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TakeSkill();
    }
}
