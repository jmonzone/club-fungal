using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : UnitController
{
    [Header("Player References")]
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private InventoryReference inventoryReference;
    [SerializeField] private CinemachineVirtualCamera povCamera;

    private NavMeshAgent navMeshAgent;
    private UnitGlyphCollect unitGlyphCollect;
    private Material[] materials;

    public override Color Color
    {
        get => materials[0].GetColor("_Outer_Color");
        set
        {
            foreach (var material in materials)
            {
                material.SetColor("_Outer_Color", value);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        unitGlyphCollect = GetComponent<UnitGlyphCollect>();
        unitGlyphCollect.OnNoteHit += UnitGlyphCollect_OnNoteHit;

        // collect only the materials that are the same instance as targetMaterial
        var mats = new List<Material>();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.name.StartsWith("Player Material"))
                {
                    mats.Add(mat);
                }
            }
        }

        materials = mats.ToArray();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = playerReference.Speed;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
            transform.position = hit.position;
        }

    }

    private void UnitGlyphCollect_OnNoteHit(DJTrack track)
    {
        inventoryReference.IncreaseShrune(track.Glyph);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        playerReference.OnPOVCameraToggled += PlayerReference_OnPOVCameraToggled;
    }

    private void PlayerReference_OnPOVCameraToggled(bool value)
    {
        povCamera.Priority = value ? 11 : 0;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        playerReference.OnPOVCameraToggled -= PlayerReference_OnPOVCameraToggled;
    }
}