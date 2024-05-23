using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public enum TrapItems
    {
        WeaponLock,   // Locks you to one of your lower-power weapons for 10-30 seconds
        GravityLock,  // Locks you to a random gravity state for 10-30 seconds
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

    public List<Timer> timers;
    
    void Start()
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(TrapItems)).Length; i++)
        {
            GameObject thisTimer = transform.GetChild(i).gameObject;
            timers.Add(new Timer
            {
                obj = thisTimer.transform,
                frameAnim = thisTimer.GetComponent<AnimationModule>(),
                bgAnim = thisTimer.transform.GetChild(0).GetComponent<AnimationModule>(),
                maskAnim = thisTimer.transform.GetChild(1).GetComponent<AnimationModule>(),
                duration = 0,
                thisMaxDuration = 0
            });
        }
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        for (int i = 0; i < timers.Count; i++)
        {

        }
    }
}
