using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class UnitController : MonoBehaviour, IInteractable
{
    [Header("Unit References")]
    [SerializeField] protected Transform renderRoot;
    [SerializeField] private UnitDestination destination;
    [SerializeField] private UnitDialogue dialogue;
    [SerializeField] private UnitHealth health;
    [SerializeField] private UnitDeath death;
    [SerializeField] private UnitBehaviour defaultBehaviour;
    [SerializeField] private UnitInteraction defaultInteraction;
    [SerializeField] private OverheadCanvasPosition overheadCanvasPosition;

    [Header("Services")]
    [SerializeField] private UnitBehaviourPriorityService behaviourPriorityService;

    [Header("Component System")]
    [SerializeField] private List<UnitComponentDefinition> componentDefinitions = new List<UnitComponentDefinition>();
    [SerializeField] private List<UnitComponentInstance> componentInstances = new List<UnitComponentInstance>();

    public UnitDestination Destination => destination;
    public UnitDialogue Dialogue => dialogue;
    public UnitHealth Health => health;
    public UnitDeath Death => death;
    public UnitBehaviourPriorityService BehaviourPriorityService => behaviourPriorityService;
    public List<UnitComponentInstance> ComponentInstances => componentInstances;
    public OverheadCanvasPosition OverheadCanvasPosition => overheadCanvasPosition;

    [Header("Runtime")]
    [SerializeField] private UnitInstance instance;
    [SerializeField] private UnitBehaviour currentBehaviour;
    [SerializeField] private Vector3 targetLookPosition;
    [SerializeField] private Transform target;
    [SerializeField] private bool isEnemy = false;

    public UnitInstance Instance => instance;
    public UnitBehaviour CurrentBehaviour => currentBehaviour;
    public bool IsEnemy => isEnemy;

    public Vector3 LookPosition => targetLookPosition;

    public bool IsDefaultBehaviour => currentBehaviour == defaultBehaviour;

    public UnitMoment CurrentMoment
    {
        get
        {
            return Instance.Moments.Find(m => !m.IsComplete);
        }
    }

    public UnitInteraction CurrentInteraction
    {
        get
        {
            return CurrentMoment?.Interaction ?? defaultInteraction;
        }
    }


    public virtual Color Color { get; set; }

    Transform ITarget.Transform => transform;

    public Transform RenderRoot => renderRoot;

    [SerializeField] private CinemachineVirtualCamera dialogueCamera;

    public event UnityAction OnInitialized;
    public event UnityAction OnNavMeshAgentReady;
    public event UnityAction OnBehaviourChanged;
    public event UnityAction OnBehaviourComplete;

    private void OnValidate()
    {
        destination = GetComponent<UnitDestination>();
        health = GetComponent<UnitHealth>();
        death = GetComponent<UnitDeath>();
    }

    protected virtual void Awake()
    {
        if (renderRoot) targetLookPosition = transform.position + renderRoot.forward;

        destination = GetComponent<UnitDestination>();
        dialogue = GetComponent<UnitDialogue>();
        health = GetComponent<UnitHealth>();
        death = GetComponent<UnitDeath>();

        if (destination)
        {
            destination.OnTargetSelected += () => SetLookTarget(destination.Target);
        }

        if (dialogue)
        {
            dialogue.OnDialogueStarted += () => currentBehaviour?.PauseBehaviour();
            dialogue.OnDialogueCompleted += () => currentBehaviour?.UnpauseBehaviour();
        }
    }

    protected virtual IEnumerator Start()
    {
        yield return null;
        OnNavMeshAgentReady?.Invoke();
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void OnDestroy()
    {
        // Clean up all components
        foreach (var component in componentInstances)
        {
            component?.OnDestroy();
        }
        componentInstances.Clear();
    }

    protected virtual void Update()
    {
        // Update all components
        UpdateComponents();

        // Update abilities
        if (instance != null)
        {
            instance.UpdateAbilities(Time.deltaTime);
        }

        // Evaluate behavior priority every frame
        EvaluateBehaviourPriority();

        // Handle look rotation
        if (target) targetLookPosition = target.transform.position;

        if (!renderRoot) return;

        Vector3 direction = targetLookPosition - renderRoot.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            renderRoot.rotation = Quaternion.Slerp(
                renderRoot.rotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }
    }

    private void EvaluateBehaviourPriority()
    {
        if (behaviourPriorityService == null)
            return;

        var allBehaviours = GetComponents<UnitBehaviour>();
        UnitBehaviour bestBehaviour = defaultBehaviour;
        int highestPriority = 0;

        foreach (var behaviour in allBehaviours)
        {
            int priority = behaviourPriorityService.GetBehaviourPriority(behaviour, this);
            if (priority > highestPriority)
            {
                highestPriority = priority;
                bestBehaviour = behaviour;
            }
        }

        // Switch to best behavior if different from current
        if (bestBehaviour != currentBehaviour)
        {
            if (currentBehaviour != null)
            {
                currentBehaviour.StopBehaviour();
            }

            currentBehaviour = bestBehaviour;

            if (currentBehaviour != null)
            {
                currentBehaviour.StartBehaviour();
            }

            OnBehaviourChanged?.Invoke();
        }
    }

    public virtual void Initialize(UnitInstance instance)
    {
        this.instance = instance;

        if (isEnemy)
        {
            name = "Enemy - " + instance.Species.Id;
        }
        else
        {
            name = "Unit - " + instance.Species.Id;
        }

        // Create ability instances now that controller exists
        if (instance.Data.abilities != null && instance.Data.abilities.Count > 0)
        {
            var abilityInstances = new List<AbilityInstance>();
            foreach (var abilityDefinition in instance.Data.abilities)
            {
                if (abilityDefinition != null)
                {
                    var abilityInstance = abilityDefinition.CreateInstance(this);
                    abilityInstances.Add(abilityInstance);
                }
            }
            instance.InitializeAbilities(abilityInstances);
        }

        InitializeComponents();
        OnInitialized?.Invoke();
    }

    #region Component Management

    /// <summary>
    /// Initialize all component instances from their definitions.
    /// Merges prefab components with species-specific components.
    /// </summary>
    private void InitializeComponents()
    {
        componentInstances.Clear();

        // Collect all component definitions (prefab + species)
        var allDefinitions = new List<UnitComponentDefinition>(componentDefinitions);

        // Add species-specific components
        if (instance?.Species?.Components != null)
        {
            foreach (var definition in instance.Species.Components)
            {
                if (definition != null && !allDefinitions.Contains(definition))
                {
                    allDefinitions.Add(definition);
                }
            }
        }

        // Create instances from all definitions
        foreach (var definition in allDefinitions)
        {
            if (definition != null)
            {
                var instance = definition.CreateInstance(this);
                componentInstances.Add(instance);
                instance.OnInitialize();
            }
        }
    }

    /// <summary>
    /// Add a component at runtime.
    /// </summary>
    public void AddComponent(UnitComponentDefinition definition)
    {
        if (definition == null) return;

        var instance = definition.CreateInstance(this);
        componentInstances.Add(instance);
        instance.OnInitialize();
    }

    /// <summary>
    /// Remove a component at runtime.
    /// </summary>
    public void RemoveComponent(UnitComponentInstance instance)
    {
        if (instance == null) return;

        instance.OnDestroy();
        componentInstances.Remove(instance);
    }

    /// <summary>
    /// Get a component instance by type.
    /// </summary>
    public T GetComponentInstance<T>() where T : UnitComponentInstance
    {
        return componentInstances.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Get all component instances of a specific type.
    /// </summary>
    public List<T> GetComponentInstances<T>() where T : UnitComponentInstance
    {
        return componentInstances.OfType<T>().ToList();
    }

    /// <summary>
    /// Update all components.
    /// </summary>
    private void UpdateComponents()
    {
        foreach (var component in componentInstances)
        {
            component?.OnUpdate();
        }
    }

    #endregion

    public void SetLookPosition(Vector3 targetPosition)
    {
        target = null;
        targetLookPosition = targetPosition;
    }

    public void Teleport(Vector3 position, Transform parent)
    {
        transform.parent = parent;
        if (Destination) Destination.TeleportToPosition(position);
        else transform.position = position;
        SetLookPosition(position);
    }

    public void SetLookTarget(Transform target)
    {
        this.target = target;
    }

    public void AddMoment(UnitInteraction interaction)
    {
        var moment = new UnitMoment(interaction, false);
        Instance.Moments.Add(moment);
    }

    void IInteractable.Select(UnitController source)
    {
        var moment = CurrentMoment;
        if (moment != null) moment.StartMoment(source, this);
        else defaultInteraction.StartInteraction(source, this);
    }

    public virtual void OnProximityChanged(bool value)
    {
    }

    public void Focus()
    {
        if (dialogueCamera) dialogueCamera.Priority = 11;
    }

    public void Unfocus()
    {
        if (dialogueCamera) dialogueCamera.Priority = 0;
    }

    public static void ArrangeUnitsInRadius(Vector3 origin, List<UnitController> units)
    {
        int count = units.Count;
        if (count == 0) return;

        if (count == 1)
        {
            units[0].Destination.SetDestination(origin);
            units[0].SetLookPosition(origin); // face the center
            return;
        }

        float radius = 1.0f; // radius of the circle
        float offset = Random.Range(0f, Mathf.PI * 2f); // random rotation of the circle

        for (int i = 0; i < count; i++)
        {
            // Evenly spaced angle around the circle
            float angle = -(i / (float)count) * Mathf.PI * 2f + offset;

            // Compute position on circle
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 destination = origin + direction;

            // Update unit
            units[i].Destination.SetDestination(destination);
            units[i].SetLookPosition(origin); // face the center
        }
    }
}
