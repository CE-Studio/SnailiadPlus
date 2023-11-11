using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public struct Button
    {
        public GameObject obj;
        public SpriteRenderer sprite;
        public string label;
        public string type;
        public int[] spriteIndeces;
        public string[] neighbors;
        public int currentState;
    };

    private const int UP = 0;
    private const int DOWN = 1;
    private const int LEFT = 2;
    private const int RIGHT = 3;

    public List<GameObject> menuObjects = new();
    public Transform parent;
    public List<Button> buttons = new();
    public GameObject selector;

    private bool menuOpen = false;
    private bool buttonDown = false;
    private readonly KeyCode terminalKey = KeyCode.BackQuote;
    private string currentSelected = "peashooter";
    private Dictionary<string, Vector2> buttonPosArray = new();
    
    private void Start()
    {
        parent = transform;

        selector = new GameObject();
        selector.transform.parent = parent;
        selector.transform.localPosition = Vector2.zero;
        selector.AddComponent<SpriteRenderer>();
        selector.GetComponent<SpriteRenderer>().sortingOrder = 2;
        selector.AddComponent<AnimationModule>();
        selector.GetComponent<AnimationModule>().Add("GenericSelector_16");
        selector.GetComponent<AnimationModule>().Play("GenericSelector_16");
        menuObjects.Add(selector);

        Vector2 itemGridZero = new(-4.5f, 2);
        Vector2 bossGridZero = new(1.5f, 2);

        AddButton(itemGridZero, "peashooter", "item_0", new int[] { 2, 3 }, new string[] { "noclip", "shellShield", "moonSnail", "boomerang" });
        AddButton(new Vector2(itemGridZero.x + 1, itemGridZero.y), "boomerang", "item_1", new int[] { 4, 5 }, new string[] { "damage", "rapidFire", "peashooter", "rainbowWave" });
        AddButton(new Vector2(itemGridZero.x + 2, itemGridZero.y), "rainbowWave", "item_2", new int[] { 6, 7 }, new string[] { "damage", "iceSnail", "boomerang", "devastator" });
        AddButton(new Vector2(itemGridZero.x + 3, itemGridZero.y), "devastator", "item_3", new int[] { 8, 9 }, new string[] { "damage", "gravitySnail", "rainbowWave", "highJump" });
        AddButton(new Vector2(itemGridZero.x + 4, itemGridZero.y), "highJump", "item_4", new int[] { 10, 11 }, new string[] { "damage", "fullMetalSnail", "devastator", "shellbreaker" });
        AddButton(new Vector2(itemGridZero.x, itemGridZero.y - 1), "shellShield", "item_5", new int[] { 12, 13 }, new string[] { "peashooter", "gravityShock", "moonSnail", "rapidFire" });
        AddButton(new Vector2(itemGridZero.x + 1, itemGridZero.y - 1), "rapidFire", "item_6", new int[] { 14, 15 }, new string[] { "boomerang", "superSecretBoomerang", "shellShield", "iceSnail" });
        AddButton(new Vector2(itemGridZero.x + 2, itemGridZero.y - 1), "iceSnail", "item_7", new int[] { 16, 17 }, new string[] { "rainbowWave", "debugRainbowWave", "rapidFire", "gravitySnail" });
        AddButton(new Vector2(itemGridZero.x + 3, itemGridZero.y - 1), "gravitySnail", "item_8", new int[] { 18, 19 }, new string[] { "devastator", "heartContainer", "iceSnail", "fullMetalSnail" });
        AddButton(new Vector2(itemGridZero.x + 4, itemGridZero.y - 1), "fullMetalSnail", "item_9", new int[] { 20, 21 }, new string[] { "highJump", "helixFragment", "gravitySnail", "shellbreaker" });
        AddButton(new Vector2(itemGridZero.x, itemGridZero.y - 2), "gravityShock", "item_10", new int[] { 22, 23 }, new string[] { "shellShield", "noclip", "player", "superSecretBoomerang" });
        AddButton(new Vector2(itemGridZero.x + 1, itemGridZero.y - 2), "superSecretBoomerang", "item_11", new int[] { 24, 25 }, new string[] { "rapidFire", "damage", "gravityShock", "debugRainbowWave" });
        AddButton(new Vector2(itemGridZero.x + 2, itemGridZero.y - 2), "debugRainbowWave", "item_12", new int[] { 26, 27 }, new string[] { "iceSnail", "damage", "superSecretBoomerang", "heartContainer" });
        AddButton(new Vector2(itemGridZero.x + 3, itemGridZero.y - 2), "heartContainer", "item_13", new int[] { 28, 29 }, new string[] { "gravitySnail", "damage", "debugRainbowWave", "helixFragment" });
        AddButton(new Vector2(itemGridZero.x + 4, itemGridZero.y - 2), "helixFragment", "item_14", new int[] { 30, 31 }, new string[] { "fullMetalSnail", "damage", "heartContainer", "player" });

        AddButton(bossGridZero, "shellbreaker", "boss_0", new int[] { 32, 33 }, new string[] { "player", "player", "highJump", "stompy" });
        AddButton(new Vector2(bossGridZero.x + 1, bossGridZero.y), "stompy", "boss_1", new int[] { 34, 35 }, new string[] { "player", "player", "shellbreaker", "spaceBox" });
        AddButton(new Vector2(bossGridZero.x + 2, bossGridZero.y), "spaceBox", "boss_2", new int[] { 36, 37 }, new string[] { "player", "player", "stompy", "moonSnail" });
        AddButton(new Vector2(bossGridZero.x + 3, bossGridZero.y), "moonSnail", "boss_3", new int[] { 38, 39 }, new string[] { "player", "player", "spaceBox", "peashooter" });

        AddButton(new Vector2(1.5f, 0), "player", "player", new int[] { 40, 41, 42, 43, 44, 45 }, new string[] { "shellbreaker", "shellbreaker", "helixFragment", "gravityShock" });

        AddButton(new Vector2(-4.5f, -2), "noclip", "option", new int[] { 0, 1 }, new string[] { "gravityShock", "peashooter", "damage", "damage" });
        AddButton(new Vector2(-3.5f, -2), "damage", "option", new int[] { 0, 1 }, new string[] { "superSecretBoomerang", "boomerang", "noclip", "noclip" });

        selector.transform.localPosition = buttonPosArray[currentSelected];
        foreach (GameObject obj in menuObjects)
            obj.SetActive(false);
    }

    private void Update()
    {
        if (!buttonDown && PlayState.gameState == PlayState.GameState.game && Control.Generic(terminalKey, true))
        {
            PlayState.gameState = PlayState.GameState.debug;
            menuOpen = true;
            foreach (GameObject obj in menuObjects)
                obj.SetActive(true);
            PlayState.ToggleHUD(false);
            PlayState.ScreenFlash("Solid Color", 0, 0, 0, 0);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 150, 0.25f, 0, 0);
            buttonDown = true;
            currentSelected = "peashooter";
            selector.transform.localPosition = buttonPosArray[currentSelected];
        }
        if (!buttonDown && PlayState.gameState == PlayState.GameState.debug && (Control.Generic(terminalKey, true) || Control.Pause(true)))
        {
            PlayState.gameState = PlayState.GameState.game;
            menuOpen = false;
            foreach (GameObject obj in menuObjects)
                obj.SetActive(false);
            PlayState.ToggleHUD(true);
            PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.25f, 0, 999);
            buttonDown = true;
        }
        if (buttonDown && !Control.Generic(terminalKey, true) && !Control.Pause(true))
            buttonDown = false;
        if (menuOpen)
        {
            selector.transform.localPosition = new Vector2(Mathf.Lerp(selector.transform.localPosition.x, buttonPosArray[currentSelected].x, 15 * Time.deltaTime),
                Mathf.Lerp(selector.transform.localPosition.y, buttonPosArray[currentSelected].y, 15 * Time.deltaTime));
            if (Control.UpPress(0, true))
            {
                currentSelected = GetNeighbor(UP);
                PlayState.PlaySound("MenuBeep1");
            }
            if (Control.DownPress(0, true))
            {
                currentSelected = GetNeighbor(DOWN);
                PlayState.PlaySound("MenuBeep1");
            }
            if (Control.LeftPress(0, true))
            {
                currentSelected = GetNeighbor(LEFT);
                PlayState.PlaySound("MenuBeep1");
            }
            if (Control.RightPress(0, true))
            {
                currentSelected = GetNeighbor(RIGHT);
                PlayState.PlaySound("MenuBeep1");
            }

            foreach (Button button in buttons)
            {
                string[] typeParts = button.type.Split('_');
                switch (typeParts[0])
                {
                    default:
                        button.sprite.sprite = GetSprite(0);
                        break;
                    case "item":
                        switch (button.label)
                        {
                            default:
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                {
                                    PlayState.currentProfile.items[int.Parse(typeParts[1])] = PlayState.currentProfile.items[int.Parse(typeParts[1])] == 0 ? 1 : 0;
                                    PlayState.minimapScript.RefreshMap();
                                    PlayState.globalFunctions.shellStateBuffer = PlayState.GetShellLevel();
                                }
                                button.sprite.sprite = GetSprite(PlayState.CheckForItem(int.Parse(typeParts[1])) ? button.spriteIndeces[1] : button.spriteIndeces[0]);
                                break;
                            case "heartContainer":
                                button.sprite.sprite = GetSprite(PlayState.CountHearts() > 0 ? button.spriteIndeces[1] : button.spriteIndeces[0]);
                                break;
                            case "helixFragment":
                                button.sprite.sprite = GetSprite(PlayState.CountFragments() > 0 ? button.spriteIndeces[1] : button.spriteIndeces[0]);
                                break;
                        }
                        break;
                    case "boss":
                        if (Control.JumpPress(0, true) && button.label == currentSelected)
                            PlayState.currentProfile.bossStates[int.Parse(typeParts[1])] = PlayState.currentProfile.bossStates[int.Parse(typeParts[1])] == 0 ? 1 : 0;
                        button.sprite.sprite = GetSprite(PlayState.IsBossAlive(int.Parse(typeParts[1])) ? button.spriteIndeces[1] : button.spriteIndeces[0]);
                        break;
                    case "player":
                        switch (PlayState.currentProfile.character)
                        {
                            case "Snaily":
                                button.sprite.sprite = GetSprite(button.spriteIndeces[0]);
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    //PlayState.currentProfile.character = "Sluggy";
                                    PlayState.SetPlayer("Sluggy");
                                break;
                            case "Sluggy":
                                button.sprite.sprite = GetSprite(button.spriteIndeces[1]);
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    //PlayState.currentProfile.character = "Upside";
                                    PlayState.SetPlayer("Snaily");
                                break;
                            case "Upside":
                                button.sprite.sprite = GetSprite(button.spriteIndeces[2]);
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    PlayState.currentProfile.character = "Leggy";
                                break;
                            case "Leggy":
                                button.sprite.sprite = GetSprite(button.spriteIndeces[3]);
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    PlayState.currentProfile.character = "Blobby";
                                break;
                            case "Blobby":
                                button.sprite.sprite = GetSprite(button.spriteIndeces[4]);
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    PlayState.currentProfile.character = "Leechy";
                                break;
                            case "Leechy":
                                button.sprite.sprite = GetSprite(button.spriteIndeces[5]);
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    PlayState.currentProfile.character = "Snaily";
                                break;
                        }
                        break;
                    case "option":
                        switch (button.label)
                        {
                            default:
                                button.sprite.sprite = GetSprite(0);
                                break;
                            case "noclip":
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    PlayState.noclipMode = !PlayState.noclipMode;
                                button.sprite.sprite = PlayState.noclipMode ? GetSprite(1) : GetSprite(0);
                                break;
                            case "damage":
                                if (Control.JumpPress(0, true) && button.label == currentSelected)
                                    PlayState.damageMult = !PlayState.damageMult;
                                button.sprite.sprite = PlayState.damageMult ? GetSprite(1) : GetSprite(0);
                                break;
                        }
                        break;
                }
            }
        }
    }

    private void AddButton(Vector2 position, string newLabel, string newType, int[] states, string[] newNeighbors)
    {
        Button newButton = new()
        {
            obj = new GameObject(),
            label = newLabel,
            type = newType,
            spriteIndeces = states,
            neighbors = newNeighbors,
            currentState = 0
        };
        newButton.sprite = newButton.obj.AddComponent<SpriteRenderer>();
        newButton.sprite.sortingOrder = 1;
        newButton.sprite.sprite = PlayState.GetSprite("UI/DebugIcons", newButton.spriteIndeces[0]);
        newButton.obj.transform.parent = parent;
        newButton.obj.transform.localPosition = position;
        buttons.Add(newButton);
        menuObjects.Add(newButton.obj);
        buttonPosArray.Add(newLabel, position);
    }

    private string GetNeighbor(int direction)
    {
        string[] foundNeighbors = null;
        int i = 0;
        while (i < buttons.Count && foundNeighbors == null)
        {
            if (buttons[i].label == currentSelected)
                foundNeighbors = buttons[i].neighbors;
            else
                i++;
        }
        if (foundNeighbors == null)
            return "peashooter";
        else
            return foundNeighbors[direction];
    }

    private Sprite GetSprite(int index)
    {
        return PlayState.GetSprite("UI/DebugIcons", index);
    }
}
