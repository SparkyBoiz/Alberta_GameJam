using UnityEngine;
using UnityEngine.VFX;

public class SoundWord : MonoBehaviour
{
    VisualEffect visualEffect;
    [SerializeField] Texture2D texture;
    [SerializeField] float speed;
    Vector3 position;
    Vector3 direction;
    public bool isPlaying { get; private set; }

    void Awake()
    {
        visualEffect = GetComponent<VisualEffect>();
        isPlaying = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        visualEffect.SetTexture("Texture", texture);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void Spawn(Vector3 position, Vector3 direction, float size)
    {
        visualEffect.SetVector3("Position", position);
        visualEffect.SetVector3("Direction", direction);
        visualEffect.SetFloat("Size", size);
        visualEffect.SetFloat("Speed", speed);
    }
}
