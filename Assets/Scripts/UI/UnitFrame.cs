using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameClient;
using GameClient.Entities;
using Serilog;

public class UnitFrame : MonoBehaviour
{
    public Image avatarImage; //头像图片
    public Image healthBar; //生命条图片
    public Image manabar;   //蓝条图片
    public Text levelText;  //等级文本
    public Text nameText;   //名字文本
    public Text hpText;     //生命值文本
    public Text mpText;     //法力值文本
    
    public Actor actor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (actor == null) return;
        nameText.text = actor.Info.Name;
        levelText.text = actor.Level + "";
        healthBar.fillAmount = actor.Hp / actor.HpMax;
        manabar.fillAmount = actor.Mp / actor.MpMax;
        hpText.text = actor.Hp + " / " + actor.HpMax;
        mpText.text = actor.Mp + " / " + actor.MpMax;
    }
}
