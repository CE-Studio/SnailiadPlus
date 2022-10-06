using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTrigger:MonoBehaviour {
    public BoxCollider2D box;
    public bool active = true;
    private float initializationBuffer = 0;

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
    private float splashTimeout = 0;

    public struct RoomCommand {
        public string name;
        public string[] args;
    };
    public string[] roomCommands = new string[] { };

    public TextMesh roomNameText;
    public TextMesh roomNameShadow;

    public Tilemap bg;
    public Tilemap fg;
    public Tilemap specialMap;
    public GameObject breakableBlock;

    public Dictionary<string, object>[] roomContent;
    public string[] roomContentTypes;
    public Vector3[] roomContentPos;
    //public PlayState.RoomEntity[] preplacedEntities;
    public PlayState.Breakable[] breakables;

    void Awake() {
        box = GetComponent<BoxCollider2D>();
        roomNameText = GameObject.Find("View/Minimap Panel/Room Name Parent/Room Name Text").GetComponent<TextMesh>();
        roomNameShadow = GameObject.Find("View/Minimap Panel/Room Name Parent/Room Name Shadow").GetComponent<TextMesh>();
        bg = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
        fg = GameObject.Find("Grid/Foreground").GetComponent<Tilemap>();
        specialMap = GameObject.Find("Grid/Special").GetComponent<Tilemap>();
        breakableBlock = (GameObject)Resources.Load("Objects/Breakable Block");
        DespawnEverything();
        specialMap.color = new Color32(255, 255, 255, 0);
    }

    void Update() {
        if (!active) {
            if (initializationBuffer > 0)
                initializationBuffer -= Time.deltaTime;
            splashTimeout = Mathf.Clamp(splashTimeout - Time.deltaTime, 0, Mathf.Infinity);

            int effectVarIndex = 0;
            foreach (string effect in environmentalEffects) {
                switch (effect.ToLower()) {
                    default:
                        break;
                    case "bubble":
                        if (!initializedEffects) {
                            for (int i = 0; i < 8; i++) {
                                Vector2 bubblePos = new Vector2(Random.Range(0, box.size.x + 0.5f), 0);
                                bubblePos.y = Random.Range(0, waterLevel[WaterPoint(bubblePos.x)].y);
                                Vector2 truePos = new Vector2(transform.position.x - (box.size.x * 0.5f) + bubblePos.x, transform.position.y - (box.size.y * 0.5f) + bubblePos.y);
                                PlayState.RequestParticle(truePos, "bubble", new float[] { transform.position.y - (box.size.y * 0.5f) + waterLevel[WaterPoint(bubblePos.x)].y, 0 });
                            }
                            effectVars.Add(Random.Range(0f, 1f) * 12);
                        } else {
                            if (effectVars[effectVarIndex] <= 0) {
                                Vector2 bubblePos = new Vector2(Random.Range(0, box.size.x + 0.5f), 0);
                                bubblePos.y = Random.Range(0, waterLevel[WaterPoint(bubblePos.x)].y);
                                Vector2 truePos = new Vector2(transform.position.x - (box.size.x * 0.5f) + bubblePos.x, transform.position.y - (box.size.y * 0.5f) - 0.25f);
                                PlayState.RequestParticle(truePos, "bubble", new float[] { transform.position.y - (box.size.y * 0.5f) + waterLevel[WaterPoint(bubblePos.x)].y, 0 });
                                effectVars[effectVarIndex] = Random.Range(0f, 1f) * 12;
                            } else {
                                if (PlayState.gameState == "Game")
                                    effectVars[effectVarIndex] -= Time.deltaTime;
                            }
                        }
                        break;
                    case "snow":
                        if (!initializedEffects) {
                            for (int i = 0; i < 60; i++) {
                                Vector2 snowPos = new Vector2(Random.Range(PlayState.cam.transform.position.x - 13f, PlayState.cam.transform.position.x + 13f),
                                    Random.Range(PlayState.cam.transform.position.y - 8f, PlayState.cam.transform.position.y + 8f));
                                PlayState.RequestParticle(snowPos, "snow");
                            }
                        }
                        break;
                }
                effectVarIndex++;
            }

            if (waterLevel.Length > 0) {
                float playerY = PlayState.player.transform.position.y;
                float waterY = transform.position.y - (box.size.y * 0.5f) - 0.25f +
                    waterLevel[WaterPoint(PlayState.player.transform.position.x)].y;
                if (((playerY > waterY && PlayState.playerScript.underwater) || (playerY < waterY && !PlayState.playerScript.underwater)) && initializedEffects) {
                    if (initializationBuffer <= 0 && splashTimeout <= 0) {
                        PlayState.RequestParticle(new Vector2(PlayState.player.transform.position.x, waterY + 0.5f), "splash", true);
                        if (playerY < waterY && (PlayState.gameOptions[11] == 1 || PlayState.gameOptions[11] == 3 || PlayState.gameOptions[11] == 5)) {
                            for (int i = Random.Range(2, 8); i > 0; i--)
                                PlayState.RequestParticle(new Vector2(PlayState.player.transform.position.x, waterY - 0.5f), "bubble", new float[] { waterY, 1 });
                        }
                    }
                    PlayState.playerScript.underwater = playerY < waterY;
                    splashTimeout = 0.125f;
                }
            } else
                PlayState.playerScript.underwater = false;

            initializedEffects = true;
        }
    }

    private int WaterPoint(float x) {
        bool foundPointLeftOf = false;
        float relativeX = x - transform.position.x + (box.size.x * 0.5f);
        int waterPoint = waterLevel.Length - 1;
        while (!foundPointLeftOf && waterPoint != -1) {
            if (relativeX > waterLevel[waterPoint].x)
                foundPointLeftOf = true;
            else
                waterPoint--;
        }
        if (waterPoint == -1)
            waterPoint = 0;
        return waterPoint;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") && active && PlayState.gameState == "Game") {
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

            if (!PlayState.playerScript.grounded && PlayState.playerScript.shelled) {
                Vector2 playerPos = PlayState.player.transform.position;
                switch (PlayState.playerScript.gravityDir) {
                    default:
                    case 0:
                    case 3:
                        if (PlayState.IsTileSolid(new Vector2(playerPos.x + 1, playerPos.y)) || PlayState.IsTileSolid(new Vector2(playerPos.x - 1, playerPos.y)))
                            PlayState.player.transform.position = new Vector2(Mathf.Floor(playerPos.x) + 0.5f + (PlayState.playerScript.facingLeft ? -0.125f : 0.125f), playerPos.y);
                        break;
                    case 1:
                    case 2:
                        if (PlayState.IsTileSolid(new Vector2(playerPos.x, playerPos.y + 1)) || PlayState.IsTileSolid(new Vector2(playerPos.x, playerPos.y - 1)))
                            PlayState.player.transform.position = new Vector2(playerPos.x, Mathf.Floor(playerPos.y) + 0.5f + (PlayState.playerScript.facingDown ? -0.125f : 0.125f));
                        break;
                }
            }

            PlayState.camTempBuffersX = Vector2.zero;
            PlayState.camTempBuffersY = Vector2.zero;
            Vector2 thisTriggerPos = new Vector2(areaID, transform.GetSiblingIndex());
            initializationBuffer = 0.25f;
            if (thisTriggerPos != PlayState.positionOfLastRoom) {
                Transform previousTrigger = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
                previousTrigger.GetComponent<Collider2D>().enabled = true;
                previousTrigger.GetComponent<RoomTrigger>().active = true;
                previousTrigger.GetComponent<RoomTrigger>().DespawnEverything();
                PlayState.positionOfLastRoom = thisTriggerPos;
            }

            string newRoomName = "";

            if (newRoomName == "") {
                foreach (char character in PlayState.GetText("room_" + (areaID < 10 ? "0" : "") + areaID + "_" + transform.name)) {
                    if (character == '|')
                        newRoomName += "\n";
                    else
                        newRoomName += character;
                }
            }
            roomNameText.text = newRoomName;
            roomNameShadow.text = newRoomName;

            PlayState.breakablePositions.Clear();
            CheckSpecialLayer();
            SpawnFromInternalList();

            box.enabled = false;

            for (int i = 0; i < roomCommands.Length; i++) {
                string[] command = roomCommands[i].ToLower().Replace(" ", "").Split(',');
                switch (command[0]) {
                    default:
                        Debug.LogWarning("Unknown room command \"" + command[0] + "\"");
                        break;
                    case "setmaptile":
                        Vector2 mapPos = new Vector2(int.Parse(command[1]), int.Parse(command[2]));
                        PlayState.SetMapTile(mapPos, bool.Parse(command[3]));
                        break;
                    case "achievement":
                        PlayState.QueueAchievementPopup(command[1]);
                        break;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            active = false;
        }
    }

    public void DespawnEverything() {
        initializedEffects = false;
        for (int i = transform.childCount - 1; i >= 0; i--) {
            if (transform.GetChild(i).name == "Cutscene Controller")
                transform.GetChild(i).GetComponent<CutsceneController>().StopAllCoroutines();
            Destroy(transform.GetChild(i).gameObject);
        }
        GameObject pool = GameObject.Find("Player Bullet Pool");
        for (int i = 0; i < pool.transform.childCount; i++) {
            if (pool.transform.GetChild(i).transform.GetComponent<Bullet>().isActive) {
                pool.transform.GetChild(i).transform.GetComponent<Bullet>().Despawn();
            }
            pool.transform.GetChild(i).transform.position = Vector2.zero;
        }
        PlayState.ReplaceAllTempTiles();
    }

    private void CheckSpecialLayer() {
        int limitX = (int)Mathf.Round((box.size.x + 0.5f) * 0.5f + 1);
        int limitY = (int)Mathf.Round((box.size.y + 0.5f) * 0.5f + 1);

        for (int x = -limitX; x <= limitX; x++) {
            for (int y = -limitY; y <= limitY; y++) {
                Vector3Int tilePos = new Vector3Int((int)Mathf.Round(transform.position.x) + x, (int)Mathf.Round(transform.position.y) + y, 0);
                Vector2 worldPos = new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f);
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
                        case 27:
                            Instantiate(Resources.Load<GameObject>("Objects/Grass"), worldPos, Quaternion.identity, transform);
                            break;
                        case 30:
                            Instantiate(Resources.Load<GameObject>("Objects/Power Grass"), worldPos, Quaternion.identity, transform);
                            break;
                        case 31:
                            PlayState.RequestParticle(worldPos, "smoke");
                            break;
                        case 376:
                            GameObject block = new GameObject { name = "Enemy-Collidable Tile", layer = 9 };
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
                        case 386:
                            Instantiate(Resources.Load<GameObject>("Objects/Hazards/Muck"), worldPos, Quaternion.identity, transform);
                            break;
                        case 387:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Ghost Dandelion"), worldPos, Quaternion.identity, transform);
                            break;
                        case 389:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Floatspike (black)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 397:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Chirpy (aqua)"), worldPos, Quaternion.identity, transform);
                            break;
                        case 414:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Batty Bat"), worldPos, Quaternion.identity, transform);
                            break;
                        case 424:
                            Instantiate(Resources.Load<GameObject>("Objects/Enemies/Snelk"), worldPos, Quaternion.identity, transform);
                            break;
                        case 425:
                            GameObject runningSnelk = Instantiate(Resources.Load<GameObject>("Objects/Enemies/Snelk"), worldPos, Quaternion.identity, transform);
                            runningSnelk.GetComponent<Snelk>().SetState(1);
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
                    }
                }
            }
        }
    }

    public void MoveEntitiesToInternalList() {
        List<Dictionary<string, object>> newContent = new List<Dictionary<string, object>>();
        List<string> newTypes = new List<string>();
        List<Vector3> newPos = new List<Vector3>();
        for (int i = transform.childCount - 1; i >= 0; i--) {
            GameObject obj = transform.GetChild(i).gameObject;
            IRoomObject roomObject = (IRoomObject)obj.GetComponent(typeof(IRoomObject));
            if (roomObject != null) {
                print(obj.name);
                newContent.Add(roomObject.save());
                newTypes.Add(roomObject.objType);
                newPos.Add(obj.transform.position);
            }
            Destroy(obj);
        }
        roomContent = newContent.ToArray();
        roomContentTypes = newTypes.ToArray();
        roomContentPos = newPos.ToArray();
        //List<PlayState.RoomEntity> newList = new List<PlayState.RoomEntity>();
        //for (int i = transform.childCount - 1; i >= 0; i--) {
        //    PlayState.RoomEntity newEntity = new PlayState.RoomEntity();
        //
        //    GameObject obj = transform.GetChild(i).gameObject;
        //    string objName = obj.name.Replace("(Clone)", "");
        //    if (objName[objName.Length - 1] == ')') {
        //        string[] tempArray = objName.Split(' ');
        //        objName = objName.Substring(0, objName.Length - tempArray[tempArray.Length - 1].Length - 1);
        //    }
        //
        //    newEntity.name = obj.name;
        //    newEntity.tag = obj.tag;
        //    newEntity.pos = obj.transform.position;
        //
        //    List<int> newConditions = new List<int>();
        //    switch (objName) {
        //        default:
        //            newConditions.Add(0);
        //            break;
        //        case "Cutscene Controller": // Script, ID, trigger state, trigger size X, trigger size y, all actors
        //            List<string> newCutsceneData = new List<string> {
        //                obj.GetComponent<CutsceneController>().sceneScript,
        //                PlayState.cutsceneData.Count.ToString(),
        //                obj.GetComponent<CutsceneController>().triggerActive ? "true" : "false",
        //                obj.GetComponent<BoxCollider2D>().size.x.ToString(),
        //                obj.GetComponent<BoxCollider2D>().size.y.ToString()
        //            };
        //            for (int j = 0; j < obj.GetComponent<CutsceneController>().actors.Count; j++) {
        //                int npcNum = 0;
        //                int thisNpcNum = obj.GetComponent<CutsceneController>().actors[j].transform.GetSiblingIndex();
        //                for (int k = 0; k < thisNpcNum; k++) {
        //                    if (transform.GetChild(k).CompareTag("NPC"))
        //                        npcNum++;
        //                }
        //                newCutsceneData.Add(npcNum.ToString());
        //            }
        //            PlayState.cutsceneData.Add(newCutsceneData.ToArray());
        //            newConditions.Add(PlayState.cutsceneData.Count - 1);
        //            break;
        //        case "Door":
        //            newConditions.Add(obj.GetComponent<Door>().doorWeapon);
        //            newConditions.Add(obj.GetComponent<Door>().locked ? 1 : 0);
        //            newConditions.Add(obj.GetComponent<Door>().bossLock);
        //            newConditions.Add(obj.GetComponent<Door>().direction);
        //            break;
        //        case "Fake Boundary":
        //            newConditions.Add(obj.GetComponent<FakeRoomBorder>().direction ? 1 : 0);
        //            newConditions.Add(obj.GetComponent<FakeRoomBorder>().workingDirections);
        //            break;
        //        case "Item":
        //            Item script = obj.GetComponent<Item>();
        //
        //            newConditions.Add(script.itemID);
        //            newConditions.Add(script.isSuperUnique ? 1 : 0);
        //            for (int j = 0; j < 3; j++)
        //                newConditions.Add(script.difficultiesPresentIn[j] ? 1 : 0);
        //            for (int j = 0; j < 6; j++)
        //                newConditions.Add(script.charactersPresentFor[j] ? 1 : 0);
        //
        //            if (PlayState.itemData.Length == 0)
        //                PlayState.itemData = new bool[PlayState.itemCollection.Length][];
        //            PlayState.itemData[script.itemID] = new bool[]
        //            {
        //                script.difficultiesPresentIn[0],
        //                script.difficultiesPresentIn[1],
        //                script.difficultiesPresentIn[2],
        //                script.charactersPresentFor[0],
        //                script.charactersPresentFor[1],
        //                script.charactersPresentFor[2],
        //                script.charactersPresentFor[3],
        //                script.charactersPresentFor[4],
        //                script.charactersPresentFor[5]
        //            };
        //            break;
        //        case "NPC":
        //            NPC thisNPC = obj.GetComponent<NPC>();
        //            newConditions.Add(thisNPC.ID);
        //            newConditions.Add(thisNPC.upsideDown ? 1 : 0);
        //            newConditions.Add(thisNPC.lookMode);
        //            break;
        //        case "Turtle NPC":
        //            newConditions.Add(obj.GetComponent<TurtleNPC>().upsideDown ? 1 : 0);
        //            break;
        //    }
        //    newEntity.spawnData = newConditions.ToArray();
        //
        //    newList.Add(newEntity);
        //
        //    Destroy(obj);
        //}
        //preplacedEntities = newList.ToArray();
    }

    public void LogBreakables() {
        int limitX = (int)Mathf.Round((box.size.x + 0.5f) * 0.5f + 1);
        int limitY = (int)Mathf.Round((box.size.y + 0.5f) * 0.5f + 1);
        List<PlayState.Breakable> newBreakableList = new List<PlayState.Breakable>();

        for (int x = -limitX; x <= limitX; x++) {
            for (int y = -limitY; y <= limitY; y++) {
                Vector3Int tilePos = new Vector3Int((int)Mathf.Round(transform.position.x) + x, (int)Mathf.Round(transform.position.y) + y, 0);
                Vector2 worldPos = new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f);
                TileBase currentTile = specialMap.GetTile(tilePos);
                if (currentTile != null) {
                    PlayState.Breakable newBreakable = new PlayState.Breakable {
                        pos = worldPos,
                        weaponLevel = 2,
                        isSilent = false
                    };

                    bool isBreakableTileHere = true;
                    switch (int.Parse(specialMap.GetSprite(tilePos).name.Split('_')[1])) {
                        default:
                            isBreakableTileHere = false;
                            break;
                        case 72:
                            newBreakable.weaponLevel = 2;
                            break;
                        case 73:
                            newBreakable.weaponLevel = 3;
                            break;
                        case 74:
                            newBreakable.weaponLevel = 4;
                            break;
                        case 439:
                            newBreakable.weaponLevel = 4;
                            newBreakable.isSilent = true;
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

                    if (newBreakable.tiles != new int[] { -1, -1, -1 } && isBreakableTileHere) {
                        newBreakableList.Add(newBreakable);
                        PlayState.groundLayer.GetComponent<Tilemap>().SetTile(tilePos, null);
                        PlayState.fg1Layer.GetComponent<Tilemap>().SetTile(tilePos, null);
                        PlayState.fg2Layer.GetComponent<Tilemap>().SetTile(tilePos, null);
                    }
                }
            }
        }

        breakables = newBreakableList.ToArray();
    }

    public void SpawnFromInternalList() {
        foreach (PlayState.Breakable thisBreakable in breakables) {
            GameObject breakable = Instantiate(breakableBlock, transform);
            breakable.GetComponent<BreakableBlock>().Instantiate(thisBreakable);
        }
        for (int i = 0; i < roomContent.Length; i++) {
            GameObject obj = Resources.Load<GameObject>("Objects/" + roomContentTypes[i]);
            GameObject newGameObject = Instantiate(obj, roomContentPos[i], Quaternion.identity, transform);
            IRoomObject newRoomObject = newGameObject.GetComponent<IRoomObject>();
            newRoomObject.load(roomContent[i]);
        }
        //List<CutsceneController> cutscenes = new List<CutsceneController>();
        //List<int> cutsceneIDs = new List<int>();
        //foreach (PlayState.RoomEntity entity in preplacedEntities) {
        //    GameObject newObject = Instantiate(CheckResourcesFor(entity.name, entity.tag), entity.pos, Quaternion.identity, transform);
        //    newObject.name = entity.name.Replace("(Clone)", "").Trim();
        //    switch (newObject.name) {
        //        default:
        //            break;
        //        case "Cutscene Controller": // Script, ID, trigger state, trigger size X, trigger size y, all actors
        //            string[] thisData = PlayState.cutsceneData[entity.spawnData[0]];
        //            if (PlayState.cutscenesToNotSpawn.Contains(int.Parse(thisData[1]))) {
        //                Destroy(newObject);
        //                break;
        //            }
        //            newObject.GetComponent<CutsceneController>().sceneScript = thisData[0];
        //            newObject.GetComponent<CutsceneController>().cutsceneID = int.Parse(thisData[1]);
        //            newObject.GetComponent<CutsceneController>().triggerActive = thisData[2] == "true";
        //            newObject.GetComponent<CutsceneController>().GetComponent<BoxCollider2D>().size = new Vector2(float.Parse(thisData[3]), float.Parse(thisData[4]));
        //            cutscenes.Add(newObject.GetComponent<CutsceneController>());
        //            cutsceneIDs.Add(entity.spawnData[0]);
        //            break;
        //        case "Door":
        //            newObject.GetComponent<Door>().Spawn(entity.spawnData);
        //            break;
        //        case "Fake Boundary":
        //            newObject.GetComponent<FakeRoomBorder>().Spawn(entity.spawnData);
        //            if (roomNameText.text.Contains("/")) {
        //                string name = roomNameText.text;
        //                int charOffset = areaID >= 100 ? 6 + areaID.ToString().Length : 8;
        //                name = name.Substring(charOffset, name.Length - charOffset);
        //                FakeRoomBorder script = newObject.GetComponent<FakeRoomBorder>();
        //                string[] splitString = name.Split('/');
        //                script.downLeftRoomName = PlayState.GetText("room_" + (areaID < 10 ? "0" : "") + areaID + "_" + splitString[0]).Replace("|", "\n");
        //                script.upRightRoomName = PlayState.GetText("room_" + (areaID < 10 ? "0" : "") + areaID + "_" + splitString[1]).Replace("|", "\n");
        //            }
        //            break;
        //        case "Item":
        //            int thisID = newObject.GetComponent<Item>().itemID;
        //            int charCheck = PlayState.currentCharacter switch { "Snaily" => 0, "Sluggy" => 1, "Upside" => 2, "Leggy" => 3, "Blobby" => 4, "Leechy" => 5, _ => 0 };
        //            if (PlayState.itemCollection[entity.spawnData[0]] == 0 || !PlayState.itemData[thisID][PlayState.currentDifficulty] || !PlayState.itemData[thisID][charCheck])
        //                newObject.GetComponent<Item>().Spawn(entity.spawnData);
        //            else
        //                Destroy(newObject);
        //            break;
        //        case "NPC":
        //            newObject.GetComponent<NPC>().Spawn(entity.spawnData);
        //            break;
        //        case "Save Point":
        //            newObject.GetComponent<SavePoint>().Spawn();
        //            break;
        //        case "Turtle NPC":
        //            newObject.GetComponent<TurtleNPC>().Spawn(entity.spawnData);
        //            break;
        //    }
        //}
        //for (int i = 0; i < cutscenes.Count; i++) {
        //    string[] thisCutsceneData = PlayState.cutsceneData[cutsceneIDs[i]];
        //    for (int j = 5; j < thisCutsceneData.Length; j++) {
        //        int desiredID = int.Parse(thisCutsceneData[j]);
        //        int foundNPCs = -1;
        //        int currentChildIndex = 0;
        //        while (foundNPCs < desiredID) {
        //            if (transform.GetChild(currentChildIndex).CompareTag("NPC"))
        //                foundNPCs++;
        //            if (foundNPCs != desiredID)
        //                currentChildIndex++;
        //        }
        //        cutscenes[i].actors.Add(transform.GetChild(currentChildIndex).GetComponent<NPC>());
        //    }
        //    cutscenes[i].Spawn();
        //}
    }

    //public GameObject CheckResourcesFor(string objName, string tag) {
    //    if (tag == "Enemy")
    //        return Resources.Load<GameObject>("Objects/Enemies/" + objName);
    //    else
    //        return Resources.Load<GameObject>("Objects/" + objName);
    //}
}
