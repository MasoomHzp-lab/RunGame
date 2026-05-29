using UnityEngine;

public class TimedLoopSound : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;
    [SerializeField] private float interval = 20f;
    [SerializeField] private float volume = 1f;

    private float timer;

    void Start()
    {
        timer = interval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Play();
            timer = interval;
        }
    }

    void Play()
    {
        if (clip == null || source == null) return;

        source.PlayOneShot(clip, volume);
    }
}