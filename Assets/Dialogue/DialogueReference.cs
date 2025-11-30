using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "DialogueReference", menuName = "Club Fungal/Dialogue/Dialogue Reference")]
public class DialogueReference : ScriptableObject
{
    [Header("References")]
    [SerializeField] private PlayerService playerReference;
    [SerializeField] private PhotoReference photoReference;
    [SerializeField] private InventoryReference inventory;
    [SerializeField] private Navigation navigation;
    [SerializeField] private ViewReference dialogueView;

    [Header("Runtime")]
    [SerializeField] private bool isActive;
    [SerializeField] private UnitController currentUnit;
    [SerializeField] private List<UnitController> units;

    [SerializeField] private Dialogue dialogue;

    public bool IsActive => isActive;
    public UnitController Unit => currentUnit;
    public Dialogue Dialogue => dialogue;

    public event UnityAction OnIsActiveChanged;
    public event UnityAction OnDialogueStart;
    public event UnityAction OnDialogueComplete;

    public void StartDialogueInteraction(List<UnitController> units)
    {
        this.units = units;
    }

    public void StartDialogue(UnitController unit, Dialogue dialogue)
    {
        this.dialogue = dialogue;

        if (currentUnit) currentUnit.Unfocus();
        currentUnit = unit;
        currentUnit.Focus();

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
        // OnGiveComplete?.Invoke();
        navigation.GoBack();
    }
}
