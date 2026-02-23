using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ItemStack
{
    public Item Item;
    public int Count;

    public ItemStack(Item item, int count = 1)
    {
        Item = item;
        Count = count;
    }
}

[CreateAssetMenu]
public class InventoryReference : ScriptableObject
{
    [Header("References")]
    [SerializeField] private LocalData localData;
    [SerializeField] private Navigation navigation;
    [SerializeField] private ViewReference inventoryView;
    [SerializeField] private GlyphCollection glyphCollection;

    [Header("Settings")]
    [SerializeField] private int initialSporeCount = 124;
    [SerializeField] private List<ItemStack> initialItems;

    [Header("Runtime")]
    [SerializeField] private int sporeCount = 0;
    [SerializeField] private List<ItemStack> items;
    [SerializeField] private Dictionary<GlyphData, int> glyphs;

    public int SporeCount => sporeCount;
    public List<ItemStack> Items => items;
    public Dictionary<GlyphData, int> Glyphs => glyphs;

    public event UnityAction<SporeController> OnSporeCollected;
    public event UnityAction<int> OnSporeCountChanged;
    public event UnityAction<GlyphData, int> OnGlyphCountChanged;
    public event UnityAction OnItemSummoned;
    public event UnityAction OnInventoryOpened;
    public event UnityAction<Item> OnItemSelected;
    public event UnityAction OnInventoryChanged;

    private const string SPORE_KEY = "spore";
    private const string SHRUNE_KEY = "shrune";

    public void Initialize()
    {
        items = new List<ItemStack>();
        foreach (var itemStack in initialItems)
        {
            if (itemStack.Item != null && itemStack.Count > 0)
            {
                items.Add(new ItemStack(itemStack.Item, itemStack.Count));
            }
        }
        glyphs = new Dictionary<GlyphData, int>();

        if (localData.JsonFile.ContainsKey(SPORE_KEY))
        {
            var sporeJson = localData.JsonFile[SPORE_KEY];
            sporeCount = (int)sporeJson;
        }
        else
        {
            sporeCount = initialSporeCount;
        }

        if (localData.JsonFile.ContainsKey(SHRUNE_KEY))
        {
            foreach (var unit in localData.JsonFile[SHRUNE_KEY] as JArray)
            {
                if (unit is JObject glyphJson)
                {
                    var glyphName = glyphJson.Value<string>("id");
                    var matchingGlyph = glyphCollection.Glyphs.Find(u => u.Id == glyphName);

                    var glyphCount = glyphJson.Value<int>("count");

                    glyphs.Add(matchingGlyph, glyphCount);
                }
            }
        }
    }

    public void CollectSpore(SporeController sporeController)
    {
        IncreaseSporeCount(1);
        OnSporeCollected?.Invoke(sporeController);
    }

    public void AddItem(Item item, int count = 1)
    {
        var existingStack = items.Find(stack => stack.Item == item);
        if (existingStack != null)
        {
            existingStack.Count += count;
        }
        else
        {
            items.Add(new ItemStack(item, count));
        }
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(Item item, int count = 1)
    {
        var existingStack = items.Find(stack => stack.Item == item);
        if (existingStack != null)
        {
            existingStack.Count -= count;
            if (existingStack.Count <= 0)
            {
                items.Remove(existingStack);
            }
            OnInventoryChanged?.Invoke();
        }
    }

    public int GetItemCount(Item item)
    {
        var existingStack = items.Find(stack => stack.Item == item);
        return existingStack != null ? existingStack.Count : 0;
    }

    public void IncreaseSporeCount(int value = 1)
    {
        sporeCount += value;
        OnSporeCountChanged?.Invoke(sporeCount);
        localData.SaveData(SPORE_KEY, sporeCount);
    }

    public void DecreaseSporeCount(int value = 1)
    {
        sporeCount -= value;
        OnSporeCountChanged?.Invoke(sporeCount);
        localData.SaveData(SPORE_KEY, sporeCount);
    }

    public void IncreaseShrune(GlyphData glyph)
    {
        if (glyphs.ContainsKey(glyph))
        {
            glyphs[glyph]++;
        }
        else
        {
            glyphs.Add(glyph, 1);
        }

        OnGlyphCountChanged?.Invoke(glyph, glyphs[glyph]);
        SaveData();
    }

    public void DecreaseShrune(GlyphData glyph)
    {
        if (glyphs.ContainsKey(glyph) && glyphs[glyph] > 0)
        {
            glyphs[glyph]--;
        }
        else
        {
            glyphs.Add(glyph, 0);
        }

        OnGlyphCountChanged?.Invoke(glyph, glyphs[glyph]);
        SaveData();
    }

    private void SaveData()
    {
        var glyphJson = new JArray();

        foreach (var glyph in glyphs.Keys)
        {
            glyphJson.Add(new JObject
            {
                ["id"] = glyph.name,
                ["count"] = glyphs[glyph]
            });
        }

        localData.SaveData(SHRUNE_KEY, glyphJson);

    }

    public void SummonItem(Item item)
    {
        AddItem(item, 1);
        DecreaseSporeCount(item.Price);
        OnItemSummoned?.Invoke();
    }

    public void OpenInventory()
    {
        navigation.Navigate(inventoryView);
        OnInventoryOpened?.Invoke();
    }

    public void SelectItem(Item item)
    {
        OnItemSelected?.Invoke(item);
    }
}
