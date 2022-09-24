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
    List<GameObject> helixCount = new List<GameObject>();
    List<float> helixCountOriginYs = new List<float>();

    AnimationModule anim;
    AnimationModule mapAnim;
    AnimationModule playerAnim;
    AnimationModule selectorAnim;
    AnimationModule helixAnim;

    List<TextMesh> texts = new List<TextMesh>();

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

        helixCount.Add(transform.GetChild(1).gameObject);
        helixCount.Add(transform.GetChild(23).gameObject);
        helixAnim = helixCount[0].GetComponent<AnimationModule>();
        helixAnim.Add("Item_helixFragment");
        helixAnim.pauseOnMenu = false;
        helixCountOriginYs.Add(helixCount[0].transform.localPosition.y);
        helixCountOriginYs.Add(helixCount[1].transform.localPosition.y);
        helixCount[0].transform.localPosition = new Vector2(helixCount[0].transform.localPosition.x, helixCountOriginYs[0] + 16.5f);
        helixCount[1].transform.localPosition = new Vector2(helixCount[1].transform.localPosition.x, helixCountOriginYs[1] + 16.5f);

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
                cellMask.frontSortingOrder = 2;
                cellMask.backSortingOrder = 0;
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

        for (int i = 2; i < transform.childCount; i++)
        {
            texts.Add(transform.GetChild(i).GetChild(0).GetComponent<TextMesh>());
            texts.Add(transform.GetChild(i).GetChild(1).GetComponent<TextMesh>());
        }
    }

    void Update()
    {
        if (!buttonDown && PlayState.gameState == "Game" && Control.Map())
        {
            menuOpen = true;
            PlayState.gameState = "Map";
            PlayState.ToggleHUD(false);
            PlayState.TogglableHUDElements[1].SetActive(true);
            PlayState.TogglableHUDElements[3].SetActive(true);
            PlayState.ScreenFlash("Solid Color", 0, 0, 0, 0);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 150, 0.25f, -2);
            buttonDown = true;
            anim.Play("Subscreen_" + PlayState.currentCharacter.ToLower());
            helixAnim.Play("Item_helixFragment");
            UpdateCells();
            UpdateText();
        }
        if (!buttonDown && PlayState.gameState == "Map" && !isSelecting && (Control.Pause() || Control.Map()))
        {
            menuOpen = false;
            PlayState.gameState = "Game";
            PlayState.ToggleHUD(true);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.25f, 999);
            buttonDown = true;
            PlayState.minimapScript.RefreshMap();
        }
        if (buttonDown && !Control.JumpHold() && !Control.Map() && !Control.Pause())
            buttonDown = false;

        transform.localPosition = new Vector2(0, Mathf.Lerp(transform.localPosition.y, menuOpen ? 0 : -15, 10 * Time.deltaTime));
        for (int i = 0; i < helixCount.Count; i++)
            helixCount[i].transform.localPosition = new Vector2(helixCount[i].transform.localPosition.x,
                Mathf.Lerp(helixCount[i].transform.localPosition.y, menuOpen ? helixCountOriginYs[i] : helixCountOriginYs[i] + 16.5f, 10 * Time.deltaTime));
        PlayState.TogglableHUDElements[3].transform.localPosition = new Vector2(0, Mathf.Lerp(PlayState.TogglableHUDElements[3].transform.localPosition.y,
            menuOpen ? 13.5f : 0, 10 * Time.deltaTime));

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
                    {
                        PlayState.playerMarkerLocations.Remove(currentlySelectedCell);
                        PlayState.minimapScript.currentMap[currentlySelectedCell] -= 10;
                    }
                    else
                    {
                        PlayState.playerMarkerLocations.Add(currentlySelectedCell, "placeholder for multiplayer name");
                        PlayState.minimapScript.currentMap[currentlySelectedCell] += 10;
                    }
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
                PlayState.PlaySound("MenuBeep2");
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
            int thisCellValue = (PlayState.minimapScript.currentMap[i] < 10) ? PlayState.minimapScript.currentMap[i] : (PlayState.minimapScript.currentMap[i] - 10);
            if (thisCellValue == 0 || thisCellValue == 2)
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

    private void UpdateText()
    {
        for (int i = 0; i < texts.Count; i += 2)
        {
            string newText = "";
            string shell = PlayState.currentCharacter == "Sluggy" ? PlayState.GetText("species_sluggy") : (PlayState.currentCharacter == "Blobby" ?
                PlayState.GetText("species_blobby") : (PlayState.currentCharacter == "Leechy" ? PlayState.GetText("species_leechy") : PlayState.GetText("subscreen_shell")));
            bool hasShell = shell == PlayState.GetText("subscreen_shell");
            switch (Mathf.RoundToInt(i * 0.5f))
            {
                case 0:
                    newText = PlayState.GetText("subscreen_header_name");
                    break;
                case 1:
                    newText = PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower());
                    break;
                case 2:
                    newText = PlayState.GetText("subscreen_header_weapon");
                    break;
                case 3:
                    if (PlayState.CheckForItem("Peashooter"))
                        newText = PlayState.GetText("item_peashooter");
                    break;
                case 4:
                    if (PlayState.CheckForItem("Boomerang") || PlayState.CheckForItem("Super Secret Boomerang"))
                        newText = PlayState.GetText("item_boomerang");
                    break;
                case 5:
                    if (PlayState.CheckForItem("Rainbow Wave") || PlayState.CheckForItem("Debug Rainbow Wave"))
                        newText = PlayState.GetText("item_rainbowWave");
                    break;
                case 6:
                    newText = PlayState.GetText("subscreen_header_shell");
                    break;
                case 7:
                    newText = PlayState.GetText("subscreen_shellNormal").Replace("_", PlayState.currentCharacter == "Sluggy" ? PlayState.GetText("species_sluggy") :
                        (PlayState.currentCharacter == "Blobby" ? PlayState.GetText("species_blobby") : (PlayState.currentCharacter == "Leechy" ?
                        PlayState.GetText("species_leechy") : PlayState.GetText("subscreen_shell"))));
                    break;
                case 8:
                    if (PlayState.CheckForItem("Ice Snail") || PlayState.CheckForItem("Gravity Snail") || PlayState.CheckForItem("Full-Metal Snail"))
                        newText = PlayState.GetText("item_iceSnail").Replace("_", shell);
                    break;
                case 9:
                    if (PlayState.CheckForItem("Gravity Snail") || PlayState.CheckForItem("Full-Metal Snail"))
                        newText = PlayState.GetText(PlayState.currentCharacter switch {
                            "Upside" => "item_magneticFoot",
                            "Leggy" => "item_corkscrewJump",
                            "Blobby" => "item_angelJump",
                            _ => "item_gravitySnail"
                        }).Replace("_", shell);
                    break;
                case 10:
                    if (PlayState.CheckForItem("Full-Metal Snail"))
                        newText = PlayState.GetText(hasShell ? "item_fullMetalSnail_generic" : (PlayState.currentCharacter == "Blobby" ?
                            "item_fullMetalSnail_blob" : "item_fullMetalSnail_noShell")).Replace("_", shell);
                    break;
                case 11:
                    newText = PlayState.GetText("subscreen_header_ability");
                    break;
                case 12:
                    if (PlayState.CheckForItem("Shell Shield") && !(PlayState.currentCharacter == "Sluggy" || PlayState.currentCharacter == "Leechy"))
                        newText = PlayState.GetText(PlayState.currentCharacter == "Blobby" ? "item_shelmet" : "item_shellShield");
                    break;
                case 13:
                    if (PlayState.CheckForItem("High Jump"))
                        newText = PlayState.GetText(PlayState.currentCharacter == "Blobby" ? "item_wallGrab" : "item_highJump");
                    break;
                case 14:
                    if (PlayState.CheckForItem("Rapid Fire"))
                        newText = PlayState.GetText(PlayState.currentCharacter == "Leechy" ? "item_backfire" : "item_rapidFire");
                    break;
                case 15:
                    if (PlayState.CheckForItem("Devastator"))
                        newText = PlayState.GetText("item_devastator");
                    break;
                case 16:
                    if (PlayState.CheckForItem("Gravity Shock"))
                        newText = PlayState.GetText("item_gravityShock");
                    break;
                case 17:
                    newText = PlayState.GetText("subscreen_markers").Replace("_", Control.ParseKeyName(4));
                    break;
                case 18:
                    newText = PlayState.GetText("subscreen_mapRate").Replace("##", PlayState.GetMapPercentage().ToString());
                    break;
                case 19:
                    newText = PlayState.GetText("subscreen_itemRate").Replace("##", PlayState.GetItemPercentage().ToString());
                    break;
                case 20:
                    newText = PlayState.GetText("subscreen_time").Replace("_", PlayState.GetTimeString());
                    break;
                case 21:
                    newText = "X " + PlayState.helixCount;
                    break;
            }
            texts[i].text = newText;
            texts[i + 1].text = newText;
        }
    }

    public void RefreshAnims()
    {
        anim.ReloadList();
        mapAnim.ReloadList();
        playerAnim.ReloadList();
        selectorAnim.ReloadList();
        helixAnim.ReloadList();
        foreach (GameObject cell in cells)
            cell.GetComponent<AnimationModule>().ReloadList();
    }
}