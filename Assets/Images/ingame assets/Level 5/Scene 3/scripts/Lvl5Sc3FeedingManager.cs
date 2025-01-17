using System.Collections;
using TMPro;
using UnityEngine;

public class Lvl5Sc3FeedingManager : MonoBehaviour
{
    public int animalsFed = 0;
    public GameObject birdImage;
    public GameObject board;
    public SubtitleManager subtitleManager;
    private int currentAnimalIndex = 0;
    private Transform[] animals;
    private Animator birdAnimator;
    private AudioSource audioSource;
    public LVL1helperhandController helperhand;

    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;
    public AudioClip audio5;
    public AudioClip audio6;
    public AudioClip audio7;
    public AudioClip audio8;
    public AudioClip audio9;
    public AudioClip audio10;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;
    public Sprite sprite4;
    public Sprite sprite5;
    public Sprite sprite6;

    public GameObject glowPrefab2;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        birdAnimator = birdImage.GetComponent<Animator>();
        animals = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            animals[i] = transform.GetChild(i);
            animals[i].gameObject.SetActive(false);
        }

        if (board != null)
        {

        }
            
        StartCoroutine(HideBoard());
        StartCoroutine(StartFeedingSequence());
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    private IEnumerator StartFeedingSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (birdAnimator != null)
        {
            birdAnimator.SetTrigger("Dialogue1");
            PlayAudioClip(audio1);
            subtitleManager.DisplaySubtitle("the Animals are Hungry.", "Kiki", audio1);
        }

        StartCoroutine(FeedNextAnimal());
    }

    private IEnumerator FeedNextAnimal()
    {
        yield return new WaitForSeconds(2f);

        if (currentAnimalIndex >= animals.Length)
        {
            if (birdAnimator != null)
            {
                birdAnimator.SetTrigger("finalDialogue");
                PlayAudioClip(audio10);
                subtitleManager.DisplaySubtitle("The zoo was so much Fun.", "Kiki", audio10);
            }
            yield break;
        }

        for (int i = 0; i < animals.Length; i++)
        {
            if (animals[i] != null)
                animals[i].gameObject.SetActive(i == currentAnimalIndex);
        }

        Transform currentAnimal = animals[currentAnimalIndex];
        string animalName = currentAnimal.name;
        Transform rightFood = currentAnimal.Find("RightFood");
        Transform wrongFood = currentAnimal.Find("WrongFood");
        Transform dropTarget = currentAnimal.Find("DropTarget");

        SpriteRenderer animalSprite = currentAnimal.GetComponent<SpriteRenderer>();
        if (animalSprite != null)
            yield return StartCoroutine(FadeInSprite(animalSprite, 1f, 1f));

        SetBoardSprite(animalName);

        if (birdAnimator != null)
            birdAnimator.SetTrigger(animalName);
        PlayAudioBasedOnAnimal(animalName);
        yield return new WaitForSeconds(2f);

        SpriteRenderer rightFoodSprite = rightFood.GetComponent<SpriteRenderer>();
        SpriteRenderer wrongFoodSprite = wrongFood.GetComponent<SpriteRenderer>();

        Coroutine rightFoodFade = null;
        Coroutine wrongFoodFade = null;

        if (rightFoodSprite != null)
            rightFoodFade = StartCoroutine(FadeInSprite(rightFoodSprite, 1f, 0.5f));
        if (wrongFoodSprite != null)
            wrongFoodFade = StartCoroutine(FadeInSprite(wrongFoodSprite, 1f, 0.5f));

        if (rightFoodFade != null)
            yield return rightFoodFade;
        if (wrongFoodFade != null)
            yield return wrongFoodFade;
        if (rightFood != null)
            SpawnAndAnimateGlow(rightFood.position, rightFood);
        if (wrongFood != null)
            SpawnAndAnimateGlow(wrongFood.position, wrongFood);

        // Wait for glow animations to complete before enabling colliders
        yield return new WaitForSeconds(2.5f);

        rightFood.GetComponent<Collider2D>().enabled = true;
        wrongFood.GetComponent<Collider2D>().enabled = true;
        helperhand.StartTimer(rightFood.position, dropTarget.position);
    }

    private void SetBoardSprite(string animalName)
    {
        SpriteRenderer boardSpriteRenderer = board.GetComponent<SpriteRenderer>();
        if (boardSpriteRenderer == null) return;

        switch (animalName)
        {
            case "Hippo":
                boardSpriteRenderer.sprite = sprite1;
                break;
            case "Lion":
                boardSpriteRenderer.sprite = sprite2;
                break;
            case "Monkey":
                boardSpriteRenderer.sprite = sprite3;
                break;
            case "Croc":
                boardSpriteRenderer.sprite = sprite4;
                break;
            case "Panda":
                boardSpriteRenderer.sprite = sprite5;
                break;
            case "Tiger":
                boardSpriteRenderer.sprite = sprite6;
                break;
            default:
                return;
        }

        StartCoroutine(FadeInSprite(boardSpriteRenderer, 1f, 0.5f));
    }

    private IEnumerator HideBoard()
    {
        SpriteRenderer boardSpriteRenderer = board.GetComponent<SpriteRenderer>();
        if (boardSpriteRenderer != null)
            yield return StartCoroutine(FadeOutSprite(boardSpriteRenderer, 0f, 0.5f));
    }

    private IEnumerator FadeInSprite(SpriteRenderer spriteRenderer, float targetAlpha, float duration)
    {
        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float time = 0;

       /* // Locate the "Glow 2" child explicitly
        Transform glowChild = spriteRenderer.transform.Find("Glow 2");
        if (glowChild != null)
        {
            // Scale the "Glow 2" child to 6
            LeanTween.scale(glowChild.gameObject, Vector3.one * 6, duration).setEaseOutBack();
        }*/

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        spriteRenderer.color = new Color(color.r, color.g, color.b, targetAlpha);
    }

    private IEnumerator FadeOutSprite(SpriteRenderer spriteRenderer, float targetAlpha, float duration)
    {
        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float time = 0;

       /* // Locate the "Glow 2" child explicitly
        Transform glowChild = spriteRenderer.transform.Find("Glow 2");
        if (glowChild != null)
        {
            // Scale the "Glow 2" child to 0
            LeanTween.scale(glowChild.gameObject, Vector3.zero, duration).setEaseInBack();
        }*/

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        spriteRenderer.color = new Color(color.r, color.g, color.b, targetAlpha);
    }




    public void OnFoodDropped(Transform food, bool isRightFood)
    {
        helperhand.ResetTimer();

        if (isRightFood)
            StartCoroutine(HandleCorrectFood(food));
        else
            StartCoroutine(HandleIncorrectFood(food));
    }

    private IEnumerator HandleCorrectFood(Transform food)
    {
        Transform rightFood = animals[currentAnimalIndex].Find("RightFood");
        Transform wrongFood = animals[currentAnimalIndex].Find("WrongFood");

        if (rightFood != null)
            rightFood.GetComponent<Collider2D>().enabled = false;
        if (wrongFood != null)
            wrongFood.GetComponent<Collider2D>().enabled = false;      

        Animator animator = animals[currentAnimalIndex].GetComponent<Animator>();
        animator.SetTrigger("eat");
        if (SfxAudioSource != null)
        {
            SfxAudioSource.loop = false;
            SfxAudioSource.PlayOneShot(SfxAudio1);
        }

        yield return new WaitForSeconds(1f);

        if (birdAnimator != null)
            birdAnimator.SetTrigger("rightFood");
        PlayAudioClip(audio8);

        LeanTween.scale(food.gameObject, Vector3.zero, 0.5f);

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        LeanTween.alpha(animals[currentAnimalIndex].gameObject, 0f, 1f);

        if (wrongFood != null && wrongFood.gameObject.activeSelf)
            LeanTween.alpha(wrongFood.gameObject, 0f, 1f);

        yield return new WaitForSeconds(1f);

        animalsFed++;
        animals[currentAnimalIndex].gameObject.SetActive(false);
        currentAnimalIndex++;

        if (rightFood != null)
            rightFood.GetComponent<Collider2D>().enabled = true;
        if (wrongFood != null && wrongFood.gameObject != null)
            wrongFood.GetComponent<Collider2D>().enabled = true;

        StartCoroutine(HideBoard());
        StartCoroutine(FeedNextAnimal());
    }

    private IEnumerator HandleIncorrectFood(Transform food)
    {
        Transform rightFood = animals[currentAnimalIndex].Find("RightFood");
        Transform wrongFood = animals[currentAnimalIndex].Find("WrongFood");

        if (rightFood != null)
            rightFood.GetComponent<Collider2D>().enabled = false;
        if (wrongFood != null)
            wrongFood.GetComponent<Collider2D>().enabled = false;

        if (birdAnimator != null)
            birdAnimator.SetTrigger("wrongFood");
        PlayAudioClip(audio9);

        SpriteRenderer sr = food.GetComponent<SpriteRenderer>();
        for (int i = 0; i < 2; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        Destroy(food.gameObject);

        if (rightFood != null)
            rightFood.GetComponent<Collider2D>().enabled = true;
        if (wrongFood != null && wrongFood.gameObject != null)
            wrongFood.GetComponent<Collider2D>().enabled = true;

        
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void PlayAudioBasedOnAnimal(string animalName)
    { 
        switch (animalName)
        {
            case "Hippo":
                PlayAudioClip(audio2);
                subtitleManager.DisplaySubtitle("Let's feed the hippo.", "Kiki", audio2);
                break;
            case "Lion":
                PlayAudioClip(audio3);
                subtitleManager.DisplaySubtitle("Let's feed the lion.", "Kiki", audio3);
                break;
            case "Monkey":
                PlayAudioClip(audio4);
                subtitleManager.DisplaySubtitle("Let's feed the monkey.", "Kiki", audio4);
                break;
            case "Croc":
                PlayAudioClip(audio5);
                subtitleManager.DisplaySubtitle("Let's feed the crocodile.", "Kiki", audio5);
                break;
            case "Panda":
                PlayAudioClip(audio6);
                subtitleManager.DisplaySubtitle("Let's feed the panda", "Kiki", audio6);
                break;
            case "Tiger":
                PlayAudioClip(audio7);
                subtitleManager.DisplaySubtitle("Let's feed the tiger", "Kiki", audio7);
                break;
        }
    }
    private void SpawnAndAnimateGlow(Vector3 position, Transform parent)
    {
        if (glowPrefab2 == null)
        {
            Debug.LogError("Glow prefab is not assigned!");
            return;
        }

        // Instantiate glow prefab and set it as a child of the specified parent
        GameObject glow = Instantiate(glowPrefab2, position, Quaternion.identity, parent);
        glow.transform.localScale = Vector3.zero;

        // Tween the scale of the glow prefab
        LeanTween.scale(glow, Vector3.one * 10f, 0.5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() =>
        {
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
}
