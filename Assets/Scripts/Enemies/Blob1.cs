using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob1 : Enemy
{
    private readonly float[] HOP_TIMEOUTS = { 2.4f, 3.5f, 2.2f, 1.6f, 3.9f, 3.5f, 2.9f, 3.1f, 1.8f };
    private readonly float[] TOP_HEIGHTS = { 1, 1, 1, 1.2f, 2, 1, 1.2f, 1, 2 };

    private int hopNum = 0;
    private float hopTimeout = 0;
    private Vector2 velocity = Vector2.zero;
    private bool facingRight = false;

    void Awake()
    {
        Begin();
        box.size = new Vector2(0.95f, 0.95f);
        attack = 2;
        defense = 1;
        maxHealth = 50;
        health = 50;
        letsPermeatingShotsBy = true;

        anim.Add("Enemy_blob1_normal");
        anim.Add("Enemy_blob1_jump");
        anim.Add("Enemy_blob1_quiver");

        gameObject.SetActive(false);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Face(PlayState.player.transform.position.x > transform.position.x);
        hopNum = Mathf.FloorToInt(transform.position.x) % HOP_TIMEOUTS.Length;
        hopTimeout = HOP_TIMEOUTS[hopNum] / 3;
        anim.Play("Enemy_blob1_normal");
    }

    void Update()
    {
        
    }

    private void Face(bool direction)
    {
        facingRight = direction;
        sprite.flipX = direction;
    }
}
