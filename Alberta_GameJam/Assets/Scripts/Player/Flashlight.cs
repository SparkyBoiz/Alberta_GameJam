using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float attackRate = 0.3f;

    class TrackedGhost
    {
        public Ghost ghost;
        public float nextAttackTime;
    }

    readonly List<TrackedGhost> _trackedGhosts = new List<TrackedGhost>();

    void Update()
    {
        if (_trackedGhosts.Count == 0)
        {
            return;
        }

        var time = Time.time;
        var interval = GetAttackInterval();

        for (int i = _trackedGhosts.Count - 1; i >= 0; i--)
        {
            var entry = _trackedGhosts[i];
            if (entry == null || entry.ghost == null)
            {
                _trackedGhosts.RemoveAt(i);
                continue;
            }

            if (time >= entry.nextAttackTime)
            {
                ApplyDamage(entry.ghost);

                if (entry.ghost == null)
                {
                    _trackedGhosts.RemoveAt(i);
                    continue;
                }

                entry.nextAttackTime = time + interval;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ghost = other.GetComponentInParent<Ghost>();
        if (ghost == null)
        {
            return;
        }

        var interval = GetAttackInterval();
        var time = Time.time;

        for (int i = 0; i < _trackedGhosts.Count; i++)
        {
            var entry = _trackedGhosts[i];
            if (entry != null && entry.ghost == ghost)
            {
                entry.nextAttackTime = time + interval;
                return;
            }
        }

        ApplyDamage(ghost);
        if (ghost == null)
        {
            return;
        }

        _trackedGhosts.Add(new TrackedGhost
        {
            ghost = ghost,
            nextAttackTime = time + interval
        });
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var ghost = other.GetComponentInParent<Ghost>();
        if (ghost == null)
        {
            return;
        }

        for (int i = _trackedGhosts.Count - 1; i >= 0; i--)
        {
            var entry = _trackedGhosts[i];
            if (entry != null && entry.ghost == ghost)
            {
                entry.ghost = null;
                break;
            }
        }
    }

    void OnDisable()
    {
        _trackedGhosts.Clear();
    }

    void ApplyDamage(Ghost ghost)
    {
        if (ghost == null || damage <= 0)
        {
            return;
        }

        ghost.TakeDamage(damage);
    }

    float GetAttackInterval()
    {
        return attackRate > 0f ? attackRate : Time.deltaTime;
    }
}
