using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider2 : Enemy
{
    private const float MOVE_TIMEOUT = 0.3f;
    private const float SPEED = 5.3125f;

    private readonly float[] waitTable = new float[]
    {
        0.0031435364f, 0.5049282581f, 0.4116654365f, 0.5281877909f, 0.5897768281f, 0.1593536691f, 0.4039032663f, 0.4059745561f, 0.0582864048f, 0.7516076167f,
        0.5521423097f, 0.5566189571f, 0.179637362f, 0.3594656996f, 0.4505799066f, 0.3204984303f, 0.6052265986f, 0.6324895486f, 0.4429925385f, 0.8837227036f,
        0.9752844916f, 0.6930966018f, 0.1752373743f, 0.3443669975f, 0.4504892571f, 0.5988067483f, 0.4101707911f, 0.2408469776f, 0.5234045926f, 0.0236403878f,
        0.6238985334f, 0.6958397015f, 0.270161708f, 0.621824504f, 0.9523979626f, 0.234787262f, 0.3882103268f, 0.2584722478f, 0.8966220748f, 0.6939320403f,
        0.2789706634f, 0.0615652706f, 0.9490876411f, 0.3085988573f, 0.4638467981f, 0.6217814413f, 0.7608899529f, 0.4005289448f, 0.1308170265f, 0.4694031184f,
        0.893697234f, 0.0653774173f, 0.7188370915f, 0.6329837926f, 0.1353358423f, 0.2568268114f, 0.8794799448f, 0.0097632309f, 0.6912924617f, 0.7049703039f,
        0.9255215652f, 0.9475764837f, 0.8637740622f, 0.1508763211f, 0.3508634319f, 0.5790814897f, 0.7811909111f, 0.7848816088f, 0.5664273614f, 0.0356774301f,
        0.3960516107f, 0.6490766209f, 0.947092078f, 0.5093130463f, 0.1259762285f, 0.6807200132f, 0.5178459596f, 0.6560252702f, 0.5672414089f, 0.890198107f,
        0.230094122f, 0.642458186f, 0.3116517752f, 0.7473356059f, 0.4938040994f, 0.0920139665f, 0.4272894969f, 0.3406688463f, 0.8804234661f, 0.1334528937f,
        0.9890385972f, 0.9199415992f, 0.0593612118f, 0.5765849264f, 0.7873382929f, 0.827340683f, 0.1771514581f, 0.7038809678f, 0.5615721056f, 0.6086805593f
    };

    private float moveTimeout = 0;
    private int moveIndex = 0;
    private Vector2 velocity = Vector2.zero;
    private bool useDirectionalAnims = false;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(2100, 4, 10, true);
        PlayState.enemyGlobalMoveIndex = (PlayState.enemyGlobalMoveIndex + 1) % waitTable.Length;
        moveIndex = PlayState.enemyGlobalMoveIndex;
        moveTimeout = waitTable[moveIndex] * MOVE_TIMEOUT;

        int animData = PlayState.GetAnim("Enemy_spider_red_data").frames[0];
        useDirectionalAnims = animData == 1;

        anim.Add("Enemy_spider_red_normal");
        anim.Add("Enemy_spider_red_N");
        anim.Add("Enemy_spider_red_NE");
        anim.Add("Enemy_spider_red_E");
        anim.Add("Enemy_spider_red_SE");
        anim.Add("Enemy_spider_red_S");
        anim.Add("Enemy_spider_red_SW");
        anim.Add("Enemy_spider_red_W");
        anim.Add("Enemy_spider_red_NW");
        anim.Play("Enemy_spider_red_normal");
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.OnScreen(transform.position, col))
        {
            moveTimeout -= Time.deltaTime;
            if (moveTimeout <= 0)
            {
                moveIndex = (moveIndex + 1) % waitTable.Length;
                moveTimeout = waitTable[moveIndex] * MOVE_TIMEOUT;
                float angle = Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y,
                    PlayState.player.transform.position.x - transform.position.x);
                velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SPEED;
                if (useDirectionalAnims)
                    UpdateAnim(angle);
            }
            transform.position += (Vector3)velocity * Time.deltaTime;
        }
    }

    private void UpdateAnim(float angle)
    {
        string expectedAnimName = "Enemy_spider_red_";
        if (angle > Mathf.PI - PlayState.PI_OVER_EIGHT)
            expectedAnimName += "W";
        else if (angle > PlayState.PI_OVER_TWO + PlayState.PI_OVER_FOUR - PlayState.PI_OVER_EIGHT)
            expectedAnimName += "NW";
        else if (angle > PlayState.PI_OVER_TWO - PlayState.PI_OVER_EIGHT)
            expectedAnimName += "N";
        else if (angle > PlayState.PI_OVER_EIGHT)
            expectedAnimName += "NE";
        else if (angle > -PlayState.PI_OVER_EIGHT)
            expectedAnimName += "E";
        else if (angle > -PlayState.PI_OVER_TWO + PlayState.PI_OVER_EIGHT)
            expectedAnimName += "SE";
        else if (angle > -PlayState.PI_OVER_TWO - PlayState.PI_OVER_FOUR + PlayState.PI_OVER_EIGHT)
            expectedAnimName += "S";
        else if (angle > -Mathf.PI + PlayState.PI_OVER_EIGHT)
            expectedAnimName += "SW";
        else
            expectedAnimName += "W";
        if (anim.currentAnimName != expectedAnimName)
            anim.Play(expectedAnimName);
    }
}
