using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    [Serializable]
    public class MaterialAmount
    {
        public Materials material;
        public int amount;

        public MaterialAmount(Materials material, int amount)
        {
            this.material = material;
            this.amount = amount;
        }
    }

    public class Warehouse : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private List<MaterialAmount> needs = new List<MaterialAmount>();
        [SerializeField] private List<MaterialAmount> inventory = new List<MaterialAmount>();

        public int Id => id;
        public IReadOnlyList<MaterialAmount> Needs => needs;
        public IReadOnlyList<MaterialAmount> Inventory => inventory;

        public int GetInventoryAmount(Materials material)
        {
            return GetAmount(inventory, material);
        }

        public int GetNeededAmount(Materials material)
        {
            return GetAmount(needs, material);
        }

        public void SetInventoryAmount(Materials material, int amount)
        {
            SetAmount(inventory, material, amount);
        }

        public void SetNeededAmount(Materials material, int amount)
        {
            SetAmount(needs, material, amount);
        }

        private static int GetAmount(List<MaterialAmount> source, Materials material)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].material == material)
                {
                    return source[i].amount;
                }
            }

            return 0;
        }

        private static void SetAmount(List<MaterialAmount> target, Materials material, int amount)
        {
            for (int i = 0; i < target.Count; i++)
            {
                if (target[i].material == material)
                {
                    target[i].amount = amount;
                    return;
                }
            }

            target.Add(new MaterialAmount(material, amount));
        }

        private void Reset()
        {
            // Example defaults: wood 500, steel 300.
            needs = new List<MaterialAmount>
            {
                new MaterialAmount(Materials.Wood, 500),
                new MaterialAmount(Materials.Steel, 300)
            };

            inventory = new List<MaterialAmount>();
        }
    }
}
