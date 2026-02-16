using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryReference inventory;

    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Runtime")]
    [SerializeField] private ItemStack itemStack;

    private void Awake()
    {
        button.onClick.AddListener(() => inventory.SelectItem(itemStack?.Item));
    }

    public void SetItemStack(ItemStack itemStack)
    {
        this.itemStack = itemStack;

        if (itemStack?.Item)
        {
            if (nameText)
            {
                nameText.text = itemStack.Item.Name;
            }
            image.sprite = itemStack.Item.Sprite;
            if (countText)
            {
                countText.text = itemStack.Count > 1 ? $"x{itemStack.Count}" : "";
            }
        }

        if (nameText)
        {
            nameText.gameObject.SetActive(itemStack?.Item);
        }
        image.enabled = itemStack?.Item;
        if (countText)
        {
            countText.gameObject.SetActive(itemStack?.Item && itemStack.Count > 1);
        }
    }
}
