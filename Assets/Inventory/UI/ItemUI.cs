using System;
using System.Collections;
using System.Collections.Generic;
using GameClient;
using Serilog;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    , IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 offset;
    private Transform initialParent;
    private Vector3 initialPosition;
    private bool isDragging;
    private UISlot originSlot;//原始的插槽

    private Text textAmount; //显示物品数量的文本组件
    private Image imageIcon; //图片组件

    public Item Item { get; set; }


    private void Start()
    {
        textAmount = GetComponentInChildren<Text>();
        imageIcon = GetComponent<Image>();
        textAmount.raycastTarget = false;

    }

    private void Update()
    {
        if(Item != null)
        {
            int amount = Item.amount;
            textAmount.text = amount.ToString();
            textAmount.gameObject.SetActive(amount > 1);
            imageIcon.sprite = Item.SpriteImage;
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //检查是否可以拖拽
        if (!CanDrag(eventData)) return;
        //记录原始的插槽
        originSlot = transform.parent.GetComponent<UISlot>();
        //记录初始位置和偏移量
        offset = transform.position - Input.mousePosition;
        initialParent = transform.parent;
        initialPosition = transform.position;

        //将物品UI从原来的格子中移除
        transform.SetParent(transform.root);

        //标记为正在拖拽中
        isDragging = true;

        //隐藏物品的RaycastTarget，避免干扰鼠标事件
        imageIcon.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 检查是否可以拖拽
        if (!CanDrag(eventData)) return;
        // 更新物品位置
        transform.position = Input.mousePosition + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 检查是否可以拖拽
        if (!CanDrag(eventData)) return;

        UISlot targetSlot = null;
        if (EventSystem.current.IsPointerOverGameObject()) //是否指向UI组件
        {
            //获取鼠标位置的游戏对象
            Debug.Log(eventData.pointerEnter.gameObject);
            //查找插槽
            targetSlot = eventData.pointerEnter.gameObject.GetComponent<UISlot>();
            //通过父对象找插槽
            if(targetSlot == null)
            {
                targetSlot = eventData.pointerEnter.GetComponentInParent<UISlot>();
            }
            //放置物品
            if (targetSlot != null)
            {
                if (targetSlot.ItemUI == null)
                {
                    Debug.Log("将物品放置到目标格子中");
                    // 将物品放置到目标格子中
                    //targetSlot.ItemUI = this;
                }
                else
                {
                    Debug.Log("物品交换");
                    //originSlot.ItemUI = targetSlot.ItemUI;
                    //targetSlot.ItemUI = this;
                }
                //因为重新加载服务端的物品，所以当前正在拖拽的图标可以删除
                Destroy(this.gameObject);
                //放置物品
                ItemPlacement(originSlot.Index, targetSlot.Index);
            }
            else
            {
                // 还原物品位置和父级格子
                Debug.Log("还原物品位置和父级格子");
                originSlot.ItemUI = this;
            }
        }
        else
        {
            //丢弃或者还原
            //originSlot.ItemUI = this;
            Log.Debug("丢弃或者还原");
            //销毁图标
            Destroy(this.gameObject);

            //丢弃物品
            
            NumberInputBox.Instance.Show("丢弃物品", this.Item.amount,
                ok: (value) => {
                    Kaiyun.Event.FireIn("ItemDiscard", originSlot.Index, value);
                },
                cancel: () => {
                    Kaiyun.Event.FireIn("ItemDiscard", originSlot.Index, 0);
                });


        }

        // 取消拖拽标记
        isDragging = false;
        // 恢复物品的RaycastTarget
        imageIcon.raycastTarget = true;
    }

    //物品放置
    private void ItemPlacement(int originIndex, int targetIndex)
    {
        Log.Debug("插槽发生交换:{0}=>{1}", originIndex, targetIndex);
        Kaiyun.Event.FireIn("ItemPlacement",originIndex,targetIndex);
    }



    //是否允许拖拽
    private bool CanDrag(PointerEventData eventData)
    {
        return (eventData.button == PointerEventData.InputButton.Left);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string content = "<color=#ffffff>物品信息为空</color>";
        if (this.Item != null)
        {
            content = this.Item.GetTipText();
        }

        ToolTip.Instance.Show(content);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //双击使用物品
        if(eventData.clickCount == 2)
        {
            UISlot slot = transform.parent.GetComponent<UISlot>();
            if (slot != null)
            {
                Kaiyun.Event.FireIn("UseItem", slot.Index);
            }
            
        }
    }
}
