using UnityEngine;

[CreateAssetMenu(fileName = "ResourceCondition", menuName = "Club Fungal/Network Run/Resource Condition")]
public class ResourceCondition : DoorCondition
{
    [SerializeField] private ItemTemplate requiredItem;
    [SerializeField] private int requiredAmount;

    public ItemTemplate RequiredItem => requiredItem;
    public int RequiredAmount => requiredAmount;

    public void Initialize(ItemTemplate item, int amount)
    {
        requiredItem = item;
        requiredAmount = amount;
    }

    public override bool IsMet(Inventory inventory)
    {
        if (requiredItem == null) return true;
        return inventory.GetItemCount(requiredItem) >= requiredAmount;
    }

    public override string GetDescription()
    {
        if (requiredItem == null) return "No requirement";
        return $"{requiredAmount}x {requiredItem.DisplayName}";
    }

    public override void Satisfy(Inventory inventory)
    {
        if (requiredItem == null || inventory == null) return;

        for (int i = 0; i < requiredAmount; i++)
        {
            inventory.RemoveItem(requiredItem);
        }
    }
}