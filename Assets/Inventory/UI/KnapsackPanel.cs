using GameClient;
using GameClient.InventorySystem;
using Proto;
using Serilog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnapsackPanel : MonoBehaviour
{
    public static KnapsackPanel Instance { get; private set; }


    //显示金币数量的组件
    public Text goldText;


    public void Toggle()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
        if (this.gameObject.activeSelf)
        {
            InventoryRequest req = new InventoryRequest()
            {
                EntityId = GameApp.Character.entityId,
                QueryKnapsack = true,
            };
            NetClient.Send(req);
        }
    }


    /// <summary>
    /// 设置UI插槽的数量
    /// </summary>
    /// <param name="slotCount"></param>
    public void SetSlotCount(int slotCount)
    {
        Transform gridTransform = transform.Find("Grid");

        // 获取现有的Slot组件数量
        int currentSlotCount = gridTransform.childCount;

        // 如果现有数量小于目标数量，则补齐
        if (currentSlotCount < slotCount)
        {
            int slotsToAdd = slotCount - currentSlotCount;
            UISlot prefab = Resources.Load<UISlot>("Beibao/UISlot");
            for (int i = 0; i < slotsToAdd; i++)
            {
                UISlot newSlot = Instantiate(prefab,gridTransform);

                UISlot slot = newSlot.GetComponent<UISlot>();

            }
        }

        // 如果现有数量大于目标数量，则删除多余的Slot组件
        else if (currentSlotCount > slotCount)
        {
            int slotsToRemove = currentSlotCount - slotCount;

            for (int i = currentSlotCount - 1; i >= slotCount; i--)
            {
                GameObject slotToRemove = gridTransform.GetChild(i).gameObject;
                GameObject.Destroy(slotToRemove);
            }
        }

        UISlot[] slotList = transform.GetComponentsInChildren<UISlot>();
        for (int i = 0; i < slotList.Length; i++)
        {
            slotList[i].Index = i;
        }
    }


    public void OnKnapsackReloaded()
    {
        GameClient.Entities.Character chr = GameApp.Character;
        //初始化背包插槽
        SetSlotCount(chr.Knapsack.Capacity);
        UISlot[] slotList = transform.GetComponentsInChildren<UISlot>();
        for (int i = 0; i < slotList.Length; i++)
        {
            UISlot slot = slotList[i];
            //查找插槽是否有物品
            if(chr.Knapsack.ItemDict.TryGetValue(i,out Item item))
            {
                
                slot.SetItem(item);
            }
            else
            {
                Log.Information("清空插槽：" + i);
                slot.SetItem(null);
            }
            
        }
    }



    void Start()
    {
        Instance = this;
        this.gameObject.SetActive(false);

        Kaiyun.Event.RegisterOut("OnKnapsackReloaded", this, "OnKnapsackReloaded");
        //SetSlotCount(10);
        //销毁当前全部的插槽
        foreach(UISlot slot in transform.GetComponentsInChildren<UISlot>())
        {
            Destroy(slot.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(GameApp.Character!=null)
        {
            goldText.text = GameApp.Character.Gold.ToString();
        }
    }
}
