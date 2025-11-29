using UnityEngine;

[CreateAssetMenu(fileName = "GameService", menuName = "Club Fungal/Game Service")]
public class GameService : ScriptableObject
{
    [Header("Room Settings")]
    public bool useOuterWalls = false;
    public bool selectRoomOnTransition = true;

    [Header("Services")]
    [SerializeField] private LocalData localData;
    [SerializeField] private BuildReference build;
    [SerializeField] private InventoryReference inventory;
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private PartyInstanceService partyInstanceService;
    [SerializeField] private PartyControllerService partyControllerService;
    [SerializeField] private PartyReference partyReference;
    [SerializeField] private StoryReference partyLogReference;
    [SerializeField] private SporeReference sporeReference;
    [SerializeField] private GlyphCollection glyphCollection;
    [SerializeField] private DJTableReference djReference;


    public void Initialize()
    {
        localData.OnReset += InitializeSystems;
        InitializeSystems();
    }

    public void ResetJsonFile()
    {
        localData.ResetData();
        InitializeSystems();
    }

    public void InitializeSystems()
    {
        localData.Initialize();
        inventory.Initialize();
        build.Initialize();
        playerReference.Initialize();
        unitInstanceService.Initialize();
        unitControllerService.Initialize();
        partyInstanceService.Initialize();
        partyReference.Initialize();
        partyLogReference.Initialize();
        sporeReference.Initialize();
        glyphCollection.Initialize();
        djReference.Initialize();
    }
}