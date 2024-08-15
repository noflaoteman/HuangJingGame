using GameClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 聊天组件
/// </summary>
public class SimpleChatBox : MonoBehaviour
{
    //全局唯一单例
    public static SimpleChatBox Instance;

    public InputField inputField;
    public Scrollbar scrollbar;
    public ScrollRect scrollRect;
    public GameObject contentNode;
    public Text defaultText;


    //创建新的文字
    public void CreateText(string value)
    {
        Text clonedText = Instantiate(defaultText, contentNode.transform);
        clonedText.text = value;
        clonedText.gameObject.SetActive(true);
        //滚动到底部
        if (scrollbar.value < 0.01f)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(contentNode != null && contentNode.transform.childCount > 200+1)
        {
            Destroy(contentNode.transform.GetChild(1).gameObject);
        }
        
    }

}
