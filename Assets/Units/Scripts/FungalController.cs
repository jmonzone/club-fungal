using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public interface IJob
{
    public bool IsAble { get; }
    public bool IsMoving { get; }
    public Vector3 TargetPosition { get; }
    public event UnityAction OnIsAbleChanged;
    public event UnityAction OnIsMovingChanged;
}

public class FungalController : UnitController
{
    [SerializeField] private NetworkRunService networkRunService;

    private UnitFollow unitFollow;
    private UnitColorPalette colorPalette;
    public override Color Color
    {
        get => colorPalette.SecondaryColor;
        set
        {
            colorPalette.SetSecondaryColor(value);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        unitFollow = GetComponent<UnitFollow>();
        colorPalette = GetComponent<UnitColorPalette>();
    }

    protected override IEnumerator Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.speed = networkRunService.Settings.baseMovementSpeed;

        agent.enabled = false;
        yield return null; // wait 1 frame
        agent.enabled = true;
        agent.Warp(transform.position);
        yield return base.Start();
    }

    public override void Initialize(UnitInstance instance)
    {
        if (renderRoot != null)
        {
            // Quaternion randomYRotation = Quaternion.Euler(0, Random.Range(135f, 225f), 0);
            // renderRoot.rotation = randomYRotation;
            Instantiate(instance.Species.Prefab, transform.position, Quaternion.identity, renderRoot);
        }

        base.Initialize(instance);
    }

    public void Follow(UnitFollow leader)
    {
        unitFollow.FollowUnit(leader);
        // Priority system will automatically activate follow behavior
    }

    public override void OnProximityChanged(bool value)
    {
        base.OnProximityChanged(value);
        //chatIcon.SetActive(!dialogueReference.IsActive && value);
    }

    public void GreetFriend(UnitController friend)
    {
        StartCoroutine(GreetRoutine(friend));
    }

    private IEnumerator GreetRoutine(UnitController friend)
    {
        Destination.SetTarget(friend.transform);
        yield return new WaitUntil(() => Destination.IsAtDestination);
        var greetingDialogue = new GreetingDialogue(this, friend);
        Dialogue.StartGreetDialogue(greetingDialogue, friend);
        friend.Dialogue.StartGreetDialogue(greetingDialogue, this);
    }
}
