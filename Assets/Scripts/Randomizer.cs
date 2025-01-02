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
    private int itemPhase = 0; // 1 = required majors, 2 = remaining majors, 3 = hearts, 4 = fragments/traps

    private readonly int[] majorWeights = new int[] { 4, 3, 2, 1, 3, 4, 3, 2, 1, 1, 1 };
    private readonly int[] progMajorWeights = new int[] { 1, 1, 1, 1, 3, 4, 2, 1, 1, 1, 1 };
    private const int HEART_WEIGHT = 2;
    private const int HELIX_WEIGHT = 3;
    private readonly List<int> majorLocations = new() { 13, 18, 21, 24, 30, 32, 44, 45, 48, 51 };
    private const int STEPS_PER_FRAME = 2;
    private const int MINIMUM_TRAPS = 2;

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
        Broom,
        None
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
            new int[] {  1, (int)Items.Devastator },
        }
    };

    private readonly List<int>[] locationPhases = new List<int>[]
    {
        new() {  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 30, 56 },
        new() { 23, 24, 25, 26, 27, 28, 29, 31, 32, 33 },
        new() { 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46 },
        new() { 47, 48, 49, 50, 51, 52, 53, 54, 55 }
    };

    private readonly int[] sphereSplitLimits = new int[] { 3, 0, 0, 0 };

    private readonly float[] sphereHelixPercentageRequirements = new float[] { 0.6f, 0.7f, 0.8f, 0.9f };

    //private readonly List<int> proOnlyLocations = new() { 0, 3, 23, 35, 36, 54 };

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
        List<Items> majorsToAdd = new();
        List<Items> majorsPlaced = new();
        List<int> unplacedTraps = new();
        int currentSphere = 0;
        int progWeapons = 0;
        int progMods = 0;
        int progShells = 0;
        int placedHelixes = 0;
        int placedHearts = 0;
        List<int> trapsToPlace = new();
        int[] currentMajorCombo = new int[0];
        bool initializedPhase = false;

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
                        //randoPhase = PlayState.currentRando.randoLevel == 1 ? 2 : 3;
                        randoPhase = 2;
                        itemPhase = 1;
                        //locks = (bool[])defaultLocksThisGen.Clone();
                        string[] tempKeys = locks.Keys.ToArray();
                        foreach (string key in tempKeys)
                            locks[key] = false;
                        if (PlayState.currentRando.broomStart)
                        {
                            majorsPlaced.Add(Items.Broom);
                            TweakLocks(ItemEnumToID(Items.Broom), 0);
                        }
                        if (PlayState.currentRando.randoLevel == 3)
                            locks["Knowledge"] = true;
                        if (PlayState.currentRando.openAreas)
                        {
                            locks["Boss1"] = true;
                            locks["Boss2"] = true;
                            locks["Boss3"] = true;
                            locks["Boss4"] = true;
                        }
                        locks[PlayState.currentProfile.character] = true;

                        //int[] currentMajorWeights = PlayState.currentRando.progressivesOn ? (int[])progMajorWeights.Clone() : (int[])majorWeights.Clone();
                        //for (int i = 0; i < currentMajorWeights.Length; i++)
                        //{
                        //    for (int j = 0; j < currentMajorWeights[i]; j++)
                        //        itemsToAdd.Add(i);
                        //}
                        //if (randoPhase == 3)
                        //{
                        //    for (int i = 0; i < PlayState.MAX_HEARTS; i++)
                        //        for (int j = 0; j < HEART_WEIGHT; j++)
                        //            itemsToAdd.Add(i + PlayState.OFFSET_HEARTS);
                        //    for (int i = 0; i < PlayState.MAX_FRAGMENTS - 5; i++)
                        //        for (int j = 0; j < HELIX_WEIGHT; j++)
                        //            itemsToAdd.Add(i + PlayState.OFFSET_FRAGMENTS);
                        //    if (PlayState.currentRando.trapsActive)
                        //    {
                        //        int trapTypes = System.Enum.GetNames(typeof(TrapManager.TrapItems)).Length;
                        //        List<int> trapsToAdd = new();
                        //        for (int i = 0; i < trapTypes; i++)
                        //            trapsToAdd.Add(1000 + i);
                        //        unplacedTraps = trapsToAdd;
                        //        int numberOfTraps = Mathf.CeilToInt(Random.value * trapTypes);
                        //        for (int i = 0; i < numberOfTraps; i++)
                        //        {
                        //            int trapID = Mathf.FloorToInt(Random.value * trapsToAdd.Count);
                        //            itemsToAdd.Add(trapsToAdd[trapID]);
                        //            trapsToAdd.RemoveAt(trapID);
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    while (itemsToAdd.Contains(10))
                        //        itemsToAdd.Remove(10); // Remove Gravity Shock from the pool on Split shuffle
                        //    foreach (int i in new int[] { 0, 3, 23, 35, 36, 54 })
                        //        locations[i] = -1; // Remove super secret items, snelk rooms, and test rooms as viable locations when not on Pro shuffle
                        //    splitPhase = 1;
                        //}
                        //Debug.Log("-------------------------------------------------");
                        break;

                    case 2: // Items
                        switch (itemPhase)
                        {
                            case 1: // Required majors
                                //Debug.Log(string.Format("Sphere {0}/{1}", currentSphere, validMajorCombos.Length - 1));
                                if (!initializedPhase)
                                {
                                    majorsToAdd = new List<Items>
                                    {
                                        Items.Peashooter, Items.Boomerang, Items.RainbowWave, Items.Devastator,
                                        Items.IceShell, Items.FlyShell, Items.HighJump
                                    };
                                    initializedPhase = true;
                                }
                                if (currentSphere >= validMajorCombos.Length)
                                {
                                    itemPhase++;
                                    initializedPhase = false;
                                }
                                else
                                {
                                    if (currentMajorCombo.Length == 0)
                                        currentMajorCombo = SelectNewMajorCombo(currentSphere,
                                            PlayState.currentRando.randoLevel == 1 ? sphereSplitLimits[currentSphere] : 0);

                                    if (CheckIfFullComboPlaced(majorsToAdd, currentMajorCombo))
                                    {
                                        currentSphere++;
                                        currentMajorCombo = new int[0];
                                    }
                                    else
                                    {
                                        List<int> currentOpenLocations = GetLocations(PlayState.currentRando.randoLevel == 1);
                                        int openLocationID = Mathf.FloorToInt(Random.value * currentOpenLocations.Count);

                                        int majorPointer = Mathf.FloorToInt(Random.value * currentMajorCombo.Length);
                                        int pointerAdjustmentSign = Random.value <= 0.5f ? -1 : 1;
                                        // Something broke here. Fix it
                                        if (majorsPlaced.Count > 0)
                                            while (majorsPlaced.Contains(ItemIDToEnum(currentMajorCombo[majorPointer])))
                                                majorPointer = (majorPointer + (1 * pointerAdjustmentSign)) % currentMajorCombo.Length;

                                        bool validPlacement = false;
                                        int pointerShiftsLeft = currentMajorCombo.Length;
                                        while (!validPlacement && pointerShiftsLeft > 0)
                                        {
                                            List<int> projectedOpenLocations = GetLocations(TweakLocks(currentMajorCombo[majorPointer], 0, false),
                                                PlayState.currentRando.randoLevel == 1);
                                            if (projectedOpenLocations.Count > 0 || CountUnplacedMajors(majorsToAdd, currentMajorCombo) == 1)
                                                validPlacement = true;
                                            else
                                            {
                                                majorPointer = (majorPointer + (1 * pointerAdjustmentSign)) % currentMajorCombo.Length;
                                                pointerShiftsLeft--;
                                            }
                                        }
                                        if (validPlacement)
                                        {
                                            int thisMajorID = currentMajorCombo[majorPointer];
                                            Items thisMajorEnum = ItemIDToEnum(thisMajorID);
                                            majorsPlaced.Add(thisMajorEnum);
                                            majorsToAdd.Remove(thisMajorEnum);
                                            locations[currentOpenLocations[openLocationID]] = thisMajorID;
                                            TweakLocks(thisMajorID, 0);
                                            //PrintPlacement(thisMajorID, currentOpenLocations[openLocationID]);
                                        }
                                    }
                                }
                                //Debug.Log(string.Format("{0} major(s) left to place this sphere", CountUnplacedMajors(majorsToAdd, currentMajorCombo)));
                                break;
                            case 2: // Remaining majors
                                //for (int i = 0; i < locations.Length; i++)
                                //    if (locations[i] == -2)
                                //        locations[i] = -1;
                                //PlayState.currentRando.itemLocations = (int[])locations.Clone();
                                //randoPhase++;
                                if (!initializedPhase)
                                {
                                    majorsToAdd = new();
                                    List<Items> tempMajors = new() { Items.Peashooter, Items.Boomerang, Items.RainbowWave, Items.Devastator, Items.HighJump,
                                        Items.ShellShield, Items.RapidFire, Items.IceShell, Items.FlyShell, Items.MetalShell, Items.GravityShock };
                                    if (PlayState.currentRando.randoLevel == 1)
                                        tempMajors.Remove(Items.GravityShock);
                                    for (int i = 0; i < tempMajors.Count; i++)
                                        if (!majorsPlaced.Contains(tempMajors[i]))
                                            majorsToAdd.Add(tempMajors[i]);
                                }
                                
                                if (majorsToAdd.Count > 0)
                                {
                                    List<int> currentOpenLocations = GetLocations(PlayState.currentRando.randoLevel == 1);
                                    int openLocationID = Mathf.FloorToInt(Random.value * currentOpenLocations.Count);
                                    int majorPointer = Mathf.FloorToInt(Random.value * majorsToAdd.Count);
                                    Items thisMajorEnum = majorsToAdd[majorPointer];
                                    int thisMajorID = ItemEnumToID(thisMajorEnum);
                                    majorsPlaced.Add(thisMajorEnum);
                                    majorsToAdd.Remove(thisMajorEnum);
                                    locations[currentOpenLocations[openLocationID]] = thisMajorID;
                                    TweakLocks(thisMajorID, 0);
                                }
                                else
                                {
                                    itemPhase++;
                                    initializedPhase = false;
                                }
                                break;
                            case 3: // Hearts
                                if (placedHearts < PlayState.MAX_HEARTS)
                                {
                                    List<int> currentOpenLocations = GetLocations();
                                    int openLocationID = Mathf.FloorToInt(Random.value * currentOpenLocations.Count);
                                    locations[currentOpenLocations[openLocationID]] = PlayState.OFFSET_HEARTS + placedHearts;
                                    placedHearts++;
                                }
                                else
                                    itemPhase++;
                                break;
                            case 4: // Traps
                                if (PlayState.currentRando.trapsActive)
                                {
                                    if (!initializedPhase)
                                    {
                                        int totalTraps = System.Enum.GetNames(typeof(TrapManager.TrapItems)).Length;
                                        List<int> validTraps = new();
                                        for (int i = 0; i < totalTraps; i++)
                                            validTraps.Add(1000 + i);
                                        int trapCount = Mathf.FloorToInt(Random.value * (validTraps.Count - MINIMUM_TRAPS)) + MINIMUM_TRAPS + 1;
                                        for (int i = 0; i < trapCount; i++)
                                        {
                                            int trapIndex = Mathf.FloorToInt(Random.value * validTraps.Count);
                                            trapsToPlace.Add(validTraps[trapIndex]);
                                            validTraps.RemoveAt(trapIndex);
                                        }
                                        initializedPhase = true;
                                    }

                                    if (trapsToPlace.Count > 0)
                                    {
                                        List<int> currentLocations = GetLocations();
                                        int locationID = Mathf.FloorToInt(Random.value * currentLocations.Count);
                                        int trapIndex = Mathf.FloorToInt(Random.value * trapsToPlace.Count);
                                        locations[currentLocations[locationID]] = trapsToPlace[trapIndex];
                                        trapsToPlace.RemoveAt(trapIndex);
                                    }
                                    else
                                    {
                                        itemPhase++;
                                        initializedPhase = false;
                                    }
                                }
                                else
                                    itemPhase++;
                                break;
                            case 5: // Fragments
                                List<int> remainingLocations = GetLocations();
                                if (placedHelixes < PlayState.MAX_FRAGMENTS && remainingLocations.Count > 0)
                                {
                                    int locationID = Mathf.FloorToInt(Random.value * remainingLocations.Count);
                                    locations[remainingLocations[locationID]] = PlayState.OFFSET_FRAGMENTS + placedHelixes;
                                    placedHelixes++;
                                }
                                else
                                    itemPhase++;
                                break;
                            case 6: // Finalization and export
                                for (int i = 0; i < locations.Length; i++)
                                    if (locations[i] == -2)
                                        locations[i] = -1;

                                PlayState.currentRando.helixesRequired = new int[locationPhases.Length];
                                int helixesCounted = 0;
                                for (int i = 0; i < locationPhases.Length; i++)
                                {
                                    for (int j = 0; j < locationPhases[i].Count; j++)
                                    {
                                        int thisItemID = locations[locationPhases[i][j]];
                                        if (thisItemID >= PlayState.OFFSET_FRAGMENTS && thisItemID < 1000)
                                            helixesCounted++;
                                    }
                                    PlayState.currentRando.helixesRequired[i] = Mathf.FloorToInt((float)helixesCounted * sphereHelixPercentageRequirements[i]);
                                }

                                PlayState.currentRando.itemLocations = (int[])locations.Clone();
                                PlayState.currentRando.trapLocations = new int[System.Enum.GetNames(typeof(TrapManager.TrapItems)).Length];
                                randoPhase++;
                                break;
                        }
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
                        itemPhase = 0;
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

    private int[] SelectNewMajorCombo(int sphereID, int lengthLimit = 0)
    {
        List<int> comboPool = new();
        for (int i = 0; i < validMajorCombos[sphereID].Length; i++)
            if (lengthLimit == 0 || validMajorCombos[sphereID][i].Length <= lengthLimit)
                for (int j = 0; j < validMajorCombos[sphereID][i][0]; j++)
                    comboPool.Add(i);
        
        int poolIndex = Mathf.FloorToInt(Random.value * comboPool.Count);
        int chosenPoolID = comboPool[poolIndex];
        
        List<int> newCombo = new();
        for (int i = 1; i < validMajorCombos[sphereID][chosenPoolID].Length; i++)
            newCombo.Add(validMajorCombos[sphereID][chosenPoolID][i]);

        //string output = "Selected combo ";
        //for (int i = 0; i < newCombo.Count; i++)
        //    output += ItemIDToEnum(newCombo[i]).ToString() + " ";
        //Debug.Log(output);

        return newCombo.ToArray();
    }

    private bool CheckIfFullComboPlaced(List<Items> currentUnplacedMajors, int[] currentCombo)
    {
        bool completelyPlaced = true;
        for (int i = 0; i < currentCombo.Length; i++)
            if (currentUnplacedMajors.Contains((Items)currentCombo[i]))
                completelyPlaced = false;
        return completelyPlaced;
    }

    private int CountUnplacedMajors(List<Items> currentUnplacedMajors, int[] currentCombo)
    {
        int unplacedInCombo = 0;
        for (int i = 0; i < currentCombo.Length; i++)
            if (currentUnplacedMajors.Contains(ItemIDToEnum(currentCombo[i])))
                unplacedInCombo++;
        return unplacedInCombo;
    }

    private int ItemEnumToID(Items item)
    {
        return item switch
        {
            Items.Peashooter => 0,
            Items.Boomerang => 1,
            Items.RainbowWave => 2,
            Items.Devastator => 3,
            Items.HighJump => 4,
            Items.ShellShield => 5,
            Items.RapidFire => 6,
            Items.IceShell => 7,
            Items.FlyShell => 8,
            Items.MetalShell => 9,
            Items.GravityShock => 10,
            Items.SSBoomerang => 11,
            Items.DebugRWave => 12,
            Items.Broom => -1,
            _ => -2
        };
    }
    private Items ItemIDToEnum(int item)
    {
        return item switch
        {
            0 => Items.Peashooter,
            1 => Items.Boomerang,
            2 => Items.RainbowWave,
            3 => Items.Devastator,
            4 => Items.HighJump,
            5 => Items.ShellShield,
            6 => Items.RapidFire,
            7 => Items.IceShell,
            8 => Items.FlyShell,
            9 => Items.MetalShell,
            10 => Items.GravityShock,
            11 => Items.SSBoomerang,
            12 => Items.DebugRWave,
            -1 => Items.Broom,
            _ => Items.None
        };
    }

    private List<int> GetLocations(bool majorsOnly = false)
    {
        return GetLocations(locks, majorsOnly);
    }
    private List<int> GetLocations(Dictionary<string, bool> _locks, bool majorsOnly = false, int sphereConstraint = -1)
    {
        List<int> newLocations = new();

        for (int i = 0; i < PlayState.baseItemLocations.Count; i++)
        {
            if ((sphereConstraint == -1 || locationPhases[sphereConstraint].Contains(i)) &&
                //(PlayState.currentRando.randoLevel == 3 || !proOnlyLocations.Contains(i)) &&
                (!majorsOnly || majorLocations.Contains(i)))
            {
                if (i switch
                {
                    0 => _locks["Knowledge"] && _locks["L2Blocks"] && (_locks["Jump"] || _locks["Upside"] || _locks["Leggy"]),
                                                                                                                            // Original Testing Room
                    1 => _locks["L1Blocks"],
                                                                                                                            // Leggy Snail's Tunnel
                    2 => _locks["L1Blocks"] || (_locks["Jump"] && _locks["Knowledge"]) || ((_locks["Sluggy"] || _locks["Upside"] ||
                        _locks["Leggy"] || _locks["Leechy"]) && _locks["Knowledge"]),                                       // Town Overtunnel
                    3 => _locks["Knowledge"] && (_locks["L1Blocks"] || _locks["Jump"] || _locks["Sluggy"] ||
                        _locks["Upside"] || _locks["Leggy"] || _locks["Leechy"]),                                           // Super Secret Alcove
                    4 => _locks["L1Blocks"] || _locks["Jump"] || _locks["Sluggy"] || _locks["Upside"] || _locks["Leggy"] || _locks["Leechy"],
                                                                                                                            // Love Snail's Alcove
                    5 => _locks["L2Blocks"],
                                                                                                                            // Suspicious Tree
                    6 => _locks["L2Blocks"],
                                                                                                                            // Anger Management Room
                    7 => _locks["L2Blocks"] && ((_locks["Blobby"] && _locks["Jump"]) || !_locks["Blobby"]),
                                                                                                                            // Percentage Snail's Hidey Hole
                    8 => true,
                                                                                                                            // Digging Grounds
                    9 => true,
                                                                                                                            // Cave Snail's Cave
                    10 => _locks["L2Blocks"],
                                                                                                                            // Fragment Cave
                    11 => (_locks["Knowledge"] && ((_locks["Blobby"] && _locks["Jump"]) || !_locks["Blobby"])) ||
                        _locks["Fly"] || _locks["Upside"] || _locks["Leggy"],                                               // Discombobulatory Alcove
                    12 => (_locks["Blobby"] && _locks["Jump"]) || !_locks["Blobby"],
                                                                                                                            // Seabed Caves
                    13 => true,
                                                                                                                            // Fine Dining (Peashooter)
                    14 => _locks["L2Blocks"],
                                                                                                                            // Fine Dining (Fragment)
                    15 => _locks["L1Blocks"],
                                                                                                                            // The Maze Room
                    16 => _locks["L1Blocks"],
                                                                                                                            // Monument of Greatness
                    17 => _locks["RedDoor"],
                                                                                                                            // Heart of the Sea
                    18 => _locks["BlueDoor"] && (_locks["PinkDoor"] || _locks["Knowledge"]),
                                                                                                                            // Daily Helping of Calcium
                    19 => _locks["GreenDoor"],
                                                                                                                            // Dig, Snaily, Dig
                    20 => _locks["L1Blocks"],
                                                                                                                            // Skywatcher's Loot
                    21 => _locks["Boss1"],
                                                                                                                            // Signature Croissants (Boomerang)
                    22 => _locks["Boss1"] && _locks["L1Blocks"],
                                                                                                                            // Signature Croissants (Heart)
                    23 => _locks["Boss1"] && _locks["Knowledge"] && (_locks["Fly"] || _locks["Upside"] || _locks["Leggy"]),
                                                                                                                            // Squared Snelks
                    24 => _locks["Boss1"] && _locks["PinkDoor"],
                                                                                                                            // Frost Shrine
                    25 => _locks["Boss1"] && (_locks["Ice"] || (_locks["Health"] && (_locks["Fly"] || _locks["Leggy"]))) && _locks["L1Blocks"] &&
                        ((_locks["Blobby"] && _locks["Jump"]) || !_locks["Blobby"]),                                        // Sweater Required
                    26 => _locks["Boss1"] && _locks["PinkDoor"],
                                                                                                                            // A Secret to Snowbody
                    27 => _locks["Boss1"] && _locks["GreenDoor"],
                                                                                                                            // Devil's Alcove
                    28 => _locks["Boss1"] && (_locks["Knowledge"] || _locks["Jump"] || _locks["Fly"] || _locks["Ice"] || _locks["Upside"] || _locks["Leggy"]),
                                                                                                                            // Ice Climb
                    29 => _locks["Boss1"] && _locks["L2Blocks"],
                                                                                                                            // The Labyrinth (Fragment)
                    30 => (_locks["Boss1"] && _locks["L2Blocks"]) || (_locks["Knowledge"] && !_locks["Upside"]),
                                                                                                                            // The Labyrinth (High Jump)
                    31 => _locks["Boss1"] && _locks["RedDoor"],
                                                                                                                            // Sneaky, Sneaky
                    32 => _locks["Boss2"] || _locks["RedDoor"],
                                                                                                                            // Prismatic Prize (Rainbow Wave)
                    33 => _locks["Boss2"] && _locks["RedDoor"],
                                                                                                                            // Prismatic Prize (Heart)
                    34 => _locks["Boss2"] && (_locks["Metal"] || _locks["Health"]) && (_locks["PinkDoor"] || _locks["RedDoor"]),
                                                                                                                            // Hall of Fire
                    35 => _locks["Boss2"] && _locks["Knowledge"] && _locks["Metal"] && (_locks["Fly"] || _locks["L3Blocks"] || _locks["RedDoor"]),
                                                                                                                            // Scorching Snelks
                    36 => _locks["Boss2"] && _locks["Knowledge"] && (_locks["Fly"] || _locks["L3Blocks"]),
                                                                                                                            // Hidden Hideout
                    37 => _locks["Boss2"] && _locks["RedDoor"] || _locks["L3Blocks"],
                                                                                                                            // Green Cache
                    38 => _locks["Boss2"] && _locks["L2Blocks"],
                                                                                                                            // Furnace
                    39 => _locks["Boss2"] && _locks["RedDoor"],
                                                                                                                            // Slitherine Grove
                    40 => _locks["Boss2"] && _locks["PinkDoor"] && (_locks["Fly"] || _locks["Upside"] || _locks["Leggy"]),
                                                                                                                            // Floaty Fortress (Top Left)
                    41 => _locks["Boss2"] && _locks["PinkDoor"] && (_locks["Fly"] || _locks["Upside"] || _locks["Leggy"]),
                                                                                                                            // Floaty Fortress (Bottom Right)
                    42 => _locks["Boss2"] && _locks["L2Blocks"],
                                                                                                                            // Woah Mama
                    43 => _locks["Boss2"] && _locks["L2Blocks"] && (_locks["Jump"] || _locks["Sluggy"] || _locks["Upside"]
                        || _locks["Leggy"] || _locks["Leechy"]),                                                            // Shocked Shell
                    44 => _locks["Boss2"] && _locks["L2Blocks"],
                                                                                                                            // Gravity Shrine
                    45 => _locks["Boss2"] && _locks["Fly"] || (_locks["Knowledge"] && _locks["RedDoor"]),
                                                                                                                            // Fast Food
                    46 => _locks["Boss3"] || (_locks["RedDoor"] && (_locks["Jump"] || _locks["Sluggy"] ||
                        _locks["Upside"] || _locks["Leggy"] || _locks["Leechy"])),                                          // The Bridge
                    47 => _locks["Boss3"] && _locks["L3Blocks"],
                                                                                                                            // Transit 90
                    48 => _locks["Boss3"] && _locks["RedDoor"] && (_locks["Metal"] || _locks["Health"]),
                                                                                                                            // Steel Shrine
                    49 => _locks["Boss3"] && _locks["L3Blocks"] && (_locks["Metal"] || _locks["Health"]),
                                                                                                                            // Space Balcony (Heart)
                    50 => _locks["Boss3"] && _locks["L3Blocks"] && (_locks["Metal"] || _locks["Health"]),
                                                                                                                            // Space Balcony (Fragment)
                    51 => _locks["Boss3"] && _locks["RedDoor"] && (_locks["Metal"] || _locks["Health"]),
                                                                                                                            // The Vault
                    52 => _locks["Boss3"] && _locks["RedDoor"] && (_locks["Fly"] || _locks["Upside"] || _locks["Leggy"] || _locks["Blobby"]) &&
                        (_locks["Health"] || _locks["Metal"]),                                                              // Holy Hideaway
                    53 => _locks["Boss3"] && _locks["RedDoor"] && (_locks["Fly"] || _locks["Upside"] || _locks["Leggy"] || _locks["Blobby"]) &&
                        (_locks["Health"] || _locks["Metal"]),                                                              // Arctic Alcove
                    54 => _locks["Boss3"] && _locks["Knowledge"] && _locks["RedDoor"] && (_locks["Fly"] || _locks["Upside"] || _locks["Leggy"] ||
                        _locks["Blobby"]) && (_locks["Health"] || _locks["Metal"]),                                         // Lost Loot
                    55 => _locks["Boss3"] && _locks["GreenDoor"] && (_locks["Fly"] || _locks["Upside"] || _locks["Leggy"] || _locks["Blobby"]) &&
                        (_locks["Health"] || _locks["Metal"]),                                                              // Reinforcements
                    56 => _locks["PinkDoor"],
                                                                                                                            // Glitched Goodies
                    _ => false
                })
                    if (locations[i] == -2)
                        newLocations.Add(i);
            }
        }

        return newLocations;
    }

    private Dictionary<string, bool> TweakLocks(int itemID, int helixCount, bool writeToMainLockDict = true)
    {
        Dictionary<string, bool> _locks = new(locks);

        switch (itemID)
        {
            case -1: // Broom
                _locks["BlueDoor"] = true;
                //if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                    _locks["Boss1"] = true;
                break;
            case 0: // Peashooter
                _locks["BlueDoor"] = true;
                if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                    _locks["Boss1"] = true;
                if (hasPlacedDevastator)
                {
                    _locks["PinkDoor"] = true;
                    _locks["RedDoor"] = true;
                    _locks["GreenDoor"] = true;
                    _locks["L1Blocks"] = true;
                    _locks["L2Blocks"] = true;
                    _locks["L3Blocks"] = true;
                    //if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                    //{
                        _locks["Boss2"] = true;
                        _locks["Boss3"] = true;
                        _locks["Boss4"] = true;
                    //}
                }
                break;
            case 1: // Boomerang
                _locks["BlueDoor"] = true;
                _locks["PinkDoor"] = true;
                _locks["L1Blocks"] = true;
                //if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                //{
                    _locks["Boss1"] = true;
                    _locks["Boss2"] = true;
                //}
                if (hasPlacedDevastator)
                {
                    _locks["RedDoor"] = true;
                    _locks["GreenDoor"] = true;
                    _locks["L2Blocks"] = true;
                    _locks["L3Blocks"] = true;
                    //if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                    //{
                        _locks["Boss3"] = true;
                        _locks["Boss4"] = true;
                    //}
                }
                break;
            case 2: // Rainbow Wave
                _locks["BlueDoor"] = true;
                _locks["PinkDoor"] = true;
                _locks["RedDoor"] = true;
                _locks["L1Blocks"] = true;
                _locks["L2Blocks"] = true;
                //if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                //{
                    _locks["Boss1"] = true;
                    _locks["Boss2"] = true;
                    _locks["Boss3"] = true;
                //}
                if (hasPlacedDevastator)
                {
                    _locks["L3Blocks"] = true;
                    _locks["GreenDoor"] = true;
                    //if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                        _locks["Boss4"] = true;
                }
                break;
            case 3: // Devastator
                if (_locks["BlueDoor"] || _locks["PinkDoor"] || _locks["RedDoor"])
                {
                    _locks["BlueDoor"] = true;
                    _locks["PinkDoor"] = true;
                    _locks["RedDoor"] = true;
                    _locks["L1Blocks"] = true;
                    _locks["L2Blocks"] = true;
                    _locks["L3Blocks"] = true;
                    _locks["GreenDoor"] = true;
                    //if (!PlayState.currentRando.bossesLocked && !PlayState.currentRando.openAreas)
                    //{
                        _locks["Boss1"] = true;
                        _locks["Boss2"] = true;
                        _locks["Boss3"] = true;
                        _locks["Boss4"] = true;
                    //}
                }
                hasPlacedDevastator = true;
                break;
            case 4: // High Jump
                _locks["Jump"] = true;
                break;
            case 5: // Shell Shield
                break;
            case 6: // Rapid Fire
                break;
            case 7: // Ice Snail
                _locks["Ice"] = true;
                break;
            case 8: // Gravity Snail
                _locks["Jump"] = true;
                _locks["Fly"] = true;
                break;
            case 9: // Full Metal Snail
                _locks["Metal"] = true;
                break;
            case 10: // Gravity Shock
                _locks["Shock"] = true;
                break;
            default:
                if (PlayState.currentRando.bossesLocked)
                {
                    if (helixCount >= 5)
                        _locks["Boss1"] = true;
                    if (helixCount >= 10)
                        _locks["Boss2"] = true;
                    if (helixCount >= 15)
                        _locks["Boss3"] = true;
                    if (helixCount >= 25)
                        _locks["Boss4"] = true;
                }
                break;
        }
        if (writeToMainLockDict)
            locks = new(_locks);
        return _locks;
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
