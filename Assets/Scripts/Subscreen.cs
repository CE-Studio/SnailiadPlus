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
    List<GameObject> cells = new();
    List<SpriteRenderer> cellsWithMarkers = new();

    private readonly Vector2 topLeftCell = new(-6.25f, 5.25f);

    GameObject map;
    GameObject playerMarker;
    GameObject cellSelector;
    GameObject helixIcon;
    TextObject helixCount;
    List<float> helixCountOriginYs = new();
    private const float HELIX_COUNT_OFF_Y = 16.5f;
    private float helixCountPos = HELIX_COUNT_OFF_Y;
    private const float BOTTOM_BUTTON_OFF_Y = 13.5f;
    private float mapButtonPos = 0;
    private float mapButtonOriginY;
    private float radarSparkleTimer = -1f;
    private const float SPARKLE_TIMER_SPEED = 3f;
    private Vector2 radarSparkleOrigin = Vector2.zero;
    private Vector2 radarSparkleRadii = new(4, 1);

    AnimationModule anim;
    AnimationModule mapAnim;
    AnimationModule playerAnim;
    AnimationModule selectorAnim;
    AnimationModule helixAnim;

    List<TextObject> texts = new();

    private GameObject pauseButton;
    private TextObject mapButton;

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
        anim.affectedByGlobalEntityColor = false;
        mapAnim.Add("Minimap");
        mapAnim.Play("Minimap");
        mapAnim.pauseOnMenu = false;
        mapAnim.affectedByGlobalEntityColor = false;

        playerMarker = new GameObject("Player Marker");
        playerMarker.transform.parent = map.transform;
        SpriteRenderer markerSprite = playerMarker.AddComponent<SpriteRenderer>();
        markerSprite.sortingOrder = 5;
        playerAnim = playerMarker.AddComponent<AnimationModule>();
        playerAnim.Add("Minimap_icon_playerNormal");
        playerAnim.Add("Minimap_icon_playerHighlight");
        playerAnim.pauseOnMenu = false;
        playerAnim.affectedByGlobalEntityColor = false;

        cellSelector = new GameObject("Selector");
        cellSelector.transform.parent = map.transform;
        SpriteRenderer selectorSprite = cellSelector.AddComponent<SpriteRenderer>();
        selectorSprite.sortingOrder = 6;
        selectorAnim = cellSelector.AddComponent<AnimationModule>();
        selectorAnim.Add("GenericSelector_8");
        selectorAnim.pauseOnMenu = false;
        selectorAnim.affectedByGlobalEntityColor = false;
        cellSelector.SetActive(false);

        for (int i = 2; i < transform.childCount; i++)
            texts.Add(transform.GetChild(i).GetComponent<TextObject>());

        helixIcon = transform.Find("Helix Fragment").gameObject;
        helixCount = transform.Find("Helix Count").GetComponent<TextObject>();
        helixAnim = helixIcon.GetComponent<AnimationModule>();
        helixAnim.Add("Item_helixFragment");
        helixAnim.pauseOnMenu = false;
        helixAnim.affectedByGlobalEntityColor = false;
        helixCountOriginYs.Add(helixIcon.transform.localPosition.y);
        helixCountOriginYs.Add(helixCount.position.y);
        helixIcon.transform.localPosition = new Vector2(helixIcon.transform.localPosition.x, helixCountOriginYs[0] + HELIX_COUNT_OFF_Y);
        helixCount.position = new Vector2(helixCount.position.x, helixCountOriginYs[1] + HELIX_COUNT_OFF_Y);
        texts[22].position = new Vector2(texts[22].position.x, helixCountOriginYs[1] + HELIX_COUNT_OFF_Y);

        int cellID = 0;
        for (int y = 0; y < PlayState.WORLD_SIZE.y; y++)
        {
            for (int x = 0; x < PlayState.WORLD_SIZE.x; x++)
            {
                GameObject newCell = new("Cell " + cellID.ToString());
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
                cellAnim.affectedByGlobalEntityColor = false;
                newCell.transform.localPosition = new Vector2(topLeftCell.x + (x * 0.5f), topLeftCell.y - (y * 0.5f));
                cells.Add(newCell);
                cellID++;
            }
        }

        pauseButton = PlayState.TogglableHUDElements[11].transform.Find("Pause").gameObject;
        mapButton = PlayState.TogglableHUDElements[11].transform.Find("Map").GetComponent<TextObject>();
        mapButtonOriginY = mapButton.position.y;
    }

    void Update()
    {
        if (!buttonDown && PlayState.gameState == PlayState.GameState.game && PlayState.creditsState == 0 && Control.Map() && !PlayState.isInBossRush)
        {
            menuOpen = true;
            PlayState.gameState = PlayState.GameState.map;
            PlayState.ToggleHUD(false);
            PlayState.TogglableHUDElements[1].SetActive(true);
            PlayState.TogglableHUDElements[3].SetActive(true);
            PlayState.TogglableHUDElements[11].SetActive(true);
            pauseButton.SetActive(false);
            PlayState.ScreenFlash("Solid Color", 0, 0, 0, 0);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 150, 0.25f, 0, -2);
            buttonDown = true;
            anim.Play("Subscreen_" + PlayState.currentProfile.character.ToLower());
            helixAnim.Play("Item_helixFragment");
            UpdateCells();
            UpdateText();
        }
        if (!buttonDown && PlayState.gameState == PlayState.GameState.map && !isSelecting && (Control.Pause() || Control.Map()))
        {
            menuOpen = false;
            PlayState.gameState = PlayState.GameState.game;
            pauseButton.SetActive(true);
            PlayState.ToggleHUD(true);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.25f, 0, 999);
            buttonDown = true;
            PlayState.minimapScript.RefreshMap();
        }
        if (buttonDown && !Control.JumpHold() && !Control.Map() && !Control.Pause())
            buttonDown = false;

        transform.localPosition = new Vector2(0, Mathf.Lerp(transform.localPosition.y, menuOpen ? 0 : -15, 10 * Time.deltaTime));
        helixCountPos = Mathf.Lerp(helixCountPos, menuOpen ? 0 : HELIX_COUNT_OFF_Y, 10 * Time.deltaTime);
        helixIcon.transform.localPosition = new Vector2(helixIcon.transform.localPosition.x, helixCountOriginYs[0] + helixCountPos);
        helixCount.position = new Vector2(helixCount.position.x, helixCountOriginYs[1] + helixCountPos);
        texts[22].position = new Vector2(texts[22].position.x, helixCountOriginYs[1] + helixCountPos);
        PlayState.TogglableHUDElements[3].transform.localPosition = new Vector2(0, Mathf.Lerp(PlayState.TogglableHUDElements[3].transform.localPosition.y,
            menuOpen ? BOTTOM_BUTTON_OFF_Y : 0, 10 * Time.deltaTime));
        mapButtonPos = Mathf.Lerp(mapButtonPos, menuOpen ? BOTTOM_BUTTON_OFF_Y : 0, 10 * Time.deltaTime);
        mapButton.position = new Vector2(mapButton.position.x, mapButtonOriginY + mapButtonPos);

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
                        PlayState.currentProfile.exploredMap[currentlySelectedCell] -= 10;
                    }
                    else
                    {
                        PlayState.playerMarkerLocations.Add(currentlySelectedCell, "placeholder for multiplayer name");
                        PlayState.currentProfile.exploredMap[currentlySelectedCell] += 10;
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

            radarSparkleOrigin = (Vector2)texts[22].transform.position + new Vector2(texts[22].GetWidth(true) * 0.5f, -0.375f);
            if (radarSparkleTimer >= 0)
            {
                radarSparkleTimer = (radarSparkleTimer + SPARKLE_TIMER_SPEED * Time.deltaTime) % Mathf.PI;
                Vector2 spawnRadius = new(Mathf.Cos(radarSparkleTimer) * radarSparkleRadii.x, -Mathf.Sin(radarSparkleTimer) * radarSparkleRadii.y);
                PlayState.RequestParticle(radarSparkleOrigin + spawnRadius, "sparkle");
                PlayState.RequestParticle(radarSparkleOrigin - spawnRadius, "sparkle");
            }
        }
    }

    private void UpdateCells()
    {
        cellsWithMarkers.Clear();
        for (int i = 0; i < cells.Count; i++)
        {
            int thisCellValue = (PlayState.currentProfile.exploredMap[i] < 10) ? PlayState.currentProfile.exploredMap[i] : (PlayState.currentProfile.exploredMap[i] - 10);
            if (thisCellValue == 1 || thisCellValue == 11 || ((thisCellValue == 3 || thisCellValue == 13) && PlayState.generalData.secretMapTilesVisible))
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
                    int thisItemId = PlayState.baseItemLocations[PlayState.itemLocations[i]];
                    if (PlayState.isRandomGame)
                        thisItemId = PlayState.currentRando.itemLocations[PlayState.itemLocations[i]];
                    if (thisItemId != -1)
                    {
                        if (PlayState.currentProfile.items[thisItemId] == 0 && PlayState.GetItemAvailabilityThisCharacter(thisItemId) &&
                            PlayState.GetItemAvailabilityThisDifficulty(thisItemId))
                            cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_itemNormal", true);
                        else
                            cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_itemCollected", true);
                    }
                    else
                        cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_blank", true);
                }
            }
            else
            {
                cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_blank", true);
                if (PlayState.playerMarkerLocations.ContainsKey(i))
                {
                    cells[i].GetComponent<AnimationModule>().Play("Minimap_icon_marker", true);
                    cellsWithMarkers.Add(cells[i].GetComponent<SpriteRenderer>());
                }
                cells[i].GetComponent<SpriteMask>().enabled = thisCellValue != -1 || thisCellValue != 9;
            }
        }

        Vector2 playerCellPos = PlayState.WorldPosToMapPos(PlayState.player.transform.position);
        playerMarker.transform.localPosition = new Vector2(topLeftCell.x + (playerCellPos.x * 0.5f), topLeftCell.y - (playerCellPos.y * 0.5f));
        int playerCellID = PlayState.WorldPosToMapGridID(PlayState.player.transform.position);
        bool isItemCell = false;
        if (PlayState.itemLocations.ContainsKey(playerCellID))
        {
            //if (PlayState.GetItemAvailabilityThisCharacter(PlayState.isRandomGame ? PlayState.currentRando.itemLocations[playerCellID] : PlayState.baseItemLocations[playerCellID]) &&
            //    PlayState.GetItemAvailabilityThisDifficulty(PlayState.isRandomGame ? PlayState.currentRando.itemLocations[playerCellID] : PlayState.baseItemLocations[playerCellID]))
            //    isItemCell = true;
            int locationID = PlayState.itemLocations[playerCellID];
            int itemID = PlayState.isRandomGame ? PlayState.currentRando.itemLocations[locationID] : PlayState.baseItemLocations[locationID];
            if (PlayState.GetItemAvailabilityThisCharacter(itemID) && PlayState.GetItemAvailabilityThisDifficulty(itemID) && PlayState.currentProfile.items[itemID] == 1)
                isItemCell = true;
        }
        //if (PlayState.playerMarkerLocations.ContainsKey(playerCellID) || PlayState.bossLocations.Contains(playerCellID) || PlayState.saveLocations.Contains(playerCellID) ||
        //    (PlayState.itemLocations.ContainsKey(playerCellID) && PlayState.currentProfile.items[PlayState.itemLocations[playerCellID]] == 0 &&
        //    PlayState.GetItemAvailabilityThisCharacter(PlayState.itemLocations[playerCellID]) && PlayState.GetItemAvailabilityThisDifficulty(PlayState.itemLocations[playerCellID])))
        if (PlayState.playerMarkerLocations.ContainsKey(playerCellID) || PlayState.bossLocations.Contains(playerCellID) ||
            PlayState.saveLocations.Contains(playerCellID) || isItemCell)
            playerMarker.GetComponent<AnimationModule>().Play("Minimap_icon_playerHighlight");
        else
            playerMarker.GetComponent<AnimationModule>().Play("Minimap_icon_playerNormal");
    }

    private void UpdateText()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            string newText = "";
            string shell = PlayState.currentProfile.character == "Sluggy" ? PlayState.GetText("species_sluggy") : (PlayState.currentProfile.character == "Blobby" ?
                PlayState.GetText("species_blobby") : (PlayState.currentProfile.character == "Leechy" ? PlayState.GetText("species_leechy") :
                PlayState.GetText("subscreen_shell")));
            bool hasShell = shell == PlayState.GetText("subscreen_shell");
            switch (i)
            {
                case 0:
                    newText = PlayState.GetText("subscreen_header_name");
                    break;
                case 1:
                    newText = PlayState.GetText("char_full_" + PlayState.currentProfile.character.ToLower());
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
                    newText = string.Format(PlayState.GetText("subscreen_shellNormal"), PlayState.currentProfile.character == "Sluggy" ? PlayState.GetText("species_sluggy") :
                        (PlayState.currentProfile.character == "Blobby" ? PlayState.GetText("species_blobby") : (PlayState.currentProfile.character == "Leechy" ?
                        PlayState.GetText("species_leechy") : PlayState.GetText("subscreen_shell"))));
                    break;
                case 8:
                    if (PlayState.CheckForItem("Ice Snail") || PlayState.CheckForItem("Gravity Snail") || PlayState.CheckForItem("Full-Metal Snail"))
                        newText = string.Format(PlayState.GetText("item_iceSnail"), shell);
                    break;
                case 9:
                    if (PlayState.CheckForItem("Gravity Snail") || PlayState.CheckForItem("Full-Metal Snail"))
                        newText = string.Format(PlayState.GetText(PlayState.currentProfile.character switch {
                            "Upside" => "item_magneticFoot",
                            "Leggy" => "item_corkscrewJump",
                            "Blobby" => "item_angelJump",
                            _ => "item_gravitySnail"
                        }), shell);
                    break;
                case 10:
                    if (PlayState.CheckForItem("Full-Metal Snail"))
                        newText = string.Format(PlayState.GetText(hasShell ? "item_fullMetalSnail_generic" : (PlayState.currentProfile.character == "Blobby" ?
                            "item_fullMetalSnail_blob" : "item_fullMetalSnail_noShell")), shell);
                    break;
                case 11:
                    newText = PlayState.GetText("subscreen_header_ability");
                    break;
                case 12:
                    if (PlayState.CheckForItem("Shell Shield") && !(PlayState.currentProfile.character == "Sluggy" || PlayState.currentProfile.character == "Leechy"))
                        newText = PlayState.GetText(PlayState.currentProfile.character == "Blobby" ? "item_shelmet" : "item_shellShield");
                    break;
                case 13:
                    if (PlayState.CheckForItem("High Jump"))
                        newText = PlayState.GetText(PlayState.currentProfile.character == "Blobby" ? "item_wallGrab" : "item_highJump");
                    break;
                case 14:
                    if (PlayState.CheckForItem("Rapid Fire"))
                        newText = PlayState.GetText(PlayState.currentProfile.character == "Leechy" ? "item_backfire" : "item_rapidFire");
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
                    newText = string.Format(PlayState.GetText("subscreen_markers"), Control.lastInputIsCon ? Control.ParseButtonName(4) : Control.ParseKeyName(4));
                    break;
                case 18:
                    newText = string.Format(PlayState.GetText("subscreen_mapRate"), PlayState.GetMapPercentage().ToString());
                    break;
                case 19:
                    newText = string.Format(PlayState.GetText("subscreen_itemRate"), PlayState.GetItemPercentage().ToString());
                    break;
                case 20:
                    newText = string.Format(PlayState.GetText("subscreen_time"), PlayState.GetTimeString());
                    break;
                case 21:
                    newText = "X " + PlayState.CountFragments();
                    break;
                case 22:
                    if (PlayState.currentArea >= (int)PlayState.Areas.BossRush)
                    {
                        newText = "";
                        break;
                    }
                    int[] itemData = PlayState.GetAreaItemRate(PlayState.currentArea);
                    newText = PlayState.IsBossAlive(3) ? "" : string.Format(PlayState.GetText("subscreen_areaItemRate"), itemData[0], itemData[1],
                        itemData[2] == 1 ? "?" : "");
                    if (!PlayState.IsBossAlive(3))
                        radarSparkleTimer = itemData[3] == 1 ? 0 : -1;
                    else
                        radarSparkleTimer = -1;
                    break;
            }
            texts[i].SetText(newText);
            if (i == 22)
                radarSparkleRadii = new Vector2(texts[22].GetWidth(true) * 0.5f + 0.25f, 0.75f);
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