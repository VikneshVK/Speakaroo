using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundMusic : MonoBehaviour
{
    // Start is called before the first frame update
    private static backgroundMusic bgMusic;

    private void Awake()
    {
        if(bgMusic == null)
        {
            bgMusic = this;
            DontDestroyOnLoad(bgMusic);
        }

        else
        {
            Destroy(gameObject);
        }
    }
}
