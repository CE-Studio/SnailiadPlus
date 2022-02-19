using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTrigger : MonoBehaviour
{
    public BoxCollider2D box;
    public bool active = true;

    public Vector2 parallaxForeground2Modifier = Vector2.zero;
    public Vector2 parallaxForeground1Modifier = Vector2.zero;
    public Vector2 parallaxBackgroundModifier = Vector2.zero;
    public Vector2 parallaxSkyModifier = Vector2.zero;

    public int areaID = 0;
    // 0 = Snail Town
    // 1 = Mare Carelia
    // 2 = Spiralis Silere
    // 3 = Amastrida Abyssus
    // 4 = Lux Lirata
    // 5 = ???
    // 6 = Shrine of Iris
    // 7 = Boss Rush
    public int areaSubzone = 0;

    public Vector2[] waterLevel = new Vector2[] { };

    public string[] environmentalEffects = new string[] { };
    // Supported effects
    //
    // - bubble
    // - star
    // - snow
    // - rain
    // - thunder
    // - dark
    // - fog
    // - heat
    private List<float> effectVars = new List<float>();
    private bool initializedEffects = false;

    public TextMesh roomNameText;
    public TextMesh roomNameShadow;

    public Tilemap bg;
    public Tilemap fg;
    public Tilemap breakableMap;
    public GameObject breakableBlock;
    
    void Start()
    {
        box = GetComponent<BoxCollider2D>();
        roomNameText = GameObject.Find("View/Minimap Panel/Room Name Parent/Room Name Text").GetComponent<TextMesh>();
        roomNameShadow = GameObject.Find("View/Minimap Panel/Room Name Parent/Room Name Shadow").GetComponent<TextMesh>();
        bg = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
        fg = GameObject.Find("Grid/Foreground").GetComponent<Tilemap>();
        breakableMap = GameObject.Find("Grid/Special").GetComponent<Tilemap>();
        breakableBlock = (GameObject)Resources.Load("Objects/Breakable Block");
        DespawnEverything();
        breakableMap.color = new Color32(255, 255, 255, 0);
    }

    void Update()
    {
        if (!active)
        {
            int effectVarIndex = 0;
            foreach (string effect in environmentalEffects)
            {
                switch (effect.ToLower())
                {
                    default:
                        break;
                    case "bubble":
                        if (!initializedEffects)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 bubblePos = new Vector2(Random.Range(0, box.size.x + 0.5f), 0);
                                bubblePos.y = Random.Range(0, waterLevel[WaterPoint(bubblePos.x)].y);
                                Vector2 truePos = new Vector2(transform.position.x - (box.size.x * 0.5f) + bubblePos.x, transform.position.y - (box.size.y * 0.5f) + bubblePos.y);
                                PlayState.RequestParticle(truePos, "bubble", transform.position.y - (box.size.y * 0.5f) + waterLevel[WaterPoint(bubblePos.x)].y);
                            }
                            effectVars.Add(Random.Range(0f, 1f) * 12);
                        }
                        else
                        {
                            if (effectVars[effectVarIndex] <= 0)
                            {
                                Vector2 bubblePos = new Vector2(Random.Range(0, box.size.x + 0.5f), 0);
                                bubblePos.y = Random.Range(0, waterLevel[WaterPoint(bubblePos.x)].y);
                                Vector2 truePos = new Vector2(transform.position.x - (box.size.x * 0.5f) + bubblePos.x, transform.position.y - (box.size.y * 0.5f) - 0.25f);
                                PlayState.RequestParticle(truePos, "bubble", transform.position.y - (box.size.y * 0.5f) + waterLevel[WaterPoint(bubblePos.x)].y);
                                effectVars[effectVarIndex] = Random.Range(0f, 1f) * 12;
                            }
                            else
                                effectVars[effectVarIndex] -= Time.deltaTime;
                        }
                        break;
                }
                effectVarIndex++;
            }

            if (waterLevel.Length > 0)
            {
                float playerY = PlayState.player.transform.position.y;
                float waterY = transform.position.y - (box.size.y * 0.5f) - 0.25f +
                    waterLevel[WaterPoint(PlayState.player.transform.position.x - transform.position.x - (box.size.x * 0.5f))].y;
                Debug.Log(playerY + ", " + waterY);
                if (((playerY > waterY && PlayState.playerScript.underwater) || (playerY < waterY && !PlayState.playerScript.underwater)) && initializedEffects)
                {
                    PlayState.RequestParticle(new Vector2(PlayState.player.transform.position.x, waterY + 0.5f), "splash");
                    PlayState.playerScript.underwater = !PlayState.playerScript.underwater;
                }
            }
            else
                PlayState.playerScript.underwater = false;

            initializedEffects = true;
        }
    }

    private int WaterPoint(float x)
    {
        bool foundPointLeftOf = false;
        int waterPoint = waterLevel.Length - 1;
        while (!foundPointLeftOf && waterPoint != -1)
        {
            if (x > waterLevel[waterPoint].x)
                foundPointLeftOf = true;
            else
                waterPoint--;
        }
        if (waterPoint == -1)
            waterPoint = 0;
        return waterPoint;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && active && PlayState.gameState == "Game")
        {
            PlayState.ResetAllParticles();
            effectVars.Clear();
            PlayState.camCenter = new Vector2(transform.position.x, transform.position.y);
            PlayState.camBoundaryBuffers = new Vector2((box.size.x + 0.5f) * 0.5f - 12.5f, (box.size.y + 0.5f) * 0.5f - 7.5f);
            PlayState.ScreenFlash("Room Transition", 0, 0, 0, 0);
            PlayState.parallaxFg2Mod = parallaxForeground2Modifier;
            PlayState.parallaxFg1Mod = parallaxForeground1Modifier;
            PlayState.parallaxBgMod = parallaxBackgroundModifier;
            PlayState.parallaxSkyMod = parallaxSkyModifier;
            PlayState.PlayAreaSong(areaID, areaSubzone);
            PlayState.CloseDialogue();
            PlayState.isTalking = false;

            PlayState.camTempBuffersX = Vector2.zero;
            PlayState.camTempBuffersY = Vector2.zero;
            Vector2 thisTriggerPos = new Vector2(areaID, transform.GetSiblingIndex());
            if (thisTriggerPos != PlayState.positionOfLastRoom)
            {
                Transform previousTrigger = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
                previousTrigger.GetComponent<Collider2D>().enabled = true;
                previousTrigger.GetComponent<RoomTrigger>().active = true;
                previousTrigger.GetComponent<RoomTrigger>().DespawnEverything();
                PlayState.positionOfLastRoom = thisTriggerPos;
            }

            string newRoomName = "";

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
                switch (child.name)
                {
                    default:
                        break;

                    case "Door":
                        if (Vector2.Distance(collision.transform.position, child.transform.position) < 2)
                            child.GetComponent<Door>().SetState1();
                        else
                            child.GetComponent<Door>().SetState2();
                        break;

                    case "Grass":
                        child.GetComponent<Grass>().Spawn();
                        break;
                    case "Power Grass":
                        child.GetComponent<PowerGrass>().Spawn();
                        break;

                    case "Save Point":
                        if (child.GetComponent<SavePoint>().hasBeenActivated)
                            child.GetComponent<SavePoint>().ToggleActiveState();
                        break;

                    case "Item":
                        child.GetComponent<Item>().SetAnim();
                        child.GetComponent<Item>().CheckIfCollected();
                        break;

                    case "Fake Boundary":
                        string thisRoomName = child.parent.name;
                        if (thisRoomName.Contains("/"))
                        {
                            FakeRoomBorder childScript = child.GetComponent<FakeRoomBorder>();
                            int stringHalf = 0;
                            if ((childScript.direction && childScript.initialPosRelative.x == 1) || (!childScript.direction && childScript.initialPosRelative.y == 1))
                                stringHalf = 1;
                            string splitString = thisRoomName.Split('/')[stringHalf];
                            foreach (char character in splitString)
                            {
                                if (character == '|')
                                    newRoomName += "\n";
                                else
                                    newRoomName += character;
                            }
                        }
                        break;

                    case "NPC":
                        child.transform.localPosition = child.GetComponent<NPC>().origin;
                        child.GetComponent<NPC>().velocity = 0;
                        break;
                }
            }

            if (newRoomName == "")
            {
                foreach (char character in transform.name)
                {
                    if (character == '|')
                        newRoomName += "\n";
                    else
                        newRoomName += character;
                }
            }
            roomNameText.text = newRoomName;
            roomNameShadow.text = newRoomName;

            CheckSpecialLayer();

            box.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            active = false;
        }
    }

    public void DespawnEverything()
    {
        initializedEffects = false;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            if (child.name == "Door")
            {
                child.GetComponent<Door>().SetStateDespawn();
            }
            else if (child.name.Contains("Grass"))
            {
                switch (child.name)
                {
                    default:
                        break;
                    case "Grass":
                        child.GetComponent<Grass>().ToggleActive(false);
                        break;
                    case "Power Grass":
                        child.GetComponent<PowerGrass>().ToggleActive(false);
                        break;
                }
            }
            else if (child.name.Contains("Breakable Block"))
            {
                child.GetComponent<BreakableBlock>().Despawn();
            }
            else if (child.name == "Enemy-Collidable Tile")
            {
                Destroy(child.gameObject);
            }
            else if (child.name == "Item")
            {
                child.transform.localPosition = child.GetComponent<Item>().originPos;
            }
        }
        GameObject pool = GameObject.Find("Player Bullet Pool");
        for (int i = 0; i < pool.transform.childCount; i++)
        {
            if (pool.transform.GetChild(i).transform.GetComponent<Bullet>().isActive)
            {
                pool.transform.GetChild(i).transform.GetComponent<Bullet>().Despawn();
            }
            pool.transform.GetChild(i).transform.position = Vector2.zero;
        }
    }

    private void CheckSpecialLayer()
    {
        int limitX = (int)Mathf.Round((box.size.x + 0.5f) * 0.5f + 1);
        int limitY = (int)Mathf.Round((box.size.y + 0.5f) * 0.5f + 1);
        for (int i = 0; i <= 1; i++)
        {
            for (int x = -limitX; x <= limitX; x++)
            {
                for (int y = -limitY; y <= limitY; y++)
                {
                    Vector3Int tilePos = new Vector3Int((int)Mathf.Round(transform.position.x) + x, (int)Mathf.Round(transform.position.y) + y, 0);
                    TileBase currentTile = breakableMap.GetTile(tilePos);
                    Sprite currentTileSprite = breakableMap.GetSprite(tilePos);
                    if (currentTile != null)
                    {
                        if (currentTileSprite.name == "Tilesheet_72" || currentTileSprite.name == "Tilesheet_73" || currentTileSprite.name == "Tilesheet_74" || currentTileSprite.name == "Tilesheet_439")
                        {
                            int weaponType = 0;
                            bool isSilent = false;
                            switch (currentTileSprite.name)
                            {
                                case "Tilesheet_72":
                                    weaponType = 2;
                                    break;
                                case "Tilesheet_73":
                                    weaponType = 3;
                                    break;
                                case "Tilesheet_74":
                                    weaponType = 4;
                                    break;
                                case "Tilesheet_439":
                                    weaponType = 4;
                                    isSilent = true;
                                    break;
                            }

                            GameObject Breakable = Instantiate(breakableBlock, new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f), Quaternion.identity);
                            Breakable.transform.parent = transform;
                            Breakable.GetComponent<BreakableBlock>().Instantiate(weaponType, isSilent);
                            switch (i)
                            {
                                case 0:
                                    Breakable.GetComponent<SpriteRenderer>().sortingOrder = -100;
                                    break;
                                case 1:
                                    Breakable.GetComponent<SpriteRenderer>().sortingOrder = -20;
                                    break;
                            }
                        }
                        else if (currentTileSprite.name == "Tilesheet_376")
                        {
                            GameObject EnemyCollideBlock = new GameObject();
                            EnemyCollideBlock.transform.parent = transform;
                            EnemyCollideBlock.transform.position = new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f);
                            EnemyCollideBlock.AddComponent<BoxCollider2D>();
                            EnemyCollideBlock.GetComponent<BoxCollider2D>().isTrigger = true;
                            EnemyCollideBlock.GetComponent<BoxCollider2D>().size = new Vector2(1, 1);
                            EnemyCollideBlock.layer = 9;
                            EnemyCollideBlock.name = "Enemy-Collidable Tile";
                            Physics2D.IgnoreCollision(EnemyCollideBlock.GetComponent<BoxCollider2D>(), PlayState.player.GetComponent<BoxCollider2D>(), true);
                        }
                    }
                }
            }
        }
    }
}
