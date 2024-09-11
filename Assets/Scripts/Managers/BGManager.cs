using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGManager : MonoBehaviour
{
    private static BGManager instance;
    private AudioSource audioSource;
    public AudioClip homeAndLevelSelectClip; // Clip for Home and LevelSelect
    public AudioClip otherLevelsClip; // Clip for other levels
    private string currentSceneName = "";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = 0.20f;  // Set volume to 75%
            audioSource.playOnAwake = false;  // Ensure it doesn't auto-play

            SceneManager.sceneLoaded += OnSceneLoaded;

            // Load the audio for the starting scene
            PlayAudioForCurrentScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Play appropriate audio based on the newly loaded scene
        PlayAudioForCurrentScene(scene.name);
    }

    void PlayAudioForCurrentScene(string sceneName)
    {
        AudioClip clipToPlay = null;

        // Check if we are on the Home or LevelSelect scene
        if (sceneName == "Home_Scene" || sceneName == "LevelSelect")
        {
            if (homeAndLevelSelectClip == null)
            {
                homeAndLevelSelectClip = Resources.Load<AudioClip>("BGAudio/Audio1");
            }
            clipToPlay = homeAndLevelSelectClip;
        }
        else
        {
            if (otherLevelsClip == null)
            {
                otherLevelsClip = Resources.Load<AudioClip>("BGAudio/Audio2");
            }
            clipToPlay = otherLevelsClip;
        }

        // Check if we are switching to a new audio clip
        if (audioSource.clip != clipToPlay)
        {
            // Assign the new clip
            audioSource.clip = clipToPlay;

            // Play from the start of the new clip
            audioSource.time = 0f;

            // Play the new audio
            audioSource.Play();
        }
        else
        {
            // Continue playing from where it left off (no need to reset time)
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
