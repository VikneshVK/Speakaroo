using System.Collections;
using TMPro;
using UnityEngine;

public class Lvl5Sc3FeedingManager : MonoBehaviour
{
    public int animalsFed = 0;
    public GameObject birdImage;
    public GameObject board;
    public TextMeshProUGUI subtitleText;
    private int currentAnimalIndex = 0;
    private Transform[] animals;
    private Animator birdAnimator;
    private AudioSource audioSource;

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
            StartCoroutine(RevealTextWordByWord("the Animals are Hungry", 0.5f));
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
                StartCoroutine(RevealTextWordByWord("The zoo was so much Fun", 0.5f));
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

        rightFood.GetComponent<Collider2D>().enabled = true;
        wrongFood.GetComponent<Collider2D>().enabled = true;
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

        // Locate the "Glow 2" child explicitly
        Transform glowChild = spriteRenderer.transform.Find("Glow 2");
        if (glowChild != null)
        {
            // Scale the "Glow 2" child to 6
            LeanTween.scale(glowChild.gameObject, Vector3.one * 6, duration).setEaseOutBack();
        }

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

        // Locate the "Glow 2" child explicitly
        Transform glowChild = spriteRenderer.transform.Find("Glow 2");
        if (glowChild != null)
        {
            // Scale the "Glow 2" child to 0
            LeanTween.scale(glowChild.gameObject, Vector3.zero, duration).setEaseInBack();
        }

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
        StartCoroutine(RevealTextWordByWord("Yumm..!", 0.5f));

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
        StartCoroutine(RevealTextWordByWord("Yuck..!", 0.5f));

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

    private void PlayAudioBasedOnAnimal(string animalName)
    {
        switch (animalName)
        {
            case "Hippo":
                PlayAudioClip(audio2);
                StartCoroutine(RevealTextWordByWord("Lets Feed the Hippo", 0.5f));
                break;
            case "Lion":
                PlayAudioClip(audio3);
                StartCoroutine(RevealTextWordByWord("Lets Feed the Lion", 0.5f));
                break;
            case "Monkey":
                PlayAudioClip(audio4);
                StartCoroutine(RevealTextWordByWord("Lets Feed the Monkey", 0.5f));
                break;
            case "Croc":
                PlayAudioClip(audio5);
                StartCoroutine(RevealTextWordByWord("Lets Feed the Crocodile", 0.5f));
                break;
            case "Panda":
                PlayAudioClip(audio6);
                StartCoroutine(RevealTextWordByWord("Lets Feed the Panda", 0.5f));
                break;
            case "Tiger":
                PlayAudioClip(audio7);
                StartCoroutine(RevealTextWordByWord("Lets Feed the Tiger", 0.5f));
                break;
        }
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
