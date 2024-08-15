using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NumberInputBox : MonoBehaviour
{
    public static NumberInputBox Instance;


    public Text titleText;
    public InputField inputField;
    public Button incButton;
    public Button decButton;
    public Button okButton;
    public Button cancelButton;

    private int currentValue;
    private Action<int> _okAction;
    private Action _cancelAction;

    private void Start()
    {
        // 初始化当前值为0
        currentValue = 0;
        // 输入框只允许填数字
        inputField.contentType = InputField.ContentType.IntegerNumber;


        // 添加按钮的点击事件
        incButton.onClick.AddListener(OnIncButtonClick);
        decButton.onClick.AddListener(OnDecButtonClick);
        okButton.onClick.AddListener(OnOkButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);

        // 更新显示文本
        UpdateDisplayText();
        // 设置全局实例
        Instance = this;
        // 初始隐藏
        this.gameObject.SetActive(false);
    }

    private void OnIncButtonClick()
    {
        // 当inc按钮被点击时，增加当前值1
        currentValue++;

        // 更新显示文本
        UpdateDisplayText();
    }

    private void OnDecButtonClick()
    {
        // 当dec按钮被点击时，减少当前值1
        currentValue--;

        // 更新显示文本
        UpdateDisplayText();
    }

    private void OnOkButtonClick()
    {
        // 当ok按钮被点击时，将当前值传递给其他组件处理
        currentValue = int.Parse(inputField.text);
        Debug.Log("Current value is " + currentValue);
        _okAction?.Invoke(currentValue);
        Hide();
    }

    private void OnCancelButtonClick()
    {
        // 当cancel按钮被点击时，取消操作
        Debug.Log("Cancelled input");
        _cancelAction?.Invoke();
        Hide();
    }

    private void UpdateDisplayText()
    {
        // 更新显示文本
        inputField.text = currentValue.ToString();
    }

    public void Show(string title, int value, Action<int> ok=null, Action cancel=null)
    {
        currentValue = value;
        titleText.text = title;
        inputField.text = currentValue.ToString();
        _okAction = ok;
        _cancelAction = cancel;
        //置于顶层显示
        transform.SetAsLastSibling();
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        _okAction = null;
        _cancelAction = null;
    }
}
