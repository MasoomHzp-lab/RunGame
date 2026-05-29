using UnityEngine;

public class BirdOrbit : MonoBehaviour
{
    public Transform centerPoint;
    public float radius = 10f;
    public float speed = 2f;
    public float height = 5f;

    private float angle;

    private void Update()
    {
        if (centerPoint == null)
            return;

        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        Vector3 pos = new Vector3(x, height, z) + centerPoint.position;
        transform.position = pos;
        transform.LookAt(centerPoint.position);
    }
}
