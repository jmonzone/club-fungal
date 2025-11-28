using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChatUI : DialoguePageUI
{
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;

    protected override void Awake()
    {
        base.Awake();
        continueButton.onClick.AddListener(() =>
        {
            dialogue.Dialogue.Continue();
            StartCoroutine(Show());
        });
    }

    public override IEnumerator Show()
    {
        unitImage.sprite = dialogue.Unit.Instance.Data.Sprite;
        unitNameText.text = dialogue.Unit.Instance.Data.Name;
        dialogueText.text = dialogue.Dialogue.Text;
        yield return base.Show();
    }
}
