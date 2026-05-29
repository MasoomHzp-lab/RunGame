using UnityEngine;

public class BirdWave : MonoBehaviour
{
    public Transform startPoint;
    public Transform directionPoint;
    public float speed = 5f;
    public float waveAmount = 2f;
    public float waveSpeed = 2f;

    private Vector3 dir;
    private Vector3 right;

    private void Start()
    {
        if (startPoint == null || directionPoint == null)
            return;

        transform.position = startPoint.position;
        dir = (directionPoint.position - startPoint.position).normalized;
        right = Vector3.Cross(Vector3.up, dir);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void Update()
    {
        if (startPoint == null || directionPoint == null)
            return;

        float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmount;
        Vector3 offset = right * wave;
        transform.position += (dir + offset) * speed * Time.deltaTime;
    }
}
