using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BurnSystem : Singleton<BurnSystem>
{
    [SerializeField] private GameObject burnVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBurnGA>(ApplyBurnPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyBurnGA>();
    }
    private IEnumerator ApplyBurnPerformer(ApplyBurnGA applyBurnGA)
    {
        CombatantView target = applyBurnGA.Target;
        Instantiate(burnVFX, target.transform.position, Quaternion.identity);
        target.Damage(applyBurnGA.BurnDamage);
        target.RemoveStatusEffect(typeof(BurnStatus), 1);
        yield return new WaitForSeconds(1f);
    }
}
