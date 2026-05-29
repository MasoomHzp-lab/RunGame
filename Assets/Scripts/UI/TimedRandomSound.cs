using UnityEngine;

public class TimedRandomSound : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float interval = 40f;
    [SerializeField] private float volume = 0.8f;

    private float timer;

    void Start()
    {
        timer = interval;

        // تست فوری
        Play();
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
        if (source == null)
        {
            Debug.LogError("AudioSource not set!");
            return;
        }

        if (clips == null || clips.Length == 0)
        {
            Debug.LogError("No clips assigned!");
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        source.PlayOneShot(clip, volume);
    }
}