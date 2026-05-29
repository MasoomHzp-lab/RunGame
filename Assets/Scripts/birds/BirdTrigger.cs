using UnityEngine;

public class BirdTrigger : MonoBehaviour
{
    public GameObject birds;

    private void OnTriggerEnter(Collider other)
    {
        if (birds != null && other.CompareTag("Player"))
            birds.SetActive(true);
    }
}
