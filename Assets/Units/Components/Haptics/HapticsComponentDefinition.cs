using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// iOS native haptics plugin wrapper.
/// Provides access to UIImpactFeedbackGenerator for sophisticated haptic feedback.
/// </summary>
public static class iOSHaptics
{
    public enum HapticStyle
    {
        Light = 0,      // Light, quick tap
        Medium = 1,     // Medium impact
        Heavy = 2,      // Heavy, strong impact
        Soft = 3,       // Soft, gentle impact (iOS 13+)
        Rigid = 4       // Rigid, sharp impact (iOS 13+)
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _InitializeHaptics();
    
    [DllImport("__Internal")]
    private static extern void _TriggerHaptic(int style, float intensity);
    
    [DllImport("__Internal")]
    private static extern bool _HapticsSupported();
    
    [DllImport("__Internal")]
    private static extern void _CleanupHaptics();
#else
    private static void _InitializeHaptics() { }
    private static void _TriggerHaptic(int style, float intensity) { }
    private static bool _HapticsSupported() { return false; }
    private static void _CleanupHaptics() { }
#endif

    private static bool initialized = false;
    private static bool supported = false;

    public static bool IsSupported
    {
        get
        {
            EnsureInitialized();
            return supported;
        }
    }

    public static void Initialize()
    {
        if (initialized) return;

        _InitializeHaptics();
        supported = _HapticsSupported();
        initialized = true;
    }

    public static void Trigger(HapticStyle style, float intensity = 1f)
    {
        EnsureInitialized();

        if (!supported)
        {
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
            return;
        }

        _TriggerHaptic((int)style, Mathf.Clamp01(intensity));
    }

    public static void Cleanup()
    {
        if (!initialized) return;

        _CleanupHaptics();
        initialized = false;
        supported = false;
    }

    private static void EnsureInitialized()
    {
        if (!initialized)
        {
            Initialize();
        }
    }
}

/// <summary>
/// ScriptableObject definition for haptic feedback based on unit type and terrain.
/// Defines haptic patterns for different movement types (sky, aqua, paw) on different terrains.
/// </summary>
[CreateAssetMenu(fileName = "HapticsComponent", menuName = "Club Fungal/Units/Components/Haptics Component")]
public class HapticsComponentDefinition : UnitComponentDefinition
{
    [System.Serializable]
    public class HapticPattern
    {
        [Tooltip("Type of haptic pattern")]
        public HapticPatternType patternType;

        [Tooltip("Base interval between haptic pulses (in seconds)")]
        public float interval = 0.5f;

        [Tooltip("Intensity variation (0-1, how much the pattern varies)")]
        [Range(0f, 1f)]
        public float variation = 0.1f;

        [Tooltip("Movement speed threshold to trigger haptics")]
        public float minSpeed = 0.1f;
    }

    public enum HapticPatternType
    {
        None,           // No haptics
        SmoothFlap,     // Smooth ambient wing flaps (sky on any terrain)
        Slippery,       // Slippery/splashy feeling (aqua on land)
        SmoothSwim,     // Smooth swimming (aqua on water)
        Quadruped,      // Quadruped footsteps (paw on land)
        Drag            // Dragging feeling (paw on water)
    }

    [Header("Haptic Patterns by Type and Terrain")]
    [Tooltip("Default pattern when no specific match is found")]
    public HapticPattern defaultPattern;

    [Header("Sky Type Patterns")]
    [Tooltip("Sky units on any terrain - smooth ambient wing flaps")]
    public HapticPattern skyPattern;

    [Header("Aqua Type Patterns")]
    [Tooltip("Aqua units on land - slippery and splashy")]
    public HapticPattern aquaOnLandPattern;

    [Tooltip("Aqua units on water - smooth swimming")]
    public HapticPattern aquaOnWaterPattern;

    [Header("Paw Type Patterns")]
    [Tooltip("Paw units on land - quadruped footsteps")]
    public HapticPattern pawOnLandPattern;

    [Tooltip("Paw units on water - dragging feeling")]
    public HapticPattern pawOnWaterPattern;

    [Header("Settings")]
    [Tooltip("Only apply haptics to the player-controlled unit")]
    public bool playerOnly = true;

    [Header("Service Reference")]
    [Tooltip("Reference to PlayerService to check if this is the player-controlled unit")]
    [SerializeField] private PlayerService playerService;

    public PlayerService PlayerService => playerService;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new HapticsComponentInstance(this, controller);
    }

    /// <summary>
    /// Get the appropriate haptic pattern for a unit type and terrain.
    /// </summary>
    public HapticPattern GetPatternForTypeAndTerrain(string unitTypeId, int navMeshArea)
    {
        // Sky units always use sky pattern regardless of terrain
        if (unitTypeId == "sky")
        {
            return skyPattern;
        }

        // Aqua units check terrain
        if (unitTypeId == "aqua")
        {
            // Check if on water/slow terrain (area 6)
            if (navMeshArea == 6)
            {
                return aquaOnWaterPattern;
            }
            return aquaOnLandPattern;
        }

        // Paw units check terrain
        if (unitTypeId == "paw")
        {
            // Check if on water/slow terrain (area 6)
            if (navMeshArea == 6)
            {
                return pawOnWaterPattern;
            }
            return pawOnLandPattern;
        }

        return defaultPattern;
    }
}

