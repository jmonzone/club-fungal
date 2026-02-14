using UnityEngine;

public class NetworkRunPartyTetherIndicator : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private GameObject indicatorObject;

    private void Update()
    {
        UpdateIndicatorPosition();
    }

    private void UpdateIndicatorPosition()
    {
        if (networkRunService == null || indicatorObject == null) return;

        Vector3 partyCenter = networkRunService.PartyCenterGround;

        // Only update if we have a valid position
        if (partyCenter != Vector3.zero)
        {
            indicatorObject.transform.position = partyCenter;
        }
    }
}
