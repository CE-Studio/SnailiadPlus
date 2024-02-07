using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGrass : MonoBehaviour
{
    public int totalBites = 12;
    public int healthPerBite = 3;
    public float biteTimeout = 0.17f;

    public int bitesRemaining;
    public bool active = false;
    public float timer = 0;

    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AnimationModule anim;

    public bool isCeilingGrass = false;

    public GameObject player;

    public void Start()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            bitesRemaining = PlayState.currentProfile.character == "Leechy" ? 3 : totalBites;
            sprite = GetComponent<SpriteRenderer>();
            box = GetComponent<BoxCollider2D>();
            anim = GetComponent<AnimationModule>();
            anim.Add(string.Format("PowerGrass_{0}_eaten", isCeilingGrass ? "ceiling" : "floor"));
            anim.AddAndPlay(string.Format("PowerGrass_{0}_idle", isCeilingGrass ? "ceiling" : "floor"));

            Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);
        }
    }

    public void ToggleActive(bool state)
    {
        active = state;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !PlayState.paralyzed && !(PlayState.currentProfile.character == "Leechy" && PlayState.playerScript.stunned))
        {
            if (timer == 0)
            {
                PlayState.PlaySound("EatPowerGrass");
                bitesRemaining--;
                if (bitesRemaining == 0)
                {
                    box.enabled = false;
                    anim.Play(string.Format("PowerGrass_{0}_eaten", isCeilingGrass ? "ceiling" : "floor"));
                }
                else
                    timer = biteTimeout;
                PlayState.playerScript.HitFor(-healthPerBite * (PlayState.currentProfile.character == "Leechy" ? -1 : 1));
                if (PlayState.generalData.particleState > 1)
                    PlayState.RequestParticle(new Vector2(transform.position.x, transform.position.y + 0.25f), "nom");
            }
        }
    }
}
