using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    public bool isShuffling = false;
    private int randoPhase = 0; // 1 = initiate item shuffle, 2 = items, 3 = music, 4 = dialogue

    private enum Locks
    {
        BlueDoor,
        PinkDoor,
        RedDoor,
        GreeDoor,
        L1Blocks,
        L2Blocks,
        L3Blocks,
        Jump,
        Ice,
        Fly,
        Metal,
        Health,
        Shock,
        Snaily,
        Sluggy,
        Upside,
        Leggy,
        Blobby,
        Leechy,
        Knowledge
    };
    private bool[] lockStates = new bool[] { };

    public void StartGeneration()
    {
        isShuffling = true;
        randoPhase = 1;
        
        lockStates = new bool[System.Enum.GetValues(typeof(Locks)).Length];
        if (PlayState.currentRando.broomStart)
            lockStates[(int)Locks.BlueDoor] = true;
        lockStates[(int)(PlayState.currentProfile.character switch
        {
            "Sluggy" => Locks.Sluggy,
            "Upside" => Locks.Sluggy,
            "Leggy" => Locks.Sluggy,
            "Blobby" => Locks.Sluggy,
            "Leechy" => Locks.Sluggy,
            _ => Locks.Snaily
        })] = true;
        if (PlayState.currentRando.randoLevel == 3)
            lockStates[(int)Locks.Knowledge] = true;

        StartCoroutine(GenerateWorld());
    }

    public IEnumerator GenerateWorld()
    {
        float currentSeed = PlayState.currentRando.seed * 0.00000001f;
        int[] locations = new int[PlayState.baseItemLocations.Count];
        List<int> itemsToAdd = new List<int>();
        int progWeapons = 0;
        int progMods = 0;
        int progShells = 0;
        int placedHelixes = 0;
        int placedHearts = 0;

        if (isShuffling)
        {
            switch (randoPhase)
            {
                default:
                case 1: // Initiate item shuffle
                    for (int i = 0; i < locations.Length; i++)
                        locations[i] = -2;
                    progWeapons = 0;
                    progMods = 0;
                    progShells = 0;
                    placedHelixes = 0;
                    placedHearts = 0;
                    itemsToAdd = new List<int>
                    {
                        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
                        30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48
                    };
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
