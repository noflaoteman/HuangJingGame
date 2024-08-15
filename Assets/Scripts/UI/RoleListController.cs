using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proto;
using Summer.Network;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// 集合了MVC全部模块
/// </summary>
public class RoleListController : MonoBehaviour
{

    public GameObject RoleSelectPanel;
    public GameObject RoleCreatePanel;

    /// <summary>
    /// 左侧玩家创建的角色GameObject列表
    /// </summary>
    List<GameObject> PanelList = new List<GameObject>();

    /// <summary>
    /// 左侧玩家角色列表的数据
    /// </summary>
    List<RoleInfo> roleList = new List<RoleInfo>();

    /// <summary>
    /// 直接在代码中写配资文件的数据
    /// </summary>
    string[] Jobs = new string[] { "", "战士", "法师", "仙术", "游侠" };

    //记录选择的角色下标
    private int SelectedIndex = -1;
    //记录选择的职业id
    private int SelectedJobId = 1;

    void Start()
    {
        //监听消息
        MessageRouter.Instance.AddFuntionToDic<ChracterCreateResponse>(_ChracterCreateResponse);
        MessageRouter.Instance.AddFuntionToDic<CharacterListResponse>(_CharacterListResponse);
        MessageRouter.Instance.AddFuntionToDic<CharacterDeleteResponse>(_CharacterDeleteResponse);

        //得到层级窗口的gameObject控件
        for (int i = 0; i < 4; i++)
        {
            PanelList.Add(GameObject.Find($"HeroPanel ({i})"));
        }

        //先隐藏所有面板
        foreach (GameObject p in PanelList)
            p.SetActive(false);

        //发送加载当前玩家创建的角色的消息请求
        CharacterListRequest listReq = new CharacterListRequest();
        NetClient.Send(listReq);
    }

    /// <summary>
    /// 删除角色的响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void _CharacterDeleteResponse(Connection conn, CharacterDeleteResponse msg)
    {
        //再次发送加载当前玩家角色的请求，那么收到消息后进入_CharacterListResponse
        CharacterListRequest listReq = new CharacterListRequest();
        NetClient.Send(listReq);
    }

