using System.Collections;
using UnityEngine;

public class UnitSpore : UnitBehaviour
{
    [SerializeField] private float emissionInterval = 1f; // Time between emissions

    private PlantSporeEmitter emitter;

    public void SetEmitter(PlantSporeEmitter emitter)
    {
        this.emitter = emitter;
    }

    protected override void OnBehaviourStart()
    {
        if (emitter != null)
        {
            Debug.Log("Starting spore emission");
            StartCoroutine(EmitSporeRoutine());
        }
        else
        {
            StopBehaviour();
        }
    }

    private IEnumerator EmitSporeRoutine()
    {
        Debug.Log("Starting spore emission routine");
        while (IsActive)
        {
            emitter.EmitSpore();
            yield return new WaitForSeconds(emissionInterval);
        }

        Debug.Log("Stopping spore emission");
    }

    public override void StopBehaviour()
    {
        Debug.Log("Stopping spore emission behaviour");
        base.StopBehaviour();
        StopAllCoroutines();
    }
}