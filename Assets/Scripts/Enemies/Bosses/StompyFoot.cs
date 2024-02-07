using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompyFoot : Enemy
{
    private void Awake()
    {
        Spawn(50000, 2, 0, true, 0);
    }

    public void StartFlash()
    {
        StartCoroutine(Flash(false));
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
            return;
        base.OnTriggerEnter2D(collision);
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
            return;
        base.OnTriggerExit2D(collision);
    }
}
