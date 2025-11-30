using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RepositionableEditor : GURUEditor
{
    private static Dictionary<Component, Vector3> lastPositions = new Dictionary<Component, Vector3>();

    static RepositionableEditor()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        foreach (var kvp in lastPositions.ToList())
        {
            Component comp = kvp.Key;
            if (comp == null) { lastPositions.Remove(comp); continue; }
            if (Vector3.Distance(comp.transform.position, kvp.Value) > 0.01f)
            {
                RepositionToAppropriateRoom(comp);
                lastPositions[comp] = comp.transform.position;
            }
        }
    }

    protected virtual void OnEnable()
    {
        Component controller = (Component)target;
        if (!lastPositions.ContainsKey(controller))
        {
            lastPositions[controller] = controller.transform.position;
        }
    }

    protected virtual void OnDisable()
    {
        Component controller = (Component)target;
        lastPositions.Remove(controller);
    }

    protected override void DrawContent()
    {
        // Base implementation does nothing; subclasses can override
    }

    protected override string Description =>
        "Handles repositioning for repositionable objects.";

    private static void RepositionToAppropriateRoom(Component comp)
    {
        var rooms = FindObjectsByType<RoomController>(FindObjectsSortMode.None);
        RoomController newRoom = null;
        foreach (var room in rooms)
        {
            if (room.Floor != null && room.Floor.bounds.Contains(comp.transform.position))
            {
                newRoom = room;
                break;
            }
        }
        if (newRoom != null)
        {
            Transform targetParent = comp is BuildController ? newRoom.BuildParent : newRoom.UnitParent;
            if (comp.transform.parent != targetParent)
            {
                comp.transform.SetParent(targetParent);
                EditorGUIUtility.PingObject(comp.gameObject);
            }
        }
    }
}