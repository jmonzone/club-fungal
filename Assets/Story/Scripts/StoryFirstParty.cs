using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("StoryFirstParty is out of date, but kept for reference.")]
public class StoryFirstParty : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyData tutorialParty;
    [SerializeField] private StoryData firstParty;
    [SerializeField] private PartyReference partyReference;
    [SerializeField] private StoryReference storyReference;
    [SerializeField] private PhotoReference photoReference;
    [SerializeField] private Transform guestPictureAnchor;
    [SerializeField] private Transform cameraPositionAnchor;


    [SerializeField] private InitialUI initialUI;
    [SerializeField] private Navigation navigation;
    [SerializeField] private ViewReference gameplayView;

    [SerializeField] private DialogueReference dialogueReference;
    [SerializeField] private List<Dialogue> lostDialogue;
    [SerializeField] private List<Dialogue> letsTakeAPhotoDialogue;
    [SerializeField] private List<Dialogue> afterPhotoTakenDialogue;


    private void Awake()
    {
        if (!storyReference.HasCompleted(firstParty))
        {
            initialUI.enabled = false;
            // unitManager.OnAllUnitsSummoned += UnitManager_OnAllUnitsSummoned;
        }
    }
}
