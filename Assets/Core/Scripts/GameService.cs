using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameService", menuName = "Club Fungal/Game Service")]
public class GameService : GURUService
{
    [Header("Room Settings")]
    public bool useOuterWalls = false;
    public bool selectRoomOnTransition = true;

    [Header("Services")]
    [SerializeField] private LocalData localData;
    [SerializeField] private BuildReference build;
    [SerializeField] private InventoryReference inventory;
    [SerializeField] private List<GURUService> services;
    [SerializeField] private PartyReference partyReference;
    [SerializeField] private StoryReference partyLogReference;
    [SerializeField] private SporeReference sporeReference;
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

    public void InitializeSystems()
    {
        // Debug.Log("Initializing Game Systems...");
        localData.Initialize();
        inventory.Initialize();
        build.Initialize();

        foreach (var service in services)
        {
            service.Initialize();
        }

        partyReference.Initialize();
        partyLogReference.Initialize();
        sporeReference.Initialize();
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
}