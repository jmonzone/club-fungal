using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "DialogueReference", menuName = "Club Fungal/Dialogue/Dialogue Reference")]
public class DialogueReference : ScriptableObject
{
    [Header("References")]
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private PhotoReference photoReference;
    [SerializeField] private InventoryReference inventory;
    [SerializeField] private Navigation navigation;
    [SerializeField] private ViewReference dialogueView;

    [Header("Runtime")]
    [SerializeField] private bool isActive;
    [SerializeField] private UnitController currentUnit;
    [SerializeField] private List<UnitController> units;

    [SerializeField] private Dialogue dialogue;
    [SerializeField] private float relationship;

    public bool IsActive => isActive;
    public UnitController Unit => currentUnit;
    public Dialogue Dialogue => dialogue;
    public float Relationship => relationship;

    public event UnityAction OnIsActiveChanged;
    public event UnityAction OnInteractionStart;
    public event UnityAction OnDialogueStart;
    public event UnityAction<Response> OnDialogueResponse;
    public event UnityAction OnGiveComplete;
    public event UnityAction OnDialogueComplete;

    private UnityAction onComplete;


    public void StartDialogue(Dialogue dialogue)
    {
        this.dialogue = dialogue;

        isActive = true;
        OnIsActiveChanged?.Invoke();
        OnDialogueStart?.Invoke();

        if (navigation.CurrentView != dialogueView)
        {
            navigation.Navigate(dialogueView);
        }
    }

    public void ContinueDialogue()
    {
        dialogue.Continue();
    }

    public void CompleteDialogue()
    {
        //Debug.Log($"CompleteDialogue");

        currentUnit.Unfocus();
        OnDialogueComplete?.Invoke();

        foreach (var unit in units)
        {
            unit.Dialogue.CompleteDialogue();
        }

        navigation.GoBackToRoot();

        currentUnit = null;
        units = new List<UnitController>();
        dialogue = null;
        isActive = false;
        OnIsActiveChanged?.Invoke();

        onComplete?.Invoke();
        onComplete = null;
    }

    public void StartPhoto()
    {
        Unit.SetLookTarget(playerReference.Player.transform);
        photoReference.SetLookTarget(Unit.transform);
        photoReference.StartPhotoView();
    }

    public void StartGive()
    {
        inventory.OnItemSelected += Inventory_OnItemSelected;
        inventory.OpenInventory();
    }

    private void Inventory_OnItemSelected(Item arg0)
    {
        inventory.OnItemSelected -= Inventory_OnItemSelected;
        // dialogue = currentUnit.Dialogue.Dialogue[0];
        OnGiveComplete?.Invoke();
        navigation.GoBack();
    }

    public void StartFollow()
    {
        if (Unit is FungalController fungal)
        {
            fungal.Follow(playerReference.Player.GetComponent<UnitFollow>());
            CompleteDialogue();
        }
    }
}
