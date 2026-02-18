using UnityEngine;
using TMPro;

public class UnitBehaviourUI : UnitComponent
{
    [SerializeField] private TextMeshProUGUI behaviourText;
    [SerializeField] private NetworkRunService networkRunService;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Controller.OnBehaviourChanged += UpdateBehaviourText;
        UpdateBehaviourText();
    }

    private void UpdateBehaviourText()
    {
        // Only show behavior UI if debug mode is enabled
        bool shouldShow = networkRunService != null && networkRunService.Settings != null && networkRunService.Settings.debugMode;

        if (behaviourText)
        {
            behaviourText.gameObject.SetActive(shouldShow);

            if (shouldShow)
            {
                string behaviourName = Controller.CurrentBehaviour ? Controller.CurrentBehaviour.GetType().Name : "Idle";
                if (behaviourName.StartsWith("Unit"))
                {
                    behaviourName = behaviourName.Substring(4);
                }
                behaviourText.text = behaviourName;
            }
        }
    }
}
