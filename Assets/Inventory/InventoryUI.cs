using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryReference inventory;
    [SerializeField] private InventoryItemUI itemViewPrefab;
    [SerializeField] private Transform itemContainer;

    private List<InventoryItemUI> itemViewList = new List<InventoryItemUI>();

    public event UnityAction<Item> OnItemSelected;

    private void Awake()
    {
        if (!itemContainer)
        {
            itemContainer = transform;
        }

        itemContainer.GetComponentsInChildren(true, itemViewList);
    }

    private void Start()
    {
        UpdateView();
    }

    private void OnEnable()
    {
        inventory.OnInventoryOpened += UpdateView;
        inventory.OnInventoryChanged += UpdateView;
        inventory.OnItemSelected += HandleItemSelected;
    }

    private void OnDisable()
    {
        inventory.OnInventoryOpened -= UpdateView;
        inventory.OnInventoryChanged -= UpdateView;
        inventory.OnItemSelected -= HandleItemSelected;
    }

    private void HandleItemSelected(Item item)
    {
        OnItemSelected?.Invoke(item);
    }

    private void UpdateView()
    {
        var sortedItems = inventory.Items
            .Where(stack => stack.Count > 0)
            .OrderBy(stack => stack.Item.Price)
            .ToList();

        int itemCount = sortedItems.Count;

        // Ensure we have enough views
        while (itemViewList.Count < itemCount)
        {
            // Instantiate new ItemView if needed
            var newView = Instantiate(itemViewPrefab, itemContainer);
            itemViewList.Add(newView);
        }

        // Update active views
        for (int i = 0; i < itemViewList.Count; i++)
        {
            if (i < itemCount)
            {
                itemViewList[i].SetItemStack(sortedItems[i]);
                itemViewList[i].gameObject.SetActive(true);
            }
            else
            {
                itemViewList[i].SetItemStack(null);
                itemViewList[i].gameObject.SetActive(false);
            }
        }
    }

}
