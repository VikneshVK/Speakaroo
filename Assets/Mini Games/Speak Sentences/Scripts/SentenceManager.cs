using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SentenceManager : MonoBehaviour
{

    [SerializeField] private GameObject[] _objectToHide;
    // Start is called before the first frame update

    [Header("Scenes to Load")]
    [SerializeField] private SceneField persistentGame;
    [SerializeField] private SceneField levelScene;

    public Sprite[] phaseSprites;  // Array to hold the sprites for each phase
    public AudioClip[] phaseAudioClips;  // Array to hold the audio clips for each phase
    public AnimationClip finalPhaseAnimation;  // Animation clip for the final phase
    public string[] phaseTexts;
    public TextMeshProUGUI PhraseTxt;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Animator animator;
    private int currentPhase = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        ShowCurrentPhase();
        UpdateText(phaseTexts[currentPhase]);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Detect a tap or click
        {
            NextPhase();
        }
    }

    private void NextPhase()
    {
        if (currentPhase < phaseSprites.Length - 1)
        {
            currentPhase++;
            ShowCurrentPhase();
        }
        else
        {
            PlayFinalPhase();
        }
    }

    private void ShowCurrentPhase()
    {
        spriteRenderer.sprite = phaseSprites[currentPhase];
        audioSource.clip = phaseAudioClips[currentPhase];
        audioSource.Play();
        UpdateText(phaseTexts[currentPhase]);
    }

    private void PlayFinalPhase()
    {
        animator.Play(finalPhaseAnimation.name);
        audioSource.clip = phaseAudioClips[currentPhase];
        audioSource.Play();
        UpdateText(phaseTexts[currentPhase]);
    }

    private void UpdateText(string text)
    {
         if (PhraseTxt != null)
        {
            PhraseTxt.text = text;
        }
    }
}

