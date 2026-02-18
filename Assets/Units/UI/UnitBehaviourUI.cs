using UnityEngine;
using TMPro;

public class UnitBehaviourUI : UnitComponent
{
    [SerializeField] private TextMeshProUGUI behaviourText;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Controller.OnBehaviourChanged += UpdateBehaviourText;
        UpdateBehaviourText();
    }

    private void UpdateBehaviourText()
    {
        if (behaviourText)
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
