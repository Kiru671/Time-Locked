using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string itemClass; // Item sınıfı (ghost tetikleyici için)
    public bool isTriggerItem = false; // Ghost tetikleyici item mi?
}
