using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthOrb : MonoBehaviour
{
    public bool isActive = false;
    private int size;
    private float initialAngle;
    private float initialVelocity;
    private float deceleration;
    private float acceleration;
    private float activeVelocity;

    private SpriteRenderer sprite;
    private AnimationModule anim;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;

        anim = GetComponent<AnimationModule>();
        anim.Add("Object_healthOrb_small");
        anim.Add("Object_healthOrb_medium");
        anim.Add("Object_healthOrb_large");
    }

    void Update()
    {
        if (!isActive)
            return;

        if (initialVelocity > 0)
        {
            Vector3 initVel = initialVelocity * Time.deltaTime * new Vector2(Mathf.Cos(initialAngle), Mathf.Sin(initialAngle));
            transform.position += initVel;
            initialVelocity -= deceleration * Time.deltaTime;
        }
        if (initialVelocity < 1.5f)
        {
            Vector3 accelDir = PlayState.playerScript.transform.position - transform.position;
            transform.position += activeVelocity * Time.deltaTime * accelDir.normalized;
            activeVelocity += acceleration * Time.deltaTime;
            if (Vector2.Distance(transform.position, PlayState.player.transform.position) < activeVelocity * Time.deltaTime)
            {
                PlayState.PlaySound("EatHealthOrb");
                HealAndDespawn();
            }
        }
    }

    public void Activate(int newSize)
    {
        isActive = true;
        sprite.enabled = true;
        size = newSize;
        initialAngle = Random.Range(0, PlayState.TAU);
        initialVelocity = Random.Range(3f, 5f);
        deceleration = Random.Range(5f, 8f);
        acceleration = Random.Range(12f, 18f);
        anim.Play("Object_healthOrb_" + size switch { 1 => "medium", 2 => "large", _ => "small" });
    }

    public void HealAndDespawn()
    {
        isActive = false;
        PlayState.playerScript.HitFor(-PlayState.HEALTH_ORB_VALUES[size]);
        sprite.enabled = false;
        activeVelocity = 0;
    }
}
