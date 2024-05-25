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
        public float duration;
        public float thisMaxDuration;
    }

    public List<Timer> timers = new();

    public float[] trapDurations = new float[] { };
    private float[] currentMaxDurations = new float[] { };

    private readonly float[] minDurations = new float[] { 10f, 10f, 10f, 3f, 3f };
    private readonly float[] maxDurations = new float[] { 20f, 20f, 15f, 3f, 3f };

    private const float SPACE_BUFFER = 1.25f;
    private List<int> trapQueue = new();

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
                maskAnim = thisTimer.transform.GetChild(1).GetComponent<AnimationModule>(),
                duration = 0f,
                thisMaxDuration = 0f
            });
            timers[i].frameAnim.Add(GetAnim(i, 0));
            timers[i].bgAnim.Add(GetAnim(i, 1));
            timers[i].maskAnim.Add(GetAnim(i, 2));
            timers[i].maskAnim.updateMask = true;
        }
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        for (int i = 0; i < timers.Count; i++)
        {
            if (timers[i].duration > 0)
            {
                timers[i].obj.localPosition = new Vector2();
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
            trapQueue.Add(trapID);
            timers[trapID].obj.localPosition = new Vector2(SPACE_BUFFER * trapQueue.Count, 0);
        }
    }
}
