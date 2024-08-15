using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameClient
{

    //物品类型
    public enum ItemType
    {
        Consumable,     //消耗品
        Equipment,      //武器&装备
        Material,       //材料
    }

    public enum Quality //物品品质
    {
        Common,     // 普通
        Uncommon,   // 非凡
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary,  // 传说
        Artifact,   // 神器
    }


    /// <summary>
    /// 物品基类
    /// </summary>
    [Serializable]
    public class Item
    {
        public int Id { get; set; } // 物品ID
        public string Name { get; set; } // 物品名称
        public ItemType ItemType { get; set; } // 物品种类
        public Quality Quality { get; set; } // 物品品质
        public string Description { get; set; } // 物品描述
        public int Capicity { get; set; } // 物品叠加数量上限
        public int BuyPrice { get; set; } // 物品买入价格
        public int SellPrice { get; set; } // 物品卖出价格
        public string Sprite { get; set; } // 存放物品的图片路径，通过Resources加载
        public ItemDefine Def { get; private set; }

        public int amount;          //数量
        public int position;        //所处位置


        private Sprite _sprite;
        public Sprite SpriteImage => _sprite ?? (_sprite = Resources.Load<Sprite>(Sprite));


        public virtual void Use()
        {

        }



        private ItemInfo _itemInfo;
        public ItemInfo ItemInfo
        {
            get
            {
                if (_itemInfo == null)
                {
                    _itemInfo = new ItemInfo() { ItemId = Id };
                }
                _itemInfo.Amount = amount;
                _itemInfo.Position = position;
                return _itemInfo;
            }
        }

        public Item(int itemId, int amount = 1, int position = 0)
            : this(DataManager.Instance.Items[itemId], amount, position)
        {

        }

        public Item(ItemInfo itemInfo) : this(DataManager.Instance.Items[itemInfo.ItemId])
        {
            this.amount = itemInfo.Amount;
            this.position = itemInfo.Position;
        }

        public Item(ItemDefine _def, int amount = 1, int position = 0) : this(_def.ID, _def.Name, ItemType.Material, Quality.Common,
            _def.Description, _def.Capicity, _def.BuyPrice, _def.SellPrice, _def.Icon)
        {
            Def = _def;
            this.amount = amount;
            this.position = position;
            switch (Def.ItemType)
            {
                case "消耗品": this.ItemType = ItemType.Consumable; break;
                case "道具": this.ItemType = ItemType.Material; break;
                case "装备": this.ItemType = ItemType.Equipment; break;
            }
            switch (Def.Quality)
            {
                case "普通": this.Quality = Quality.Common; break;
                case "非凡": this.Quality = Quality.Uncommon; break;
                case "稀有": this.Quality = Quality.Rare; break;
                case "史诗": this.Quality = Quality.Epic; break;
                case "传说": this.Quality = Quality.Legendary; break;
                case "神器": this.Quality = Quality.Artifact; break;
            }
        }
        public Item(int id, string name, ItemType itemType, Quality quality, string description, int capicity, int buyPrice, int sellPrice, string sprite)
        {
            Id = id;
            Name = name;
            ItemType = itemType;
            Quality = quality;
            Description = description;
            Capicity = capicity;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            Sprite = sprite;
        }

        /// <summary>
        /// 获取描述文本
        /// </summary>
        /// <returns></returns>
        public virtual string GetTipText()
        {
            string content =
                $"<color=#ffffff>{this.Name}</color>\n" +
                $"<color=yellow>{this.Description}</color>\n\n" +
                $"<color=bulue>堆叠上限：{this.Capicity}</color>";
            return content;
        }

    }

}
