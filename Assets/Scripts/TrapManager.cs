using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public enum TrapItems
    {
        WeaponLock,   // Locks you to one of your lower-power weapons for 10-20 seconds
        GravityLock,  // Locks you to a random gravity state for 10-20 seconds
        Lullaby,      // Puts your character to sleep for 10-15 seconds or until damage is taken
        SpiderAmbush, // Spawns a horde of 8-12 Spiders randomly around the screen. Spiders have a 10% chance to spawn as Spider Mamas
        Warp          // Warps you back to Snail Town. Blame Zed
    };

    public struct Timer
    {
        public Transform obj;
        public AnimationModule frameAnim;
        public AnimationModule bgAnim;
        public AnimationModule maskAnim;
    }

    public List<Timer> timers = new();

    public float[] trapDurations = new float[] { };
    private float[] currentMaxDurations = new float[] { };

    private readonly float[] minDurations = new float[] { 15f, 15f, 10f, 3f, 3f };
    private readonly float[] maxDurations = new float[] { 30f, 30f, 15f, 3f, 3f };

    private const float SPACE_BUFFER = 1.75f;
    private const float ACTIVE_Y = -2.5f;
    private const float ACTIVE_Y_BOSS = -4f;
    private const float LERP_RATE = 10f;
    private List<int> trapQueue = new();

    public List<int> lockedWeapons = new();
    private int lastSelectedWeapon = 0;
    public Player.Dirs lockedGravity = Player.Dirs.None;

    void Start()
    {
        int totalTrapCount = System.Enum.GetNames(typeof(TrapItems)).Length;
        trapDurations = new float[totalTrapCount];
        currentMaxDurations = new float[totalTrapCount];
        for (int i = 0; i < totalTrapCount; i++)
        {
            GameObject thisTimer = transform.GetChild(i).gameObject;
            timers.Add(new Timer
            {
                obj = thisTimer.transform,
                frameAnim = thisTimer.GetComponent<AnimationModule>(),
                bgAnim = thisTimer.transform.GetChild(0).GetComponent<AnimationModule>(),
                maskAnim = thisTimer.transform.GetChild(1).GetComponent<AnimationModule>()
            });
            timers[i].frameAnim.Add(GetAnim(i, 0));
            timers[i].bgAnim.Add(GetAnim(i, 1));
            timers[i].maskAnim.Add(GetAnim(i, 2));
            timers[i].maskAnim.updateMask = true;
            timers[i].maskAnim.GetSpriteRenderer().color = new Color(0, 0, 0, 0);
        }
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        for (int i = 0; i < timers.Count; i++)
        {
            trapDurations[i] -= Time.deltaTime;
            Vector2 timerPos = timers[i].obj.transform.localPosition;
            if (trapDurations[i] > 0)
            {
                float timerX = (SPACE_BUFFER * trapQueue.IndexOf(i)) - (SPACE_BUFFER * (trapQueue.Count - 1) * 0.5f);
                float timerY = PlayState.inBossFight ? ACTIVE_Y_BOSS : ACTIVE_Y;
                timers[i].obj.transform.localPosition = Vector2.Lerp(timerPos, new Vector2(timerX, timerY), LERP_RATE * Time.deltaTime);
                timers[i].maskAnim.transform.localPosition = Vector2.Lerp(Vector2.zero, Vector2.down, Mathf.InverseLerp(currentMaxDurations[i], 0, trapDurations[i]));
                float maskY = timers[i].maskAnim.transform.localPosition.y;
                maskY = Mathf.RoundToInt(maskY * 16) * 0.0625f;
                timers[i].maskAnim.transform.localPosition = new Vector2(0, maskY);
            }
            else
            {
                timers[i].obj.transform.localPosition = Vector2.Lerp(timerPos, new Vector2(timerPos.x, 0), LERP_RATE * Time.deltaTime);
                if (trapQueue.Contains(i))
                    DeactivateTrap(i);
            }
        }
    }

    private string GetAnim(int timerID, int animPart)
    {
        return string.Format("TrapTimer_{0}{1}",
            System.Enum.GetName(typeof(TrapItems), timerID),
            animPart switch { 1 => "_background", 2 => "_mask", _ => "" });
    }

    public void ActivateTrap(int trapID)
    {
        currentMaxDurations[trapID] = Random.Range(minDurations[trapID], maxDurations[trapID]);
        trapDurations[trapID] = currentMaxDurations[trapID];
        timers[trapID].frameAnim.Play(GetAnim(trapID, 0));
        timers[trapID].bgAnim.Play(GetAnim(trapID, 1));
        timers[trapID].maskAnim.Play(GetAnim(trapID, 2));
        if (!trapQueue.Contains(trapID))
        {
            timers[trapID].obj.localPosition = new Vector2(SPACE_BUFFER * trapQueue.Count * 0.5f, 0);
            trapQueue.Add(trapID);
        }
        switch (trapID)
        {
            case 0:
                lastSelectedWeapon = PlayState.playerScript.selectedWeapon;
                List<int> currentLoadout = new();
                if (PlayState.isRandomGame && PlayState.currentRando.broomStart)
                    currentLoadout.Add(0);
                if (PlayState.CheckForItem(0))
                    currentLoadout.Add(1);
                if (PlayState.CheckForItem(1))
                    currentLoadout.Add(2);
                if (PlayState.CheckForItem(2))
                    currentLoadout.Add(3);
                int weaponCountToLock = Mathf.FloorToInt(currentLoadout.Count * 0.5f);
                int currentCheckedID = currentLoadout.Count - 1;
                while (weaponCountToLock > 0 && currentCheckedID >= 0)
                {
                    lockedWeapons.Add(currentLoadout[currentCheckedID]);
                    weaponCountToLock--;
                    currentCheckedID--;
                    if (weaponCountToLock == 0)
                        PlayState.globalFunctions.ChangeActiveWeapon(currentLoadout[currentCheckedID]);
                }
                break;
            case 1:
                lockedGravity = (Player.Dirs)Random.Range(0, 4);
                break;
            case 2:
                PlayState.playerScript.sleepTimer = 0;
                PlayState.paralyzed = true;
                break;
            case 3:
                int spiderCount = Random.Range(8, 13);
                for (int i = 0; i < spiderCount; i++)
                {
                    float spiderX = Random.Range(PlayState.cam.transform.position.x - 12.5f, PlayState.cam.transform.position.x + 12.5f);
                    float spiderY = Random.Range(PlayState.cam.transform.position.y - 7.5f, PlayState.cam.transform.position.y + 7.5f);
                    if (Random.Range(0f, 1f) <= 0.1f)
                        Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spider Mama"), new Vector2(spiderX, spiderY),
                            Quaternion.identity, PlayState.currentRoom.transform);
                    else
                        Instantiate(Resources.Load<GameObject>("Objects/Enemies/Spider"), new Vector2(spiderX, spiderY),
                            Quaternion.identity, PlayState.currentRoom.transform);
                    PlayState.RequestParticle(new Vector2(spiderX, spiderY), "explosion", new float[] { 2 });
                }
                break;
            case 4:
                PlayState.player.transform.position = PlayState.PLAYER_SPAWNS[PlayState.mainMenu.CharacterNameToID(PlayState.currentProfile.character)];
                break;
        }
    }

    public void DeactivateTrap(int trapID)
    {
        trapDurations[trapID] = 0;
        if (trapQueue.Contains(trapID))
            trapQueue.Remove(trapID);
        switch (trapID)
        {
            case 0:
                lockedWeapons.Clear();
                PlayState.globalFunctions.ChangeActiveWeapon(lastSelectedWeapon);
                break;
            case 1:
                lockedGravity = Player.Dirs.None;
                break;
            case 2:
                PlayState.paralyzed = false;
                break;
            default:
                break;
        }
    }
}
