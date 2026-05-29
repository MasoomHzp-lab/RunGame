using UnityEngine;

public class RandomAmbientSound : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] clips;

    public float minDelay = 5f;
    public float maxDelay = 15f;

    public float volume = 0.5f;

    private float timer;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PlayRandom();
            ResetTimer();
        }
    }

    void PlayRandom()
    {
        if (clips.Length == 0) return;

        var clip = clips[Random.Range(0, clips.Length)];
        source.PlayOneShot(clip, volume);
    }

    void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }
}