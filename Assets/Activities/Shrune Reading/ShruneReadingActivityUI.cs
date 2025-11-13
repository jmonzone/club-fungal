using Cinemachine;
using UnityEngine;

public class ShruneReadingActivityUI : ActivityUI<ShruneReadingUnit, ShruneReadingActivityController>
{
    [SerializeField] private TypewriterEffect typewriterEffect;

    protected override void OnPlayerEnter(ActivityUnit player)
    {
        base.OnPlayerEnter(player);

        ShowInitialPrompt();
    }

    private void ShowInitialPrompt()
    {
        StartCoroutine(typewriterEffect.ShowAndType("What is it that you seek?"));

    }
}
