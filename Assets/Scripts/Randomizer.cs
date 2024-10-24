using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    public bool isShuffling = false;
    private int randoPhase = 0; // 1 = initiate item shuffle, 2 = items, 3 = music, 4 = dialogue
    private int splitPhase = 0; // 1 = majors, 2 = minors

    private readonly int[] majorWeights = new int[] { 4, 3, 2, 1, 3, 4, 3, 2, 1, 1, 1 };
    private readonly List<int> majorLocations = new() { 13, 18, 21, 24, 30, 32, 44, 45, 48, 51 };

    private bool[] lockStates = new bool[24];
    private bool[] defaultLocksThisGen = new bool[24];
    private readonly List<int> defaultMusicList = new() { -7, -6, -5, -1, 0, 1, 2, 3, 4, 5, 6 };

    private int[] locations = new int[] { };
    private bool hasPlacedDevastator = false;

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
                    lockStates = (bool[])defaultLocksThisGen.Clone();

                    for (int i = 0; i < majorWeights.Length; i++)
                    {
                        for (int j = 0; j < majorWeights[i]; j++)
                            itemsToAdd.Add(i);
                    }
                    if (randoPhase == 3)
                    {
                        for (int i = 0; i < PlayState.MAX_HEARTS; i++)
                            for (int j = 0; j < 4; j++)
                                itemsToAdd.Add(i + PlayState.OFFSET_HEARTS);
                        for (int i = 0; i < PlayState.MAX_FRAGMENTS - 5; i++)
                            for (int j = 0; j < 5; j++)
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

                case 2: // Items (Split shuffle)
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
                            lockStates[11] = true; // Health
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

                case 3: // Items (Full/Pro shuffle)
                    List<int> availableLocations = GetLocations();
                    if (availableLocations.Count == 0 && itemsToAdd.Count > 0)
                        randoPhase = 1;
                    else if (itemsToAdd.Count > 0)
                    {
                        int locationPointer = Mathf.FloorToInt(Random.value * availableLocations.Count);
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
                        if (unplacedTraps.Contains(itemToPlace))
                            unplacedTraps.Remove(itemToPlace);
                        locations[availableLocations[locationPointer]] = itemToPlace;
                        if (itemToPlace >= PlayState.OFFSET_FRAGMENTS && itemToPlace < 1000)
                            placedHelixes++;
                        else if (itemToPlace >= PlayState.OFFSET_HEARTS)
                            placedHearts++;
                        if (placedHearts >= 4)
                            lockStates[11] = true; // Health
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

                case 4: // Music
                    PlayState.currentRando.musicList = defaultMusicList.ToArray();
                    if (PlayState.currentRando.musicShuffled == 0)
                    {
                        randoPhase = 5;
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
                    randoPhase = 5;
                    break;

                case 5: // Dialogue
                    if (!PlayState.currentRando.npcTextShuffled)
                    {
                        randoPhase = 0;
                        isShuffling = false;
                        break;
                    }

                    int finalHintCount = Mathf.FloorToInt(Random.value * 3) + 2; // 2-5 hints per seed
                    List<int> availableHints = new();
                    int hintCount = 5;
                    for (int i = 0; i < hintCount; i++)
                        availableHints.Add(i);
                    for (int i = 0; i < finalHintCount; i++)
                    {
                        int itemID = -1;
                        int areaID = -1;
                        while (!(itemID >= 0 && itemID <= 10)) // Only major items valid for hints
                        {
                            int locationID = Mathf.FloorToInt(Random.value * PlayState.currentRando.itemLocations.Length);
                            itemID = PlayState.currentRando.itemLocations[locationID];
                            // If block for area
                        }
                        // Assemble hint data here
                    }

                    List<int> newIndeces = new();
                    List<int> availableIndeces = new();
                    int flavorCount = 47;
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
            }
            yield return new WaitForEndOfFrame();
        }
        CreateSpoilerMap();
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
                    0 => Knowledge() && L2Blocks() && (Jump() || Upside() || Leggy()),                                                  // Original Testing Room
                    1 => L1Blocks(),                                                                                                    // Leggy Snail's Tunnel
                    2 => L1Blocks() || (Jump() && Knowledge()) || ((Sluggy() || Upside() || Leggy() || Leechy()) && Knowledge()),       // Town Overtunnel
                    3 => Knowledge() && (L1Blocks() || Jump() || Sluggy() || Upside() || Leggy() || Leechy()),                          // Super Secret Alcove
                    4 => L1Blocks() || Jump() || Sluggy() || Upside() || Leggy() || Leechy(),                                           // Love Snail's Alcove
                    5 => L2Blocks(),                                                                                                    // Suspicious Tree
                    6 => L2Blocks(),                                                                                                    // Anger Management Room
                    7 => L2Blocks() && ((Blobby() && Jump()) || !Blobby()),                                                             // Percentage Snail's Hidey Hole
                    8 => true,                                                                                                          // Digging Grounds
                    9 => true,                                                                                                          // Cave Snail's Cave
                    10 => L2Blocks(),                                                                                                   // Fragment Cave
                    11 => (Knowledge() && ((Blobby() && Jump()) || !Blobby())) || Fly() || Upside() || Leggy(),                         // Discombobulatory Alcove
                    12 => (Blobby() && Jump()) || !Blobby(),                                                                            // Seabed Caves
                    13 => true,                                                                                                         // Fine Dining (Peashooter)
                    14 => L2Blocks(),                                                                                                   // Fine Dining (Fragment)
                    15 => L1Blocks(),                                                                                                   // The Maze Room
                    16 => L1Blocks(),                                                                                                   // Monument of Greatness
                    17 => RedDoor(),                                                                                                    // Heart of the Sea
                    18 => BlueDoor() && (PinkDoor() || Knowledge()),                                                                    // Daily Helping of Calcium
                    19 => GreenDoor(),                                                                                                  // Dig, Snaily, Dig
                    20 => L1Blocks(),                                                                                                   // Skywatcher's Loot
                    21 => Boss1(),                                                                                                      // Signature Croissants (Boomerang)
                    22 => Boss1() && L1Blocks(),                                                                                        // Signature Croissants (Heart)
                    23 => Boss1() && Knowledge() && (Fly() || Upside() || Leggy()),                                                     // Squared Snelks
                    24 => Boss1() && PinkDoor(),                                                                                        // Frost Shrine
                    25 => Boss1() && (Ice() || (Health() && (Fly() || Leggy()))) && L1Blocks() && ((Blobby() && Jump()) || !Blobby()),  // Sweater Required
                    26 => Boss1() && PinkDoor(),                                                                                        // A Secret to Snowbody
                    27 => Boss1() && GreenDoor(),                                                                                       // Devil's Alcove
                    28 => Boss1() && (Knowledge() || Jump() || Fly() || Ice() || Upside() || Leggy()),                                  // Ice Climb
                    29 => Boss1() && L2Blocks(),                                                                                        // The Labyrinth (Fragment)
                    30 => (Boss1() && L2Blocks()) || (Knowledge() && !Upside()),                                                        // The Labyrinth (High Jump)
                    31 => Boss1() && RedDoor(),                                                                                         // Sneaky, Sneaky
                    32 => Boss2() || RedDoor(),                                                                                         // Prismatic Prize (Rainbow Wave)
                    33 => Boss2() && RedDoor(),                                                                                         // Prismatic Prize (Heart)
                    34 => Boss2() && (Metal() || Health()) && (PinkDoor() || RedDoor()),                                                // Hall of Fire
                    35 => Boss2() && Knowledge() && Metal() && (Fly() || L3Blocks() || RedDoor()),                                      // Scorching Snelks
                    36 => Boss2() && Knowledge() && (Fly() || L3Blocks()),                                                              // Hidden Hideout
                    37 => Boss2() && RedDoor() || L3Blocks(),                                                                           // Green Cache
                    38 => Boss2() && L2Blocks(),                                                                                        // Furnace
                    39 => Boss2() && RedDoor(),                                                                                         // Slitherine Grove
                    40 => Boss2() && PinkDoor() && (Fly() || Upside() || Leggy()),                                                      // Floaty Fortress (Top Left)
                    41 => Boss2() && PinkDoor() && (Fly() || Upside() || Leggy()),                                                      // Floaty Fortress (Bottom Right)
                    42 => Boss2() && L2Blocks(),                                                                                        // Woah Mama
                    43 => Boss2() && L2Blocks() && (Jump() || Sluggy() || Upside() || Leggy() || Leechy()),                             // Shocked Shell
                    44 => Boss2() && L2Blocks(),                                                                                        // Gravity Shrine
                    45 => Boss2() && Fly() || (Knowledge() && RedDoor()),                                                               // Fast Food
                    46 => Boss3() || (RedDoor() && (Jump() || Sluggy() || Upside() || Leggy() || Leechy())),                            // The Bridge
                    47 => Boss3() && L3Blocks(),                                                                                        // Transit 90
                    48 => Boss3() && RedDoor() && (Metal() || Health()),                                                                // Steel Shrine
                    49 => Boss3() && L3Blocks() && (Metal() || Health()),                                                               // Space Balcony (Heart)
                    50 => Boss3() && L3Blocks() && (Metal() || Health()),                                                               // Space Balcony (Fragment)
                    51 => Boss3() && RedDoor() && (Metal() || Health()),                                                                // The Vault
                    52 => Boss3() && RedDoor() && (Fly() || Upside() || Leggy() || Blobby()) && (Health() || Metal()),                  // Holy Hideaway
                    53 => Boss3() && RedDoor() && (Fly() || Upside() || Leggy() || Blobby()) && (Health() || Metal()),                  // Arctic Alcove
                    54 => Boss3() && Knowledge() && RedDoor() && (Fly() || Upside() || Leggy() || Blobby()) && (Health() || Metal()),   // Lost Loot
                    55 => Boss3() && GreenDoor() && (Fly() || Upside() || Leggy() || Blobby()) && (Health() || Metal()),                // Reinforcements
                    56 => PinkDoor(),                                                                                                   // Glitched Goodies
                    _ => false
                })
                    if (locations[i] == -2)
                        newLocations.Add(i);
            }
        }

        return newLocations;
    }

    private bool BlueDoor() { return lockStates[0]; }
    private bool PinkDoor() { return lockStates[1]; }
    private bool RedDoor() { return lockStates[2]; }
    private bool GreenDoor() { return lockStates[3]; }
    private bool L1Blocks() { return lockStates[4]; }
    private bool L2Blocks() { return lockStates[5]; }
    private bool L3Blocks() { return lockStates[6]; }
    private bool Jump() { return lockStates[7]; }
    private bool Ice() { return lockStates[8]; }
    private bool Fly() { return lockStates[9]; }
    private bool Metal() { return lockStates[10]; }
    private bool Health() { return lockStates[11]; }
    private bool Shock() { return lockStates[12]; }
    private bool Snaily() { return lockStates[13]; }
    private bool Sluggy() { return lockStates[14]; }
    private bool Upside() { return lockStates[15]; }
    private bool Leggy() { return lockStates[16]; }
    private bool Blobby() { return lockStates[17]; }
    private bool Leechy() { return lockStates[18]; }
    private bool Knowledge() { return lockStates[19]; }
    private bool Boss1() { return lockStates[20]; }
    private bool Boss2() { return lockStates[21]; }
    private bool Boss3() { return lockStates[22]; }
    private bool Boss4() { return lockStates[23]; }

    private void TweakLocks(int itemID, int helixCount)
    {
        switch (itemID)
        {
            case 0: // Peashooter
                lockStates[0] = true;  // BlueDoor
                if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    lockStates[20] = true; // Boss1
                if (hasPlacedDevastator)
                {
                    lockStates[1] = true;  // PinkDoor
                    lockStates[2] = true;  // RedDoor
                    lockStates[3] = true;  // GreenDoor
                    lockStates[4] = true;  // L1Blocks
                    lockStates[5] = true;  // L2Blocks
                    lockStates[6] = true;  // L3Blocks
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    {
                        lockStates[21] = true; // Boss2
                        lockStates[22] = true; // Boss3
                        lockStates[23] = true; // Boss4
                    }
                }
                break;
            case 1: // Boomerang
                lockStates[0] = true;  // BlueDoor
                lockStates[1] = true;  // PinkDoor
                lockStates[4] = true;  // L1Blocks
                if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                {
                    lockStates[20] = true; // Boss1
                    lockStates[21] = true; // Boss2
                }
                if (hasPlacedDevastator)
                {
                    lockStates[2] = true;  // RedDoor
                    lockStates[3] = true;  // GreenDoor
                    lockStates[5] = true;  // L2Blocks
                    lockStates[6] = true;  // L3Blocks
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    {
                        lockStates[22] = true; // Boss3
                        lockStates[23] = true; // Boss4
                    }
                }
                break;
            case 2: // Rainbow Wave
                lockStates[0] = true;  // BlueDoor
                lockStates[1] = true;  // PinkDoor
                lockStates[2] = true;  // RedDoor
                lockStates[4] = true;  // L1Blocks
                lockStates[5] = true;  // L2Blocks
                if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                {
                    lockStates[20] = true; // Boss1
                    lockStates[21] = true; // Boss2
                    lockStates[22] = true; // Boss3
                }
                if (hasPlacedDevastator)
                {
                    lockStates[6] = true;  // L3Blocks
                    lockStates[3] = true;  // GreenDoor
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                        lockStates[23] = true; // Boss4
                }
                break;
            case 3: // Devastator
                if (BlueDoor() || PinkDoor() || RedDoor())
                {
                    lockStates[0] = true;  // BlueDoor
                    lockStates[1] = true;  // PinkDoor
                    lockStates[2] = true;  // RedDoor
                    lockStates[4] = true;  // L1Blocks
                    lockStates[5] = true;  // L2Blocks
                    lockStates[6] = true;  // L3Blocks
                    lockStates[3] = true;  // GreenDoor
                    if (!PlayState.currentRando.bossesLocked && PlayState.currentRando.randoLevel > 1)
                    {
                        lockStates[20] = true; // Boss1
                        lockStates[21] = true; // Boss2
                        lockStates[22] = true; // Boss3
                        lockStates[23] = true; // Boss4
                    }
                }
                hasPlacedDevastator = true;
                break;
            case 4: // High Jump
                lockStates[7] = true;  // Jump
                break;
            case 5: // Shell Shield
                break;
            case 6: // Rapid Fire
                break;
            case 7: // Ice Snail
                lockStates[8] = true;  // Ice
                break;
            case 8: // Gravity Snail
                lockStates[7] = true;  // Jump
                lockStates[9] = true;  // Gravity
                break;
            case 9: // Full Metal Snail
                lockStates[10] = true; // Metal
                break;
            case 10: // Gravity Shock
                lockStates[12] = true; // Shock
                break;
            default:
                if (PlayState.currentRando.bossesLocked)
                {
                    if (helixCount >= 5)
                        lockStates[20] = true; // Boss 1
                    if (helixCount >= 10)
                        lockStates[21] = true; // Boss 2
                    if (helixCount >= 15)
                        lockStates[22] = true; // Boss 3
                    if (helixCount >= 25)
                        lockStates[23] = true; // Boss 4
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
