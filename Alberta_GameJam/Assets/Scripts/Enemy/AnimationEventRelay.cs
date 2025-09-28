using Game.Player;
using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    public void OnDyingAnimationCompleted()
    {
        GetComponentInParent<Ghost>()?.OnDyingAnimationCompleted();
    }
}