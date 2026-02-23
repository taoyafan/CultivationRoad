using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTargetSystem : Singleton<ManualTargetSystem>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;
    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.SetupArrow(startPosition);
        arrowView.gameObject.SetActive(true);
    }

    public Damageable EndTargeting(Vector3 endPosition)
    {
        arrowView.gameObject.SetActive(false);
        if (Physics.Raycast(endPosition, Vector3.forward, out RaycastHit hit, 10f, targetLayerMask)
            && hit.collider != null
            && hit.transform.TryGetComponent(out Damageable target))
        {
            // Target must be comboview or CardView which status >= casting
            if (target is CombatantView || 
                (target is CardView cardView && cardView.Card.Status > CardStatus.Casting))
            {
                return target;
            }
        }
        return null;
    }
}
