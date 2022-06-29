using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subscreen : MonoBehaviour
{
    bool menuOpen = false;
    bool buttonDown = false;
    bool isSelecting = false;
    int currentlySelectedCell = 0;
    float currentMarkerColor = 0;
    List<GameObject> cells = new List<GameObject>();
    List<SpriteRenderer> cellsWithMarkers = new List<SpriteRenderer>();

    private readonly Vector2 topLeftCell = new Vector2(-6.25f, 5.25f);

    GameObject map;
    GameObject playerMarker;
    GameObject cellSelector;

    AnimationModule anim;
    AnimationModule mapAnim;
    AnimationModule playerAnim;
    AnimationModule selectorAnim;

    void Start()
    {
        transform.localPosition = new Vector2(0, -15);

        map = transform.GetChild(0).gameObject;

        anim = GetComponent<AnimationModule>();
        mapAnim = map.GetComponent<AnimationModule>();
        anim.Add("Subscreen_snaily");
        anim.Add("Subscreen_sluggy");
        anim.Add("Subscreen_upside");
        anim.Add("Subscreen_leggy");
        anim.Add("Subscreen_blobby");
        anim.Add("Subscreen_leechy");
        anim.pauseOnMenu = false;
        mapAnim.Add("Minimap");
        mapAnim.Play("Minimap");
        anim.pauseOnMenu = false;

        playerMarker = new GameObject("Player Marker");
        playerMarker.transform.parent = map.transform;
        SpriteRenderer markerSprite = playerMarker.AddComponent<SpriteRenderer>();
        markerSprite.sortingOrder = 5;
        playerAnim = playerMarker.AddComponent<AnimationModule>();
        playerAnim.Add("Minimap_icon_playerNormal");
        playerAnim.Add("Minimap_icon_playerHighlight");
        playerAnim.pauseOnMenu = false;

        cellSelector = new GameObject("Selector");
        cellSelector.transform.parent = map.transform;
        SpriteRenderer selectorSprite = cellSelector.AddComponent<SpriteRenderer>();
        selectorSprite.sortingOrder = 6;
        selectorAnim = cellSelector.AddComponent<AnimationModule>();
        selectorAnim.Add("GenericSelector_8");
        selectorAnim.pauseOnMenu = false;
        cellSelector.SetActive(false);

        int cellID = 0;
        for (int y = 0; y < PlayState.WORLD_SIZE.y; y++)
        {
            for (int x = 0; x < PlayState.WORLD_SIZE.x; x++)
            {
                GameObject newCell = new GameObject("Cell " + cellID.ToString());
                newCell.transform.parent = map.transform;
                SpriteRenderer cellSprite = newCell.AddComponent<SpriteRenderer>();
                cellSprite.sortingOrder = 4;
                SpriteMask cellMask = newCell.AddComponent<SpriteMask>();
                cellMask.sprite = PlayState.BlankTexture(true);
                cellMask.alphaCutoff = 0;
                cellMask.isCustomRangeActive = true;
                cellMask.frontSortingOrder = 3;
                cellMask.backSortingOrder = 1;
                AnimationModule cellAnim = newCell.AddComponent<AnimationModule>();
                cellAnim.Add("Minimap_icon_blank");
                cellAnim.Add("Minimap_icon_itemNormal");
                cellAnim.Add("Minimap_icon_itemCollected");
                cellAnim.Add("Minimap_icon_save");
                cellAnim.Add("Minimap_icon_boss");
                cellAnim.Add("Minimap_icon_unknown");
                cellAnim.Add("Minimap_icon_marker");
                cellAnim.pauseOnMenu = false;
                newCell.transform.localPosition = new Vector2(topLeftCell.x + (x * 0.5f), topLeftCell.y - (y * 0.5f));
                cells.Add(newCell);
                cellID++;
            }
        }
    }

    void Update()
    {
        if (!buttonDown && PlayState.gameState == "Game" && Control.Map())
        {
            menuOpen = true;
            PlayState.gameState = "Map";
            PlayState.ToggleHUD(false);
            PlayState.ScreenFlash("Solid Color", 0, 0, 0, 0);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 75, 0.25f);
            PlayState.screenCover.sortingOrder = 0;
            buttonDown = true;
            anim.Play("Subscreen_" + PlayState.currentCharacter.ToLower());
            UpdateCells();
        }
        if (!buttonDown && PlayState.gameState == "Map" && !isSelecting && (Control.Pause() || Control.Map()))
        {
            menuOpen = false;
            PlayState.gameState = "Game";
            PlayState.ToggleHUD(true);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.25f);
            PlayState.screenCover.sortingOrder = 999;
            buttonDown = true;
            PlayState.minimapScript.RefreshMap();
        }
        if (buttonDown && !Control.JumpHold() && !Control.Map() && !Control.Pause())
            buttonDown = false;

        transform.localPosition = new Vector2(0, Mathf.Lerp(transform.localPosition.y, menuOpen ? 0 : -15, 10 * Time.deltaTime));

        if (menuOpen)
        {
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

            if (!buttonDown && Control.JumpPress())
            {
                if (isSelecting)
                {
                    if (PlayState.playerMarkerLocations.ContainsKey(currentlySelectedCell))
                        PlayState.playerMarkerLocations.Remove(currentlySelectedCell);
                    else
                        PlayState.playerMarkerLocations.Add(currentlySelectedCell, "placeholder for multiplayer name");
                    UpdateCells();
                }
                else
                {
                    cellSelector.SetActive(true);
                    selectorAnim.Play("GenericSelector_8");
                    cellSelector.transform.localPosition = playerMarker.transform.localPosition;
                    currentlySelectedCell = PlayState.WorldPosToMapGridID(PlayState.player.transform.position);
                    isSelecting = true;
                }
                buttonDown = true;
                PlayState.PlaySound("MenuBeep2");
            }
            if (!buttonDown && (Control.Map() || Control.Pause()) && isSelecting)
            {
                isSelecting = false;
                selectorAnim.Stop(true);
                cellSelector.SetActive(false);
                PlayState.PlaySound("MenuBeep1");
            }
            if (isSelecting && !buttonDown)
            {
                if (Control.LeftPress())
                {
                    currentlySelectedCell--;
                    if ((currentlySelectedCell + 1) % PlayState.WORLD_SIZE.x == 0)
                        currentlySelectedCell += (int)PlayState.WORLD_SIZE.x;
                    PlayState.PlaySound("MenuBeep1");
                }
                if (Control.RightPress())
                {
                    currentlySelectedCell++;
                    if (currentlySelectedCell % PlayState.WORLD_SIZE.x == 0)
                        currentlySelectedCell -= (int)PlayState.WORLD_SIZE.x;
                    PlayState.PlaySound("MenuBeep1");
                }
                if (Control.DownPress())
                {
                    currentlySelectedCell += (int)PlayState.WORLD_SIZE.x;
                    if (currentlySelectedCell >= (PlayState.WORLD_SIZE.x * PlayState.WORLD_SIZE.y))
                        currentlySelectedCell -= Mathf.RoundToInt(PlayState.WORLD_SIZE.x * PlayState.WORLD_SIZE.y);
                    PlayState.PlaySound("MenuBeep1");
                }
                if (Control.UpPress())
                {
                    currentlySelectedCell -= (int)PlayState.WORLD_SIZE.x;
                    if (currentlySelectedCell < 0)
                        currentlySelectedCell += Mathf.RoundToInt(PlayState.WORLD_SIZE.x * PlayState.WORLD_SIZE.y);
                    PlayState.PlaySound("MenuBeep1");
                }
                cellSelector.transform.localPosition = new Vector2(topLeftCell.x + (currentlySelectedCell % PlayState.WORLD_SIZE.x) * 0.5f,
                    topLeftCell.y - Mathf.Floor(currentlySelectedCell / PlayState.WORLD_SIZE.x) * 0.5f);
            }
        }
    }

    private void UpdateCells()
    {
        cellsWithMarkers.Clear();
        for (int i = 0; i < cells.Count; i++)
        {
            if (PlayState.minimapScript.currentMap[i] == 0 || PlayState.minimapScript.currentMap[i] == 2)
            {
                cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_blank", true);
                if (PlayState.playerMarkerLocations.ContainsKey(i))
                {
                    cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_marker", true);
                    cellsWithMarkers.Add(cells[i].GetComponent<SpriteRenderer>());
                }
                cells[i].GetComponent<SpriteMask>().enabled = true;
            }
            else
            {
                cells[i].GetComponent<SpriteMask>().enabled = false;
                cells[i].GetComponent<SpriteRenderer>().color = PlayState.GetColor("0312");
                if (PlayState.playerMarkerLocations.ContainsKey(i))
                {
                    cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_marker", true);
                    cellsWithMarkers.Add(cells[i].GetComponent<SpriteRenderer>());
                }
                else if (PlayState.bossLocations.Contains(i))
                    cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_boss", true);
                else if (PlayState.saveLocations.Contains(i))
                    cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_save", true);
                else if (PlayState.itemLocations.ContainsKey(i))
                {
                    if (PlayState.itemCollection[PlayState.itemLocations[i]] == 0)
                        cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_itemNormal", true);
                    else
                        cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_itemCollected", true);
                }
                else
                    cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_blank", true);
            }
        }

        Vector2 playerCellPos = PlayState.WorldPosToMapPos(PlayState.player.transform.position);
        playerMarker.transform.localPosition = new Vector2(topLeftCell.x + (playerCellPos.x * 0.5f), topLeftCell.y - (playerCellPos.y * 0.5f));
        int playerCellID = PlayState.WorldPosToMapGridID(PlayState.player.transform.position);
        if (PlayState.playerMarkerLocations.ContainsKey(playerCellID) || PlayState.bossLocations.Contains(playerCellID) || PlayState.saveLocations.Contains(playerCellID) ||
            (PlayState.itemLocations.ContainsKey(playerCellID) && PlayState.itemCollection[PlayState.itemLocations[playerCellID]] == 0))
            playerMarker.GetComponent<AnimationModule>().Play("Minimap_icon_playerHighlight");
        else
            playerMarker.GetComponent<AnimationModule>().Play("Minimap_icon_playerNormal");
    }
}
