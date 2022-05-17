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

        masks = new GameObject[]
        {
            transform.GetChild(1).GetChild(0).gameObject, transform.GetChild(1).GetChild(1).gameObject, transform.GetChild(1).GetChild(2).gameObject,
            transform.GetChild(1).GetChild(3).gameObject, transform.GetChild(1).GetChild(4).gameObject, transform.GetChild(1).GetChild(5).gameObject,
            transform.GetChild(1).GetChild(6).gameObject, transform.GetChild(1).GetChild(7).gameObject, transform.GetChild(1).GetChild(8).gameObject,
            transform.GetChild(1).GetChild(9).gameObject, transform.GetChild(1).GetChild(10).gameObject, transform.GetChild(1).GetChild(11).gameObject,
            transform.GetChild(1).GetChild(12).gameObject, transform.GetChild(1).GetChild(13).gameObject, transform.GetChild(1).GetChild(14).gameObject,
            transform.GetChild(1).GetChild(15).gameObject, transform.GetChild(1).GetChild(16).gameObject, transform.GetChild(1).GetChild(17).gameObject,
            transform.GetChild(1).GetChild(18).gameObject, transform.GetChild(1).GetChild(19).gameObject, transform.GetChild(1).GetChild(20).gameObject,
            transform.GetChild(1).GetChild(21).gameObject, transform.GetChild(1).GetChild(22).gameObject, transform.GetChild(1).GetChild(23).gameObject,
            transform.GetChild(1).GetChild(24).gameObject, transform.GetChild(1).GetChild(25).gameObject, transform.GetChild(1).GetChild(26).gameObject,
            transform.GetChild(1).GetChild(27).gameObject, transform.GetChild(1).GetChild(28).gameObject, transform.GetChild(1).GetChild(29).gameObject,
            transform.GetChild(1).GetChild(30).gameObject, transform.GetChild(1).GetChild(31).gameObject, transform.GetChild(1).GetChild(32).gameObject,
            transform.GetChild(1).GetChild(33).gameObject, transform.GetChild(1).GetChild(34).gameObject
        };

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

    //private int CurrentCellID()
    //{
    //    Vector2 mapPos = PlayState.WorldPosToMapPos(PlayState.player.transform.position);
    //    return Mathf.RoundToInt(mapPos.y * PlayState.WORLD_SIZE.x + mapPos.x);
    //}

    public void RefreshMap()
    {
        for (int i = 0; i < masks.Length; i++)
        {
            int thisMaskID = currentCellID + maskIDoffsets[i];
            if (thisMaskID >= 0 && thisMaskID < currentMap.Length)
            {
                if (currentMap[thisMaskID] == 1 || (currentMap[thisMaskID] == 3 && PlayState.gameOptions[13] == 1))
                {
                    masks[i].GetComponent<SpriteMask>().enabled = false;
                    if (PlayState.bossLocations.Contains(thisMaskID))
                    {
                        anims[i + 4].Play("Minimap_icon_boss", true);
                        UpdatePlayerIcon("Minimap_icon_playerHighlight", thisMaskID == currentCellID);
                    }
                    else if (PlayState.saveLocations.Contains(thisMaskID))
                    {
                        anims[i + 4].Play("Minimap_icon_save", true);
                        UpdatePlayerIcon("Minimap_icon_playerHighlight", thisMaskID == currentCellID);
                    }
                    else if (PlayState.itemLocations.Contains(thisMaskID))
                    {
                        anims[i + 4].Play("Minimap_icon_itemNormal", true);
                        UpdatePlayerIcon("Minimap_icon_playerHighlight", thisMaskID == currentCellID);
                    }
                    else
                    {
                        anims[i + 4].Play("Minimap_icon_blank", true);
                        UpdatePlayerIcon("Minimap_icon_playerNormal", thisMaskID == currentCellID);
                    }
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
