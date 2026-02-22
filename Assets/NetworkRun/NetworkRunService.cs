using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkRunService", menuName = "Club Fungal/Network Run/Network Run Service")]
public class NetworkRunService : GURUService
{
    [Header("References")]
    [SerializeField] private NetworkRunSettings settings;
    [SerializeField] private Inventory inventory;
    [SerializeField] private NetworkRunPartyService partyService;
    [SerializeField] private CameraService cameraService;

    [Header("Runtime")]
    private Vector3 partyCenterGround;
    private float maxTetherDistance;
    private Vector3 cameraVelocity;

    public event Action OnUnitReenteredTether;
    public NetworkRunSettings Settings => settings;
    public Inventory Inventory => inventory;
    public NetworkRunPartyService PartyService => partyService;
    public Vector3 PartyCenterGround => partyCenterGround;
    public float MaxTetherDistance => maxTetherDistance;
    public Vector3 CameraVelocity => cameraVelocity;

    public float AveragePartySpeedMultiplier
    {
        get
        {
            if (partyService == null || partyService.PartyLeader == null)
                return 1f;

            var speedModifier = partyService.PartyLeader.GetComponent<UnitSpeedModifier>();
            if (speedModifier == null)
                return 1f;

            float leaderMultiplier = speedModifier.CurrentSpeedMultiplier;

            // Smooth convergence at max value using hyperbolic curve
            // This approaches maxSpeedMultiplier asymptotically as speeds increase
            return settings.maxSpeedMultiplier * leaderMultiplier / (leaderMultiplier + (settings.maxSpeedMultiplier - 1f));
        }
    }

    public void SetPartyTetherData(Vector3 centerGround, float maxDistance, Vector3 velocity)
    {
        partyCenterGround = centerGround;
        maxTetherDistance = maxDistance;
        cameraVelocity = velocity;
    }

    public void NotifyUnitReenteredTether()
    {
        OnUnitReenteredTether?.Invoke();
    }

    protected override void OnInitialize()
    {
        Debug.Log("Initializing NetworkRunService");
        inventory = new Inventory(9999999);
    }

    public override void DoUpdate()
    {
        base.DoUpdate();

        // Update camera service with current party speed
        if (cameraService != null)
        {
            cameraService.SetSpeedMultiplier(AveragePartySpeedMultiplier);
        }
    }
}
