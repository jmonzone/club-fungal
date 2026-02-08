using UnityEngine;

[CreateAssetMenu(fileName = "RestActivity", menuName = "Club Fungal/Activities/Components/Rest")]
public class RestActivity : ActivityComponent
{
    [SerializeField] private Inventory inventory;

    public Inventory Inventory => inventory;

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        base.Initialize(networkRun, activityInstance);
        inventory = new Inventory();
    }
    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Rest activity doesn't need to update - it's just a place to manage inventory
    }
}
