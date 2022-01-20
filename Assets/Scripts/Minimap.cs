using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject player;
    public GameObject minimap;

    private int lastCellID;
    private Vector2 origin = new Vector2(0.5f, 0.5f);

    private GameObject[] masks = new GameObject[] { };
    private int[] maskIDoffsets = new int[]
    {
        -55, -54, -53, -52, -51, -50, -49,
        -29, -28, -27, -26, -25, -24, -23,
        -3, -2, -1, 0, 1, 2, 3,
        23, 24, 25, 26, 27, 28, 29,
        49, 50, 51, 52, 53, 54, 45
    };

    public int[] currentMap = new int[] { };

    void Start()
    {
        currentMap = PlayState.defaultMinimapState;

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
    }

    void Update()
    {
        minimap.transform.localPosition = new Vector2(
            -Mathf.Round((origin.x + player.transform.position.x - 1 + 13) / 26) * 0.5f + 0.25f,
            -Mathf.Round((origin.y + player.transform.position.y - 1 + 8) / 16) * 0.5f + 0.25f
            );
        if (CurrentCellID() >= 0 && CurrentCellID() < currentMap.Length && !PlayState.playerScript.inDeathCutscene)
        {
            if (currentMap[CurrentCellID()] == 0 || currentMap[CurrentCellID()] == 2)
                currentMap[CurrentCellID()]++;
        }
        if (lastCellID != CurrentCellID())
        {
            for (int i = 0; i < masks.Length; i++)
            {
                if (CurrentCellID() + maskIDoffsets[i] >= 0 && CurrentCellID() + maskIDoffsets[i] < currentMap.Length)
                {
                    if (currentMap[CurrentCellID() + maskIDoffsets[i]] == 1 || currentMap[CurrentCellID() + maskIDoffsets[i]] == 3)
                        masks[i].SetActive(false);
                    else
                        masks[i].SetActive(true);
                }
            }
        }
        lastCellID = CurrentCellID();
    }

    private int CurrentCellID()
    {
        return Mathf.RoundToInt(Mathf.Abs((minimap.transform.localPosition.x - 6.5f - (origin.x * 0.5f)) * 2) +
            (Mathf.Abs(minimap.transform.localPosition.y + 5.5f - (origin.y * 0.5f)) * 2) * 26);
    }
}
