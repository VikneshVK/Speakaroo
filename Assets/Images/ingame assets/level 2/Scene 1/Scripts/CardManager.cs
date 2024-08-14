using UnityEngine;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public GameObject card1; // Reference to Card 1 GameObject
    public GameObject card2; // Reference to Card 2 GameObject
    public GameObject maskPrefab; // Reference to the Mask prefab

    private bool isCard1Shaking = false;
    private bool isCard2Shaking = false;

    private void Start()
    {
        // Start monitoring both cards
        StartCoroutine(MonitorCard(card1));
        StartCoroutine(MonitorCard(card2));
    }

    private IEnumerator MonitorCard(GameObject card)
    {
        while (card != null) // Ensure the loop exits if the card is destroyed
        {
            Collider2D cardCollider = card.GetComponent<Collider2D>();
            if (cardCollider != null && cardCollider.enabled && !IsMaskPrefabSpawned(card))
            {
                yield return new WaitForSeconds(2f); // Wait for 2 seconds before starting the shake

                if (card != null && cardCollider.enabled && !IsMaskPrefabSpawned(card)) // Check again to see if the mask is still not spawned and collider is enabled
                {
                    if (card == card1 && !isCard1Shaking)
                    {
                        isCard1Shaking = true;
                        StartCoroutine(ShakeCard(card1));
                    }
                    else if (card == card2 && !isCard2Shaking)
                    {
                        isCard2Shaking = true;
                        StartCoroutine(ShakeCard(card2));
                    }
                }
            }

            yield return null;
        }
    }

    private bool IsMaskPrefabSpawned(GameObject card)
    {
        if (card == null)
            return false;

        // Check if the mask prefab (or any of its instances) is a child of the card
        foreach (Transform child in card.transform)
        {
            if (child.name == maskPrefab.name)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator ShakeCard(GameObject card)
    {
        while (card != null) // Ensure the loop exits if the card is destroyed
        {
            Collider2D cardCollider = card.GetComponent<Collider2D>();
            if (cardCollider != null && cardCollider.enabled) // Check if the collider is enabled
            {
                // Scale up the card
                LeanTween.scale(card, card.transform.localScale * 1.2f, 0.5f).setEase(LeanTweenType.easeOutBack)
                    .setOnComplete(() =>
                    {
                        // Scale back down to original size if the card still exists and collider is enabled
                        if (card != null && card.GetComponent<Collider2D>() != null && card.GetComponent<Collider2D>().enabled)
                        {
                            LeanTween.scale(card, card.transform.localScale / 1.2f, 0.5f).setEase(LeanTweenType.easeInBack);
                        }
                    });

                yield return new WaitForSeconds(3f); // Shake every 3 seconds

                // Stop shaking if the mask prefab is spawned or if the collider is disabled
                if (IsMaskPrefabSpawned(card) || cardCollider == null || !cardCollider.enabled)
                {
                    break;
                }
            }
            else
            {
                break; // Exit if the collider is disabled
            }
        }

        // Reset shaking flag once shaking stops
        if (card == card1)
            isCard1Shaking = false;
        else if (card == card2)
            isCard2Shaking = false;
    }
}
