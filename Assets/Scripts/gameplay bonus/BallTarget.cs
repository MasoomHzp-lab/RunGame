using UnityEngine;

public class BallTarget : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float destroyZ = -10f;

    private bool activeTarget;

    public void Activate(float speed)
    {
        moveSpeed = speed;
        activeTarget = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!activeTarget) return;

        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        if (transform.position.z <= destroyZ)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        activeTarget = false;
        gameObject.SetActive(false);
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }
}