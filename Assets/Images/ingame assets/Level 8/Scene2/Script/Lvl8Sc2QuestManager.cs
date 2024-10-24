using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lvl8Sc2QuestManager : MonoBehaviour
{
    // Variable to track the colours found (0 or 1)
    public int ColoursFound = 0;

    // References to the test tube game objects
    [SerializeField] private SpriteRenderer testtube1;
    [SerializeField] private SpriteRenderer testtube2;
    [SerializeField] private SpriteRenderer testtube3;
    [SerializeField] private SpriteRenderer testtube4;

    // Sprites for when ColoursFound is 0
    [SerializeField] private Sprite Sprite1;
    [SerializeField] private Sprite Sprite2;
    [SerializeField] private Sprite Sprite3;
    [SerializeField] private Sprite Sprite4;

    // Sprites for when ColoursFound is 1
    [SerializeField] private Sprite Sprite5;
    [SerializeField] private Sprite Sprite6;
    [SerializeField] private Sprite Sprite7;
    [SerializeField] private Sprite Sprite8;

    // Start is called before the first frame update
    void Start()
    {
        UpdateTestTubeSprites();
    }

    // Update the sprites of the test tubes based on ColoursFound
    public void UpdateTestTubeSprites()
    {
        if (ColoursFound == 0)
        {
            testtube1.sprite = Sprite1;
            testtube2.sprite = Sprite2;
            testtube3.sprite = Sprite3;
            testtube4.sprite = Sprite4;
        }
        else if (ColoursFound == 1)
        {
            testtube1.sprite = Sprite5;
            testtube2.sprite = Sprite6;
            testtube3.sprite = Sprite7;
            testtube4.sprite = Sprite8;
        }
    }

    // This function can be called by other scripts when ColoursFound changes
    public void SetColoursFound(int newColoursFound)
    {
        ColoursFound = newColoursFound;
        UpdateTestTubeSprites();
    }
}
