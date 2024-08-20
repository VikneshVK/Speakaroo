using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public Scene_Manager sceneManager;  
    public Button playButton;

    void Start()
    {
        
        if (playButton != null)
        {
            
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
    }

    void OnPlayButtonClicked()
    {
        
        sceneManager.LoadLevel("LevelSelect");
    }
}
