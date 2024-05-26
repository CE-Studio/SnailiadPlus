using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTrigger : MonoBehaviour
{
    public BoxCollider2D box;
    public bool active = true; // Kinda named backward. If this is true, the collider attached to this room is enabled, waiting for the player to enter the room
    private bool temporarilyActive = false;
    private float initializationBuffer = 0;

    public Vector2 parallaxForeground2Modifier = Vector2.zero;
    public Vector2 parallaxForeground1Modifier = Vector2.zero;
    public Vector2 parallaxBackgroundModifier = Vector2.zero;
    public Vector2 parallaxSkyModifier = Vector2.zero;
    public Vector2 offsetForeground2 = Vector2.zero;
    public Vector2 offsetForeground1 = Vector2.zero;
    public Vector2 offsetBackground = Vector2.zero;
    public Vector2 offsetSky = Vector2.zero;

    private Vector2[] tempBGValues = new Vector2[8];
    private Vector2 tempCamReturnPoint = Vector2.zero;

    public int areaID = 0;
    // 0 = Snail Town
    // 1 = Mare Carelia
    // 2 = Spiralis Silere
    // 3 = Amastrida Abyssus
    // 4 = Lux Lirata
    // 5 = Shrine of Iris
    // 6 = Boss Rush
    public int areaSubzone = 0;
    public bool isSnelkRoom = false;

    public Vector2[] waterLevel = new Vector2[] { };

    public string[] environmentalEffects = new string[] { };
    // Supported effects
    //
    // - bubble
    // - star
    // - snow
    // - rain
    // - thunder
    // - fog
    // - heat
    // - glitch
    private List<float> effectVars = new();
    private bool initializedEffects = false;
    private float splashTimeout = 0;
    private List<Particle> effectParticles = new();

    public float darknessLevel = 0f;

    public struct RoomCommand {
        public string name;
        public string[] args;
    };
    public string[] roomCommands = new string[] { };

    public Tilemap bg;
    public Tilemap fg;
    public Tilemap specialMap;
    public GameObject breakableBlock;

    public Dictionary<string, object>[] roomContent;
    public string[] roomContentTypes;
    public Vector3[] roomContentPos;
    public PlayState.Breakable[] breakables;
    public PlayState.Breakable[] finalBossTiles;

    void Awake() {
        box = GetComponent<BoxCollider2D>();
        bg = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
        fg = GameObject.Find("Grid/Foreground").GetComponent<Tilemap>();
        specialMap = GameObject.Find("Grid/Special").GetComponent<Tilemap>();
        breakableBlock = (GameObject)Resources.Load("Objects/Breakable Block");
        specialMap.color = new Color32(255, 255, 255, 0);
    }

    void Update()
    {
        if (!active)
        {
            if (box.enabled)
            {
                active = true;
                return;
            }

            if (initializationBuffer > 0)
                initializationBuffer -= Time.deltaTime;
            splashTimeout = Mathf.Clamp(splashTimeout - Time.deltaTime, 0, Mathf.Infinity);

            int effectVarIndex = 0;
            foreach (string effect in environmentalEffects)
            {
                string effMain = effect.ToLower();
                string effType = "";
                if (effect.Contains('_'))
                {
                    string[] effParts = effect.Split('_');
                    effMain = effParts[0].ToLower();
                    effType = effParts[1].ToLower();
                }

                switch (effMain)
                {
                    default:
                        break;
                    case "bubble":
                        if (!initializedEffects)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 bubblePos = new(Random.Range(0, box.size.x + 0.5f), 0);
                                bubblePos.y = Random.Range(0, waterLevel[WaterPoint(bubblePos.x)].y);
                                Vector2 truePos = new(transform.position.x - (box.size.x * 0.5f) + bubblePos.x, transform.position.y - (box.size.y * 0.5f) + bubblePos.y);
                                effectParticles.Add(PlayState.RequestParticle(truePos, "bubble",
                                    new float[] { transform.position.y - (box.size.y * 0.5f) + waterLevel[WaterPoint(bubblePos.x)].y, 0 }));
                            }
                            effectVars.Add(Random.Range(0f, 1f) * 12);
                        }
                        else
                        {
                            if (effectVars[effectVarIndex] <= 0)
                            {
                                Vector2 bubblePos = new(Random.Range(0, box.size.x + 0.5f), 0);
                                bubblePos.y = Random.Range(0, waterLevel[WaterPoint(bubblePos.x)].y);
                                Vector2 truePos = new(transform.position.x - (box.size.x * 0.5f) + bubblePos.x, transform.position.y - (box.size.y * 0.5f) - 0.25f);
                                effectParticles.Add(PlayState.RequestParticle(truePos, "bubble",
                                    new float[] { transform.position.y - (box.size.y * 0.5f) + waterLevel[WaterPoint(bubblePos.x)].y, 0 }));
                                effectVars[effectVarIndex] = Random.Range(0f, 1f) * 12;
                            }
                            else
                            {
                                if (PlayState.gameState == PlayState.GameState.game)
                                    effectVars[effectVarIndex] -= Time.deltaTime;
                            }
                        }
                        break;
                    case "fog":
                        if (!initializedEffects)
                        {
                            Vector2 randPos = (Vector2)PlayState.cam.transform.position + new Vector2(Random.Range(-24f, -8f), Random.Range(0f, 16f));
                            PlayState.RequestParticle(randPos, "fog");
                            PlayState.RequestParticle(randPos + (Vector2.right * 16), "fog");
                            PlayState.RequestParticle(randPos + (Vector2.right * 32), "fog");
                            PlayState.RequestParticle(randPos + (Vector2.down * 16), "fog");
                            PlayState.RequestParticle(randPos + new Vector2(16, -16), "fog");
                            PlayState.RequestParticle(randPos + new Vector2(32, -16), "fog");
                        }
                        break;
                    case "heat":
                        if (!initializedEffects)
                            effectVars.Add(Random.Range(0f, 1f) * 0.5f);
                        if (PlayState.gameState == PlayState.GameState.game)
                        {
                            effectVars[effectVarIndex] -= Time.deltaTime;
                            if (effectVars[effectVarIndex] <= 0)
                            {
                                effectVars[effectVarIndex] = Random.Range(0f, 1f) * 0.5f;
                                Vector2 truePos = new(PlayState.cam.transform.position.x + Random.Range(-12.5f, 12.5f), PlayState.cam.transform.position.y - 7.5f);
                                effectParticles.Add(PlayState.RequestParticle(truePos, "heat"));
                            }
                        }
                        break;
                    case "rain":
                        if (!initializedEffects)
                        {
                            for (int i = 0; i < 32; i++)
                            {
                                Vector2 rainPos = new(Random.Range(PlayState.cam.transform.position.x - 13f, PlayState.cam.transform.position.x + 13f),
                                    Random.Range(PlayState.cam.transform.position.y - 8f, PlayState.cam.transform.position.y + 8f));
                                effectParticles.Add(PlayState.RequestParticle(rainPos, "rain"));
                            }
                            effectVars.Add(0);
                        }
                        break;
                    case "snow":
                        if (!initializedEffects)
                        {
                            for (int i = 0; i < 60; i++)
                            {
                                Vector2 snowPos = new(Random.Range(PlayState.cam.transform.position.x - 13f, PlayState.cam.transform.position.x + 13f),
                                    Random.Range(PlayState.cam.transform.position.y - 8f, PlayState.cam.transform.position.y + 8f));
                                effectParticles.Add(PlayState.RequestParticle(snowPos, "snow"));
                            }
                            effectVars.Add(0);
                        }
                        break;
                    case "star":
                        if (!initializedEffects)
                        {
                            int typeID = effType switch
                            {
                                "n" => 0,
                                "ne" => 1,
                                "e" => 2,
                                "se" => 3,
                                "s" => 4,
                                "sw" => 5,
                                "w" => 6,
                                "nw" => 7,
                                "center" => 8,
                                "border" => 9,
                                _ => 6
                            };
                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 starPos = new(Random.Range(PlayState.cam.transform.position.x - 13f, PlayState.cam.transform.position.x + 13f),
                                    Random.Range(PlayState.cam.transform.position.y - 8f, PlayState.cam.transform.position.y + 8f));
                                effectParticles.Add(PlayState.RequestParticle(starPos, "star", new float[] { typeID }));
                            }
                            effectVars.Add(0);
                        }
                        break;
                    case "thunder":
                        if (!initializedEffects)
                        {
                            for (int i = 0; i < 52; i++)
                            {
                                Vector2 rainPos = new(Random.Range(PlayState.cam.transform.position.x - 13f, PlayState.cam.transform.position.x + 13f),
                                    Random.Range(PlayState.cam.transform.position.y - 8f, PlayState.cam.transform.position.y + 8f));
                                effectParticles.Add(PlayState.RequestParticle(rainPos, "rain"));
                            }
                            effectVars.Add(Random.Range(5f, 10f));
                        }
                        else if ((PlayState.generalData.particleState == 1 || PlayState.generalData.particleState == 5) && PlayState.gameState == PlayState.GameState.game)
                        {
                            effectVars[effectVarIndex] -= Time.deltaTime;
                            if (effectVars[effectVarIndex] <= 0)
                            {
                                effectVars[effectVarIndex] = Random.Range(5f, 10f);
                                PlayState.RequestParticle(PlayState.cam.transform.position + new Vector3(Random.Range(-11f, 11f), 3.5f, 0), "lightning");
                                PlayState.PlaySound("Thunder");
                                PlayState.entityColor = Color.black;
                            }
                            float colAdd = Time.deltaTime * 0.5f;
                            PlayState.entityColor = new Color(PlayState.entityColor.r + colAdd, PlayState.entityColor.g + colAdd, PlayState.entityColor.b + colAdd);
                            PlayState.bgLayer.color = PlayState.entityColor;
                            PlayState.groundLayer.color = PlayState.entityColor;
                            PlayState.fg1Layer.color = PlayState.entityColor;
                            PlayState.fg2Layer.color = PlayState.entityColor;
                        }
                        break;
                }
                effectVarIndex++;
            }
            if (temporarilyActive)
                foreach (Particle particle in effectParticles)
                    particle.runInMenu = true;

            if (waterLevel.Length > 0)
            {
                float playerY = PlayState.player.transform.position.y;
                float waterY = GetWaterLevelAt(PlayState.player.transform.position.x);
                if ((playerY > waterY && PlayState.playerScript.underwater) || (playerY < waterY && !PlayState.playerScript.underwater))
                {
                    if (initializedEffects)
                    {
                        if (initializationBuffer <= 0 && splashTimeout <= 0)
                        {
                            PlayState.RequestParticle(new Vector2(PlayState.player.transform.position.x, waterY + 0.5f), "splash", true);
                            if (playerY < waterY)
                            {
                                for (int i = Random.Range(2, 8); i > 0; i--)
                                    PlayState.RequestParticle(new Vector2(PlayState.player.transform.position.x, waterY - 0.5f), "bubble", new float[] { waterY, 1 });
                            }
                        }
                        splashTimeout = 0.125f;
                    }
                    PlayState.playerScript.underwater = playerY < waterY;
                }
            }
            else
                PlayState.playerScript.underwater = false;

            initializedEffects = true;
        }
    }

    public void ResetEffects()
    {
        initializedEffects = false;
        Update();
    }

    private int WaterPoint(float x)
    {
        bool foundPointLeftOf = false;
        float relativeX = x - transform.position.x + (box.size.x * 0.5f);
        int waterPoint = waterLevel.Length - 1;
        while (!foundPointLeftOf && waterPoint != -1)
        {
            if (relativeX > waterLevel[waterPoint].x)
                foundPointLeftOf = true;
            else
                waterPoint--;
        }
        if (waterPoint == -1)
            waterPoint = 0;
        return waterPoint;
    }

    public float GetWaterLevelAt(float x)
    {
        int waterPoint = WaterPoint(x);
        float waterY = transform.position.y - (box.size.y * 0.5f) - 0.25f + waterLevel[waterPoint].y;
        return waterY;
    }

    public void RemoteActivateRoom(bool temporary = false)
    {
        if (active)
        {
            if (temporary)
            {
                temporarilyActive = true;
                tempBGValues = new Vector2[]
                {
                    PlayState.parallaxFg2Mod, PlayState.parallaxFg1Mod, PlayState.parallaxBgMod, PlayState.parallaxSkyMod,
                    PlayState.fg2Offset, PlayState.fg1Offset, PlayState.bgOffset, PlayState.skyOffset
                };
                tempCamReturnPoint = PlayState.cam.transform.position;
            }
            else
            {
                PlayState.ResetAllParticles();
                PlayState.breakablePositions.Clear();
            }
            effectVars.Clear();
            PlayState.parallaxFg2Mod = parallaxForeground2Modifier;
            PlayState.parallaxFg1Mod = parallaxForeground1Modifier;
            PlayState.parallaxBgMod = parallaxBackgroundModifier;
            PlayState.parallaxSkyMod = parallaxSkyModifier;
            PlayState.fg2Offset = offsetForeground2;
            PlayState.fg1Offset = offsetForeground1;
            PlayState.bgOffset = offsetBackground;
            PlayState.skyOffset = offsetSky;

            initializationBuffer = 0.25f;
            box.enabled = false;
            active = false;

            CheckSpecialLayer();
            SpawnFromInternalList();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && active && PlayState.gameState == PlayState.GameState.game)
        {
            PlayState.ResetAllParticles();
            PlayState.entityColor = Color.white;
            PlayState.ResetTilemapColors();
            effectVars.Clear();
            PlayState.camCenter = new Vector2(transform.position.x, transform.position.y);
            PlayState.camBoundaryBuffers = new Vector2((box.size.x + 0.5f) * 0.5f - 12.5f, (box.size.y + 0.5f) * 0.5f - 7.5f);
            PlayState.ScreenFlash("Room Transition", 0, 0, 0, 0);
            PlayState.parallaxFg2Mod = parallaxForeground2Modifier;
            PlayState.parallaxFg1Mod = parallaxForeground1Modifier;
            PlayState.parallaxBgMod = parallaxBackgroundModifier;
            PlayState.parallaxSkyMod = parallaxSkyModifier;
            PlayState.fg2Offset = offsetForeground2;
            PlayState.fg1Offset = offsetForeground1;
            PlayState.bgOffset = offsetBackground;
            PlayState.skyOffset = offsetSky;
            PlayState.SetDarkness(darknessLevel);
            PlayState.PlayAreaSong(areaID, areaSubzone, isSnelkRoom);
            PlayState.CloseDialogue();
            PlayState.isTalking = false;
            PlayState.currentRoom = this;

            if (!box.bounds.Contains(PlayState.player.transform.position))
            {
                Vector2 normal = box.ClosestPoint(PlayState.player.transform.position) - (Vector2)PlayState.player.transform.position;
                PlayState.player.transform.position += (Vector3)normal;
            }

            PlayState.camTempBuffersX = Vector2.zero;
            PlayState.camTempBuffersY = Vector2.zero;
            Vector2 thisTriggerPos = new(areaID, transform.GetSiblingIndex());
            initializationBuffer = 0.25f;
            if (thisTriggerPos != PlayState.positionOfLastRoom)
            {
                RoomTrigger previousTrigger = PlayState.LastRoom();
                previousTrigger.GetComponent<Collider2D>().enabled = true;
                previousTrigger.active = true;
                previousTrigger.DespawnEverything();
                PlayState.positionOfLastRoom = thisTriggerPos;
            }

            string newRoomName = "";

            if (newRoomName == "")
            {
                foreach (char character in PlayState.GetText("room_" + (areaID < 10 ? "0" : "") + areaID + "_" + transform.name))
                {
                    if (character == '|')
                        newRoomName += "\n";
                    else
                        newRoomName += character;
                }
            }
            if (PlayState.currentArea == (int)PlayState.Areas.ShrineOfIris && PlayState.currentSubzone == 1 && PlayState.GetNPCVar(PlayState.NPCVarIDs.HasSeenIris) != 1)
                newRoomName = "";
            PlayState.hudRoomName.SetText(newRoomName);

            PlayState.breakablePositions.Clear();
            CheckSpecialLayer();
            SpawnFromInternalList();

            box.enabled = false;

            for (int i = 0; i < roomCommands.Length; i++)
            {
                string[] command = roomCommands[i].Replace(" ", "").Split(',');
                switch (command[0].ToLower())
                {
                    default:
                        Debug.LogWarning("Unknown room command \"" + command[0] + "\"");
                        break;
                    case "setmaptile":
                        Vector2 mapPos = new(int.Parse(command[1]), int.Parse(command[2]));
                        PlayState.SetMapTile(mapPos, bool.Parse(command[3]));
                        break;
                    case "achievement":
                        AchievementPanel.Achievements thisAchievement =
                            (AchievementPanel.Achievements)System.Enum.Parse(typeof(AchievementPanel.Achievements), command[1]);
                        PlayState.QueueAchievementPopup(thisAchievement);
                        break;
                    case "resetbossrush":
                        if (areaID == (int)PlayState.Areas.BossRush && PlayState.isInBossRush)
                        {
                            for (int j = 0; j < PlayState.currentProfile.bossStates.Length; j++)
                                PlayState.currentProfile.bossStates[j] = 1;
                            for (int j = 0; j < PlayState.currentProfile.gameTime.Length; j++)
                                PlayState.currentProfile.gameTime[j] = 0;
                            PlayState.incrementRushTimer = false;
                            PlayState.hudRushTime.SetText("");
                            PlayState.activeRushData = PlayState.defaultRushData;
                            PlayState.globalFunctions.RemoveGigaBackgroundLayers();
                        }
                        break;
                    case "endbossrush":
                        PlayState.player.transform.position += 2f * (PlayState.playerScript.facingLeft ? Vector3.left : Vector3.right);
                        PlayState.suppressPause = true;
                        PlayState.globalFunctions.RunBossRushResults();
                        break;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            active = false;
    }

    public void DespawnEverything()
    {
        initializedEffects = false;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            IRoomObject roomObject = (IRoomObject)obj.GetComponent(typeof(IRoomObject));
            if (roomObject != null)
            {
                Dictionary<string, object> datout = roomObject.resave();
                if (datout != null)
                {
                    foreach (KeyValuePair<string, object> h in datout)
                    {
                        print(h.Key + ", " + h.Value + ", " + roomContent.Length);
                        roomContent[i][h.Key] = h.Value;
                    }
                }
            }
            Destroy(obj);
        }
        GameObject playerPool = GameObject.Find("Player Bullet Pool");
        for (int i = 0; i < playerPool.transform.childCount; i++)
        {
            Bullet thisBullet = playerPool.transform.GetChild(i).transform.GetComponent<Bullet>();
            if (thisBullet.isActive && thisBullet.bulletType < 7)
            {
                thisBullet.Despawn();
                thisBullet.transform.position = Vector2.zero;
            }
        }
        for (int i = 0; i < PlayState.enemyBulletPool.transform.childCount; i++)
        {
            if (PlayState.enemyBulletPool.transform.GetChild(i).transform.GetComponent<EnemyBullet>().isActive)
                PlayState.enemyBulletPool.transform.GetChild(i).transform.GetComponent<EnemyBullet>().Despawn();
            PlayState.enemyBulletPool.transform.GetChild(i).transform.position = Vector2.zero;
        }
        PlayState.ReplaceAllTempTiles();
        PlayState.enemyGlobalMoveIndex = 0;
        PlayState.activeTargets.Clear();
        PlayState.finalBossTiles.Clear();
        if (temporarilyActive)
        {
            temporarilyActive = false;
            active = true;
            foreach (Particle particle in effectParticles)
                particle.ResetParticle();

            PlayState.parallaxFg2Mod = tempBGValues[0];
            PlayState.parallaxFg1Mod = tempBGValues[1];
            PlayState.parallaxBgMod = tempBGValues[2];
            PlayState.parallaxSkyMod = tempBGValues[3];
            PlayState.fg2Offset = tempBGValues[4];
            PlayState.fg1Offset = tempBGValues[5];
            PlayState.bgOffset = tempBGValues[6];
            PlayState.skyOffset = tempBGValues[7];
            PlayState.cam.transform.position = tempCamReturnPoint;
        }
        effectParticles.Clear();
    }

    private void CheckSpecialLayer() {
        int limitX = (int)Mathf.Round((box.size.x + 0.5f) * 0.5f + 1);
        int limitY = (int)Mathf.Round((box.size.y + 0.5f) * 0.5f + 1);

        for (int x = -limitX; x <= limitX; x++) {
            for (int y = -limitY; y <= limitY; y++) {
                Vector3Int tilePos = new((int)Mathf.Round(transform.position.x) + x, (int)Mathf.Round(transform.position.y) + y, 0);
                Vector2 worldPos = new(tilePos.x + 0.5f, tilePos.y + 0.5f);
                TileBase currentTile = specialMap.GetTile(tilePos);
                if (currentTile != null) {
                    switch (int.Parse(specialMap.GetSprite(tilePos).name.Split('_')[1])) {
                        default:
                            break;
                        case 4:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Blob"), worldPos, Quaternion.identity, transform);
                            break;
                        case 5:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Blub"), worldPos, Quaternion.identity, transform);
                            break;
                        case 6:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Devilblob"), worldPos, Quaternion.identity, transform);
                            break;
                        case 7:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Chirpy (blue)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 8:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Kitty (orange)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 9:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Kitty (gray)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 10:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Chirpy (blue) Generator"), worldPos, Quaternion.identity, transform);
                            break;
                        case 11:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spikey (blue)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 12:
                            GameObject reversedBlueSpikey = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spikey (blue)"), worldPos, Quaternion.identity, transform);
                            reversedBlueSpikey.GetComponent<Spikey1>().rotation = true;
                            break;
                        case 13:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spikey (orange)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 14:
                            GameObject reversedOrangeSpikey = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spikey (orange)"), worldPos, Quaternion.identity, transform);
                            reversedOrangeSpikey.GetComponent<Spikey2>().rotation = true;
                            break;
                        case 15:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Fireball"), worldPos, Quaternion.identity, transform);
                            break;
                        case 16:
                            GameObject reversedFireball = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Fireball"), worldPos, Quaternion.identity, transform);
                            reversedFireball.GetComponent<Fireball1>().rotation = true;
                            break;
                        case 17:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Iceball"), worldPos, Quaternion.identity, transform);
                            break;
                        case 18:
                            GameObject reversedIceball = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Iceball"), worldPos, Quaternion.identity, transform);
                            reversedIceball.GetComponent<Fireball2>().rotation = true;
                            break;
                        case 19:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Ghost Dandelion Generator"), worldPos, Quaternion.identity, transform);
                            break;
                        case 23:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Shellbreaker"), worldPos, Quaternion.identity, transform);
                            break;
                        case 24:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Stompy"), worldPos, Quaternion.identity, transform);
                            break;
                        case 25:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Space Box"), worldPos, Quaternion.identity, transform);
                            break;
                        case 26:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Moon Snail"), worldPos, Quaternion.identity, transform);
                            break;
                        case 27:
                            Instantiate(Resources.Load<GameObject>("Objects/Grass"), worldPos, Quaternion.identity, transform);
                            break;
                        case 30:
                            Instantiate(Resources.Load<GameObject>("Objects/Power Grass"), worldPos, Quaternion.identity, transform);
                            break;
                        case 31:
                            PlayState.RequestParticle(worldPos, "smoke");
                            break;
                        case 358:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Super Shellbreaker"), worldPos, Quaternion.identity, transform);
                            break;
                        case 359:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Vis Vires"), worldPos, Quaternion.identity, transform);
                            break;
                        case 360:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Time Cube"), worldPos, Quaternion.identity, transform);
                            break;
                        case 361:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Sun Snail"), worldPos, Quaternion.identity, transform);
                            break;
                        case 376:
                            GameObject block = new() { name = "Enemy-Collidable Tile", layer = 9 };
                            block.transform.parent = transform;
                            block.transform.position = worldPos;
                            BoxCollider2D box = block.AddComponent<BoxCollider2D>();
                            box.isTrigger = true;
                            box.size = new Vector2(1, 1);
                            Physics2D.IgnoreCollision(box, PlayState.player.GetComponent<BoxCollider2D>(), true);
                            break;
                        case 377:
                            GameObject iceSpikeDown = Instantiate(Resources.Load<GameObject>("Objects/Hazards/Ice Spike"), worldPos, Quaternion.identity, transform);
                            iceSpikeDown.GetComponent<IceSpike>().Spawn(0);
                            break;
                        case 378:
                            GameObject iceSpikeUp = Instantiate(Resources.Load<GameObject>("Objects/Hazards/Ice Spike"), worldPos, Quaternion.identity, transform);
                            iceSpikeUp.GetComponent<IceSpike>().Spawn(2);
                            break;
                        case 379:
                            GameObject iceSpikeLeft = Instantiate(Resources.Load<GameObject>("Objects/Hazards/Ice Spike"), worldPos, Quaternion.identity, transform);
                            iceSpikeLeft.GetComponent<IceSpike>().Spawn(1);
                            break;
                        case 380:
                            GameObject iceSpikeRight = Instantiate(Resources.Load<GameObject>("Objects/Hazards/Ice Spike"), worldPos, Quaternion.identity, transform);
                            iceSpikeRight.GetComponent<IceSpike>().Spawn(3);
                            break;
                        case 381:
                            GameObject cannon1Floor = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Canon"), worldPos, Quaternion.identity, transform);
                            cannon1Floor.GetComponent<Cannon1>().PlayAnim("floor", true);
                            break;
                        case 382:
                            GameObject cannon1WallL = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Canon"), worldPos, Quaternion.identity, transform);
                            cannon1WallL.GetComponent<Cannon1>().PlayAnim("wallL", true);
                            break;
                        case 383:
                            GameObject cannon1WallR = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Canon"), worldPos, Quaternion.identity, transform);
                            cannon1WallR.GetComponent<Cannon1>().PlayAnim("wallR", true);
                            break;
                        case 384:
                            GameObject cannon1Ceil = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Canon"), worldPos, Quaternion.identity, transform);
                            cannon1Ceil.GetComponent<Cannon1>().PlayAnim("ceiling", true);
                            break;
                        case 386:
                            Instantiate(Resources.Load<GameObject>("Objects/Hazards/Muck"), worldPos, Quaternion.identity, transform);
                            break;
                        case 387:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Ghost Dandelion"), worldPos, Quaternion.identity, transform);
                            break;
                        case 389:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Floatspike (black)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 393:
                            GameObject gearUp = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spinnygear"), worldPos, Quaternion.identity, transform);
                            gearUp.GetComponent<Spinnygear>().SetDirection(PlayState.EDirsCardinal.Up);
                            break;
                        case 394:
                            GameObject gearDown = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spinnygear"), worldPos, Quaternion.identity, transform);
                            gearDown.GetComponent<Spinnygear>().SetDirection(PlayState.EDirsCardinal.Down);
                            break;
                        case 395:
                            GameObject gearRight = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spinnygear"), worldPos, Quaternion.identity, transform);
                            gearRight.GetComponent<Spinnygear>().SetDirection(PlayState.EDirsCardinal.Right);
                            break;
                        case 396:
                            GameObject gearLeft = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spinnygear"), worldPos, Quaternion.identity, transform);
                            gearLeft.GetComponent<Spinnygear>().SetDirection(PlayState.EDirsCardinal.Left);
                            break;
                        case 397:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Chirpy (aqua)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 398:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Snakey (green)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 399:
                            GameObject pincerFloor = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Pincer"), worldPos, Quaternion.identity, transform);
                            pincerFloor.GetComponent<Pincer>().SetGravity(PlayState.EDirsCardinal.Down);
                            break;
                        case 400:
                            GameObject pincerCeiling = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Pincer"), worldPos, Quaternion.identity, transform);
                            pincerCeiling.GetComponent<Pincer>().SetGravity(PlayState.EDirsCardinal.Up);
                            break;
                        case 401:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Jellyfish"), worldPos, Quaternion.identity, transform);
                            break;
                        case 402:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Syngnathida"), worldPos, Quaternion.identity, transform);
                            break;
                        case 403:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Tallfish (normal)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 404:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Federation Drone"), worldPos, Quaternion.identity, transform);
                            break;
                        case 405:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Walleye"), worldPos, Quaternion.identity, transform);
                            break;
                        case 406:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spider"), worldPos, Quaternion.identity, transform);
                            break;
                        case 407:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spider Mama"), worldPos, Quaternion.identity, transform);
                            break;
                        case 408:
                            GameObject greenTurtRight = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (green)"),
                                worldPos, Quaternion.identity, transform);
                            greenTurtRight.GetComponent<GravTurtle1>().SetGravity(GravTurtle1.Dirs.wallR);
                            break;
                        case 409:
                            GameObject greenTurtLeft = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (green)"),
                                worldPos, Quaternion.identity, transform);
                            greenTurtLeft.GetComponent<GravTurtle1>().SetGravity(GravTurtle1.Dirs.wallL);
                            break;
                        case 410:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Snakey (blue)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 411:
                            GameObject redTurtRight = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (cherry red)"),
                                worldPos, Quaternion.identity, transform);
                            redTurtRight.GetComponent<GravTurtle2>().SetGravity(GravTurtle2.Dirs.wallR);
                            break;
                        case 412:
                            GameObject redTurtLeft = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (cherry red)"),
                                worldPos, Quaternion.identity, transform);
                            redTurtLeft.GetComponent<GravTurtle2>().SetGravity(GravTurtle2.Dirs.wallL);
                            break;
                        case 413:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Balloon Buster"), worldPos, Quaternion.identity, transform);
                            break;
                        case 414:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Batty Bat"), worldPos, Quaternion.identity, transform);
                            break;
                        case 415:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Angelblob"), worldPos, Quaternion.identity, transform);
                            break;
                        case 416:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Tallfish (angry)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 417:
                            GameObject walleyeLeft = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Walleye"), worldPos, Quaternion.identity, transform);
                            walleyeLeft.GetComponent<Walleye>().Face(true);
                            break;
                        case 418:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Floatspike (blue)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 419:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Sky Viper"), worldPos, Quaternion.identity, transform);
                            break;
                        case 420:
                            GameObject cannon2Floor = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Non-Canon"), worldPos, Quaternion.identity, transform);
                            cannon2Floor.GetComponent<Cannon2>().PlayAnim("floor", true);
                            break;
                        case 421:
                            GameObject cannon2WallL = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Non-Canon"), worldPos, Quaternion.identity, transform);
                            cannon2WallL.GetComponent<Cannon2>().PlayAnim("wallL", true);
                            break;
                        case 422:
                            GameObject cannon2WallR = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Non-Canon"), worldPos, Quaternion.identity, transform);
                            cannon2WallR.GetComponent<Cannon2>().PlayAnim("wallR", true);
                            break;
                        case 423:
                            GameObject cannon2Ceil = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Non-Canon"), worldPos, Quaternion.identity, transform);
                            cannon2Ceil.GetComponent<Cannon2>().PlayAnim("ceiling", true);
                            break;
                        case 424:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Snelk"), worldPos, Quaternion.identity, transform);
                            break;
                        case 425:
                            GameObject runningSnelk = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Snelk"), worldPos, Quaternion.identity, transform);
                            runningSnelk.GetComponent<Snelk>().SetState(1);
                            break;
                        case 440:
                            GameObject collideTileFS1 = Instantiate(Resources.Load<GameObject>("Objects/Collision Tile"), worldPos, Quaternion.identity, transform);
                            collideTileFS1.GetComponent<CollisionTile>().Spawn(PlayState.EDirsSurface.Floor, CollisionTile.Types.QuarterStep);
                            break;
                        case 441:
                            GameObject collideTileFS2 = Instantiate(Resources.Load<GameObject>("Objects/Collision Tile"), worldPos, Quaternion.identity, transform);
                            collideTileFS2.GetComponent<CollisionTile>().Spawn(PlayState.EDirsSurface.Floor, CollisionTile.Types.HalfStep);
                            break;
                        case 442:
                            GameObject collideTileFS3 = Instantiate(Resources.Load<GameObject>("Objects/Collision Tile"), worldPos, Quaternion.identity, transform);
                            collideTileFS3.GetComponent<CollisionTile>().Spawn(PlayState.EDirsSurface.Floor, CollisionTile.Types.ThreeQuarterStep);
                            break;
                        case 443:
                            GameObject collideTileFS4 = Instantiate(Resources.Load<GameObject>("Objects/Collision Tile"), worldPos, Quaternion.identity, transform);
                            collideTileFS4.GetComponent<CollisionTile>().Spawn(PlayState.EDirsSurface.Floor, CollisionTile.Types.FullStep);
                            break;
                        case 445:
                            GameObject babyfishGreen = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Babyfish"), worldPos, Quaternion.identity, transform);
                            babyfishGreen.GetComponent<Babyfish>().AssignType(0);
                            break;
                        case 446:
                            GameObject babyfishPink = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Babyfish"), worldPos, Quaternion.identity, transform);
                            babyfishPink.GetComponent<Babyfish>().AssignType(1);
                            break;
                        case 451:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Chirpy (aqua) Generator"), worldPos, Quaternion.identity, transform);
                            break;
                        case 452:
                            GameObject sleepingSnelk = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Snelk"), worldPos, Quaternion.identity, transform);
                            sleepingSnelk.GetComponent<Snelk>().SetState(2);
                            break;
                        case 453:
                            GameObject greenTurtDown = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (green)"),
                                worldPos, Quaternion.identity, transform);
                            greenTurtDown.GetComponent<GravTurtle1>().SetGravity(GravTurtle1.Dirs.floor);
                            break;
                        case 454:
                            GameObject greenTurtUp = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (green)"),
                                worldPos, Quaternion.identity, transform);
                            greenTurtUp.GetComponent<GravTurtle1>().SetGravity(GravTurtle1.Dirs.ceiling);
                            break;
                        case 455:
                            GameObject redTurtDown = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (cherry red)"),
                                worldPos, Quaternion.identity, transform);
                            redTurtDown.GetComponent<GravTurtle2>().SetGravity(GravTurtle2.Dirs.floor);
                            break;
                        case 456:
                            GameObject redTurtUp = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Gravity Turtle (cherry red)"),
                                worldPos, Quaternion.identity, transform);
                            redTurtUp.GetComponent<GravTurtle2>().SetGravity(GravTurtle2.Dirs.ceiling);
                            break;
                        case 457:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Balloon Buster Generator"), worldPos, Quaternion.identity, transform);
                            break;
                        case 458:
                            GameObject pincerWallL = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Pincer"), worldPos, Quaternion.identity, transform);
                            pincerWallL.GetComponent<Pincer>().SetGravity(PlayState.EDirsCardinal.Left);
                            break;
                        case 459:
                            GameObject pincerWallR = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Pincer"), worldPos, Quaternion.identity, transform);
                            pincerWallR.GetComponent<Pincer>().SetGravity(PlayState.EDirsCardinal.Right);
                            break;
                        case 461:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.S }
                            });
                            break;
                        case 462:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.W }
                            });
                            break;
                        case 463:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.E }
                            });
                            break;
                        case 464:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.N }
                            });
                            break;
                        case 465:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.S }
                            });
                            break;
                        case 466:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.W }
                            });
                            break;
                        case 467:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.E }
                            });
                            break;
                        case 468:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.N }
                            });
                            break;
                        case 469:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.S }
                            });
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.S }
                            });
                            break;
                        case 470:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.W }
                            });
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.W }
                            });
                            break;
                        case 471:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.E }
                            });
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.E }
                            });
                            break;
                        case 472:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonTele,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.N }
                            });
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.MoonMove,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.N }
                            });
                            break;
                        case 473:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaStomp,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.S }
                            });
                            break;
                        case 474:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaStomp,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.W }
                            });
                            break;
                        case 475:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaStomp,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.E }
                            });
                            break;
                        case 476:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaStomp,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.N }
                            });
                            break;
                        case 477:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaStomp,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.N, PlayState.EDirsCompass.S }
                            });
                            break;
                        case 478:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaStomp,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.W, PlayState.EDirsCompass.E }
                            });
                            break;
                        case 479:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaStomp,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.N, PlayState.EDirsCompass.S, PlayState.EDirsCompass.E, PlayState.EDirsCompass.W }
                            });
                            break;
                        case 480:
                            PlayState.activeTargets.Add(new PlayState.TargetPoint
                            {
                                type = PlayState.TargetTypes.GigaSpawn,
                                pos = worldPos,
                                directions = new PlayState.EDirsCompass[] { PlayState.EDirsCompass.None }
                            });
                            break;
                        case 481:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Angry Pink Block"), worldPos + new Vector2(2.5f, -1.5f), Quaternion.identity, transform);
                            break;
                        case 482:
                            GameObject ceilGrass = Instantiate(Resources.Load<GameObject>("Objects/Grass"), worldPos, Quaternion.identity, transform);
                            ceilGrass.GetComponent<Grass>().isCeilingGrass = true;
                            break;
                        case 483:
                            GameObject ceilPowerGrass = Instantiate(Resources.Load<GameObject>("Objects/Power grass"), worldPos, Quaternion.identity, transform);
                            ceilPowerGrass.GetComponent<PowerGrass>().isCeilingGrass = true;
                            break;
                        case 1120:
                        case 1121:
                            Instantiate(Resources.Load<GameObject>("Objects/Hazards/Fire"), worldPos, Quaternion.identity, transform);
                            break;
                    }
                }
            }
        }
    }

    public void MoveEntitiesToInternalList()
    {
        List<Dictionary<string, object>> newContent = new();
        List<string> newTypes = new();
        List<Vector3> newPos = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            IRoomObject roomObject = (IRoomObject)obj.GetComponent(typeof(IRoomObject));
            if (roomObject != null)
            {
                newContent.Add(roomObject.save());
                newTypes.Add(roomObject.objType);
                newPos.Add(obj.transform.position);
            }
        }
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        roomContent = newContent.ToArray();
        roomContentTypes = newTypes.ToArray();
        roomContentPos = newPos.ToArray();
    }

    public void LogBreakables() {
        int limitX = (int)Mathf.Round((box.size.x + 0.5f) * 0.5f + 1);
        int limitY = (int)Mathf.Round((box.size.y + 0.5f) * 0.5f + 1);
        List<PlayState.Breakable> newBreakableList = new();
        List<PlayState.Breakable> newFinalBossList = new();

        for (int x = -limitX; x <= limitX; x++) {
            for (int y = -limitY; y <= limitY; y++) {
                Vector3Int tilePos = new((int)Mathf.Round(transform.position.x) + x, (int)Mathf.Round(transform.position.y) + y, 0);
                Vector2 worldPos = new(tilePos.x + 0.5f, tilePos.y + 0.5f);
                TileBase currentTile = specialMap.GetTile(tilePos);
                if (currentTile != null) {
                    PlayState.Breakable newBreakable = new() {
                        pos = worldPos,
                        blockType = 0,
                        isSilent = false
                    };

                    bool isBreakableTileHere = true;
                    bool isFinalBossTile = false;
                    switch (int.Parse(specialMap.GetSprite(tilePos).name.Split('_')[1])) {
                        default:
                            isBreakableTileHere = false;
                            break;
                        case 72:
                            newBreakable.blockType = 1;
                            break;
                        case 73:
                            newBreakable.blockType = 2;
                            break;
                        case 74:
                            newBreakable.blockType = 3;
                            break;
                        case 439:
                            newBreakable.blockType = 3;
                            newBreakable.isSilent = true;
                            break;
                        case 460:
                            newBreakable.blockType = -1;
                            newBreakable.isSilent = true;
                            isFinalBossTile = true;
                            break;
                    }

                    newBreakable.tiles = new int[]
                    {
                        PlayState.groundLayer.GetComponent<Tilemap>().GetTile(tilePos) != null ?
                        int.Parse(PlayState.groundLayer.GetComponent<Tilemap>().GetTile(tilePos).name.Split('_')[1]) : -1,
                        PlayState.fg1Layer.GetComponent<Tilemap>().GetTile(tilePos) != null ?
                        int.Parse(PlayState.fg1Layer.GetComponent<Tilemap>().GetTile(tilePos).name.Split('_')[1]) : -1,
                        PlayState.fg2Layer.GetComponent<Tilemap>().GetTile(tilePos) != null ?
                        int.Parse(PlayState.fg2Layer.GetComponent<Tilemap>().GetTile(tilePos).name.Split('_')[1]) : -1
                    };

                    if (newBreakable.tiles != new int[] { -1, -1, -1 } && isBreakableTileHere)
                    {
                        if (isFinalBossTile)
                            newFinalBossList.Add(newBreakable);
                        else
                            newBreakableList.Add(newBreakable);
                        PlayState.groundLayer.GetComponent<Tilemap>().SetTile(tilePos, null);
                        PlayState.fg1Layer.GetComponent<Tilemap>().SetTile(tilePos, null);
                        PlayState.fg2Layer.GetComponent<Tilemap>().SetTile(tilePos, null);
                    }
                }
            }
        }

        breakables = newBreakableList.ToArray();
        finalBossTiles = newFinalBossList.ToArray();
    }

    public void SpawnFromInternalList()
    {
        foreach (PlayState.Breakable thisBreakable in breakables)
        {
            GameObject breakable = Instantiate(breakableBlock, transform);
            breakable.GetComponent<BreakableBlock>().Instantiate(thisBreakable);
        }
        foreach (PlayState.Breakable thisFinalBossTile in finalBossTiles)
        {
            GameObject finalBossTile = Instantiate(breakableBlock, transform);
            finalBossTile.GetComponent<BreakableBlock>().Instantiate(thisFinalBossTile);
            PlayState.finalBossTiles.Add(finalBossTile);
        }
        for (int i = 0; i < roomContent.Length; i++)
            {
            GameObject obj = Resources.Load<GameObject>("Objects/" + roomContentTypes[i]);
            GameObject newGameObject = Instantiate(obj, roomContentPos[i], Quaternion.identity, transform);
            IRoomObject newRoomObject = newGameObject.GetComponent<IRoomObject>();
            newRoomObject.load(roomContent[i]);
        }
    }
}
