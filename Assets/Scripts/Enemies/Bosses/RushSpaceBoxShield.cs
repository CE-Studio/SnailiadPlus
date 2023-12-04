using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushSpaceBoxShield : Enemy
{
    public RushSpaceBox parentBoss;
    private bool active;
    private LightMask lightMask;

    private void Awake()
    {
        Spawn(100, 3, 1000, false);

        anim.Add("RushBoss_spaceBox_shield0_spawn");
        anim.Add("RushBoss_spaceBox_shield1_spawn");
        anim.Add("RushBoss_spaceBox_shield0_hit");
        anim.Add("RushBoss_spaceBox_shield1_hit");
        anim.Add("RushBoss_spaceBox_shield0_turnBlue");
        anim.Add("RushBoss_spaceBox_shield0_inactive");

        SetActive(false);

        lightMask = PlayState.globalFunctions.CreateLightMask(-1, transform);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
            anim.Play("RushBoss_spaceBox_shield" + parentBoss.attackMode.ToString() + "_hit");
        base.OnTriggerEnter2D(collision);
    }

    public void TurnBlue()
    {
        if (active)
            anim.Play("RushBoss_spaceBox_shield0_turnBlue");
    }

    public void SetActive(bool state)
    {
        active = state;
        col.enabled = state;
        if (state)
        {
            anim.Play("RushBoss_spaceBox_shield" + parentBoss.attackMode.ToString() + "_spawn");
            lightMask.SetSize(7);
        }
        else
            anim.Play("RushBoss_spaceBox_shield0_inactive");
    }
}
