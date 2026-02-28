using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reusable component for managing dynamic UI lists with prefab instantiation and pooling.
/// </summary>
[Serializable]
public class ListUI<T> where T : MonoBehaviour
{
    [SerializeField] private T itemPrefab;
    [SerializeField] private Transform itemContainer;

    private List<T> itemList = new List<T>();

    public T ItemPrefab => itemPrefab;
    public Transform ItemContainer => itemContainer;
    public List<T> Items => itemList;
    public int Count => itemList.Count;

    /// <summary>
    /// Initializes the list by collecting existing children and setting default container.
    /// </summary>
    public void Initialize(Transform fallbackContainer)
    {
        if (!itemContainer)
        {
            itemContainer = fallbackContainer;
        }

        CollectExistingItems();
    }

    /// <summary>
    /// Collects existing items from the container.
    /// </summary>
    private void CollectExistingItems()
    {
        itemList.Clear();
        if (itemContainer != null)
        {
            itemContainer.GetComponentsInChildren(true, itemList);
        }
    }

    /// <summary>
    /// Ensures the list has enough items instantiated for the given count.
    /// Reuses existing items in container before instantiating new ones.
    /// </summary>
    public void EnsureItemCount(int targetCount)
    {
        // First, refresh the list to include any manually added items
        CollectExistingItems();

        // Then instantiate only if we need more
        while (itemList.Count < targetCount)
        {
            var newItem = UnityEngine.Object.Instantiate(itemPrefab, itemContainer);
            itemList.Add(newItem);
        }
    }

    /// <summary>
    /// Shows the first 'activeCount' items and hides the rest.
    /// </summary>
    public void SetActiveItemCount(int activeCount)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].gameObject.SetActive(i < activeCount);
        }
    }

    /// <summary>
    /// Gets an item at the specified index. Returns null if index is out of bounds.
    /// </summary>
    public T GetItem(int index)
    {
        return index >= 0 && index < itemList.Count ? itemList[index] : null;
    }

    /// <summary>
    /// Updates the list with data, handling instantiation, assignment, and active/inactive states.
    /// </summary>
    /// <param name="dataList">The list of data to display</param>
    /// <param name="setItemData">Callback to set data on each item</param>
    public void UpdateList<TData>(List<TData> dataList, Action<T, TData> setItemData)
    {
        int dataCount = dataList?.Count ?? 0;

        EnsureItemCount(dataCount);

        for (int i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            if (i < dataCount)
            {
                setItemData?.Invoke(item, dataList[i]);
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Clears all items from the list.
    /// </summary>
    public void Clear()
    {
        foreach (var item in itemList)
        {
            if (item != null)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }
        itemList.Clear();
    }
}
