using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(UnitController), true)]
public class UnitControllerEditor : GURUEditor
{
    private static Dictionary<UnitController, Vector3> lastPositions = new Dictionary<UnitController, Vector3>();

    static UnitControllerEditor()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        foreach (var kvp in lastPositions.ToList())
        {
            UnitController unit = kvp.Key;
            if (unit == null) { lastPositions.Remove(unit); continue; }
            if (Vector3.Distance(unit.transform.position, kvp.Value) > 0.01f)
            {
                RepositionToAppropriateRoom(unit);
                lastPositions[unit] = unit.transform.position;
            }
        }
    }

    private void OnEnable()
    {
        UnitController controller = (UnitController)target;
        if (!lastPositions.ContainsKey(controller))
        {
            lastPositions[controller] = controller.transform.position;
        }
    }

    private void OnDisable()
    {
        UnitController controller = (UnitController)target;
        lastPositions.Remove(controller);
    }

    protected override void DrawContent()
    {
        UnitController controller = (UnitController)target;

        if (controller.CurrentBehaviour != null)
        {
            EditorGUILayout.LabelField("Current Behaviour", controller.CurrentBehaviour.GetType().Name);
        }
        else
        {
            EditorGUILayout.LabelField("Current Behaviour", "None");
        }
    }

    protected override string Description =>
        "Displays the current behaviour of the unit.";

    private static void RepositionToAppropriateRoom(UnitController unit)
    {
        var rooms = FindObjectsByType<RoomController>(FindObjectsSortMode.None);
        RoomController newRoom = null;
        foreach (var room in rooms)
        {
            if (room.Floor != null && room.Floor.bounds.Contains(unit.transform.position))
            {
                newRoom = room;
                break;
            }
        }
        if (newRoom != null && unit.transform.parent != newRoom.UnitParent)
        {
            unit.transform.SetParent(newRoom.UnitParent);
            EditorGUIUtility.PingObject(unit.gameObject);
        }
    }
}