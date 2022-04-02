using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject player;
    public GameObject minimap;
    public AnimationModule[] anims;

    private int currentCellID;
    private int lastCellID;
    private Vector2 origin = new Vector2(0.5f, 0.5f);

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
            -Mathf.Round((origin.x + player.transform.position.x - 1 + 13) / 26) * 0.5f + 0.25f,
            -Mathf.Round((origin.y + player.transform.position.y - 1 + 8) / 16) * 0.5f + 0.25f
            );
        currentCellID = CurrentCellID();
        if (currentCellID >= 0 && currentCellID < currentMap.Length && !PlayState.playerScript.inDeathCutscene)
        {
            if (currentMap[currentCellID] == 0 || currentMap[currentCellID] == 2)
                currentMap[currentCellID]++;
        }
        if (lastCellID != currentCellID)
            RefreshMap();
        lastCellID = currentCellID;
    }

    private int CurrentCellID()
    {
        return Mathf.RoundToInt(Mathf.Abs((minimap.transform.localPosition.x - 6.5f - (origin.x * 0.5f)) * 2) +
            (Mathf.Abs(minimap.transform.localPosition.y + 5.5f - (origin.y * 0.5f)) * 2) * 26);
    }

    public void RefreshMap()
    {
        for (int i = 0; i < masks.Length; i++)
        {
            if (currentCellID + maskIDoffsets[i] >= 0 && currentCellID + maskIDoffsets[i] < currentMap.Length)
            {
                if (currentMap[currentCellID + maskIDoffsets[i]] == 1 || currentMap[currentCellID + maskIDoffsets[i]] == 3)
                //masks[i].SetActive(false);
                {
                    masks[i].GetComponent<SpriteMask>().enabled = false;
                    anims[i + 4].Play("Minimap_icon_blank", true);
                }
                else
                //masks[i].SetActive(true);
                {
                    masks[i].GetComponent<SpriteMask>().enabled = true;
                    anims[i + 4].Play("Minimap_icon_blank", true);
                }
            }
        }
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
