using UnityEngine;
using System.Collections;
using TMPro;

public class Lvl7Sc2DragManager : MonoBehaviour
{
    public Lvl7Sc2QuestManager questManager;

    // References to the toppings GameObjects
    public GameObject sauceTopping;
    public GameObject cheeseTopping;
    public GameObject mushroomTopping;
    public GameObject pepperoniTopping;

    // Reference to the PizzaDrag script
    public PizzaDrag pizzaDrag;

    public GameObject kikiImage;
    private Animator kikiAnimator;

    private GameObject[] currentToppings;  // Array to store the toppings for this pizza
    private int currentToppingIndex = 0;   // Track the current topping being dropped
    private int totalToppings = 0;         // Total number of toppings for this pizza

    public AudioClip sauceAudio;
    public AudioClip cheeseAudio;
    public AudioClip mushroomAudio;
    public AudioClip pepperoniAudio;

    // Reference to AudioManager
    public Lvl7Sc2AudioManager audioManager;

    public TextMeshProUGUI subtitleText;
    public LVL7Sc2HelperFunction helperFunction;
    public Transform pizzaLocation;

    public GameObject glowPrefab;

    void Start()
    {
        if (kikiImage != null)
        {
            kikiAnimator = kikiImage.GetComponent<Animator>();
            if (kikiAnimator == null)
            {
                Debug.LogError("Kiki image is missing an Animator component.");
            }
        }
        else
        {
            Debug.LogError("Kiki image is not assigned.");
        }

        UpdateColliders();
    }

    // Method to update the enabled collider based on PizzaMade and the current progress
    public void UpdateColliders()
    {
        DisableAllColliders();

        // Determine the sequence of toppings based on PizzaMade value
        if (questManager.PizzaMade == 0)
        {
            currentToppings = new GameObject[] { sauceTopping, cheeseTopping };
            totalToppings = 2;
        }
        else if (questManager.PizzaMade == 1)
        {
            currentToppings = new GameObject[] { sauceTopping, cheeseTopping, mushroomTopping };
            totalToppings = 3;
        }
        else if (questManager.PizzaMade == 2)
        {
            currentToppings = new GameObject[] { sauceTopping, cheeseTopping, mushroomTopping, pepperoniTopping };
            totalToppings = 4;
        }

        questManager.UpdateQuestDisplay();

    }

    // Enable the next topping in the sequence
    public void EnableNextTopping()
    {
        if (currentToppingIndex < totalToppings)
        {
            GameObject topping = currentToppings[currentToppingIndex];

            // Trigger the appropriate animation on the Kiki Animator
            string animationTrigger = GetKikiAnimationTrigger(topping);
            if (!string.IsNullOrEmpty(animationTrigger) && kikiAnimator != null)
            {
                kikiAnimator.SetTrigger(animationTrigger);

                PlayToppingAudio(topping);

                SpawnAndAnimateGlow(topping.transform.position);

                StartCoroutine(WaitForAnimationAndEnableCollider(animationTrigger, topping));
            }
            else
            {
                Debug.LogError("Invalid topping or missing Kiki Animator.");
            }
        }
    }

    private void SpawnAndAnimateGlow(Vector3 position)
    {
        if (glowPrefab == null)
        {
            Debug.LogError("Glow prefab is not assigned.");
            return;
        }

        // Instantiate the glow prefab
        GameObject glow = Instantiate(glowPrefab, position, Quaternion.identity);
        glow.transform.localScale = Vector3.zero;

        // Tween the scale of the glow prefab
        LeanTween.scale(glow, Vector3.one * 8f, 0.5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() =>
        {
            // Wait for 2 seconds before fading out and destroying
            StartCoroutine(FadeOutAndDestroy(glow, 2f));
        });
    }
    public void SpawnAndAnimateGlow2(Vector3 position)
    {
        if (glowPrefab == null)
        {
            Debug.LogError("Glow prefab is not assigned.");
            return;
        }

        // Instantiate the glow prefab
        GameObject glow = Instantiate(glowPrefab, position, Quaternion.identity);
        glow.transform.localScale = Vector3.zero;

        // Tween the scale of the glow prefab
        LeanTween.scale(glow, Vector3.one * 20f, 0.5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() =>
        {
            // Wait for 2 seconds before fading out and destroying
            StartCoroutine(FadeOutAndDestroy(glow, 2f));
        });
    }

    private IEnumerator FadeOutAndDestroy(GameObject glow, float fadeDuration)
    {
        SpriteRenderer sr = glow.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color initialColor = sr.color;
            float time = 0;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(initialColor.a, 0f, time / fadeDuration);
                sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                yield return null;
            }

            sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        }

        Destroy(glow);
    }

    private void PlayToppingAudio(GameObject topping)
    {
        if (audioManager == null)
        {
            Debug.LogError("AudioManager is not assigned.");
            return;
        }

        if (topping == sauceTopping && sauceAudio != null)
        {
            audioManager.PlayAudio(sauceAudio);
            StartCoroutine(RevealTextWordByWord("Put on the Tomato Sauce", 0.5f));
        }
        else if (topping == cheeseTopping && cheeseAudio != null)
        {
            audioManager.PlayAudio(cheeseAudio);
            StartCoroutine(RevealTextWordByWord("Put on the Cheese", 0.5f));
        }
        else if (topping == mushroomTopping && mushroomAudio != null)
        {
            audioManager.PlayAudio(mushroomAudio);
            StartCoroutine(RevealTextWordByWord("Put on the Mushrooms", 0.5f));
        }
        else if (topping == pepperoniTopping && pepperoniAudio != null)
        {
            audioManager.PlayAudio(pepperoniAudio);
            StartCoroutine(RevealTextWordByWord("Put on the Pepperoni", 0.5f));
        }
        else
        {
            Debug.LogWarning($"Audio clip for {topping.name} is not assigned.");
        }
    }
    private IEnumerator WaitForAnimationAndEnableCollider(string stateName, GameObject topping)
    {
        if (kikiAnimator == null)
        {
            Debug.LogError("Kiki Animator is null.");
            yield break;
        }

        yield return new WaitForSeconds(1.5f);

        if (topping != null && topping.GetComponent<Collider2D>() != null)
        {
            topping.GetComponent<Collider2D>().enabled = true;
            Debug.Log($"{topping.name} collider enabled after Kiki animation.");
            helperFunction.ResetTimer(); // Reset any existing timer

            AudioClip audioToPlay = null;
            string animationToTrigger = string.Empty;

            // Set the audio and animation trigger based on the topping
            if (topping == sauceTopping)
            {
                audioToPlay = sauceAudio;
                animationToTrigger = "Sauce";
            }
            else if (topping == cheeseTopping)
            {
                audioToPlay = cheeseAudio;
                animationToTrigger = "Cheese";
            }
            else if (topping == mushroomTopping)
            {
                audioToPlay = mushroomAudio;
                animationToTrigger = "Mushroom";
            }
            else if (topping == pepperoniTopping)
            {
                audioToPlay = pepperoniAudio;
                animationToTrigger = "Pepperoni";
            }

            // Call StartTimer with the audio and animation trigger
            helperFunction.StartTimer(topping.transform.position, pizzaLocation.position, audioToPlay, animationToTrigger);
        }
        else
        {
            Debug.LogError("Topping or Collider2D is null.");
        }

        UpdateIconOpacityForTopping(topping);
    }

    /* public void OnHelperTimerEnded()
     {
         if (currentToppingIndex < totalToppings && helperFunction != null)
         {
             GameObject topping = currentToppings[currentToppingIndex];
             if (topping != null && pizzaLocation != null)
             {
                 Vector3 toppingPosition = topping.transform.position;
                 Vector3 pizzaPosition = pizzaLocation.position;

                 // Spawn and tween the helper hand
                 helperFunction.SpawnAndTweenHelperHand(toppingPosition, pizzaPosition);
             }
         }
     }*/


    private void EnableToppingCollider()
    {
        if (currentToppingIndex < totalToppings)
        {
            GameObject topping = currentToppings[currentToppingIndex];
            topping.GetComponent<Collider2D>().enabled = true;
            UpdateIconOpacityForTopping(topping);
        }
    }

    private string GetKikiAnimationTrigger(GameObject topping)
    {
        if (topping == sauceTopping) return "Sauce";
        if (topping == cheeseTopping) return "Cheese";
        if (topping == mushroomTopping) return "Mushroom";
        if (topping == pepperoniTopping) return "Pepperoni";
        return string.Empty;
    }

    // Call this method when a topping is dropped successfully
    public void OnToppingDropped()
    {
        if (currentToppingIndex < totalToppings)
        {
            GameObject droppedTopping = currentToppings[currentToppingIndex];
            droppedTopping.GetComponent<Collider2D>().enabled = false;

            currentToppingIndex++;

            // If the required number of toppings have been dropped, enable the pizza collider
            if (currentToppingIndex == totalToppings)
            {
                pizzaDrag.EnablePizzaCollider();
                SpawnAndAnimateGlow2(pizzaDrag.gameObject.transform.position);
            }
            else
            {
                EnableNextTopping();
            }
            UpdateIconOpacityForTopping(droppedTopping);

            // Reset the helper timer since the topping has been dropped
            if (helperFunction != null)
            {
                helperFunction.ResetTimer();
            }
        }
    }

    public void ResetDragManager()
    {
        currentToppingIndex = 0;  // Reset the topping index
        currentToppings = null;   // Reset the toppings array
        totalToppings = 0;        // Reset total toppings count   
        pizzaDrag.pizzaDropped = false;
        pizzaDrag.canTapPizzaImage = false;
        UpdateColliders();        // Reinitialize the colliders for the new pizza
    }

    // Disable all colliders to reset
    public void DisableAllColliders()
    {
        sauceTopping.GetComponent<Collider2D>().enabled = false;
        cheeseTopping.GetComponent<Collider2D>().enabled = false;
        mushroomTopping.GetComponent<Collider2D>().enabled = false;
        pepperoniTopping.GetComponent<Collider2D>().enabled = false;
    }

    private void UpdateIconOpacityForTopping(GameObject topping)
    {
        bool isColliderActive = topping.GetComponent<Collider2D>().enabled;

        if (topping == sauceTopping)
        {
            questManager.UpdateIconOpacity(questManager.sauceIcon, isColliderActive);
        }
        else if (topping == cheeseTopping)
        {
            questManager.UpdateIconOpacity(questManager.cheeseIcon, isColliderActive);
        }
        else if (topping == mushroomTopping)
        {
            questManager.UpdateIconOpacity(questManager.toppingsIcon, isColliderActive);
        }
        else if (topping == pepperoniTopping)
        {
            questManager.UpdateIconOpacity(questManager.pepperoniIcon, isColliderActive);
        }
        else
        {
            Debug.LogWarning("Topping not recognized for opacity update.");
        }

        Debug.Log($"Updated opacity for {topping.name}, Collider Active: {isColliderActive}");
    }   

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }

}