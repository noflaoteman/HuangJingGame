using GameClient.Entities;
using GameClient.InventorySystem;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.InventorySystem
{
    public class Knapsack : Inventory
    {
        public Knapsack(Character _chr) : base(_chr)
        {
        }

    }
}
