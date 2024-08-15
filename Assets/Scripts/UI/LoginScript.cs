using Chibi.Free;
using Proto;
using Summer.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LoginScript : MonoBehaviour
{


    public GameObject loginPanel;
    public GameObject registerPanel;

    public InputField loginUserName;
    public InputField loginPassword;

    public InputField registerUserName;
    public InputField registerPassword;


    // Start is called before the first frame update
    void Start()
    {
        MessageRouter.Instance.AddFuntionToDic<UserLoginResponse>(_UserLoginResponse);
        MessageRouter.Instance.AddFuntionToDic<UserRegisterResponse>(_UserRegisterResponse);
    }

    private void _UserRegisterResponse(Connection sender, UserRegisterResponse msg)
    {
        MyDialog.ShowMessage("系统信息", msg.Message);
    }

    //设置模式，1=登录，2=注册
    public void SetMode(int mode)
    {
        loginPanel.SetActive(mode == 1);
        registerPanel.SetActive(mode == 2);
    }

    /// <summary>
    /// 用户登录的响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _UserLoginResponse(Connection sender, UserLoginResponse msg)
    {
        Debug.Log(msg);
        Dialog.ActionButton ok = new Chibi.Free.Dialog.ActionButton("确定", () =>
        {
            Debug.Log("click ok");
        }, new Color(0f, 0.9f, 0.9f));
        Chibi.Free.Dialog.ActionButton[] buttons = { ok };
        MyDialog.Show("系统消息", msg.Message, buttons);

        if(msg.Success)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                //进入角色列表
                SceneManager.LoadScene("RoleList");
            });
        }

    }



    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoLogin()
    {
        Debug.Log($"Login : User={loginUserName.text};Pwd={loginPassword.text}");
        UserLoginRequest msg = new UserLoginRequest();
        msg.Username = loginUserName.text;
        msg.Password = loginPassword.text;
        NetClient.Send(msg);
    }

    public void DoRegister()
    {
        Debug.Log($"Register : User={loginUserName.text};Pwd={loginPassword.text}");
        UserRegisterRequest msg = new UserRegisterRequest();
        msg.Username = registerUserName.text;
        msg.Password = registerPassword.text;
        NetClient.Send(msg);
    }
}
