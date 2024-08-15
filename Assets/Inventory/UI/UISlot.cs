using GameClient;
using Serilog;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{

    private ItemUI _itemUI;

    //插槽的索引
    public int Index { get; set; }

    public ItemUI ItemUI
    {
        get
        {
            return transform.GetComponentInChildren<ItemUI>();
        }
        set
        {
            _itemUI = value;
            value.transform.SetParent(transform);
            value.transform.position = transform.position;
        }
    }

    //设置插槽物品
    public void SetItem(Item item)
    {
        //清空插槽
        if (item == null)
        {
            if(_itemUI != null && !_itemUI.gameObject.IsDestroyed())
            {
                Destroy(_itemUI.gameObject);
                _itemUI = null;
            }
            return;
        }
        //设置插槽
        if(transform.GetComponentInChildren<ItemUI>() == null)
        {
            ItemUI prefab = Resources.Load<ItemUI>("Beibao/ItemUI");
            _itemUI = Instantiate(prefab,transform);
        }
        _itemUI.Item = item;
    }

    private bool isHighlighted;

    public void OnDrop(PointerEventData eventData)
    {
        // 判断是否有物品被拖拽到该格子上
        /*Debug.Log("Slot:判断是否有物品被拖拽到该格子上");
        if (eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<ItemUI>() != null)
        {
            // 将物品放置在该格子内
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.transform.position = transform.position;
        }*/
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 高亮格子
        isHighlighted = true;
        Highlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 取消高亮
        isHighlighted = false;
        Highlight();
    }

    private void Highlight()
    {
        // 根据isHighlighted设置格子的高亮效果
        // 实现自定义的逻辑，例如修改背景颜色或显示/隐藏高亮边框等
    }

    


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if(Item != null)
        {
            if(Item.amount > 1)
            {
                text.text = Item.amount.ToString();
            }
            icon.sprite = Item.SpriteImage;
            icon.gameObject.SetActive(true);
        }
        else
        {
            text.text = "";
            icon.sprite=null;
            icon.gameObject.SetActive(false);
        }*/
    }

}

