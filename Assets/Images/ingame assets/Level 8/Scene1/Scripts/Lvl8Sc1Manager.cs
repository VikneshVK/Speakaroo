using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Lvl8Sc1Manager : MonoBehaviour
{
    // References to the animators
    public Animator teacherAnimator;
    public Animator npcAnimator;
    public Animator jojoAnimator;
    public Animator kikiAnimator;

    // References for audio
    public AudioSource audioSource;
    public AudioSource kikiAudioSource;
    public AudioSource WritingAudioSource;
    public AudioClip npcTalkAudio;
    public AudioClip jojoAudio1;
    public AudioClip jojoAudio2;
    public AudioClip jojoAudio3;
    public AudioClip kikiAudio1;
    public AudioClip kikiAudio2;
    public AudioClip teacherAudio1;
    public AudioClip teacherAudio2;
    public TextMeshProUGUI subtitleText;

    // Prefabs and spawn locations
    public GameObject prefabToSpawn;
    public GameObject prefabToSpawn2;
    public GameObject pencilPrefab;
    public Transform prefabSpawnLocation;
    public Transform pencilSpawnLocation;
    public Transform pencilPosition2;

    // Public variables
    public bool jojoCanTalk ;
    public bool jojoCanAsk ;

    // Internal state tracking
    private bool npcTalkCompleted;
    private bool kikiTalkCompleted;
    private bool jojoTalkTriggered ;
    private bool jojoTalkCompleted;
    private bool teacherTalkTriggered;
    private bool teacherTalkCompleted;
    private bool teacherTurnCompleted;
    private bool jojoDontHavePencilTriggered;
    private bool jojoDontHavePencilCompleted;
    private bool kikiDialogue3Triggered;
    private bool kikiDialogue3Completed;
    private bool npcTurnTriggered;
    private bool npcTurnCompleted;
    private bool pencilAnimationCompleted;
    private bool writingAudioPlay;

    void Start()
    {
        // Start the scene
        StartScene();
        ResetBooleans();        
    }

    private void StartScene()
    {
        StartCoroutine(TeacherTalk());
    }

    private IEnumerator TeacherTalk()
    {
        yield return new WaitForSeconds(0.5f); // Optional delay for better pacing
        teacherAnimator.SetTrigger("Talk");
        PlayAudio(teacherAudio1);
        StartCoroutine(RevealTextWordByWord("Good Morning Class", 0.5f));
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (!teacherTalkCompleted && IsAnimationComplete(teacherAnimator, "Talk"))
        {
            teacherTalkCompleted = true;
            StartCoroutine(NpcTalk());
        }

        // Check if NPC talk animation is complete
        if (teacherTalkCompleted && !npcTalkCompleted && IsAnimationComplete(npcAnimator, "Talk"))
        {
            npcTalkCompleted = true;
            TriggerKikiTalk();
        }

        // Check if Kiki talk animation is complete
        if (npcTalkCompleted && !kikiTalkCompleted && IsAnimationComplete(kikiAnimator, "Talk"))
        {
            kikiTalkCompleted = true;
            SpawnPrefab();
        }

        // Check if Jojo can talk and trigger Jojo talk with a delay
        if (!jojoTalkTriggered && jojoCanTalk)
        {
            jojoTalkTriggered = true;
            Invoke(nameof(TriggerJojoTalk), 1.0f); // Delay Jojo's talk by 1 second
        }

        // Check if Jojo's talk animation is complete
        if (jojoTalkTriggered && !jojoTalkCompleted && IsAnimationComplete(kikiAnimator, "hello teacher kiki"))
        {
            jojoTalkCompleted = true;
            TriggerTeacherLearn();
        }

        // Check if Teacher's turn animation is complete
        if (teacherTalkCompleted && !teacherTurnCompleted && IsAnimationComplete(teacherAnimator, "Turn"))
        {
            teacherTurnCompleted = true;
            Invoke(nameof(TriggerJojoDontHavePencil), 0.5f); // Delay Jojo's next animation by 0.5 seconds
        }

        // Check if Jojo's "Don't Have Pencil" animation is complete
        if (jojoDontHavePencilTriggered && !jojoDontHavePencilCompleted && IsAnimationComplete(jojoAnimator, "DontHavePencil"))
        {
            jojoDontHavePencilCompleted = true;
            if (!writingAudioPlay)
            {
                writingAudioPlay = true;
                WritingAudioSource.Play();
            }           
            TriggerKikiDialogue3();
        }

        // Check if Kiki's "Dialogue 3" animation is complete
        if (kikiDialogue3Triggered && !kikiDialogue3Completed && IsAnimationComplete(kikiAnimator, "Dialogue3"))
        {
            kikiDialogue3Completed = true;
            SpawnPrefab2();
        }

        // Check if Jojo can ask and trigger NPC turn
        if (!npcTurnTriggered && jojoCanAsk)
        {
            npcTurnTriggered = true;
            Invoke(nameof(TriggerNpcTurn), 0.5f); // Delay NPC turn by 0.5 seconds
        }

        // Check if NPC turn animation is complete
        if (npcTurnTriggered && !npcTurnCompleted && IsAnimationComplete(npcAnimator, "Turn"))
        {
            npcTurnCompleted = true;
            AnimatePencil();
        }
    }
    private IEnumerator NpcTalk()
    {
        yield return new WaitForSeconds(0.5f); // Optional delay for better pacing
        npcAnimator.SetTrigger("Talk");
        PlayAudio(npcTalkAudio);
        StartCoroutine(RevealTextWordByWord("Hello Teacher", 0.5f));
    }
    private void TriggerKikiTalk()
    {
        kikiAnimator.SetTrigger("Talk");
        PlayAudio(kikiAudio1);
        StartCoroutine(RevealTextWordByWord("Let's greet the Teacher ", 0.5f));
    }

    private void TriggerJojoTalk()
    {
        StartCoroutine(TriggerTalkSequence());
    }

    private IEnumerator TriggerTalkSequence()
    {
        
        StartCoroutine(RevealTextWordByWord("Hello Teacher", 0.5f));
        
        if (jojoAudio1 != null && audioSource != null)
        {
            audioSource.clip = jojoAudio1;
            audioSource.Play();
        }

        jojoAnimator.SetTrigger("Talk");

        yield return new WaitForSeconds(1f);

        kikiAnimator.SetTrigger("talk2");
        if (kikiAudioSource != null)
        {
            kikiAudioSource.Play();
        }
    }

    private void TriggerTeacherTalk()
    {
        teacherAnimator.SetTrigger("Talk");
        PlayAudio(teacherAudio1);
        StartCoroutine(RevealTextWordByWord("Hello Class", 0.5f));
        teacherTalkTriggered = true;
    }

    private void TriggerTeacherLearn()
    {
        
        teacherAnimator.SetTrigger("Learn");
        PlayAudio(teacherAudio2);
        StartCoroutine(RevealTextWordByWord("Let's learn Alphabets", 0.5f));
    }

    private void TriggerJojoDontHavePencil()
    {
        jojoAnimator.SetTrigger("Talk2"); // Transition to "Don't Have Pencil"
        if (jojoAudio2 != null && audioSource != null)
        {
            audioSource.clip = jojoAudio2;
            audioSource.Play();
            StartCoroutine(RevealTextWordByWord("Uh Oh..! I don't have a Pencil to Write", 0.5f));
        }
        jojoDontHavePencilTriggered = true;
    }

    private void TriggerKikiDialogue3()
    {
        kikiAnimator.SetTrigger("Talk3"); // Transition to "Dialogue 3"
        if (kikiAudio2 != null && kikiAudioSource != null)
        {
            kikiAudioSource.clip = kikiAudio2;
            kikiAudioSource.Play();
            StartCoroutine(RevealTextWordByWord("Don't worry Jojo, let's borrow a pencil from our Friend", 0.5f));
        }
        kikiDialogue3Triggered = true;
    }

    private void TriggerNpcTurn()
    {
        npcAnimator.SetTrigger("Turn");
    }

    private void AnimatePencil()
    {
        if (pencilPrefab != null && pencilSpawnLocation != null && pencilPosition2 != null)
        {
            GameObject pencil = Instantiate(pencilPrefab, pencilSpawnLocation.position, Quaternion.identity);

            // Tween scale to 1
            LeanTween.scale(pencil, Vector3.one * 0.2f, 0.5f).setOnComplete(() =>
            {
                // Tween position to position 2
                LeanTween.move(pencil, pencilPosition2.position, 1.0f).setOnComplete(() =>
                {
                    // Tween scale to 0
                    LeanTween.scale(pencil, Vector3.zero, 0.5f).setOnComplete(() =>
                    {

                        jojoAnimator.SetTrigger("Talk3");
                        PlayAudio(jojoAudio3);
                        StartCoroutine(RevealTextWordByWord("Thank You Friend", 0.5f));
                        StartCoroutine(CheckJojoTalk3Complete());
                    });
                });
            });
        }
    }

    private IEnumerator CheckJojoTalk3Complete()
    {
        while (!IsAnimationComplete(jojoAnimator, "Talk3"))
        {
            yield return null;
        }

        // Trigger NPC smile
        npcAnimator.SetTrigger("Smile");
    }

    private void SpawnPrefab()
    {
        if (prefabToSpawn != null && prefabSpawnLocation != null)
        {
            Instantiate(prefabToSpawn, prefabSpawnLocation.position, Quaternion.identity);
        }
    }

    private void SpawnPrefab2()
    {
        if (prefabToSpawn2 != null && prefabSpawnLocation != null)
        {
            Instantiate(prefabToSpawn2, prefabSpawnLocation.position, Quaternion.identity);
        }
    }

    private bool IsAnimationComplete(Animator animator, string animationName)
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        return currentState.IsName(animationName) && currentState.normalizedTime >= 0.95f;
    }

    private void ResetBooleans()
    {
        jojoCanTalk = false;
        jojoCanAsk = false;
        npcTalkCompleted = false;
        kikiTalkCompleted = false;
        jojoTalkTriggered = false;
        jojoTalkCompleted = false;
        teacherTalkTriggered = false;
        teacherTalkCompleted = false;
        teacherTurnCompleted = false;
        jojoDontHavePencilTriggered = false;
        jojoDontHavePencilCompleted = false;
        kikiDialogue3Triggered = false;
        kikiDialogue3Completed = false;
        npcTurnTriggered = false;
        npcTurnCompleted = false;
        pencilAnimationCompleted = false;
        writingAudioPlay = false;
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
