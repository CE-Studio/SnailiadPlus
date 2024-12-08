using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    public bool isShuffling = false;
    private int randoPhase = 0; // 1 = initiate item shuffle, 2 = items (split), 3 = items (pro/full), 4 = music, 5 = dialogue
    private int splitPhase = 0; // 1 = majors, 2 = minors
    private int itemPhase = 0; // 1 = required majors, 2 = remaining majors, 3 = hearts, 4 = fragments

    private readonly int[] majorWeights = new int[] { 4, 3, 2, 1, 3, 4, 3, 2, 1, 1, 1 };
    private readonly int[] progMajorWeights = new int[] { 1, 1, 1, 1, 3, 4, 2, 1, 1, 1, 1 };
    private const int HEART_WEIGHT = 2;
    private const int HELIX_WEIGHT = 3;
    private readonly List<int> majorLocations = new() { 13, 18, 21, 24, 30, 32, 44, 45, 48, 51 };
    private const int STEPS_PER_FRAME = 3;

    private enum Items
    {
        Peashooter,
        Boomerang,
        RainbowWave,
        Devastator,
        HighJump,
        ShellShield,
        RapidFire,
        IceShell,
        FlyShell,
        MetalShell,
        GravityShock,
        SSBoomerang,
        DebugRWave,
        Heart,
        Helix,
        Broom
    }

    //private bool[] locks = new bool[24];
    private Dictionary<string, bool> locks = new()
    {
        { "BlueDoor", false },
        { "PinkDoor", false },
        { "RedDoor", false },
        { "GreenDoor", false },
        { "L1Blocks", false },
        { "L2Blocks", false },
        { "L3Blocks", false },
        { "Jump", false },
        { "Ice", false },
        { "Fly", false },
        { "Metal", false },
        { "Health", false },
        { "Shock", false },
        { "Snaily", false },
        { "Sluggy", false },
        { "Upside", false },
        { "Leggy", false },
        { "Blobby", false },
        { "Leechy", false },
        { "Knowledge", false },
        { "Boss1", false },
        { "Boss2", false },
        { "Boss3", false },
        { "Boss4", false },
    };
    private bool[] defaultLocksThisGen = new bool[24];
    private readonly List<int> defaultMusicList = new() { -7, -6, -5, -1, 0, 1, 2, 3, 4, 5, 6 };

    private int[] locations = new int[] { };
    private bool hasPlacedDevastator = false;

    private readonly int[][][] validMajorCombos = new int[][][] // First number in each combo is that combo's weight
    {
        new int[][] { // EARLYGAME. Goal: Shellbreaker, minimum breakable weapon
            new int[] { 50, (int)Items.Boomerang },
            new int[] { 40, (int)Items.Peashooter, (int)Items.Boomerang },
            new int[] { 40, (int)Items.RainbowWave },
            new int[] { 25, (int)Items.Peashooter, (int)Items.Devastator },
            new int[] { 30, (int)Items.Broom, (int)Items.Devastator },
            new int[] { 20, (int)Items.HighJump, (int)Items.Boomerang },
            new int[] { 15, (int)Items.HighJump, (int)Items.Peashooter, (int)Items.Boomerang },
            new int[] { 12, (int)Items.HighJump, (int)Items.RainbowWave },
            new int[] {  8, (int)Items.HighJump, (int)Items.Peashooter, (int)Items.Devastator },
            new int[] { 10, (int)Items.HighJump, (int)Items.Broom, (int)Items.Devastator },
            new int[] {  8, (int)Items.FlyShell, (int)Items.Boomerang },
            new int[] {  4, (int)Items.FlyShell, (int)Items.Peashooter, (int)Items.Boomerang },
            new int[] {  4, (int)Items.FlyShell, (int)Items.RainbowWave },
            new int[] {  2, (int)Items.FlyShell, (int)Items.Peashooter, (int)Items.Devastator },
            new int[] {  6, (int)Items.FlyShell, (int)Items.Broom, (int)Items.Devastator },
            new int[] { 20, (int)Items.IceShell, (int)Items.HighJump, (int)Items.Boomerang },
            new int[] { 15, (int)Items.IceShell, (int)Items.HighJump, (int)Items.Peashooter, (int)Items.Boomerang },
            new int[] { 12, (int)Items.IceShell, (int)Items.HighJump, (int)Items.RainbowWave },
            new int[] {  8, (int)Items.IceShell, (int)Items.HighJump, (int)Items.Peashooter, (int)Items.Devastator },
            new int[] { 10, (int)Items.IceShell, (int)Items.HighJump, (int)Items.Broom, (int)Items.Devastator },
            new int[] {  8, (int)Items.IceShell, (int)Items.FlyShell, (int)Items.Boomerang },
            new int[] {  4, (int)Items.IceShell, (int)Items.FlyShell, (int)Items.Peashooter, (int)Items.Boomerang },
            new int[] {  4, (int)Items.IceShell, (int)Items.FlyShell, (int)Items.RainbowWave },
            new int[] {  2, (int)Items.IceShell, (int)Items.FlyShell, (int)Items.Peashooter, (int)Items.Devastator },
            new int[] {  6, (int)Items.IceShell, (int)Items.FlyShell, (int)Items.Broom, (int)Items.Devastator },
        },
        new int[][] { // MIDGAME. Goal: Stompy
            new int[] { 10, (int)Items.Boomerang },
            new int[] { 10, (int)Items.RainbowWave },
            new int[] {  6, (int)Items.Devastator },
            new int[] {  8, (int)Items.IceShell, (int)Items.Boomerang },
            new int[] {  7, (int)Items.IceShell, (int)Items.RainbowWave },
            new int[] {  4, (int)Items.IceShell, (int)Items.Devastator },
            new int[] {  7, (int)Items.HighJump, (int)Items.Boomerang },
            new int[] {  5, (int)Items.HighJump, (int)Items.RainbowWave },
            new int[] {  3, (int)Items.HighJump, (int)Items.Devastator },
            new int[] {  3, (int)Items.FlyShell, (int)Items.Boomerang },
            new int[] {  2, (int)Items.FlyShell, (int)Items.RainbowWave },
            new int[] {  1, (int)Items.FlyShell, (int)Items.Devastator },
        },
        new int[][] { // LATEGAME. Goal: Space Box, fly
            new int[] {  5, (int)Items.FlyShell, (int)Items.RainbowWave },
            new int[] {  3, (int)Items.FlyShell, (int)Items.Devastator },
            new int[] {  3, (int)Items.HighJump, (int)Items.FlyShell, (int)Items.RainbowWave },
            new int[] {  2, (int)Items.HighJump, (int)Items.FlyShell, (int)Items.Devastator },
        },
        new int[][] { // ENDGAME. Goal: Moon Snail, full weapons
            new int[] {  1, (int)Items.Devastator, (int)Items.FlyShell },
        }
    };

    private int[][] locationPhases = new int[][]
    {
        new int[] {  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 56 },
        new int[] { 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33 },
        new int[] { 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46 },
        new int[] { 47, 48, 49, 50, 51, 52, 53, 54, 55 }
    };

    public void StartGeneration()
    {
        isShuffling = true;
        randoPhase = 1;

        defaultLocksThisGen = new bool[24];
        if (PlayState.currentRando.broomStart)
            defaultLocksThisGen[0] = true;
        defaultLocksThisGen[PlayState.currentProfile.character switch
        {
            "Sluggy" => 14,
            "Upside" => 15,
            "Leggy" => 16,
            "Blobby" => 17,
            "Leechy" => 18,
            _ => 13
        }] = true;
        if (PlayState.currentRando.randoLevel == 3)
            defaultLocksThisGen[19] = true;
        if (PlayState.currentRando.openAreas)
        {
            defaultLocksThisGen[20] = true;
            defaultLocksThisGen[21] = true;
            defaultLocksThisGen[22] = true;
            defaultLocksThisGen[23] = true;
        }

        StartCoroutine(GenerateWorld());
    }

    public IEnumerator GenerateWorld()
    {
        Random.InitState(PlayState.currentRando.seed);
        locations = new int[PlayState.baseItemLocations.Count];
        List<int> itemsToAdd = new();
        List<int> unplacedTraps = new();
        int progWeapons = 0;
        int progMods = 0;
        int progShells = 0;
        int placedHelixes = 0;
        int placedHearts = 0;

        while (isShuffling)
        {
            for (int step = 0; step < STEPS_PER_FRAME; step++)
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
                        hasPlacedDevastator = false;
                        itemsToAdd = new();
                        randoPhase = PlayState.currentRando.randoLevel == 1 ? 2 : 3;
                        //locks = (bool[])defaultLocksThisGen.Clone();
                        string[] tempKeys = locks.Keys.ToArray();
                        foreach (string key in tempKeys)
                            locks[key] = false;

                        int[] currentMajorWeights = PlayState.currentRando.progressivesOn ? (int[])progMajorWeights.Clone() : (int[])majorWeights.Clone();
                        for (int i = 0; i < currentMajorWeights.Length; i++)
                        {
                            for (int j = 0; j < currentMajorWeights[i]; j++)
                                itemsToAdd.Add(i);
                        }
                        if (randoPhase == 3)
                        {
                            for (int i = 0; i < PlayState.MAX_HEARTS; i++)
                                for (int j = 0; j < HEART_WEIGHT; j++)
                                    itemsToAdd.Add(i + PlayState.OFFSET_HEARTS);
                            for (int i = 0; i < PlayState.MAX_FRAGMENTS - 5; i++)
                                for (int j = 0; j < HELIX_WEIGHT; j++)
                                    itemsToAdd.Add(i + PlayState.OFFSET_FRAGMENTS);
                            if (PlayState.currentRando.trapsActive)
                            {
                                int trapTypes = System.Enum.GetNames(typeof(TrapManager.TrapItems)).Length;
                                List<int> trapsToAdd = new();
                                for (int i = 0; i < trapTypes; i++)
                                    trapsToAdd.Add(1000 + i);
                                unplacedTraps = trapsToAdd;
                                int numberOfTraps = Mathf.CeilToInt(Random.value * trapTypes);
                                for (int i = 0; i < numberOfTraps; i++)
                                {
                                    int trapID = Mathf.FloorToInt(Random.value * trapsToAdd.Count);
                                    itemsToAdd.Add(trapsToAdd[trapID]);
                                    trapsToAdd.RemoveAt(trapID);
                                }
                            }
                        }
                        else
                        {
                            while (itemsToAdd.Contains(10))
                                itemsToAdd.Remove(10); // Remove Gravity Shock from the pool on Split shuffle
                            foreach (int i in new int[] { 0, 3, 23, 35, 36, 54 })
                                locations[i] = -1; // Remove super secret items, snelk rooms, and test rooms as viable locations when not on Pro shuffle
                            splitPhase = 1;
                        }
                        //Debug.Log("-------------------------------------------------");
                        break;

                    case 2: // Items

                        break;

                    case 3: // Music
                        PlayState.currentRando.musicList = defaultMusicList.ToArray();
                        if (PlayState.currentRando.musicShuffled == 0)
                        {
                            randoPhase = 4;
                            break;
                        }

                        List<int> songsToAdd;
                        if (PlayState.currentRando.musicShuffled == 1)
                        {
                            songsToAdd = new() { 0, 1, 2, 3, 4, 5 };
                            for (int i = 0; i < 6; i++)
                            {
                                int randomIndex = Mathf.FloorToInt(Random.value * songsToAdd.Count);
                                PlayState.currentRando.musicList[i + 4] = songsToAdd[randomIndex];
                                songsToAdd.RemoveAt(randomIndex);
                            }
                        }
                        else
                        {
                            songsToAdd = defaultMusicList;
                            //for (int i = 0; i < defaultMusicList.Count; i++)
                            while (songsToAdd.Count > 0) // This wasn't working as a for loop for some reason
                            {
                                int randomIndex = Mathf.FloorToInt(Random.value * songsToAdd.Count);
                                PlayState.currentRando.musicList[songsToAdd.Count - 1] = songsToAdd[randomIndex];
                                songsToAdd.RemoveAt(randomIndex);
                            }
                        }
                        randoPhase = 4;
                        break;

                    case 4: // Dialogue
                        if (!PlayState.currentRando.npcTextShuffled)
                        {
                            randoPhase = 0;
                            isShuffling = false;
                            break;
                        }

                        List<int> totalHints = new();
                        int finalHintCount = Mathf.FloorToInt(Random.value * 3) + 2; // 2-5 hints per seed
                        List<int> availableHints = new();
                        int hintCount = 5;
                        for (int i = 0; i < hintCount; i++)
                            availableHints.Add(i);
                        List<int> availableNPCs = new();
                        for (int i = 0; i < PlayState.npcCount; i++)
                            availableNPCs.Add(i);
                        for (int i = 0; i < finalHintCount; i++)
                        {
                            int itemID = -1;
                            int areaID = -1;
                            int locationID = 0;
                            while (!(itemID >= 0 && itemID <= 10)) // Only major items valid for hints
                            {
                                locationID = Mathf.FloorToInt(Random.value * PlayState.currentRando.itemLocations.Length);
                                itemID = PlayState.currentRando.itemLocations[locationID];
                            }
                            int countedUpItems = 0;
                            int potentialArea = 0;
                            while (areaID == -1) // This whole thing assumes the areas are in order. Which they should be
                            {
                                countedUpItems += PlayState.totaItemsPerArea[potentialArea];
                                if (locationID < countedUpItems)
                                    areaID = potentialArea;
                                else
                                    potentialArea++;
                            }

                            int npcIndex = Mathf.FloorToInt(Random.value * availableNPCs.Count);
                            totalHints.Add(availableNPCs[npcIndex]);
                            availableNPCs.RemoveAt(npcIndex);
                            int hintIndex = Mathf.FloorToInt(Random.value * availableHints.Count);
                            totalHints.Add(availableHints[hintIndex]);
                            availableHints.RemoveAt(hintIndex);
                            totalHints.Add(itemID);
                            totalHints.Add(areaID);
                        }
                        PlayState.currentRando.npcHintData = totalHints.ToArray();

                        List<int> newIndeces = new();
                        List<int> availableIndeces = new();
                        int flavorCount = 48;
                        for (int i = 0; i < flavorCount; i++)
                            availableIndeces.Add(i);
                        for (int i = 0; i < PlayState.npcCount; i++)
                        {
                            if (availableIndeces.Count == 0)
                                i = PlayState.npcCount;
                            else
                            {
                                int indexIndex = Mathf.FloorToInt(Random.value * availableIndeces.Count);
                                newIndeces.Add(availableIndeces[indexIndex]);
                                availableIndeces.RemoveAt(indexIndex);
                            }
                        }
                        PlayState.currentRando.npcTextIndeces = newIndeces.ToArray();
                        randoPhase = 0;
                        isShuffling = false;
                        break;

                    #region LegacyShuffle
                    case 5: // Items (Split shuffle) -- LEGACY
                        List<int> availableSplitLocations = GetLocations(splitPhase == 1);
                        if (availableSplitLocations.Count == 0 && itemsToAdd.Count > 0)
                            randoPhase = 1;
                        else if (itemsToAdd.Count > 0 && splitPhase == 1)
                        {
                            int locationPointer = Mathf.FloorToInt(Random.value * availableSplitLocations.Count);
                            int itemToPlace = itemsToAdd[Mathf.FloorToInt(Random.value * itemsToAdd.Count)];
                            if (PlayState.currentRando.progressivesOn)
                            {
                                itemToPlace = itemToPlace switch
                                {
                                    0 or 1 or 2 => progWeapons,
                                    3 or 6 => progMods == 0 ? 6 : 3,
                                    7 or 8 or 9 => 7 + progShells,
                                    _ => itemToPlace
                                };
                            }
                            TweakLocks(itemToPlace, placedHelixes);
                            while (itemsToAdd.Contains(itemToPlace))
                                itemsToAdd.Remove(itemToPlace);
                            locations[availableSplitLocations[locationPointer]] = itemToPlace;
                            switch (itemToPlace)
                            {
                                case 0: case 1: case 2: progWeapons++; break;
                                case 7: case 8: case 9: progShells++; break;
                                case 6: case 3: progMods++; break;
                                default: break;
                            }

                            if (itemsToAdd.Count == 0)
                            {
                                for (int j = 0; j < PlayState.MAX_HEARTS; j++)
                                    itemsToAdd.Add(j + PlayState.OFFSET_HEARTS);
                                for (int j = 0; j < PlayState.MAX_FRAGMENTS - 5; j++)
                                    itemsToAdd.Add(j + PlayState.OFFSET_FRAGMENTS);
                                if (PlayState.currentRando.trapsActive)
                                {
                                    int trapTypes = System.Enum.GetNames(typeof(TrapManager.TrapItems)).Length;
                                    List<int> trapsToAdd = new();
                                    for (int i = 0; i < trapTypes; i++)
                                        trapsToAdd.Add(1000 + i);
                                    unplacedTraps = trapsToAdd;
                                    int numberOfTraps = Mathf.CeilToInt(Random.value * trapTypes);
                                    for (int i = 0; i < numberOfTraps; i++)
                                    {
                                        int trapID = Mathf.FloorToInt(Random.value * trapsToAdd.Count);
                                        itemsToAdd.Add(trapsToAdd[trapID]);
                                        trapsToAdd.RemoveAt(trapID);
                                    }
                                }
                                splitPhase = 2;
                            }
                        }
                        else if (itemsToAdd.Count > 0 && splitPhase == 2)
                        {
                            int locationPointer = Mathf.FloorToInt(Random.value * availableSplitLocations.Count);
                            int itemToPlace = itemsToAdd[Mathf.FloorToInt(Random.value * itemsToAdd.Count)];
                            while (itemsToAdd.Contains(itemToPlace))
                                itemsToAdd.Remove(itemToPlace);
                            if (unplacedTraps.Contains(itemToPlace))
                                unplacedTraps.Remove(itemToPlace);
                            locations[availableSplitLocations[locationPointer]] = itemToPlace;
                            if (itemToPlace >= PlayState.OFFSET_FRAGMENTS && itemToPlace < 1000)
                                placedHelixes++;
                            else if (itemToPlace >= PlayState.OFFSET_HEARTS)
                                placedHearts++;
                            if (placedHearts >= 4)
                                locks["Health"] = true;
                        }
                        else
                        {
                            while (placedHelixes < PlayState.MAX_FRAGMENTS)
                            {
                                List<int> remainingLocations = GetLocations();
                                if (remainingLocations.Count == 0)
                                    placedHelixes = PlayState.MAX_FRAGMENTS;
                                else
                                {
                                    int locationID = Mathf.FloorToInt(Random.value * remainingLocations.Count);
                                    locations[remainingLocations[locationID]] = PlayState.OFFSET_FRAGMENTS + placedHelixes;
                                    placedHelixes++;
                                }
                            }
                            while (unplacedTraps.Count > 0)
                            {
                                List<int> remainingLocations = GetLocations();
                                if (remainingLocations.Count == 0)
                                    unplacedTraps.Clear();
                                else
                                {
                                    int locationID = Mathf.FloorToInt(Random.value * remainingLocations.Count);
                                    int trapID = Mathf.FloorToInt(Random.value * unplacedTraps.Count);
                                    locations[remainingLocations[locationID]] = unplacedTraps[trapID];
                                    unplacedTraps.RemoveAt(trapID);
                                }
                            }
                            for (int i = 0; i < locations.Length; i++)
                                if (locations[i] == -2)
                                    locations[i] = -1;
                            PlayState.currentRando.itemLocations = (int[])locations.Clone();
                            PlayState.currentRando.trapLocations = new int[System.Enum.GetNames(typeof(TrapManager.TrapItems)).Length];
                            randoPhase = 4;
                        }
                        break;

                    case 6: // Items (Full/Pro shuffle) -- LEGACY
                        List<int> availableLocations = GetLocations();
                        if (availableLocations.Count == 0 && itemsToAdd.Count > 0)
                            randoPhase = 1;
                        else if (itemsToAdd.Count > 0)
                        {
                            int locationPointer = Mathf.FloorToInt(Random.value * availableLocations.Count);
                            int itemToPlace = itemsToAdd[Mathf.FloorToInt(Random.value * itemsToAdd.Count)];
                            if (PlayState.currentRando.progressivesOn)
                            {
                                //int originalItem = itemToPlace;
                                itemToPlace = itemToPlace switch
                                {
                                    0 or 1 or 2 => progWeapons,
                                    3 or 6 => progMods == 0 ? 6 : 3,
                                    7 or 8 or 9 => 7 + progShells,
                                    _ => itemToPlace
                                };
                                //Debug.Log(string.Format("{0} => {1}", originalItem, itemToPlace));
                            }
                            TweakLocks(itemToPlace, placedHelixes);
                            while (itemsToAdd.Contains(itemToPlace))
                                itemsToAdd.Remove(itemToPlace);
                            if (unplacedTraps.Contains(itemToPlace))
                                unplacedTraps.Remove(itemToPlace);
                            locations[availableLocations[locationPointer]] = itemToPlace;
                            if (itemToPlace >= PlayState.OFFSET_FRAGMENTS && itemToPlace < 1000)
                                placedHelixes++;
                            else if (itemToPlace >= PlayState.OFFSET_HEARTS)
                                placedHearts++;
                            if (placedHearts >= 4)
                                locks["Health"] = true;
                            switch (itemToPlace)
                            {
                                case 0: case 1: case 2: progWeapons++; break;
                                case 7: case 8: case 9: progShells++; break;
                                case 6: case 3: progMods++; break;
                                default: break;
                            }
                            //PrintPlacement(itemToPlace, availableLocations[locationPointer]);
                        }
                        else
                        {
                            while (placedHelixes < PlayState.MAX_FRAGMENTS)
                            {
                                List<int> remainingLocations = GetLocations();
                                if (remainingLocations.Count == 0)
                                    placedHelixes = PlayState.MAX_FRAGMENTS;
                                else
                                {
                                    int locationID = Mathf.FloorToInt(Random.value * remainingLocations.Count);
                                    locations[remainingLocations[locationID]] = PlayState.OFFSET_FRAGMENTS + placedHelixes;
                                    placedHelixes++;
                                }
                            }
                            while (unplacedTraps.Count > 0)
                            {
                                List<int> remainingLocations = GetLocations();
                                if (remainingLocations.Count == 0)
                                    unplacedTraps.Clear();
                                else
                                {
                                    int locationID = Mathf.FloorToInt(Random.value * remainingLocations.Count);
                                    int trapID = Mathf.FloorToInt(Random.value * unplacedTraps.Count);
                                    locations[remainingLocations[locationID]] = unplacedTraps[trapID];
                                    unplacedTraps.RemoveAt(trapID);
                                }
                            }
                            for (int i = 0; i < locations.Length; i++)
                                if (locations[i] == -2)
                                    locations[i] = -1;
                            PlayState.currentRando.itemLocations = (int[])locations.Clone();
                            PlayState.currentRando.trapLocations = new int[System.Enum.GetNames(typeof(TrapManager.TrapItems)).Length];
                            randoPhase = 4;

                            //List<int> printedLocations = new();
                            //for (int i = 0; i < locations.Length; i++)
                            //    printedLocations.Add(locations[i]);
                            //printedLocations.Sort();
                            //string output = "";
                            //for (int i = 0; i < printedLocations.Count; i++)
                            //    output += printedLocations[i] + ", ";
                            //Debug.Log(output);
                        }
                        break;
                        #endregion
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private List<int> GetLocations(bool majorsOnly = false)
    {
        List<int> newLocations = new();

        for (int i = 0; i < PlayState.baseItemLocations.Count; i++)
        {
            if (!majorsOnly || majorLocations.Contains(i))
            {
                if (i switch
                {
                    0 => locks["Knowledge"] && locks["L2Blocks"] && (locks["Jump"] || locks["Upside"] || locks["Leggy"]),
                                                                                                                            // Original Testing Room
                    1 => locks["L1Blocks"],
                                                                                                                            // Leggy Snail's Tunnel
                    2 => locks["L1Blocks"] || (locks["Jump"] && locks["Knowledge"]) || ((locks["Sluggy"] || locks["Upside"] ||
                        locks["Leggy"] || locks["Leechy"]) && locks["Knowledge"]),                                          // Town Overtunnel
                    3 => locks["Knowledge"] && (locks["L1Blocks"] || locks["Jump"] || locks["Sluggy"] ||
                        locks["Upside"] || locks["Leggy"] || locks["Leechy"]),                                              // Super Secret Alcove
                    4 => locks["L1Blocks"] || locks["Jump"] || locks["Sluggy"] || locks["Upside"] || locks["Leggy"] || locks["Leechy"],
                                                                                                                            // Love Snail's Alcove
                    5 => locks["L2Blocks"],
                                                                                                                            // Suspicious Tree
                    6 => locks["L2Blocks"],
                                                                                                                            // Anger Management Room
                    7 => locks["L2Blocks"] && ((locks["Blobby"] && locks["Jump"]) || !locks["Blobby"]),
                                                                                                                            // Percentage Snail's Hidey Hole
                    8 => true,
                                                                                                                            // Digging Grounds
                    9 => true,
                                                                                                                            // Cave Snail's Cave
                    10 => locks["L2Blocks"],
                                                                                                                            // Fragment Cave
                    11 => (locks["Knowledge"] && ((locks["Blobby"] && locks["Jump"]) || !locks["Blobby"])) ||
                        locks["Fly"] || locks["Upside"] || locks["Leggy"],                                                  // Discombobulatory Alcove
                    12 => (locks["Blobby"] && locks["Jump"]) || !locks["Blobby"],
                                                                                                                            // Seabed Caves
                    13 => true,
                                                                                                                            // Fine Dining (Peashooter)
                    14 => locks["L2Blocks"],
                                                                                                                            // Fine Dining (Fragment)
                    15 => locks["L1Blocks"],
                                                                                                                            // The Maze Room
                    16 => locks["L1Blocks"],
                                                                                                                            // Monument of Greatness
                    17 => locks["RedDoor"],
                                                                                                                            // Heart of the Sea
                    18 => locks["BlueDoor"] && (locks["PinkDoor"] || locks["Knowledge"]),
                                                                                                                            // Daily Helping of Calcium
                    19 => locks["GreenDoor"],
                                                                                                                            // Dig, Snaily, Dig
                    20 => locks["L1Blocks"],
                                                                                                                            // Skywatcher's Loot
                    21 => locks["Boss1"],
                                                                                                                            // Signature Croissants (Boomerang)
                    22 => locks["Boss1"] && locks["L1Blocks"],
                                                                                                                            // Signature Croissants (Heart)
                    23 => locks["Boss1"] && locks["Knowledge"] && (locks["Fly"] || locks["Upside"] || locks["Leggy"]),
                                                                                                                            // Squared Snelks
                    24 => locks["Boss1"] && locks["PinkDoor"],
                                                                                                                            // Frost Shrine
                    25 => locks["Boss1"] && (locks["Ice"] || (locks["Health"] && (locks["Fly"] || locks["Leggy"]))) && locks["L1Blocks"] &&
                        ((locks["Blobby"] && locks["Jump"]) || !locks["Blobby"]),                                           // Sweater Required
                    26 => locks["Boss1"] && locks["PinkDoor"],
                                                                                                                            // A Secret to Snowbody
                    27 => locks["Boss1"] && locks["GreenDoor"],
                                                                                                                            // Devil's Alcove
                    28 => locks["Boss1"] && (locks["Knowledge"] || locks["Jump"] || locks["Fly"] || locks["Ice"] || locks["Upside"] || locks["Leggy"]),
                                                                                                                            // Ice Climb
                    29 => locks["Boss1"] && locks["L2Blocks"],
                                                                                                                            // The Labyrinth (Fragment)
                    30 => (locks["Boss1"] && locks["L2Blocks"]) || (locks["Knowledge"] && !locks["Upside"]),
                                                                                                                            // The Labyrinth (High Jump)
                    31 => locks["Boss1"] && locks["RedDoor"],
                                                                                                                            // Sneaky, Sneaky
                    32 => locks["Boss2"] || locks["RedDoor"],
                                                                                                                            // Prismatic Prize (Rainbow Wave)
                    33 => locks["Boss2"] && locks["RedDoor"],
                                                                                                                            // Prismatic Prize (Heart)
                    34 => locks["Boss2"] && (locks["Metal"] || locks["Health"]) && (locks["PinkDoor"] || locks["RedDoor"]),
                                                                                                                            // Hall of Fire
                    35 => locks["Boss2"] && locks["Knowledge"] && locks["Metal"] && (locks["Fly"] || locks["L3Blocks"] || locks["RedDoor"]),
                                                                                                                            // Scorching Snelks
                    36 => locks["Boss2"] && locks["Knowledge"] && (locks["Fly"] || locks["L3Blocks"]),
                                                                                                                            // Hidden Hideout
                    37 => locks["Boss2"] && locks["RedDoor"] || locks["L3Blocks"],
                                                                                                                            // Green Cache
                    38 => locks["Boss2"] && locks["L2Blocks"],
                                                                                                                            // Furnace
                    39 => locks["Boss2"] && locks["RedDoor"],
                                                                                                                            // Slitherine Grove
                    40 => locks["Boss2"] && locks["PinkDoor"] && (locks["Fly"] || locks["Upside"] || locks["Leggy"]),
                                                                                                                            // Floaty Fortress (Top Left)
                    41 => locks["Boss2"] && locks["PinkDoor"] && (locks["Fly"] || locks["Upside"] || locks["Leggy"]),
                                                                                                                            // Floaty Fortress (Bottom Right)
                    42 => locks["Boss2"] && locks["L2Blocks"],
                                                                                                                            // Woah Mama
                    43 => locks["Boss2"] && locks["L2Blocks"] && (locks["Jump"] || locks["Sluggy"] || locks["Upside"] || locks["Leggy"] || locks["Leechy"]),
                                                                                                                            // Shocked Shell
                    44 => locks["Boss2"] && locks["L2Blocks"],
                                                                                                                            // Gravity Shrine
                    45 => locks["Boss2"] && locks["Fly"] || (locks["Knowledge"] && locks["RedDoor"]),
                                                                                                                            // Fast Food
                    46 => locks["Boss3"] || (locks["RedDoor"] && (locks["Jump"] || locks["Sluggy"] || locks["Upside"] || locks["Leggy"] || locks["Leechy"])),
                                                                                                                            // The Bridge
                    47 => locks["Boss3"] && locks["L3Blocks"],
                                                                                                                            // Transit 90
                    48 => locks["Boss3"] && locks["RedDoor"] && (locks["Metal"] || locks["Health"]),
                                                                                                                            // Steel Shrine
                    49 => locks["Boss3"] && locks["L3Blocks"] && (locks["Metal"] || locks["Health"]),
                                                                                                                            // Space Balcony (Heart)
                    50 => locks["Boss3"] && locks["L3Blocks"] && (locks["Metal"] || locks["Health"]),
                                                                                                                            // Space Balcony (Fragment)
                    51 => locks["Boss3"] && locks["RedDoor"] && (locks["Metal"] || locks["Health"]),
                                                                                                                            // The Vault
                    52 => locks["Boss3"] && locks["RedDoor"] && (locks["Fly"] || locks["Upside"] || locks["Leggy"] || locks["Blobby"]) &&
                        (locks["Health"] || locks["Metal"]),                                                                // Holy Hideaway
                    53 => locks["Boss3"] && locks["RedDoor"] && (locks["Fly"] || locks["Upside"] || locks["Leggy"] || locks["Blobby"]) &&
                        (locks["Health"] || locks["Metal"]),                                                                // Arctic Alcove
                    54 => locks["Boss3"] && locks["Knowledge"] && locks["RedDoor"] && (locks["Fly"] || locks["Upside"] || locks["Leggy"] ||
                        locks["Blobby"]) && (locks["Health"] || locks["Metal"]),                                            // Lost Loot
                    55 => locks["Boss3"] && locks["GreenDoor"] && (locks["Fly"] || locks["Upside"] || locks["Leggy"] || locks["Blobby"]) &&
                        (locks["Health"] || locks["Metal"]),                                                                // Reinforcements
                    56 => locks["PinkDoor"],
                                                                                                                            // Glitched Goodies
                    _ => false
                })
                    if (locations[i] == -2)
                        newLocations.Add(i);
            }
        }

        return newLocations;
    }

    private void TweakLocks(int itemID, int helixCount)
    {
        switch (itemID)
        {
            case 0: // Peashooter
                locks["BlueDoor"] = true;
                if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    locks["Boss1"] = true;
                if (hasPlacedDevastator)
                {
                    locks["PinkDoor"] = true;
                    locks["RedDoor"] = true;
                    locks["GreenDoor"] = true;
                    locks["L1Blocks"] = true;
                    locks["L2Blocks"] = true;
                    locks["L3Blocks"] = true;
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    {
                        locks["Boss2"] = true;
                        locks["Boss3"] = true;
                        locks["Boss4"] = true;
                    }
                }
                break;
            case 1: // Boomerang
                locks["BlueDoor"] = true;
                locks["PinkDoor"] = true;
                locks["L1Blocks"] = true;
                if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                {
                    locks["Boss1"] = true;
                    locks["Boss2"] = true;
                }
                if (hasPlacedDevastator)
                {
                    locks["RedDoor"] = true;
                    locks["GreenDoor"] = true;
                    locks["L2Blocks"] = true;
                    locks["L3Blocks"] = true;
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    {
                        locks["Boss3"] = true;
                        locks["Boss4"] = true;
                    }
                }
                break;
            case 2: // Rainbow Wave
                locks["BlueDoor"] = true;
                locks["PinkDoor"] = true;
                locks["RedDoor"] = true;
                locks["L1Blocks"] = true;
                locks["L2Blocks"] = true;
                if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                {
                    locks["Boss1"] = true;
                    locks["Boss2"] = true;
                    locks["Boss3"] = true;
                }
                if (hasPlacedDevastator)
                {
                    locks["L3Blocks"] = true;
                    locks["GreenDoor"] = true;
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                        locks["Boss4"] = true;
                }
                break;
            case 3: // Devastator
                if (locks["BlueDoor"] || locks["PinkDoor"] || locks["RedDoor"])
                {
                    locks["BlueDoor"] = true;
                    locks["PinkDoor"] = true;
                    locks["RedDoor"] = true;
                    locks["L1Blocks"] = true;
                    locks["L2Blocks"] = true;
                    locks["L3Blocks"] = true;
                    locks["GreenDoor"] = true;
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    {
                        locks["Boss1"] = true;
                        locks["Boss2"] = true;
                        locks["Boss3"] = true;
                        locks["Boss4"] = true;
                    }
                }
                hasPlacedDevastator = true;
                break;
            case 4: // High Jump
                locks["Jump"] = true;
                break;
            case 5: // Shell Shield
                break;
            case 6: // Rapid Fire
                break;
            case 7: // Ice Snail
                locks["Ice"] = true;
                break;
            case 8: // Gravity Snail
                locks["Jump"] = true;
                locks["Fly"] = true;
                break;
            case 9: // Full Metal Snail
                locks["Metal"] = true;
                break;
            case 10: // Gravity Shock
                locks["Shock"] = true;
                break;
            default:
                if (PlayState.currentRando.bossesLocked)
                {
                    if (helixCount >= 5)
                        locks["Boss1"] = true;
                    if (helixCount >= 10)
                        locks["Boss2"] = true;
                    if (helixCount >= 15)
                        locks["Boss3"] = true;
                    if (helixCount >= 25)
                        locks["Boss4"] = true;
                }
                break;
        }
    }

    private void PrintPlacement(int itemID, int locationID)
    {
        Debug.Log(string.Format("Placed {0} at {1}",
            itemID switch {
                0 => "Peashooter",
                1 => "Boomerang",
                2 => "Rainbow Wave",
                3 => "Devastator",
                4 => "High Jump",
                5 => "Shell Shield",
                6 => "Rapid Fire",
                7 => "Ice Snail",
                8 => "Gravity Snail",
                9 => "Full Metal Snail",
                10 => "Gravity Shock",
                _ => "Nothing"
            },
            locationID switch {
                0 => "Original Testing Room",
                1 => "Leggy Snail's Tunnel",
                2 => "Town Overtunnel",
                3 => "Super Secret Alcove",
                4 => "Love Snail's Alcove",
                5 => "Suspicious Tree",
                6 => "Anger Management Room",
                7 => "Percentage Snail's Hidey Hole",
                8 => "Digging Grounds",
                9 => "Cave Snail's Cave",
                10 => "Fragment Cave",
                11 => "Discombobulatory Alcove",
                12 => "Seabed Caves",
                13 => "Fine Dining (Peashooter)",
                14 => "Fine Dining (Fragment)",
                15 => "The Maze Room",
                16 => "Monument of Greatness",
                17 => "Heart of the Sea",
                18 => "Daily Helping of Calcium",
                19 => "Dig, Snaily, Dig",
                20 => "Skywatcher's Loot",
                21 => "Signature Croissants (Boomerang)",
                22 => "Signature Croissants (Heart)",
                23 => "Squared Snelks",
                24 => "Frost Shrine",
                25 => "Sweater Required",
                26 => "A Secret to Snowbody",
                27 => "Devil's Alcove",
                28 => "Ice Climb",
                29 => "The Labyrinth (Fragment)",
                30 => "The Labyrinth (High Jump)",
                31 => "Sneaky, Sneaky",
                32 => "Prismatic Prize (Rainbow Wave)",
                33 => "Prismatic Prize (Heart)",
                34 => "Hall of Fire",
                35 => "Scorching Snelks",
                36 => "Hidden Hideout",
                37 => "Green Cache",
                38 => "Furnace",
                39 => "Slitherine Grove",
                40 => "Floaty Fortress (Top Left)",
                41 => "Floaty Fortress (Bottom Right)",
                42 => "Woah Mama",
                43 => "Shocked Shell",
                44 => "Gravity Shrine",
                45 => "Fast Food",
                46 => "The Bridge",
                47 => "Transit 90",
                48 => "Steel Shrine",
                49 => "Space Balcony (Heart)",
                50 => "Space Balcony (Fragment)",
                51 => "The Vault",
                52 => "Holy Hideaway",
                53 => "Arctic Alcove",
                54 => "Lost Loot",
                55 => "Reinforcements",
                56 => "Glitched Goodies",
                _ => "Nowhere"
            })
            );
    }

    public void CreateSpoilerMap()
    {
        Sprite mapSprite = Resources.Load<Sprite>("Images/UI/Minimap");
        Texture2D mapTexture = mapSprite.texture;
        Texture2D map = new(mapTexture.width, mapTexture.height);
        map.SetPixels32(mapTexture.GetPixels32());
        Sprite iconSprite = Resources.Load<Sprite>("Images/SpoilerLogIcons");
        Texture2D icons = iconSprite.texture;
        Vector2Int iconDimensions = new(Mathf.RoundToInt(icons.width * 0.125f), Mathf.RoundToInt(icons.height * 0.125f));

        for (int mapX = 0; mapX < PlayState.WORLD_SIZE.x; mapX++)
        {
            for (int mapY = 0; mapY < PlayState.WORLD_SIZE.y; mapY++)
            {
                int cellID = Mathf.RoundToInt((mapY * PlayState.WORLD_SIZE.x) + mapX);
                if (PlayState.itemLocations.ContainsKey(cellID))
                {
                    int itemID = PlayState.currentRando.itemLocations[PlayState.itemLocations[cellID]];
                    if (itemID != -1)
                    {
                        int iconID;
                        if (itemID >= 1000)
                            iconID = 13;
                        else if (itemID >= PlayState.OFFSET_FRAGMENTS)
                            iconID = 12;
                        else if (itemID >= PlayState.OFFSET_HEARTS)
                            iconID = 11;
                        else
                            iconID = itemID;

                        Vector2Int originPixel = new(mapX * 8, Mathf.Abs((mapY * 8) - ((int)PlayState.WORLD_SIZE.y * 8)) - 8);
                        Vector2Int iconOriginPixel = new((iconID % iconDimensions.x) * 8,
                            Mathf.Abs(Mathf.FloorToInt(iconID / iconDimensions.x) - iconDimensions.y + 1) * 8);
                        for (int iconX = 0; iconX < 8; iconX++)
                        {
                            for (int iconY = 0; iconY < 8; iconY++)
                            {
                                Color iconPixel = icons.GetPixel(iconOriginPixel.x + iconX, iconOriginPixel.y + iconY);
                                if (iconPixel.a != 0)
                                    map.SetPixel(originPixel.x + iconX, originPixel.y + iconY, iconPixel);
                            }
                        }
                    }
                }
            }
        }

        int upscaleRate = 3;
        float downscaleRate = 1f / (float)upscaleRate; // Doing this so the game only has to perform one division operation
        Texture2D upscaledMap = new(map.width * upscaleRate, map.height * upscaleRate);
        for (int upX = 0; upX < upscaledMap.width; upX++)
        {
            for (int upY = 0; upY < upscaledMap.height; upY++)
            {
                Vector2Int mapCoord = new(Mathf.FloorToInt(upX * downscaleRate), Mathf.FloorToInt(upY * downscaleRate));
                Color mapColor = map.GetPixel(mapCoord.x, mapCoord.y);
                upscaledMap.SetPixel(upX, upY, mapColor);
            }
        }

        byte[] mapBytes = upscaledMap.EncodeToPNG();
        File.WriteAllBytes(string.Format("{0}/Saves/Spoiler Map {1}.png", Application.persistentDataPath,
            PlayState.mainMenu.ParseSeed(PlayState.currentRando.seed)), mapBytes);
    }
}
