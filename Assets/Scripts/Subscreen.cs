using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subscreen : MonoBehaviour
{
    bool menuOpen = false;
    bool buttonDown = false;
    int currentMarkerColor = 0;
    List<SpriteRenderer> cellsWithMarkers = new List<SpriteRenderer>();

    private readonly Vector2 topLeftCell = new Vector2(-0.25f, 5.25f);

    GameObject map;
    GameObject playerMarker;

    AnimationModule anim;
    AnimationModule mapAnim;

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
        mapAnim.Add("Minimap");
        mapAnim.Play("Minimap");

        int cellID = 0;
        for (int y = 0; y < PlayState.WORLD_SIZE.y; y++)
        {
            for (int x = 0; x < PlayState.WORLD_SIZE.x; x++)
            {
                GameObject newMask = new GameObject("Cell " + cellID.ToString());
                SpriteRenderer cellSprite = newMask.AddComponent<SpriteRenderer>();
                SpriteMask cellMask = newMask.AddComponent<SpriteMask>();
                cellMask.frontSortingLayerID = 3;
                cellMask.backSortingLayerID = 2;
                AnimationModule cellAnim = newMask.AddComponent<AnimationModule>();
                cellAnim.Add("Minimap_icon_blank");
                cellAnim.Add("Minimap_icon_itemNormal");
                cellAnim.Add("Minimap_icon_itemCollected");
                cellAnim.Add("Minimap_icon_save");
                cellAnim.Add("Minimap_icon_boss");
                cellAnim.Add("Minimap_icon_unknown");
                cellAnim.Add("Minimap_icon_marker");
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
        }
        if (!buttonDown && PlayState.gameState == "Map" && (Control.Pause() || Control.Map()))
        {
            menuOpen = false;
            PlayState.gameState = "Game";
            PlayState.ToggleHUD(true);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.25f);
            PlayState.screenCover.sortingOrder = 999;
            buttonDown = true;
        }
        if (buttonDown && !Control.Map())
            buttonDown = false;

        transform.localPosition = new Vector2(0, Mathf.Lerp(transform.localPosition.y, menuOpen ? 0 : -15, 15 * Time.deltaTime));

        if (menuOpen)
        {
            foreach (SpriteRenderer thisSprite in cellsWithMarkers)
            {
                thisSprite.color = currentMarkerColor switch
                {
                    1 => PlayState.GetColor("0309"),
                    2 => PlayState.GetColor("0304"),
                    3 => PlayState.GetColor("0206"),
                    _ => PlayState.GetColor("0012")
                };
            }
            currentMarkerColor = (currentMarkerColor + 1) % 4;
        }
    }
}
