using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class Lvl8Sc2QuestManager : MonoBehaviour
{
    public static Lvl8Sc2QuestManager Instance; // Singleton for global access
    public int colorsMade = 0; // Tracks the number of colors made
    public BeakerDragHandler beakerDragHandler; // Reference to BeakerDragHandler
    public Animator beakerAnimator; // Animator for the beaker
    public GameObject beakerAnimationPanel;
    public Animator beakerImageAnimator;
    public SpriteRenderer beakerSpriteRenderer; // SpriteRenderer for the beaker
    public Image beakerImage;
    public Sprite[] beakerSprites; // Array of sprites for the beaker after animations
    public GameObject Board;
    public Sprite BoardSprite1;
    public Sprite BoardSprite2;

    public Animator kikiAnimator; // Reference to Kiki's Animator
    public Animator kikiImageAnimator;
    public Animator jojoAnimator;
    public Animator jojoImageAnimator;
    public AudioSource kikiAudioSource; // AudioSource for Kiki
    public AudioClip firstColorAudio; // Audio for FirstColor animation
    public AudioClip secondColorAudio; // Audio for SecondColor animation
    public AudioClip thirdColorAudio;
    public AudioClip fourthColorAudio;
    public AudioClip wantMoreAudio;
    public AudioClip wrongAudioClip;
    public AudioClip finalAudio;
    public AudioClip GreenAudio;
    public AudioClip OrangeAudio;
    public SubtitleManager subtitleManager;

    public Lvl8SSc2HelperController helperController; // Reference to HelperController
    public GameObject leftTestTubeStand; // Reference to left stand
    public GameObject rightTestTubeStand; // Reference to right stand
    public GameObject leftTestTubeStand2;
    public GameObject rightTestTubeStand2;
    public GameObject tt2; // Reference to TT2 test tube
    public GameObject tt4;
    public GameObject tt5; // Reference to TT4 test tube
    public GameObject tt8;

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;

    public Sprite defaultBeakerSprite;
    private bool isFirstAttemptFailed = false;

    private List<string> testTubeDropSequence = new List<string>(); // Sequence of dropped test tubes

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
   void Start()
    {
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    public void TestTubeDropped(string testTubeName)
    {
        testTubeDropSequence.Add(testTubeName);

        if (testTubeDropSequence.Count == 1)
        {
            TriggerBeakerAnimation(testTubeName);
        }

        if (testTubeDropSequence.Count == 2)
        {
            // Use a delayed LeanTween call with 1.5-second delay
            LeanTween.delayedCall(1.5f, () =>
            {
                // Tween beaker panel children one by one to 1
                LeanTweenChildrenToOne(() =>
                {
                    // Start validation process after the tween is complete
                    StartCoroutine(ValidateSequenceAfterAnimation());
                });
            });
        }
    }

    private void LeanTweenChildrenToOne(System.Action onComplete)
    {
        if (beakerAnimationPanel == null)
        {
            Debug.LogError("BeakerAnimationPanel is not assigned in the Inspector!");
            return;
        }

        Transform panelTransform = beakerAnimationPanel.transform; // Use the inspector-assigned panel
        int childCount = panelTransform.childCount;

        beakerAnimationPanel.SetActive(true);
        kikiAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        jojoAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        beakerAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = false;

        LeanTween.scale(beakerImage.gameObject, Vector3.one, 0.2f).setOnComplete(() =>
        {
            LeanTween.scale(kikiImageAnimator.gameObject, Vector3.one, 0.2f).setOnComplete(() =>
            {
                LeanTween.move(kikiImageAnimator.GetComponent<RectTransform>(),
                    new Vector2(222, 155), 0.3f).setEase(LeanTweenType.easeInOutQuad);
            });

            LeanTween.scale(jojoImageAnimator.gameObject, Vector3.one, 0.2f).setOnComplete(() =>
            {
                LeanTween.move(jojoImageAnimator.GetComponent<RectTransform>(),
                    new Vector2(-230, 230), 0.3f).setEase(LeanTweenType.easeInOutQuad);
            });

            float delay = 0f;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = panelTransform.GetChild(i);
                LeanTween.scale(child.gameObject, Vector3.one, 0.2f).setDelay(delay);
                delay += 0.2f; 
            }

            LeanTween.delayedCall(delay + 0.5f, () =>
            {
                onComplete?.Invoke(); 
            });
        });
    }
    

    private IEnumerator ValidateSequenceAfterAnimation()
    {
        
        if (ValidateSequence())
        {
            if (colorsMade == 0)
            {
                yield return PlayCorrectAnimation("CorrectGreen");
            }
            else if (colorsMade == 1)
            {
                yield return PlayCorrectAnimation("CorrectOrange");
            }
        }
        else
        {
            if (!isFirstAttemptFailed)
            {
                isFirstAttemptFailed = true;                
            }
        }
    }

    private void StartHelperSequence()
    {
        if (colorsMade == 0)
        {
            helperController.DisableAllTestTubeColliders(leftTestTubeStand, rightTestTubeStand);
            StartCoroutine(HelperFirstColorSequence());
        }
        else if (colorsMade == 1)
        {
            helperController.DisableAllTestTubeColliders(leftTestTubeStand2, rightTestTubeStand2);
            StartCoroutine(HelperThirdColorSequence());
        }
    }

    private IEnumerator HelperThirdColorSequence()
    {
        kikiAnimator.SetTrigger("ThirdColor");
        if (thirdColorAudio != null && kikiAudioSource != null)
        {
            kikiAudioSource.clip = thirdColorAudio;
            kikiAudioSource.Play();
            subtitleManager.DisplaySubtitle("Pour the half red testtube into the beaker.", "Kiki", thirdColorAudio);
        }

        yield return new WaitForSeconds(1.8f);

        helperController.SpawnGlow(tt5);

        yield return new WaitForSeconds(2.5f);
        helperController.EnableTestTubeCollider(tt5);
    }

    private IEnumerator HelperFirstColorSequence()
    {
        kikiAnimator.SetTrigger("FirstColor");
        if (firstColorAudio != null && kikiAudioSource != null)
        {
            kikiAudioSource.clip = firstColorAudio;
            kikiAudioSource.Play();
            subtitleManager.DisplaySubtitle("Pour the full blue testtube into the beaker.", "Kiki", firstColorAudio);
        }

        yield return new WaitForSeconds(1.8f);

        helperController.SpawnGlow(tt2);

        yield return new WaitForSeconds(2.5f);
        helperController.EnableTestTubeCollider(tt2);
    }

    public void OnTestTubeDropped(GameObject droppedTube)
    {
        if (droppedTube == tt2 && isFirstAttemptFailed)
        {
            StartCoroutine(HelperSecondColorSequence());
        }
        else if (droppedTube == tt5 && isFirstAttemptFailed)
        {
            StartCoroutine(HelperForthColorSequence());
        }
    }

    private IEnumerator HelperSecondColorSequence()
    {
        kikiAnimator.SetTrigger("SecondColor");
        if (secondColorAudio != null && kikiAudioSource != null && isFirstAttemptFailed)
        {
            kikiAudioSource.clip = secondColorAudio;
            kikiAudioSource.Play();
            subtitleManager.DisplaySubtitle("Pour the half yellow testtube into the beaker.", "Kiki", secondColorAudio);
        }

        yield return new WaitForSeconds(1.8f);

        helperController.SpawnGlow(tt4);

        yield return new WaitForSeconds(2.5f);
        helperController.EnableTestTubeCollider(tt4);
    }

    private IEnumerator HelperForthColorSequence()
    {
        kikiAnimator.SetTrigger("ForthColor");
        if (fourthColorAudio != null && kikiAudioSource != null && isFirstAttemptFailed)
        {
            kikiAudioSource.clip = fourthColorAudio;
            kikiAudioSource.Play();
            subtitleManager.DisplaySubtitle("Pour the full yellow testtube into the beaker.", "Kiki", fourthColorAudio);
        }

        yield return new WaitForSeconds(1.8f);

        helperController.SpawnGlow(tt8);

        yield return new WaitForSeconds(2.5f);
        helperController.EnableTestTubeCollider(tt8);
    }

    private void TriggerSecondColorLogic()
    {
        if (kikiAnimator != null && !isFirstAttemptFailed)
        {
            kikiAnimator.SetTrigger("SecondColor");
        }

        if (kikiAudioSource != null && secondColorAudio != null && !isFirstAttemptFailed)
        {
            kikiAudioSource.clip = secondColorAudio;
            kikiAudioSource.Play();
            subtitleManager.DisplaySubtitle("Pour the half yellow testtube into the beaker.", "Kiki", secondColorAudio);
        }
    }

    private void TriggerForthColorLogic()
    {
        if (kikiAnimator != null && !isFirstAttemptFailed)
        {            
            kikiAnimator.SetTrigger("ForthColor");
        }

        if (kikiAudioSource != null && fourthColorAudio != null && !isFirstAttemptFailed)
        {
            kikiAudioSource.clip = fourthColorAudio;
            kikiAudioSource.Play();
            subtitleManager.DisplaySubtitle("Pour the full yellow testtube into the beaker.", "Kiki", fourthColorAudio);
        }
    }

    private IEnumerator PlayCorrectAnimation(string trigger)
    {
        beakerImageAnimator.SetTrigger(trigger);        

        yield return new WaitForSeconds(1.8f);

        if (colorsMade == 0)
        {
            kikiImageAnimator.SetTrigger("Green");
            if (kikiAudioSource != null && GreenAudio != null)
            {
                kikiAudioSource.clip = GreenAudio;
                kikiAudioSource.Play();
                subtitleManager.DisplaySubtitle("Wow! Yellow and blue made green.", "Kiki", GreenAudio);
            }
        }
        else if (colorsMade == 1)
        {
            kikiImageAnimator.SetTrigger("Orange");
            if (kikiAudioSource != null && OrangeAudio != null)
            {
                kikiAudioSource.clip = OrangeAudio;
                kikiAudioSource.Play();
                subtitleManager.DisplaySubtitle("Wow..! Yellow and red made orange.", "Kiki", OrangeAudio);
            }
        }
        SfxAudioSource.PlayOneShot(SfxAudio1);
        
        yield return new WaitForSeconds(3.8f);

        yield return StartCoroutine(LeanTweenChildrenToZeroCoroutine());

        if (colorsMade == 0)
        {
            if (jojoAnimator != null)
            {
                jojoAnimator.SetTrigger("WantMore");
            }

            if (wantMoreAudio != null && kikiAudioSource != null)
            {
                kikiAudioSource.clip = wantMoreAudio;
                kikiAudioSource.Play();
                subtitleManager.DisplaySubtitle("haha! Let's do it again.", "Kiki", wantMoreAudio);
            }

            yield return new WaitForSeconds(2.0f);

            colorsMade++;

            Board.GetComponent<SpriteRenderer>().sprite = BoardSprite1;

            beakerDragHandler.TweenBackTestTubeStands();

            testTubeDropSequence.Clear();

            ResetBeakerSprite();

            isFirstAttemptFailed = false;
        }
        else if (colorsMade == 1)
        {
            if (jojoAnimator != null)
            {
                jojoAnimator.SetTrigger("FinalDialogue");
            }

            if (finalAudio != null && kikiAudioSource != null)
            {
                kikiAudioSource.clip = finalAudio;
                kikiAudioSource.Play();
                subtitleManager.DisplaySubtitle("So fun..! I love mixing colors.", "Kiki", finalAudio);
            }
            Board.GetComponent<SpriteRenderer>().sprite = BoardSprite2;
        }
    }
    private bool ValidateSequence()
    {
        string tube1 = testTubeDropSequence[0];
        string tube2 = testTubeDropSequence[1];

        if (colorsMade == 0)
        {
            if (tube1 == "TT2" && tube2 == "TT4")
            {
                return true;
            }
            else
            {
                HandleIncorrectCombination(tube1, tube2, "DarkGreen", "LightGreen");
                return false;
            }
        }
        else if (colorsMade == 1)
        {
            if (tube1 == "TT5" && tube2 == "TT8")
            {
                return true;
            }
            else
            {
                HandleIncorrectCombination(tube1, tube2, "DarkOrange", "LightOrange");
                return false;
            }
        }

        return false;
    }

    private void HandleIncorrectCombination(string tube1, string tube2, string darkTrigger, string lightTrigger)
    {
        if ((tube1 == "TT1" && tube2 == "TT3") || (tube1 == "TT3" && tube2 == "TT1"))
        {
            TriggerIncorrectBeakerAnimation(darkTrigger);
        }
        else if ((tube1 == "TT1" && tube2 == "TT4") || (tube1 == "TT4" && tube2 == "TT1") ||
                 (tube1 == "TT2" && tube2 == "TT3") || (tube1 == "TT3" && tube2 == "TT2") ||
                 (tube1 == "TT4" && tube2 == "TT2"))
        {
            TriggerIncorrectBeakerAnimation(lightTrigger);
        }
        else if ((tube1 == "TT5" && tube2 == "TT7") || (tube1 == "TT7" && tube2 == "TT5") ||
                 (tube1 == "TT8" && tube2 == "TT5"))
        {
            TriggerIncorrectBeakerAnimation(lightTrigger);
        }
        else if ((tube1 == "TT6" && tube2 == "TT8") || (tube1 == "TT8" && tube2 == "TT6") ||
                 (tube1 == "TT6" && tube2 == "TT7") || (tube1 == "TT7" && tube2 == "TT6"))
        {
            TriggerIncorrectBeakerAnimation(darkTrigger);
        }
    }
    private void TriggerIncorrectBeakerAnimation(string trigger)
    {
        /*beakerImageAnimator.enabled = true;*/

        beakerImageAnimator.SetTrigger(trigger);

        if (SfxAudioSource != null)
        {
            SfxAudioSource.PlayOneShot(SfxAudio2);
        }

        StartCoroutine(HandleBeakerAnimation());
    }

    private IEnumerator HandleBeakerAnimation()
    {
        yield return new WaitForSeconds(1.8f); 

        // Disable the Animator
        /*beakerImageAnimator.enabled = false;*/

        kikiImageAnimator.SetTrigger("WrongFeedback");
        if (kikiAudioSource != null && wrongAudioClip != null)
        {
            kikiAudioSource.clip = wrongAudioClip;
            kikiAudioSource.Play();
        }

        yield return new WaitForSeconds(2.5f);

        yield return StartCoroutine(LeanTweenChildrenToZeroCoroutine());

        ResetTestTubeStates();
    }

    private IEnumerator LeanTweenChildrenToZeroCoroutine()
    {
        if (beakerAnimationPanel == null)
        {
            Debug.LogError("BeakerAnimationPanel is not assigned in the Inspector!");
            yield break;
        }

        Transform panelTransform = beakerAnimationPanel.transform;
        int childCount = panelTransform.childCount;

        LeanTween.move(kikiImageAnimator.GetComponent<RectTransform>(),
            new Vector2(222, -155), 0.3f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(jojoImageAnimator.GetComponent<RectTransform>(),
            new Vector2(-230, -230), 0.3f).setEase(LeanTweenType.easeInOutQuad);

        yield return new WaitForSeconds(0.3f);

        // Scale all children to zero one by one
        float delay = 0f;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = panelTransform.GetChild(i);
            LeanTween.scale(child.gameObject, Vector3.zero, 0.5f).setDelay(delay);
            delay += 0.2f;
        }

        yield return new WaitForSeconds(delay + 0.5f);

        beakerAnimationPanel.SetActive(false);
        kikiAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        jojoAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        beakerAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = true;

        Debug.Log("Panel and children animations complete, panel disabled.");
    }

    
    private void ResetTestTubeStates()
    {
        testTubeDropSequence.Clear();

        beakerDragHandler.ResetTestTubeDragging();

        ResetBeakerSprite();

        ResetTestTubes();

        if (isFirstAttemptFailed) 
        { 
            StartHelperSequence(); 
        }
    }

    public void ResetBeakerSprite()
    {
        if (defaultBeakerSprite != null && beakerSpriteRenderer != null)
        {
            beakerAnimator.enabled = false;

            beakerSpriteRenderer.sprite = defaultBeakerSprite;

            Debug.Log("Beaker sprite reset to default.");
        }
        else
        {
            Debug.LogError("Default beaker sprite or beakerSpriteRenderer is not assigned in the Inspector!");
        }
    }


    private IEnumerator ReenableAnimatorAfterDelay()
    {
        yield return null; // Wait for 1 frame
        /*beakerAnimator.enabled = true;*/
    }

    private void ResetTestTubes()
    {
        TTDragHandler[] testTubes = FindObjectsOfType<TTDragHandler>();

        foreach (TTDragHandler testTube in testTubes)
        {
            if (ShouldDestroyTestTube(testTube.testTubeName))
            {
                Debug.Log($"Destroying test tube: {testTube.testTubeName} for colorsMade: {colorsMade}");
                Destroy(testTube.gameObject);
            }
            else
            {
                Debug.Log($"Resetting sprite for valid test tube: {testTube.testTubeName}");
                testTube.testTubeAnimator.enabled = true; // Reset to default animation
                testTube.isDraggable = true; // Re-enable dragging
            }
        }
    }

    private bool ShouldDestroyTestTube(string testTubeName)
    {
        switch (colorsMade)
        {
            case 0:
                
                return testTubeName == "TT1" || testTubeName == "TT3";

            case 1:
                
                return testTubeName == "TT6" || testTubeName == "TT7";

            default:
                return false; // Keep all other test tubes
        }
    }


    private bool IsValidTestTube(string testTubeName)
    {
        // Define the valid test tube names for the current validation sequence
        if (colorsMade == 0)
        {
            return testTubeName == "TT2" || testTubeName == "TT4";
        }
        else if (colorsMade == 1)
        {
            return testTubeName == "TT5" || testTubeName == "TT8";
        }

        return false; // Default to invalid if no valid combination is found
    }

    private void TriggerJojoAndKikiAnimations()
    {
        Debug.Log("Triggering Jojo and Kiki animations!");
        // Animation logic for Jojo and Kiki goes here
    }

    private void TriggerBeakerAnimation(string testTubeName)
    {
        string animationTrigger = "";
        Sprite newSprite = null;

        // Determine the animation trigger and the new sprite based on the test tube name
        switch (testTubeName)
        {
            case "TT1":
                animationTrigger = "HalfBlue";
                newSprite = beakerSprites[0]; // Sprite 1
                break;
            case "TT2":
                animationTrigger = "FullBlue";
                newSprite = beakerSprites[1]; // Sprite 2
                break;
            case "TT3":
                animationTrigger = "FullYellow";
                newSprite = beakerSprites[2]; // Sprite 3
                break;
            case "TT4":
                animationTrigger = "HalfYellow";
                newSprite = beakerSprites[3]; // Sprite 4
                break;
            case "TT5":
                animationTrigger = "HalfRed";
                newSprite = beakerSprites[4]; // Sprite 5
                break;
            case "TT6":
                animationTrigger = "FullRed";
                newSprite = beakerSprites[5]; // Sprite 6
                break;
            case "TT7":
                animationTrigger = "HalfYellow";
                newSprite = beakerSprites[3]; // Sprite 4
                break;
            case "TT8":
                animationTrigger = "FullYellow";
                newSprite = beakerSprites[2]; // Sprite 3
                break;
        }

        if (!string.IsNullOrEmpty(animationTrigger) && newSprite != null)
        {
            // Enable the Animator
            beakerAnimator.enabled = true;

            // Trigger the animation
            beakerAnimator.SetTrigger(animationTrigger);

            // Start coroutine to disable Animator after the animation and set the new sprite
            StartCoroutine(HandleBeakerAnimation(newSprite));
        }
    }

    private IEnumerator HandleBeakerAnimation(Sprite newSprite)
    {
        // Wait for the animation to complete (adjust time to match the animation length)
        yield return new WaitForSeconds(1.5f);

        // Disable the Animator
        beakerAnimator.enabled = false;
        /*beakerImageAnimator.enabled = false;*/

        // Set the new sprite
        if (beakerSpriteRenderer != null)
        {
            beakerSpriteRenderer.sprite = newSprite;
            beakerImage.sprite = newSprite;
           Debug.Log($"Beaker sprite changed to: {newSprite.name}");
        }

        if (colorsMade == 0)
        {
            TriggerSecondColorLogic();
        }
        else if (colorsMade == 1)
        {
            TriggerForthColorLogic();
        }

    }
}
