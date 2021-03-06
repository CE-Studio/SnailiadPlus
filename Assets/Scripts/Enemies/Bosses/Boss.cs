using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    public int ID;

    private float[] introTimestamps = new float[] { 0.5f, 1, 2.5f, 2.7f, 2.9f };
                                                 // Pan to boss
                                                       // Begin filling bar
                                                          // Finish filling bar
                                                                // Pan to player
                                                                      // Become vulnerable
    public float introTimer = 0;
    private readonly int[] songIndeces = new int[] { -4, -3, -2, -1 };

    public AnimationModule frameAnim;
    public AnimationModule barAnim;
    public GameObject barMask;
    private int[] barData;
    /*\
     *   ANIMATION DATA VALUES
     * 0 - Update frame on hit
     * 1 - Update bar on hit
     * 2 - Leftmost point of bar in pixels
     * 3 - Width of bar in pixels
     * 4 - Frame used by the mask
    \*/
    private float barPointLeft;
    private float barPointRight;

    public void SpawnBoss(int hp, int atk, int def, bool piercable, Vector2 hitboxSize, int bossID, List<int> wea = null, List<int> res = null, List<int> imm = null)
    {
        Spawn(hp, atk, def, piercable, hitboxSize, wea, res, imm);
        ID = bossID;
        barData = PlayState.GetAnim("BossBar_data").frames;
        frameAnim = PlayState.TogglableHUDElements[12].GetComponent<AnimationModule>();
        barAnim = PlayState.TogglableHUDElements[12].transform.GetChild(0).GetComponent<AnimationModule>();
        barMask = PlayState.TogglableHUDElements[12].transform.GetChild(1).gameObject;
        if (frameAnim.animList.Count == 0)
        {
            frameAnim.Add("BossBar_frame_idle");
            frameAnim.Add("BossBar_frame_hit");
            frameAnim.Add("BossBar_frame_despawn");
            barAnim.Add("BossBar_bar_idle");
            barAnim.Add("BossBar_bar_hit");
            barAnim.Add("BossBar_bar_despawn");
        }
        barPointLeft = ((barData[3] * 0.5f) + barData[2]) * 0.0625f;
        barPointRight = barPointLeft + (barData[3] * 0.0625f);
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        if (introTimer >= introTimestamps[4])
            barMask.transform.localPosition = new Vector2(
                Mathf.Floor(Mathf.Lerp(barPointLeft, barPointRight, Mathf.InverseLerp(0, maxHealth, health)) * 16) * 0.0625f,
                barMask.transform.localPosition.y);
    }

    public override IEnumerator Flash()
    {
        if (barData[0] == 1)
            frameAnim.Play("BossBar_frame_hit");
        if (barData[1] == 1)
            barAnim.Play("BossBar_bar_hit");
        return base.Flash();
    }

    public override void Kill()
    {
        PlayState.bossStates[ID] = 0;
        PlayState.ToggleBossfightState(false, 0);
        PlayState.RequestQueuedExplosion(transform.position, 2.7f, 0, true);
        foreach (Transform bullet in PlayState.enemyBulletPool.transform)
            bullet.GetComponent<EnemyBullet>().Despawn();
        Destroy(gameObject);
    }

    public IEnumerator RunIntro(bool focusOnMe = true)
    {
        PlayState.paralyzed = true;
        PlayState.playerScript.UpdateMusic(-1, 0, 1);
        PlayState.ToggleBossfightState(true, songIndeces[ID]);
        PlayState.TogglableHUDElements[12].GetComponent<SpriteRenderer>().enabled = true;
        PlayState.TogglableHUDElements[12].transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        barMask.GetComponent<SpriteMask>().sprite = PlayState.GetSprite("UI/BossHealthBar", barData[4]);
        barMask.transform.localPosition = new Vector2(barPointLeft, barMask.transform.localPosition.y);
        frameAnim.Play("BossBar_frame_idle");
        box.enabled = false;
        PlayState.playerScript.currentBossName = PlayState.GetText(
            "boss_" + ID switch { 1 => "stompy", 2 => "spaceBox", 3 => "moonSnail", _ => "shellbreaker" } + (PlayState.currentArea == 7 ? "_rush" : ""));

        int playBeep = 0;
        while (introTimer <= introTimestamps[4])
        {
            introTimer += Time.deltaTime;
            if(introTimer > introTimestamps[0] && introTimer <= introTimestamps[1])
            {
                if (focusOnMe)
                    PlayState.SetCamFocus(transform);
            }
            else if(introTimer > introTimestamps[1] && introTimer <= introTimestamps[2])
            {
                if (barAnim.currentAnimName != "BossBar_bar_idle")
                    barAnim.Play("BossBar_bar_idle");
                playBeep--;
                if (playBeep <= 0)
                {
                    PlayState.PlaySound("BossHPBleep");
                    playBeep = Application.targetFrameRate switch { 30 => 1, 60 => 3, 120 => 7, _ => 15 };
                }
                barMask.transform.localPosition = new Vector2(
                    Mathf.Lerp(barPointLeft, barPointRight, Mathf.InverseLerp(introTimestamps[1], introTimestamps[2], introTimer)),
                    barMask.transform.localPosition.y);
            }
            else if(introTimer > introTimestamps[2] && introTimer <= introTimestamps[3])
            {
                barMask.transform.localPosition = new Vector2(barPointRight, barMask.transform.localPosition.y);
            }
            else if(introTimer > introTimestamps[3] && introTimer <= introTimestamps[4])
            {
                PlayState.SetCamFocus(PlayState.player.transform);
            }
            yield return new WaitForEndOfFrame();
        }
        box.enabled = true;
        PlayState.paralyzed = false;
        barMask.transform.localPosition = new Vector2(barPointRight, barMask.transform.localPosition.y);
    }
}
