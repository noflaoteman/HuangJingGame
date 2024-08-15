using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Entities
{
    /// <summary>
    /// 场景里的物品
    /// </summary>
    public class ItemEntity : Actor
    {
        public ItemEntity(NetActor info) : base(info)
        {
            this.Item = new Item(info.ItemInfo);
        }

        //真正的物品对象
        public Item Item { get; set; }


    }
}