    /// <summary>
    /// 根据网络中接收到的数据对Roles进行赋值及其显示
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void _CharacterListResponse(Connection sender, CharacterListResponse msg)
    {
        //清空左侧角色的数据
        roleList.Clear();

        //通过网络中的数据对roleList进行赋值
        foreach (NetActor Nactor in msg.CharacterList)
        {
            roleList.Add(new RoleInfo() { Name = Nactor.Name, Job = Nactor.Tid, Level = Nactor.Level, RoleId = Nactor.Id });
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //先隐藏所有的角色按钮
            foreach (GameObject p in PanelList)
                p.SetActive(false);
            //再根据角色列表逐个显示
            for (int i = 0; i < roleList.Count; i++)
            {
                PanelList[i].SetActive(true);
                PanelList[i].transform.Find("Text (名字)").GetComponent<Text>().text = roleList[i].Name;
                PanelList[i].transform.Find("Text (职业)").GetComponent<Text>().text = Jobs[roleList[i].Job];
                PanelList[i].transform.Find("Text (等级)").GetComponent<Text>().text = roleList[i].Level + "";
            }
        });

    }

    private void _ChracterCreateResponse(Connection conn, ChracterCreateResponse msg)
    {

        MyDialog.ShowMessage("系统消息", msg.Message);

        if (msg.Success)
        {
            //再次发送加载当前玩家角色的请求，那么收到消息后进入_CharacterListResponse
            CharacterListRequest listReq = new CharacterListRequest();
            NetClient.Send(listReq);
            //切换UI
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                RoleSelectPanel.SetActive(true);
                RoleCreatePanel.SetActive(false);
            });
        }
    }
    void Update()
    {

    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    public void LoadRoles()
    {
        //先隐藏所有面板
        foreach (GameObject p in PanelList) p.SetActive(false);
        //通过roleList数据更新左侧面板
        for (int i = 0; i < roleList.Count; i++)
        {
            PanelList[i].SetActive(true);
            PanelList[i].transform.Find("Text (名字)").GetComponent<Text>().text = roleList[i].Name;
            PanelList[i].transform.Find("Text (职业)").GetComponent<Text>().text = Jobs[roleList[i].Job];
            PanelList[i].transform.Find("Text (等级)").GetComponent<Text>().text = roleList[i].Level + "";
        }

    }

    public void RoleClick(int index)
    {
        Debug.Log(index);
        //记录选择的角色索引
        SelectedIndex = index;

        //通过数据更新右侧面板
        RoleInfo role = roleList[index];
        GameObject rightPanel = GameObject.Find("Panel2");
        rightPanel.transform.Find("Name/Text").GetComponent<Text>().text = role.Name;
        rightPanel.transform.Find("Job/Text").GetComponent<Text>().text = Jobs[role.Job];
        rightPanel.transform.Find("Level/Text").GetComponent<Text>().text = role.Level + "";

        for (int i = 0; i < PanelList.Count; i++)
        {
            //控制选择的背景
            PanelList[i].transform.Find("Image").gameObject.SetActive(i == index);
        }

    }

    //选择角色的按钮事件传入JobID
    public void SelectJob(int jobId)
    {
        //记录选择的职业ID
        SelectedJobId = jobId;

        GameObject.Find("JobText").GetComponent<Text>().text = Jobs[jobId];
    }

    public void ToCreate()
    {
        RoleSelectPanel.SetActive(false);
        RoleCreatePanel.SetActive(true);
    }

    public void ToSelect()
    {
        RoleSelectPanel.SetActive(true);
        RoleCreatePanel.SetActive(false);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    public void DeleteRole()
    {
        if (SelectedIndex < 0)
            return;

        Chibi.Free.Dialog.ActionButton ok = new Chibi.Free.Dialog.ActionButton("确定", () =>
        {
            RoleInfo role = roleList[SelectedIndex];
            Debug.Log($"删除角色：{role.RoleId} , {role.Name}");
            //发送删除角色的请求
            CharacterDeleteRequest delReq = new CharacterDeleteRequest();
            delReq.CharacterId = role.RoleId;
            NetClient.Send(delReq);
        }, new Color(0f, 0.9f, 0.9f));
        Chibi.Free.Dialog.ActionButton cannel = new Chibi.Free.Dialog.ActionButton("取消", () => { }, new Color(0f, 0.9f, 0.9f));
        Chibi.Free.Dialog.ActionButton[] buttons = { ok, cannel };
        MyDialog.Show("系统提示", "确定删除此角色吗？删除后无法恢复。", buttons);


    }
    /// <summary>
    /// 进入游戏
    /// </summary>
    public void StartGame()
    {
        if (SelectedIndex < 0)
            return;
        RoleInfo role = roleList[SelectedIndex];
        Debug.Log("进入游戏：" + role.Name);
        //执行NetStart中的EnterGame方法
        Kaiyun.Event.FireIn("EnterGame", role.RoleId);
    }
    /// <summary>
    /// 创建角色
    /// </summary>
    public void CreateRole()
    {
        InputField input = GameObject.Find("InputField (TempName)").GetComponent<InputField>();
        Debug.Log($"Job={SelectedJobId};Name={input.text}");

        //发送创建角色的消息数据
        CharacterCreateRequest req = new CharacterCreateRequest();
        req.Name = input.text;
        req.JobType = SelectedJobId;
        NetClient.Send(req);

    }


    class RoleInfo
    {
        public GameObject TargetPanel;
        public string Name; //玩家创建的名字
        public int Job;     //1战士，2法师，3仙术，4游侠
        public int Level;   //等级
        public int RoleId;  //角色ID
    }
}
