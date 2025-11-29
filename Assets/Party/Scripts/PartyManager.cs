
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PartyControllerService partyControllerService;

    private void Start()
    {
        partyControllerService.Initialize();
    }
}