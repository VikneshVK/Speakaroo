//using UnityEngine;

//public class BalloonInteraction : MonoBehaviour
//{
//    public ParticleSystem burstEffectPrefab; // Reference to the burst particle effect
//    private ParticleSystem burstEffectInstance;

//    void Start()
//    {
//        // Initialize the burst effect instance from the prefab
//        if (burstEffectPrefab != null)
//        {
//            burstEffectInstance = Instantiate(burstEffectPrefab, transform.position, Quaternion.identity);
//            burstEffectInstance.transform.SetParent(transform); // Set the burst effect as a child
//            burstEffectInstance.gameObject.SetActive(false); // Initially disable it
//        }
//        else
//        {
//            Debug.LogError("Burst effect prefab is not assigned.");
//        }
//    }

//    void OnMouseDown()
//    {
//        // Play the burst effect
//        if (burstEffectInstance != null)
//        {
//            burstEffectInstance.transform.position = transform.position; // Move to balloon position
//            burstEffectInstance.gameObject.SetActive(true); // Enable the effect
//            burstEffectInstance.Play(); // Play the effect
//        }

//        // Destroy the balloon after a delay to allow the effect to play
//        Destroy(gameObject, 0.5f); // Adjust the delay if needed
//    }
//}
