
using GameClient.Entities;
using Google.Protobuf;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.InventorySystem
{
    /// <summary>
    /// 库存对象
    /// </summary>
    public class Inventory
    {

        public Character Chr { get; private set; }
        //背包容量
        public int Capacity { get; private set; }

        //物品字典 <插槽索引，物品对象>
        public ConcurrentDictionary<int, Item> ItemDict { get; private set; } = new();


        public Inventory(Character _chr)
        {
            Chr = _chr;

        }

        /// <summary>
        /// 重新加载背包
        /// </summary>
        /// <param name="inventoryInfo"></param>
        public void Reload(InventoryInfo info)
        {
            ItemDict.Clear();
            this.Capacity = info.Capacity;
            foreach (ItemInfo _itemInfo in info.List)
            {
                Item item = new Item(_itemInfo);
                ItemDict.TryAdd(item.position, item);
            }
            //Log.Information("第2个插槽的物品：{0}", ItemDict[1].Name);
        }




    }
}
