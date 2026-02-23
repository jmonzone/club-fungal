using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "GameService", menuName = "Club Fungal/Game Service")]
public class GameService : GURUService
{
    [Header("Room Settings")]
    public bool useOuterWalls = false;
    public bool selectRoomOnTransition = true;

    [Header("Core Services")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private UnitControllerService unitControllerService;

    public UnitInstanceService UnitInstanceService => unitInstanceService;
    public UnitControllerService UnitControllerService => unitControllerService;

    [Header("Other Services")]
    [SerializeField] private LocalData localData;
    [SerializeField] private SceneNavigation sceneNavigation;
    [SerializeField] private BuildReference build;
    [SerializeField] private InventoryReference inventory;
    [SerializeField] private List<GURUService> services;
    [SerializeField] private PartyReference partyReference;
    [SerializeField] private StoryReference partyLogReference;
    [SerializeField] private GlyphCollection glyphCollection;
    [SerializeField] private DJTableReference djReference;


    protected override void OnInitialize()
    {
        // localData.OnReset += InitializeSystems;
        InitializeSystems();
    }

    public void ResetData()
    {
        localData.ResetData();
        InitializeSystems();
    }

    public void ResetGame()
    {
        Debug.Log("Resetting game: clearing local data and reloading scene 0...");
        localData.ResetData();

        // Subscribe to scene loaded event to reinitialize after scene loads
        SceneManager.sceneLoaded += OnSceneLoadedAfterReset;

        if (sceneNavigation != null)
        {
            sceneNavigation.NavigateToScene(0);
        }
        else
        {
            Debug.LogWarning("SceneNavigation not assigned. Cannot reload scene.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private void OnSceneLoadedAfterReset(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe immediately to avoid duplicate calls
        SceneManager.sceneLoaded -= OnSceneLoadedAfterReset;

        // Reinitialize all game systems
        Debug.Log("Scene loaded after reset. Reinitializing game systems...");
        InitializeSystems();
    }

    public void InitializeSystems()
    {
        Debug.Log("Initializing Game Systems...");
        localData.Initialize();
        inventory.Initialize();
        build.Initialize();

        var servicesToInitialize = new List<GURUService>()
        {
            unitInstanceService,
            unitControllerService,
        };

        servicesToInitialize.AddRange(services);

        // Debug.Log($"Initializing {servicesToInitialize.Count} services...");

        foreach (var service in servicesToInitialize)
        {
            // Debug.Log($"Initializing service: {service.name}");
            service.Initialize();
        }

        partyReference.Initialize();
        partyLogReference.Initialize();
        glyphCollection.Initialize();
        djReference.Initialize();
    }

    public void NotifySceneLoaded()
    {
        foreach (var service in services)
        {
            service.OnSceneLoaded();
        }
    }

    public override void DoUpdate()
    {
        // Update core services
        unitInstanceService?.DoUpdate();
        unitControllerService?.DoUpdate();

        // Update all other services
        foreach (var service in services)
        {
            service?.DoUpdate();
        }
    }
}