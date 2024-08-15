using Summer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameClient.Battle;
using UnityEngine.UI;
using DG.Tweening;
using GameClient;

/// <summary>
/// 根据GameApp的数据控制蓄力条，聊天框,背包按钮的显影以及
/// 简单的摄像机抖动静态工具函数
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //技能蓄力条
    public Slider IntonateSlider;
    public SimpleChatBox ChatBox;
    public GameObject playerButtons;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //设置背包按钮
        playerButtons.SetActive(GameApp.Character != null);

        //根据技能的数据设置技能蓄力条
        Skill currSkill = GameApp.CurrSkill;
        if (currSkill != null && currSkill.skillStage == Skill.Stage.Intonate
                              && currSkill.Def.IntonateTime > 0.1f)
        {
            IntonateSlider.gameObject.SetActive(true);
            IntonateSlider.value = currSkill.IntonateProgress;
        }
        else
        {
            IntonateSlider.gameObject.SetActive(false);
        }

        //设置聊天框组件的显影
        if (GameApp.Character != null)
        {
            ChatBox.gameObject.SetActive(true);
        }
        else
        {
            ChatBox.gameObject.SetActive(false);
        }

        //快捷的选择敌人
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameApp.SelectTarget();
        }

        //如果目标死亡
        if (GameApp.Target != null && GameApp.Target.IsDeath)
        {
            GameApp.Target = null;
        }

        //快捷键显示或隐藏背包面板
        if (Input.GetKeyDown(KeyCode.U))
        {
            this.KnapsackToggle();
        }

    }

    //控制背包显影的函数
    public void KnapsackToggle()
    {
        if (GameApp.Character != null)
        {
            KnapsackPanel.Instance.Toggle();
        }
    }

    //控制射线机抖动
    public static void ShakeScreen(float shakeDuration = 0.5f, float shakeAmount = 0.1f)
    {
        Camera.main.transform.DOShakePosition(shakeDuration, shakeAmount);
    }


}
