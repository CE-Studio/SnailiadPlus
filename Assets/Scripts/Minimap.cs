using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject player;
    public GameObject minimap;

    private int currentCellID;                            // This integer tracks the specific tile ID currently in the center of the minimap
    private int lastCellID;
    private Vector2 origin = new Vector2(104.5f, 96.5f);  // This marks the centerpoint of the world map, to keep the minimap's tracking relative to the game world

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

        minimap.transform.localPosition = new Vector2(
            -Mathf.Round(((origin.x + player.transform.position.x - 1 - 13) / 26) + 0.5f) * 0.5f + 0.25f,
            -Mathf.Round(((origin.y + player.transform.position.y - 1 - 8) / 16) + 0.5f) * 0.5f + 0.25f
            );
        GetNewCurrentCellID();
    }

    void Update()
    {
        minimap.transform.localPosition = new Vector2(
            -Mathf.Round(((origin.x + player.transform.position.x - 1 - 13) / 26) + 0.5f) * 0.5f + 0.25f,
            -Mathf.Round(((origin.y + player.transform.position.y - 1 - 8) / 16) + 0.5f) * 0.5f + 0.25f
            );
        GetNewCurrentCellID();
        if (currentCellID >= 0 && currentCellID < currentMap.Length && !player.GetComponent<Player>().inDeathCutscene)
        {
            if (currentMap[currentCellID] == 0 || currentMap[currentCellID] == 2)
                currentMap[currentCellID]++;
        }
        if (lastCellID != currentCellID)
        {
            for (int i = 0; i < masks.Length; i++)
            {
                if (currentCellID + maskIDoffsets[i] >= 0 && currentCellID + maskIDoffsets[i] < currentMap.Length)
                {
                    if (currentMap[currentCellID + maskIDoffsets[i]] == 1 || currentMap[currentCellID + maskIDoffsets[i]] == 3)
                        masks[i].SetActive(false);
                    else
                        masks[i].SetActive(true);
                }
            }
        }
        lastCellID = currentCellID;
    }

    private void GetNewCurrentCellID()
    {
        currentCellID = (int)((Mathf.Abs((-Mathf.Round(((origin.x + player.transform.position.x - 1 - 13) / 26) + 0.5f) * 0.5f + 0.5f) * 2) + 13) +
            (Mathf.Abs((-Mathf.Round(((origin.y + player.transform.position.y - 1 - 8) / 16) + 0.5f) * 0.5f + 0.5f) * 2 + 10) * 26));
    }
}
