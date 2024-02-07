using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBoxShield : Enemy
{
    public SpaceBox parentBoss;
    private bool active;
    private LightMask lightMask;

    private void Awake()
    {
        Spawn(100, 4, 1000, false, 0);
        shieldTypeEntity = true;

        anim.Add("Boss_spaceBox_shield0_spawn");
        anim.Add("Boss_spaceBox_shield1_spawn");
        anim.Add("Boss_spaceBox_shield0_hit");
        anim.Add("Boss_spaceBox_shield1_hit");
        anim.Add("Boss_spaceBox_shield0_turnBlue");
        anim.Add("Boss_spaceBox_shield0_inactive");

        SetActive(false);

        lightMask = PlayState.globalFunctions.CreateLightMask(-1, transform);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
            anim.Play("Boss_spaceBox_shield" + parentBoss.attackMode.ToString() + "_hit");
        base.OnTriggerEnter2D(collision);
    }

    public void TurnBlue()
    {
        if (active)
            anim.Play("Boss_spaceBox_shield0_turnBlue");
    }

    public void SetActive(bool state)
    {
        active = state;
        col.enabled = state;
        if (state)
        {
            lightMask.SetSize(7);
            anim.Play("Boss_spaceBox_shield" + parentBoss.attackMode.ToString() + "_spawn");
        }
        else
            anim.Play("Boss_spaceBox_shield0_inactive");
    }
}
