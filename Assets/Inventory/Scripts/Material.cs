using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameClient
{


    /// <summary>
    /// 材料
    /// </summary>
    public class Material : Item
    {
        public Material(int id, string name, ItemType itemType, Quality quality, string description, int capicity, int buyPrice, int sellPrice, string sprite) : base(id, name, itemType, quality, description, capicity, buyPrice, sellPrice, sprite)
        {
        }
    }



}

