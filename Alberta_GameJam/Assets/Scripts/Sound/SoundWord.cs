using UnityEngine;
using UnityEngine.VFX;

public class SoundWord : MonoBehaviour
{
    VisualEffect visualEffect;
    [SerializeField] Texture2D texture;
    [SerializeField] float speed;
    [SerializeField] float playInterval = 0.2f;
    float nextPlayTime;

    void Awake()
    {
        visualEffect = GetComponent<VisualEffect>();
        visualEffect.SetTexture("Texture", texture);
        nextPlayTime = 0f;
    }

    public void Spawn(Vector3 position, Vector3 direction, float size)
    {
        if (playInterval > 0f && Time.time < nextPlayTime)
        {
            return;
        }

        if (playInterval > 0f)
        {
            nextPlayTime = Time.time + playInterval;
        }

        visualEffect.SetVector3("Position", position);
        visualEffect.SetVector3("Direction", direction);
        visualEffect.SetFloat("Size", size);
        visualEffect.SetFloat("Speed", speed);
        visualEffect.SendEvent("OnSpawn");
    }
}