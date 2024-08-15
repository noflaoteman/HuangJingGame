using GameClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通过GameApp的数据对于下拉框，角色技能UI控件们，主角/角色信息框进行显影控制
/// </summary>
public class FrameDropSksGroup : MonoBehaviour
{

    public Dropdown dropdown;

    public AbilityGroup abilityGroup;
    //主角信息框
    public UnitFrame playerFrame;
    //目标信息框
    public UnitFrame targetFrame;

    //对于dropDown进行操作
    void Start()
    {
        //如果是移动平台
        if (Application.isMobilePlatform)
        {
            return;
        }
        dropdown.onValueChanged.AddListener((value) =>
        {
            Debug.Log(value.ToString());
            if (value == 0)
            {
                Screen.SetResolution(1280, 720, false);
            }
            else if (value == 1)
            {
                Screen.SetResolution(800, 360, false);
            }
            else if (value == 2)
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
        });
        Screen.SetResolution(1280, 720, false);
        dropdown.SetValueWithoutNotify(0);
    }

    //根据GameApp的数据对UI进行显隐的控制
    void Update()
    {
        abilityGroup.gameObject.SetActive(GameApp.Character != null);
        playerFrame.gameObject.SetActive(GameApp.Character != null);
        targetFrame.gameObject.SetActive(GameApp.Target != null);

        playerFrame.actor = GameApp.Character;
        targetFrame.actor = GameApp.Target;
    }
}
