using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class ShruneReadingSpreadOptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button optionButton;

    public void SetOption(ShruneReadingSpread spread, UnityAction<ShruneReadingSpread> onSelected)
    {
        labelText.text = spread.SpreadName;
        iconImage.sprite = spread.Icon;
        optionButton.onClick.RemoveAllListeners();
        optionButton.onClick.AddListener(() =>
        {
            onSelected?.Invoke(spread);
        });
    }
}
