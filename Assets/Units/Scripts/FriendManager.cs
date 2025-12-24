using System.Collections;
using UnityEngine;

public class FriendManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private TextAsset textAsset;

    [Header("Spawn Settings")]
    [SerializeField] private PortalController portalPrefab;
    [SerializeField] private AnimationCurve riseCurve;
    [SerializeField] private float riseDuration = 1f;

    private void OnEnable()
    {
        unitControllerService.OnFriendInvited += UnitList_OnFriendInvited;
    }

    private void OnDisable()
    {
        unitControllerService.OnFriendInvited -= UnitList_OnFriendInvited;
    }

    private void UnitList_OnFriendInvited(UnitController unit, UnitController spawnedUnit)
    {
        StartCoroutine(SpawnFriendRoutine(unit, spawnedUnit));
    }

    private IEnumerator SpawnFriendRoutine(UnitController unit, UnitController spawnedUnit)
    {
        // Capture the spawn position from the unit's current position
        Vector3 spawnPosition = spawnedUnit.transform.position;

        // Move the already-spawned unit underground for animation
        spawnedUnit.transform.position = spawnPosition + Vector3.down;

        yield return new WaitForSeconds(2f);

        // Step 1: create portal
        var portal = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);

        bool portalOpened = false;
        portal.OnOpened.AddListener(() => portalOpened = true);

        // Wait until the portal finishes opening
        yield return new WaitUntil(() => portalOpened);

        // Step 2: animate the fungal rising out of the portal
        float elapsed = 0f;
        Vector3 start = spawnedUnit.transform.position;
        Vector3 end = spawnPosition;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = riseCurve.Evaluate(elapsed / riseDuration);
            spawnedUnit.transform.position = Vector3.LerpUnclamped(start, end, t);
            yield return null;
        }

        spawnedUnit.transform.position = end;

        if (unit is FungalController fungal)
        {
            fungal.GreetFriend(spawnedUnit);
        }
    }
}
