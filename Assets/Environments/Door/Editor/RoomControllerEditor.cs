using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(RoomController))]
public class RoomControllerEditor : Editor
{
    private TextureSelectorComponent wallTextureSelector;
    private MaterialSelectorComponent wallMaterialSelector;
    private MaterialSelectorComponent floorMaterialSelector;
    private UnitSelectorComponent unitSelector;
    private bool showUnits = true;

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

        unitSelector = new UnitSelectorComponent("Add Unit", (unit) =>
        {
            if (unit != null)
            {
                Undo.RecordObject(roomController, "Add Unit to Room");
                roomController.AddUnit(unit);
                EditorUtility.SetDirty(roomController);
                unitSelector.Reset();
            }
        });
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomController roomController = (RoomController)target;

        // Initialize selectors with current materials
        wallTextureSelector.Initialize(roomController.Walls[0].Renderer.sharedMaterial.mainTexture);
        wallMaterialSelector.Initialize(roomController.Walls[0].Renderer.sharedMaterial);
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
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);

        DrawUnitsSection(roomController);

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

    private void DrawUnitsSection(RoomController roomController)
    {
        // Header with foldout
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };
        showUnits = EditorGUILayout.Foldout(showUnits, $"Units ({roomController.Units.Count})", true, foldoutStyle);

        if (showUnits)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);

            // Display existing units
            if (roomController.Units.Count > 0)
            {
                for (int i = 0; i < roomController.Units.Count; i++)
                {
                    var unitController = roomController.Units[i];
                    if (unitController == null) continue;

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    // Unit name and instance info
                    string unitName = unitController.Instance != null
                        ? unitController.Instance.Data.Name
                        : "Unknown Unit";
                    EditorGUILayout.LabelField($"{i + 1}. {unitName}", EditorStyles.boldLabel);

                    // Select button
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = unitController.gameObject;
                        EditorGUIUtility.PingObject(unitController.gameObject);
                    }

                    // Remove button
                    GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
                    if (GUILayout.Button("✕", GUILayout.Width(30)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Unit",
                            $"Are you sure you want to remove {unitName}?",
                            "Remove", "Cancel"))
                        {
                            Undo.RecordObject(roomController, "Remove Unit from Room");
                            DestroyImmediate(unitController.gameObject);
                            EditorUtility.SetDirty(roomController);
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();

                    // Show unit instance details
                    if (unitController.Instance != null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField("Instance", unitController.Instance, typeof(UnitInstance), false);
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.EndVertical();
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space(5);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No units in this room", MessageType.Info);
                EditorGUILayout.Space(5);
            }

            // Add new unit section
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Add New Unit", EditorStyles.boldLabel);
            unitSelector.DrawGUI();

            EditorGUILayout.EndVertical();
        }
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