/// <summary>
/// Runtime instance of haptics component.
/// Monitors unit movement and terrain to trigger appropriate haptic feedback.
/// </summary>
[System.Serializable]
public class HapticsComponentInstance : UnitComponentInstance
{
    [SerializeField] private HapticsComponentDefinition.HapticPattern currentPattern;
    [SerializeField] private float timeSinceLastHaptic;
    [SerializeField] private float nextHapticInterval;
    [SerializeField] private int currentNavMeshArea;
    [SerializeField] private bool isMoving;

    private NavMeshAgent navMeshAgent;

    public HapticsComponentDefinition HapticsDefinition => definition as HapticsComponentDefinition;

    public HapticsComponentInstance(HapticsComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        navMeshAgent = controller.GetComponent<NavMeshAgent>();

        // Initialize iOS haptics system
        iOSHaptics.Initialize();

        UpdateCurrentPattern();
        ResetHapticTimer();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Only apply haptics to player-controlled unit if playerOnly is true
        var playerService = HapticsDefinition.PlayerService;
        if (HapticsDefinition.playerOnly && playerService != null && controller != playerService.Player)
        {
            return;
        }

        Debug.Log($"Updating HapticsComponentInstance for {controller.name}");

        // Update current terrain
        UpdateNavMeshArea();

        // Check if pattern should change based on terrain
        UpdateCurrentPattern();

        // Check if moving
        UpdateMovementState();

        // Trigger haptics if conditions are met
        if (isMoving && currentPattern != null && currentPattern.patternType != HapticsComponentDefinition.HapticPatternType.None)
        {
            timeSinceLastHaptic += Time.deltaTime;

            if (timeSinceLastHaptic >= nextHapticInterval)
            {
                TriggerHaptic();
                ResetHapticTimer();
            }
        }
    }

    private void UpdateNavMeshArea()
    {
        if (navMeshAgent == null) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(controller.transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            currentNavMeshArea = hit.mask;

            // Convert mask to area index
            for (int i = 0; i < 32; i++)
            {
                if ((hit.mask & (1 << i)) != 0)
                {
                    currentNavMeshArea = i;
                    break;
                }
            }
        }
    }

    private void UpdateCurrentPattern()
    {
        if (UnitInstance?.Species?.Type == null) return;

        string typeId = UnitInstance.Species.Type.Id;
        currentPattern = HapticsDefinition.GetPatternForTypeAndTerrain(typeId, currentNavMeshArea);
    }

    private void UpdateMovementState()
    {
        if (navMeshAgent != null)
        {
            isMoving = navMeshAgent.velocity.magnitude >= (currentPattern?.minSpeed ?? 0.1f);
        }
    }

    private void TriggerHaptic()
    {
        if (currentPattern == null || currentPattern.patternType == HapticsComponentDefinition.HapticPatternType.None)
            return;

        // Use iOS native haptics if available
        if (iOSHaptics.IsSupported)
        {
            var style = GetHapticStyleForPattern(currentPattern.patternType);
            iOSHaptics.Trigger(style, 1f);
        }
        else
        {
            // Fallback to basic vibration
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        // Log for debugging
        Debug.Log($"Haptic triggered: {currentPattern.patternType} on area {currentNavMeshArea}");
    }

    private iOSHaptics.HapticStyle GetHapticStyleForPattern(HapticsComponentDefinition.HapticPatternType patternType)
    {
        switch (patternType)
        {
            case HapticsComponentDefinition.HapticPatternType.SmoothFlap:
                return iOSHaptics.HapticStyle.Soft; // Gentle wing flaps

            case HapticsComponentDefinition.HapticPatternType.Slippery:
                return iOSHaptics.HapticStyle.Light; // Quick, light splashes

            case HapticsComponentDefinition.HapticPatternType.SmoothSwim:
                return iOSHaptics.HapticStyle.Soft; // Smooth swimming

            case HapticsComponentDefinition.HapticPatternType.Quadruped:
                return iOSHaptics.HapticStyle.Medium; // Footsteps

            case HapticsComponentDefinition.HapticPatternType.Drag:
                return iOSHaptics.HapticStyle.Heavy; // Dragging through water

            default:
                return iOSHaptics.HapticStyle.Medium;
        }
    }

    private void ResetHapticTimer()
    {
        timeSinceLastHaptic = 0f;

        if (currentPattern != null)
        {
            // Add variation to the interval for more natural feeling
            float variationAmount = UnityEngine.Random.Range(-currentPattern.variation, currentPattern.variation);
            nextHapticInterval = currentPattern.interval * (1f + variationAmount);
        }
        else
        {
            nextHapticInterval = 0.5f;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        // Note: We don't cleanup iOS haptics here since it's a static system
        // that may be used by other components. It will be cleaned up on app shutdown.
    }
}
