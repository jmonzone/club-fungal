using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(PartyService))]
public class PartyServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PartyService service = (PartyService)target;

        // Draw default
        DrawDefaultInspector();

        EditorGUILayout.Space();

        GURUStyler.DrawGuruSection(() =>
        {
            var partyMembersField = typeof(PartyService).GetField("partyMembers", BindingFlags.NonPublic | BindingFlags.Instance);
            var partyMembers = (List<UnitInstance>)partyMembersField.GetValue(service);

            var playerReferenceField = typeof(PartyService).GetField("playerReference", BindingFlags.NonPublic | BindingFlags.Instance);
            var playerReference = (PlayerReference)playerReferenceField.GetValue(service);

            UnitInstanceListDrawer.DrawList(
                partyMembers,
                onToggleParty: (unit) =>
                {
                    var controller = service.PartyMembers.FirstOrDefault(c => c.Instance == unit);
                    if (controller != null && controller != playerReference.Player)
                    {
                        service.RemoveFromParty(controller);
                    }
                },
                onRemove: null,
                canRemoveFunc: null,
                toggleButtonTextFunc: (unit) => "R",
                backgroundColorFunc: null,
                showPartyStatus: false
            );
        }, "Manage party members.", service);
    }
}