using UnityEngine;

public class BirdFlockMove : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 2f;

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        float turn = Mathf.Sin(Time.time) * turnSpeed;
        transform.Rotate(0f, turn * Time.deltaTime, 0f);
    }
}
