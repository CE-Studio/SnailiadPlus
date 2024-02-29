using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    public bool isShuffling = false;
    private int randoPhase = 0; // 1 = initiate item shuffle, 2 = items, 3 = music, 4 = dialogue

    public void StartGeneration()
    {
        isShuffling = true;
        randoPhase = 1;
        StartCoroutine(GenerateWorld());
    }

    public IEnumerator GenerateWorld()
    {
        float currentSeed = PlayState.currentRando.seed * 0.00000001f;

        if (isShuffling)
        {
            switch (randoPhase)
            {
                default:
                case 1: // Initiate item shuffle
                    break;
                case 2: // Items
                    break;
                case 3: // Music
                    break;
                case 4: // Dialogue
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
