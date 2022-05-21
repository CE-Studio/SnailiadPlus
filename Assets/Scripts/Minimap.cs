using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    //public GameObject player;
    public GameObject minimap;
    public AnimationModule[] anims;

    private int currentCellID;
    private int lastCellID;

    private GameObject[] masks = new GameObject[] { };
    private int[] maskIDoffsets = new int[]
    {
        -55, -54, -53, -52, -51, -50, -49,
        -29, -28, -27, -26, -25, -24, -23,
        -3, -2, -1, 0, 1, 2, 3,
        23, 24, 25, 26, 27, 28, 29,
        49, 50, 51, 52, 53, 54, 55
    };

    public int[] currentMap = new int[] { };

    void Start()
    {
        currentMap = (int[])PlayState.defaultMinimapState.Clone();

        Transform maskParent = transform.Find("Room Masks");
        List<GameObject> newMaskList = new List<GameObject>();
        for (int i = 0; i < maskIDoffsets.Length; i++)
            newMaskList.Add(maskParent.GetChild(i).gameObject);
        masks = newMaskList.ToArray();

        List<AnimationModule> newAnimList = new List<AnimationModule>();
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
        }
        anims[3].Play("Minimap_icon_playerNormal");
    }

    void Update()
    {
        minimap.transform.localPosition = new Vector2(
            -Mathf.Round((PlayState.WORLD_ORIGIN.x + PlayState.player.transform.position.x - 1 + (PlayState.ROOM_SIZE.x * 0.5f)) / PlayState.ROOM_SIZE.x) * 0.5f + 0.25f,
            -Mathf.Round((PlayState.WORLD_ORIGIN.y + PlayState.player.transform.position.y - 1 + (PlayState.ROOM_SIZE.y * 0.5f)) / PlayState.ROOM_SIZE.y) * 0.5f + 0.25f
            );
        currentCellID = PlayState.WorldPosToMapGridID(PlayState.player.transform.position);
        if (currentCellID >= 0 && currentCellID < currentMap.Length && !PlayState.playerScript.inDeathCutscene)
        {
            if (currentMap[currentCellID] == 0 || currentMap[currentCellID] == 2)
                currentMap[currentCellID]++;
        }
        if (lastCellID != currentCellID)
            RefreshMap();
        lastCellID = currentCellID;
    }

    public void RefreshMap()
    {
        for (int i = 0; i < masks.Length; i++)
        {
            int thisMaskID = currentCellID + maskIDoffsets[i];
            if (thisMaskID >= 0 && thisMaskID < currentMap.Length)
            {
                if (currentMap[thisMaskID] == 1 || (currentMap[thisMaskID] == 3 && PlayState.gameOptions[13] == 1))
                {
                    bool highlightPlayerTile = true;
                    masks[i].GetComponent<SpriteMask>().enabled = false;
                    if (PlayState.bossLocations.Contains(thisMaskID))
                        anims[i + 4].Play("Minimap_icon_boss", true);
                    else if (PlayState.saveLocations.Contains(thisMaskID))
                        anims[i + 4].Play("Minimap_icon_save", true);
                    else if (PlayState.itemLocations.ContainsKey(thisMaskID))
                    {
                        if (PlayState.itemCollection[PlayState.itemLocations[thisMaskID]] == 0)
                            anims[i + 4].Play("Minimap_icon_itemNormal", true);
                        else
                            highlightPlayerTile = false;
                    }
                    else
                    {
                        anims[i + 4].Play("Minimap_icon_blank", true);
                        highlightPlayerTile = false;
                    }
                    UpdatePlayerIcon(highlightPlayerTile ? "Minimap_icon_playerHighlight" : "Minimap_icon_playerNormal", thisMaskID == currentCellID);
                }
                else
                {
                    masks[i].GetComponent<SpriteMask>().enabled = true;
                    anims[i + 4].Play("Minimap_icon_blank", true);
                }
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
