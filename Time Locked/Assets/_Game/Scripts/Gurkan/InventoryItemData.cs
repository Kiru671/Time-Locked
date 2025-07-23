using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public NetworkObject prefab;
}
