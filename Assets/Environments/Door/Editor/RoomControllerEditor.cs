using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(RoomController))]
public class RoomControllerEditor : Editor
{
    private TextureSelectorComponent wallTextureSelector;
    private MaterialSelectorComponent wallMaterialSelector;
    private MaterialSelectorComponent floorMaterialSelector;

    private void OnEnable()
    {
        RoomController roomController = (RoomController)target;

        wallTextureSelector = new TextureSelectorComponent("Wall Texture", (texture) =>
        {
            foreach (var wall in roomController.Walls)
            {
                wall.Renderer.sharedMaterial.mainTexture = texture;
            }
        });

        wallMaterialSelector = new MaterialSelectorComponent("Wall Material", (material) =>
        {
            foreach (var wall in roomController.Walls)
            {
                wall.Renderer.material = material;
            }
            wallTextureSelector.Reset();
        });

        floorMaterialSelector = new MaterialSelectorComponent("Floor Material", (material) =>
        {
            Debug.Log("Applying floor material");
            Undo.RecordObject(roomController.Floor, "Change Floor Material");
            roomController.Floor.sharedMaterial = material;
            EditorUtility.SetDirty(roomController.Floor);
            PrefabUtility.RecordPrefabInstancePropertyModifications(roomController.Floor);
        });
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomController roomController = (RoomController)target;



        // Initialize selectors with current materials
        wallTextureSelector.Initialize(roomController.Walls[0].Renderer.sharedMaterial.mainTexture);
        wallMaterialSelector.Initialize(roomController.Walls[0].Renderer.sharedMaterial);

        Debug.Log("Initializing floor material selector with: " + roomController.Floor.sharedMaterial?.name);
        floorMaterialSelector.Initialize(roomController.Floor.sharedMaterial);

        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.65f, 0.25f, 0.7f, 0.4f);
        GUIStyle paddedHelpBox = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(15, 15, 15, 15)
        };
        EditorGUILayout.BeginVertical(paddedHelpBox);
        GUI.backgroundColor = Color.white;

        GUIStyle logoStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
        };
        EditorGUILayout.LabelField("GURU", logoStyle);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.HelpBox("These fields apply changes to all walls of the room at once", MessageType.None);
        EditorGUILayout.Space(15);

        wallTextureSelector.DrawGUI();
        wallMaterialSelector.DrawGUI();
        floorMaterialSelector.DrawGUI();

        EditorGUILayout.Space(15);

        // Door Status Display
        EditorGUILayout.LabelField("Active Doors", EditorStyles.boldLabel);
        DrawDoorStatusGrid(roomController);

        EditorGUILayout.Space(15);

        // Duplicate room section
        EditorGUILayout.LabelField("Duplicate Room", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.8f, 0.6f, 0.9f, 1f);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("↖ NW", GUILayout.Height(30)))
        {
            DuplicateRoom(roomController, Direction.NorthWest);
        }
        if (GUILayout.Button("↗ NE", GUILayout.Height(30)))
        {
            DuplicateRoom(roomController, Direction.NorthEast);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("↙ SW", GUILayout.Height(30)))
        {
            DuplicateRoom(roomController, Direction.SouthWest);
        }
        if (GUILayout.Button("↘ SE", GUILayout.Height(30)))
        {
            DuplicateRoom(roomController, Direction.SouthEast);
        }
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
    }

    private void DuplicateRoom(RoomController original, Direction direction)
    {
        // Calculate offset based on direction using Direction.GetOffset
        Vector3 offset = direction.GetOffset(RoomManager.RoomOffset);

        // Duplicate the GameObject
        GameObject duplicatedObject = Instantiate(original.gameObject, original.transform.parent);
        duplicatedObject.name = original.gameObject.name + $" ({direction})";

        // Position it in the specified direction
        duplicatedObject.transform.localPosition = original.transform.localPosition + offset;
        duplicatedObject.transform.rotation = original.transform.rotation;
        duplicatedObject.transform.localScale = original.transform.localScale;

        // Get the duplicated room controller
        RoomController duplicatedRoom = duplicatedObject.GetComponent<RoomController>();

        // Activate the door on the original room in the specified direction
        DoorController originalDoor = original.Walls[direction.WallIndex].DoorController;
        original.ActivateDoor(originalDoor);

        // Activate the opposite door on the duplicated room
        DoorController duplicatedDoor = duplicatedRoom.Walls[direction.Opposite.WallIndex].DoorController;
        duplicatedRoom.ActivateDoor(duplicatedDoor);

        // Register undo
        Undo.RegisterCreatedObjectUndo(duplicatedObject, $"Duplicate Room {direction}");

        // Select the new object
        Selection.activeGameObject = duplicatedObject;
    }

    private void DrawDoorStatusGrid(RoomController roomController)
    {
        GUIStyle activeStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { textColor = new Color(0.3f, 0.8f, 0.3f) },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUIStyle inactiveStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) },
            alignment = TextAnchor.MiddleCenter
        };

        // Top row (NW, NE)
        EditorGUILayout.BeginHorizontal();
        DrawDoorStatus(roomController, Direction.NorthWest, "↖ NW", activeStyle, inactiveStyle);
        DrawDoorStatus(roomController, Direction.NorthEast, "↗ NE", activeStyle, inactiveStyle);
        EditorGUILayout.EndHorizontal();

        // Bottom row (SW, SE)
        EditorGUILayout.BeginHorizontal();
        DrawDoorStatus(roomController, Direction.SouthWest, "↙ SW", activeStyle, inactiveStyle);
        DrawDoorStatus(roomController, Direction.SouthEast, "↘ SE", activeStyle, inactiveStyle);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDoorStatus(RoomController roomController, Direction direction, string label, GUIStyle activeStyle, GUIStyle inactiveStyle)
    {
        var door = roomController.Walls[direction.WallIndex].DoorController;
        bool isActive = door != null && door.gameObject.activeSelf;

        GUIStyle style = isActive ? activeStyle : inactiveStyle;
        string statusLabel = isActive ? $"{label} ✓" : label;

        GUILayout.Box(statusLabel, style, GUILayout.Height(25), GUILayout.ExpandWidth(true));
    }
}
