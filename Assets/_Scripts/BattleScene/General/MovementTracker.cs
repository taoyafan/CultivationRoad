using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public static class MovementTracker
{
    private static readonly Dictionary<Transform, Vector3> destinations = new();

    public static Tween MoveAndTrack(Transform target, Vector3 destination, float duration = 0.1f)
    {
        if (target == null) return null;
        destinations[target] = destination;
        Tween t = target.DOMove(destination, duration).SetEase(Ease.OutCubic);
        t.OnKill(() => destinations.Remove(target));
        t.OnComplete(() => destinations.Remove(target));
        return t;
    }

    public static bool TryGetDestination(Transform target, out Vector3 destination)
    {
        if (target == null)
        {
            destination = default;
            return false;
        }
        return destinations.TryGetValue(target, out destination);
    }

    public static void Register(Transform target, Vector3 destination)
    {
        if (target != null) destinations[target] = destination;
    }

    public static void Unregister(Transform target)
    {
        if (target != null) destinations.Remove(target);
    }
}
