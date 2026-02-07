using UnityEngine;

[CreateAssetMenu(fileName = "NetworkRunSettings", menuName = "Club Fungal/Network Run/Settings")]
public class NetworkRunSettings : ScriptableObject
{
    // Add settings fields as needed
    public int defaultPartySize = 3;
    public float updateInterval = 1.0f;
    public bool debugMode = false;
    public float speedMultiplier = 1.0f; // Simulation speed (1.0 = normal, 2.0 = 2x speed, etc.)
    // Add more settings here
}
