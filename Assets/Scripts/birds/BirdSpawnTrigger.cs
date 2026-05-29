using UnityEngine;

public class BirdSpawnTrigger : MonoBehaviour
{
    public GameObject birdGroup;
    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated || birdGroup == null)
            return;

        if (other.CompareTag("Player"))
        {
            birdGroup.SetActive(true);
            activated = true;
        }
    }
}
