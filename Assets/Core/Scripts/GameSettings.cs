using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Club Fungal/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Room Settings")]
    public bool useOuterWalls = false;
    public bool selectRoomOnTransition = true;
}