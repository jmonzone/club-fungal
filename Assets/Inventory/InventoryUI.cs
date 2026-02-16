using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryReference inventory;
    [SerializeField] private InventoryItemUI itemViewPrefab;
    [SerializeField] private Transform itemContainer;

    private List<InventoryItemUI> itemViewList = new List<InventoryItemUI>();

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
    }

    private void OnDisable()
    {
        inventory.OnInventoryOpened -= UpdateView;
        inventory.OnInventoryChanged -= UpdateView;
    }

    private void UpdateView()
    {
        int itemCount = inventory.Items.Count;

        var sortedItems = inventory.Items.OrderBy(stack => stack.Item.Price).ToList();

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
