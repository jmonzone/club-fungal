using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitListReference unitList;
    [SerializeField] private Transform unitSpawnAnchor;
    [SerializeField] private TextAsset textAsset;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> unitControllers;


    [Header("Spawn Settings")]
    [SerializeField] private PortalController portalPrefab;
    [SerializeField] private AnimationCurve riseCurve;
    [SerializeField] private float riseDuration = 1f;

    public List<UnitController> UnitControllers => unitControllers;

    private void Awake()
    {
        unitControllers = new List<UnitController>();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        var spawnPosition = unitSpawnAnchor.transform.position;
        var randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        spawnPosition += randomDirection * 2f;
        return spawnPosition;
    }

    private void OnEnable()
    {
        unitList.OnFriendInvited += UnitList_OnFriendInvited;
    }

    private void OnDisable()
    {
        unitList.OnFriendInvited -= UnitList_OnFriendInvited;
    }

    private void UnitList_OnFriendInvited(UnitController unit, UnitInstance friend)
    {
        StartCoroutine(SpawnFriendRoutine(unit, friend));
    }

    private IEnumerator SpawnFriendRoutine(UnitController unit, UnitInstance friend)
    {
        yield return new WaitForSeconds(2f);

        var spawnPosition = GetRandomSpawnPosition();

        // Step 1: create portal
        var portal = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);

        bool portalOpened = false;
        portal.OnOpened.AddListener(() => portalOpened = true);

        // Wait until the portal finishes opening
        yield return new WaitUntil(() => portalOpened);

        // Step 2: spawn fungal
        var summonedUnit = unitList.SpawnUnit(friend, spawnPosition + Vector3.down, null);

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
