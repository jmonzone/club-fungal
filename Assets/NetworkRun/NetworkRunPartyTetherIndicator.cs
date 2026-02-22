using UnityEngine;

public class NetworkRunPartyTetherIndicator : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private GameObject indicatorObject;

    private void Update()
    {
        UpdateIndicatorPosition();
        HandleClickInput();
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

    private void HandleClickInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (networkRunService == null || indicatorObject == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if we clicked on the indicator object or any of its children
            if (hit.transform == indicatorObject.transform || hit.transform.IsChildOf(indicatorObject.transform))
            {
                networkRunService?.PartyService?.CyclePartyLeader();
            }
        }
    }
}
