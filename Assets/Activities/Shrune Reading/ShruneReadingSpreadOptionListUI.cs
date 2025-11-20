using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ShruneReadingSpreadOptionListUI : MonoBehaviour
{
    [SerializeField] private FadeCanvasGroup fadeCanvasGroup;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShruneReadingSpreadOptionUI optionPrefab;
    private List<ShruneReadingSpreadOptionUI> instantiatedOptions = new();

    public IEnumerator PopulateOptions(List<ShruneReadingSpread> spreads, UnityAction<ShruneReadingSpread> onOptionSelected = null)
    {
        yield return fadeCanvasGroup.FadeIn();
        instantiatedOptions = new List<ShruneReadingSpreadOptionUI>(GetComponentsInChildren<ShruneReadingSpreadOptionUI>(includeInactive: true));
        if (instantiatedOptions.Count < spreads.Count)
        {
            int optionsToCreate = spreads.Count - instantiatedOptions.Count;
            for (int i = 0; i < optionsToCreate; i++)
            {
                ShruneReadingSpreadOptionUI newOption = Instantiate(optionPrefab, contentParent);
                instantiatedOptions.Add(newOption);
            }
        }

        for (int i = 0; i < instantiatedOptions.Count; i++)
        {
            if (i < spreads.Count)
            {
                var option = spreads[i];
                instantiatedOptions[i].SetOption(option, onOptionSelected);
                instantiatedOptions[i].gameObject.SetActive(true);
            }
            else
            {
                instantiatedOptions[i].gameObject.SetActive(false);
            }
        }

    }

    public IEnumerator Hide(UnityAction onComplete = null)
    {
        yield return fadeCanvasGroup.FadeOut(onComplete);
    }
}
