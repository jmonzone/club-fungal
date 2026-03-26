using UnityEngine;

/// <summary>
/// Camera controller that responds to control mode changes.
/// Listens to ControlModeService and adjusts camera behavior accordingly.
/// </summary>
public class CameraControlModeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ControlModeService controlModeService;
    [SerializeField] private CameraInputHandler cameraInputHandler;
    [SerializeField] private CameraRotationController cameraRotationController;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private PlayerService playerService;
    [SerializeField] private NetworkRunPartyService partyService;

    private void Awake()
    {
        controlModeService.OnModeChanged += HandleModeChanged;
        controlModeService.OnUnitSelected += HandleUnitSelected;
    }

    private void Start()
    {
        HandleModeChanged(controlModeService.CurrentMode);
    }

    private void OnDestroy()
    {
        controlModeService.OnModeChanged -= HandleModeChanged;
        controlModeService.OnUnitSelected -= HandleUnitSelected;
    }

    private void HandleModeChanged(ControlModeService.ControlMode mode)
    {
        switch (mode)
        {
            case ControlModeService.ControlMode.FreeCamera:
                EnterFreeCameraMode();
                break;
            case ControlModeService.ControlMode.UnitSelected:
                EnterUnitSelectedMode();
                break;
            case ControlModeService.ControlMode.ManualControl:
                EnterManualControlMode();
                break;
        }
    }

    private void HandleUnitSelected(UnitInstance unit)
    {
        // When a unit is selected while already in a mode, update camera/player
        UpdateCameraAndPlayer();
    }

    private void UpdateCameraAndPlayer()
    {
        if (controlModeService.SelectedUnit == null || unitControllerService == null) return;

        var controller = unitControllerService.Controllers.Find(c => c.Instance == controlModeService.SelectedUnit);
        if (controller == null) return;

        // Update camera target
        if (cameraRotationController != null)
        {
            cameraRotationController.target = controller.transform;
        }

        // Set as party leader in both UnitSelected and ManualControl modes
        if (partyService != null)
        {
            partyService.SetPartyLeader(controller);
        }

        // Only set player assignment if in manual control mode
        if (controlModeService.IsManualControlActive())
        {
            if (playerService != null)
            {
                playerService.SetPlayer(controller);
            }
        }
    }

    private void EnterFreeCameraMode()
    {
        cameraInputHandler.enabled = true;
        cameraInputHandler.SetCanOrbit(false); // Disable orbit in free camera mode
        cameraInputHandler.SetCanPan(true);

        // Stop following any unit
        if (cameraRotationController != null)
        {
            cameraRotationController.target = null;
        }

        // Unassign player when exiting manual control
        if (playerService != null)
        {
            playerService.SetPlayer(null);
        }
    }

    private void EnterUnitSelectedMode()
    {
        cameraInputHandler.enabled = true;
        cameraInputHandler.SetCanOrbit(true);
        cameraInputHandler.SetCanPan(true);

        // Follow selected unit and set as party leader
        if (controlModeService.SelectedUnit != null && unitControllerService != null && cameraRotationController != null)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == controlModeService.SelectedUnit);
            if (controller != null)
            {
                cameraRotationController.target = controller.transform;

                // Set as party leader so other units follow
                if (partyService != null)
                {
                    partyService.SetPartyLeader(controller);
                }
            }
        }

        // Unassign player when returning from manual control (no manual control in this mode)
        if (playerService != null)
        {
            playerService.SetPlayer(null);
        }
    }

    private void EnterManualControlMode()
    {
        cameraInputHandler.SetCanOrbit(false);
        cameraInputHandler.SetCanPan(false);

        // Continue following the selected unit during manual control
        if (controlModeService.SelectedUnit != null && unitControllerService != null && cameraRotationController != null)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == controlModeService.SelectedUnit);
            if (controller != null)
            {
                cameraRotationController.target = controller.transform;

                // Assign as player when entering manual control
                if (playerService != null)
                {
                    playerService.SetPlayer(controller);
                }

                // Set as party leader when entering manual control
                if (partyService != null)
                {
                    partyService.SetPartyLeader(controller);
                }
            }
        }
    }
}
