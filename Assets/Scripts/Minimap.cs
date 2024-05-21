using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject minimap;
    public AnimationModule[] anims;

    private int currentCellID;
    private int lastCellID;

    private int maxNormalCells;

    private GameObject[] masks = new GameObject[] { };
    private SpriteRenderer[] sprites = new SpriteRenderer[] { };
    private readonly int[] maskIDoffsets = new int[]
    {
        -55, -54, -53, -52, -51, -50, -49,
        -29, -28, -27, -26, -25, -24, -23,
         -3,  -2,  -1,   0,   1,   2,   3,
         23,  24,  25,  26,  27,  28,  29,
         49,  50,  51,  52,  53,  54,  55
    };

    private float currentMarkerColor = 0;
    private List<SpriteRenderer> cellsWithMarkers = new();

    //   Cell value legend
    //
    // -1 - Empty cell
    //  0 - Unexplored cell
    //  1 - Explored cell
    //  2 - Unexplored secret cell
    //  3 - Explored secret cell
    //
    // Any cells with value 10 or higher denote a cell with a player-set marker within them. The true value is the number minus 10

    void Start()
    {
        Transform maskParent = transform.Find("Room Masks");
        List<GameObject> newMaskList = new();
        List<SpriteRenderer> newSpriteList = new();
        for (int i = 0; i < maskIDoffsets.Length; i++)
        {
            newMaskList.Add(maskParent.GetChild(i).gameObject);
            newSpriteList.Add(maskParent.GetChild(i).GetComponent<SpriteRenderer>());
        }
        masks = newMaskList.ToArray();
        sprites = newSpriteList.ToArray();

        List<AnimationModule> newAnimList = new();
        newAnimList.Add(GetComponent<AnimationModule>());
        newAnimList.Add(transform.Find("Minimap").GetComponent<AnimationModule>());
        newAnimList.Add(transform.Find("Minimap Mask").GetComponent<AnimationModule>());
        newAnimList.Add(transform.Find("Icons").Find("Player").GetComponent<AnimationModule>());
        foreach (GameObject mask in masks)
            newAnimList.Add(mask.GetComponent<AnimationModule>());
        anims = newAnimList.ToArray();

        anims[0].Add("Minimap_panel");
        anims[1].Add("Minimap");
        anims[2].Add("Minimap_mask");
        anims[3].Add("Minimap_icon_playerNormal");
        anims[3].Add("Minimap_icon_playerHighlight");
        for (int i = 4; i < anims.Length; i++)
        {
            anims[i].Add("Minimap_icon_blank");
            anims[i].Add("Minimap_icon_itemNormal");
            anims[i].Add("Minimap_icon_itemCollected");
            anims[i].Add("Minimap_icon_save");
            anims[i].Add("Minimap_icon_boss");
            anims[i].Add("Minimap_icon_unknown");
            anims[i].Add("Minimap_icon_marker");
        }
        anims[3].Play("Minimap_icon_playerNormal");
        for (int i = 0; i < anims.Length; i++)
            anims[i].affectedByGlobalEntityColor = false;

        foreach (int cell in PlayState.defaultMinimapState)
            if (cell == 0)
                maxNormalCells++;
    }

    void Update()
    {
        minimap.transform.localPosition = new Vector2(
            -Mathf.Round((PlayState.WORLD_ORIGIN.x + PlayState.player.transform.position.x - 1 + (PlayState.ROOM_SIZE.x * 0.5f)) / PlayState.ROOM_SIZE.x) * 0.5f + 0.25f,
            -Mathf.Round((PlayState.WORLD_ORIGIN.y + PlayState.player.transform.position.y - 1 + (PlayState.ROOM_SIZE.y * 0.5f)) / PlayState.ROOM_SIZE.y) * 0.5f + 0.25f
            );
        currentCellID = PlayState.WorldPosToMapGridID(PlayState.player.transform.position);
        if (currentCellID >= 0 && currentCellID < PlayState.currentProfile.exploredMap.Length && !PlayState.playerScript.inDeathCutscene)
        {
            if (PlayState.currentProfile.exploredMap[currentCellID] == 0 || PlayState.currentProfile.exploredMap[currentCellID] == 2 ||
                PlayState.currentProfile.exploredMap[currentCellID] == 10 || PlayState.currentProfile.exploredMap[currentCellID] == 12)
            {
                PlayState.currentProfile.exploredMap[currentCellID]++;
                int newCurrentCells = 0;
                foreach (int cell in PlayState.currentProfile.exploredMap)
                    if (cell == 1 || cell == 11)
                        newCurrentCells++;
                if (newCurrentCells == maxNormalCells)
                    PlayState.QueueAchievementPopup(AchievementPanel.Achievements.Map100);
            }
        }
        if (lastCellID != currentCellID)
            RefreshMap();
        lastCellID = currentCellID;

        foreach (SpriteRenderer thisSprite in cellsWithMarkers)
        {
            thisSprite.color = Mathf.FloorToInt(currentMarkerColor * 8) switch
            {
                1 => PlayState.GetColor("0309"),
                2 => PlayState.GetColor("0304"),
                3 => PlayState.GetColor("0206"),
                _ => PlayState.GetColor("0012")
            };
        }
        currentMarkerColor += Time.deltaTime;
        if (currentMarkerColor > 0.5f)
            currentMarkerColor -= 0.5f;
    }

    public void RefreshMap()
    {
        cellsWithMarkers.Clear();
        for (int i = 0; i < masks.Length; i++)
        {
            int thisMaskID = currentCellID + maskIDoffsets[i];
            if (thisMaskID >= 0 && thisMaskID < PlayState.currentProfile.exploredMap.Length)
            {
                int thisCellValue = (PlayState.currentProfile.exploredMap[thisMaskID] < 10) ? PlayState.currentProfile.exploredMap[thisMaskID] :
                    (PlayState.currentProfile.exploredMap[thisMaskID] - 10);
                if (thisCellValue == 1 || thisCellValue == 11 || ((thisCellValue == 3 || thisCellValue == 13) && PlayState.generalData.secretMapTilesVisible))
                {
                    bool highlightPlayerTile = true;
                    masks[i].GetComponent<SpriteMask>().enabled = false;
                    sprites[i].color = PlayState.GetColor("0312");

                    int thisRow = Mathf.FloorToInt((currentCellID + maskIDoffsets[i]) / PlayState.WORLD_SIZE.x);
                    int intendedRow = Mathf.FloorToInt((currentCellID + maskIDoffsets[Mathf.FloorToInt(i / 7) * 7 + 3]) / PlayState.WORLD_SIZE.x);
                    if (thisRow == intendedRow)
                    {
                        if (PlayState.playerMarkerLocations.ContainsKey(thisMaskID))
                        {
                            cellsWithMarkers.Add(sprites[i]);
                            anims[i + 4].Play("Minimap_icon_marker", true);
                        }
                        else if (PlayState.bossLocations.Contains(thisMaskID))
                            anims[i + 4].Play("Minimap_icon_boss", true);
                        else if (PlayState.saveLocations.Contains(thisMaskID))
                            anims[i + 4].Play("Minimap_icon_save", true);
                        else if (PlayState.itemLocations.ContainsKey(thisMaskID))
                        {
                            int thisItemId = PlayState.baseItemLocations[PlayState.itemLocations[thisMaskID]];
                            if (PlayState.isRandomGame)
                                thisItemId = PlayState.currentRando.itemLocations[PlayState.itemLocations[thisMaskID]];
                            if (thisItemId != -1)
                            {
                                bool markItem = false;
                                bool markCollected = false;
                                if (thisItemId >= 1000)
                                {
                                    markItem = true;
                                    if (PlayState.currentRando.trapLocations[thisItemId - 1000] == 1)
                                        markCollected = true;
                                }
                                else if (PlayState.GetItemAvailabilityThisCharacter(thisItemId) && PlayState.GetItemAvailabilityThisDifficulty(thisItemId))
                                {
                                    markItem = true;
                                    if (PlayState.currentProfile.items[thisItemId] == 1)
                                        markCollected = true;
                                }
                                if (markItem)
                                {
                                    if (!markCollected)
                                        anims[i + 4].Play("Minimap_icon_itemNormal", true);
                                    else
                                    {
                                        anims[i + 4].Play("Minimap_icon_itemCollected", true);
                                        highlightPlayerTile = false;
                                    }
                                }
                                else
                                {
                                    anims[i + 4].Play("Minimap_icon_blank", true);
                                    highlightPlayerTile = false;
                                }
                            }
                            else
                            {
                                anims[i + 4].Play("Minimap_icon_blank", true);
                                highlightPlayerTile = false;
                            }
                        }
                        else
                        {
                            anims[i + 4].Play("Minimap_icon_blank", true);
                            highlightPlayerTile = false;
                        }
                    }
                    UpdatePlayerIcon(highlightPlayerTile ? "Minimap_icon_playerHighlight" : "Minimap_icon_playerNormal", thisMaskID == currentCellID);
                }
                else
                {
                    masks[i].GetComponent<SpriteMask>().enabled = true;
                    anims[i + 4].Play("Minimap_icon_blank", true);
                    if (PlayState.playerMarkerLocations.ContainsKey(thisMaskID))
                    {
                        cellsWithMarkers.Add(sprites[i]);
                        anims[i + 4].Play("Minimap_icon_marker", true);
                    }
                }
            }
            else
            {
                masks[i].GetComponent<SpriteMask>().enabled = true;
                anims[i + 4].Play("Minimap_icon_blank", true);
            }
        }
    }

    private void UpdatePlayerIcon(string state, bool confirm)
    {
        if (anims[3].currentAnimName != state && confirm)
            anims[3].Play(state);
    }

    public void RefreshAnims()
    {
        foreach (AnimationModule anim in anims)
        {
            string animName = anim.name;
            bool animPlaying = anim.isPlaying;
            if (animPlaying)
                anim.Stop();
            anim.ReloadList();
            if (animPlaying)
                anim.Play(animName);
        }
    }
}
