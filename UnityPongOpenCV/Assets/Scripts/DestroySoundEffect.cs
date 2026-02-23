using UnityEngine;

public class DestroySoundEffect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject,1.0f); // Destroy the GameObject after 1 second
    }

}
