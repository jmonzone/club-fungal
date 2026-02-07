using UnityEngine;

[CreateAssetMenu(fileName = "NetworkRunSettings", menuName = "Club Fungal/Network Run/Settings")]
public class NetworkRunSettings : ScriptableObject
{
    // Add settings fields as needed
    public int defaultPartySize = 3;
    public float updateInterval = 1.0f;
    public bool debugMode = false;
    // Add more settings here
}
