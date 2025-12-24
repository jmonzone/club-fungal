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

    private Vector3 GetRandomSpawnPosition(Vector3 startPosition)
    {
        var spawnPosition = startPosition;
        var randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        spawnPosition += randomDirection * 2f;
        return spawnPosition;
    }

    private void OnEnable()
    {
        unitControllerService.OnFriendInvited += UnitList_OnFriendInvited;
    }

    private void OnDisable()
    {
        unitControllerService.OnFriendInvited -= UnitList_OnFriendInvited;
    }

    private void UnitList_OnFriendInvited(UnitController unit, UnitInstance friend)
    {
        StartCoroutine(SpawnFriendRoutine(unit, friend));
    }

    private IEnumerator SpawnFriendRoutine(UnitController unit, UnitInstance friend)
    {
        yield return new WaitForSeconds(2f);

        var spawnPosition = GetRandomSpawnPosition(unit.transform.position);

        // Step 1: create portal
        var portal = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);

        bool portalOpened = false;
        portal.OnOpened.AddListener(() => portalOpened = true);

        // Wait until the portal finishes opening
        yield return new WaitUntil(() => portalOpened);

        // Step 2: spawn fungal
        var summonedUnit = unitControllerService.SpawnUnit(friend, spawnPosition + Vector3.down, null);

        // Animate the fungal rising out of the portal
        float elapsed = 0f;
        Vector3 start = summonedUnit.transform.position;
        Vector3 end = spawnPosition;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = riseCurve.Evaluate(elapsed / riseDuration);
            summonedUnit.transform.position = Vector3.LerpUnclamped(start, end, t);
            yield return null;
        }

        summonedUnit.transform.position = end;

        if (unit is FungalController fungal)
        {
            fungal.GreetFriend(summonedUnit);
        }
    }
}
