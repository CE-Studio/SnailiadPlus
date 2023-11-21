using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryBlock : Enemy, IRoomObject
{
    private const float MIN_PHASE_TIME = 0.75f;
    private const float MAX_PHASE_TIME = 2f;
    private const int MAX_PHASES_BETWEEN_IDLE = 3;
    private const float LERP_TIME = 12.5f;
    private const float PLAYER_STARE_RADIUS = 6f;
    private const float IDLE_PHASE_INC_MULT = 1.25f;
    private const float SHOOT_COOLDOWN = 4f;
    private const float SHOOT_DELAY = 0.75f;
    private const float SHOOT_SPEED = 0.15f;
    private const int BULLET_COUNT = 8;
    private const float BULLET_SPEED = 9.25f;
    private const float BULLET_SPREAD = 0.135f;

    private float phaseTime = 0;
    private bool facePlayer = false;
    private int phasesSinceIdle = MAX_PHASES_BETWEEN_IDLE;
    private readonly Vector2 faceRadii = new(0.5f, 0.15f);
    private Vector2 lookDir = Vector2.zero;
    private float weaponTimer = SHOOT_COOLDOWN;
    private float shotCooldown = SHOOT_DELAY;
    private int bulletCount = 0;
    private bool isFiring = false;

    private AnimationModule face;

    public static readonly string myType = "Enemies/Angry Pink Block";

    public Dictionary<string, object> resave()
    {
        return null;
    }

    public string objType
    {
        get
        {
            return myType;
        }
    }

    public Dictionary<string, object> save()
    {
        Dictionary<string, object> content = new();
        return content;
    }

    public void load(Dictionary<string, object> content)
    {
        
    }

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(1500, 0, 32, true);
        canDamage = false;

        face = transform.GetChild(0).GetComponent<AnimationModule>();
        anim.Add("Enemy_angryBlock_body");
        anim.Play("Enemy_angryBlock_body");
        foreach (string newAnim in new string[] { "idle", "N", "NE", "E", "SE", "S", "SW", "W", "NW", "shoot" })
            face.Add("Enemy_angryBlock_face_" + newAnim);
        face.Play("Enemy_angryBlock_face_idle");
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        float playerAngle = Mathf.Atan2(-(transform.position.y - PlayState.player.transform.position.y),
            transform.position.x - PlayState.player.transform.position.x);
        Vector2 playerVector = new(-Mathf.Cos(playerAngle), Mathf.Sin(playerAngle));
        facePlayer = Vector2.Distance(transform.position, PlayState.player.transform.position) < PLAYER_STARE_RADIUS;
        phaseTime -= Time.deltaTime;
        if (PlayState.currentProfile.difficulty == 2)
        {
            weaponTimer -= Time.deltaTime;
            if (weaponTimer <= 0)
            {
                if (!isFiring)
                {
                    bulletCount = 0;
                    shotCooldown = SHOOT_DELAY;
                    face.Play("Enemy_angryBlock_face_shoot");
                    isFiring = true;
                    facePlayer = true;
                }
                shotCooldown -= Time.deltaTime;
                if (shotCooldown <= 0)
                {
                    shotCooldown = SHOOT_SPEED;
                    Vector2 tweakedVector = playerVector + new Vector2(Random.Range(-BULLET_SPREAD, BULLET_SPREAD), Random.Range(-BULLET_SPREAD, BULLET_SPREAD));
                    PlayState.ShootEnemyBullet(this, face.transform.position, EnemyBullet.BulletType.donutLinear, new float[] { BULLET_SPEED, tweakedVector.x, tweakedVector.y });
                    bulletCount++;
                    if (bulletCount >= BULLET_COUNT)
                    {
                        isFiring = false;
                        facePlayer = false;
                        weaponTimer = SHOOT_COOLDOWN;
                    }
                }
            }
        }
        if (phaseTime <= 0 && !isFiring)
        {
            if (facePlayer)
            {
                face.Play("Enemy_angryBlock_face_" + PlayState.VectorToCompass(playerVector));
            }
            else
            {
                if (Random.Range(0f, 1f) > 0.75f || phasesSinceIdle == MAX_PHASES_BETWEEN_IDLE)
                {
                    phasesSinceIdle = 0;
                    lookDir = Vector2.zero;
                    face.Play("Enemy_angryBlock_face_idle");
                }
                else
                {
                    phasesSinceIdle++;
                    float newAngle = Random.Range(0f, PlayState.TAU);
                    Vector2 normalDir = new(-Mathf.Cos(newAngle), Mathf.Sin(newAngle));
                    lookDir = normalDir * faceRadii;
                    face.Play("Enemy_angryBlock_face_" + PlayState.VectorToCompass(normalDir));
                }
            }
            phaseTime = Random.Range(MIN_PHASE_TIME, MAX_PHASE_TIME) * (phasesSinceIdle == 0 ? IDLE_PHASE_INC_MULT : 1);
        }
        if (facePlayer)
        {
            phasesSinceIdle = MAX_PHASES_BETWEEN_IDLE;
            lookDir = playerVector * faceRadii;
            if (!isFiring)
            {
                string newAnim = "Enemy_angryBlock_face_" + PlayState.VectorToCompass(playerVector);
                if (newAnim != face.lastAnimName)
                    face.Play(newAnim);
            }
        }
        face.transform.localPosition = Vector2.Lerp(face.transform.localPosition, lookDir, LERP_TIME * Time.deltaTime);
    }
}
