using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("InteractionReference is deprecated. Each interactable should handle its own interaction logic.")]
[CreateAssetMenu(fileName = "InteractionReference", menuName = "Club Fungal/Dialogue/Interaction Reference")]
public class InteractionReference : ScriptableObject
{
    [SerializeField] private DialogueReference dialogueReference;



}
