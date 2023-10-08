using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsEntity : MonoBehaviour
{
    public enum Entities
    {
        None,
        BlueSpikey,
        OrangeSpikey,
        GreenBabyfish,
        PinkBabyfish,
        BlackFloatspike,
        BlueFloatspike,
        Blob,
        Blub,
        Angelblob,
        Devilblob,
        BlueChirpy,
        AquaChirpy,
        BattyBat,
        Fireball,
        Iceball,
        Snelk,
        GrayKitty,
        OrangeKitty,
        Dandelion,
        Canon,
        CanonBase,
        NonCanon,
        NonCanonBase,
        GreenSnakey,
        BlueSnakey,
        SkyViper,
        Spider,
        SpiderMama,
        GreenTurtle,
        CherryTurtle,
        Jellyfish,
        Syngnathida,
        Tallfish,
        AngryTallfish,
        Walleye,
        Pincer,
        Pouncer,
        SkyPincer,
        Spinnygear,
        FederationDrone,
        BalloonBuster,
        Shellbreaker,
        ShellbreakerEye,
        ShellbreakerHand,
        Stompy,
        StompyFootL,
        StompyEyeL,
        StompyEyelidL,
        StompyPupilL,
        StompyFootR,
        StompyEyeR,
        StompyEyelidR,
        StompyPupilR,
        SpaceBox,
        MoonSnail,
        GigaSnail,
        Snaily,
        Sluggy,
        Upside,
        Leggy,
        Blobby,
        Leechy,
        NewStarshipSmell,
        Xdanond,
        AdamAtomic,
        Auriplane,
        Epsilon,
        Clarence,
        Nat,
        Broomie,
        Zettex,
        Minervo,
        GoldGuy,
        Discord,
        Xander,
        ForTheFunny,
        TheEnd
    }
    public Entities type = Entities.None;

    private readonly float[] entityHeights = new float[]
    {
        0, 16, 16, 10, 10, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 24, 16, 16, 16, 24, 16, 24, 16, 16, 16, 16, 16, 16, 32, 32, 16, 32, 48, 48, 16, 16, 16, 16,
        32, 32, 32, 48, 48, 24, 180, 156, 52, 52, 52, 156, 52, 52, 52, 128, 16, 44, 16, 16, 16, 16, 16, 16, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 122
    };

    public SpriteRenderer sprite;
    public AnimationModule anim;
    public GameObject creditsEntity;
    public CreditsEntity entity;

    private bool stateInitialized = false;
    private Vector2 origin;
    public float genericTheta;

    public void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        creditsEntity = Resources.Load<GameObject>("Objects/Credits Entity");
        entity = Resources.Load<CreditsEntity>("Objects/Credits Entity");
    }

    public void Update()
    {
        switch (type)
        {
            default:
                break;
            case Entities.BlackFloatspike:
                genericTheta += Time.deltaTime * 0.8f;
                transform.localPosition = new Vector2(origin.x, origin.y + (0.125f * Mathf.Sin(genericTheta)));
                break;
            case Entities.BlueFloatspike:
                genericTheta += Time.deltaTime * 0.85f;
                transform.localPosition = new Vector2(origin.x, origin.y + (0.125f * Mathf.Cos(genericTheta)));
                break;
            case Entities.Dandelion:
            case Entities.BalloonBuster:
                genericTheta += Time.deltaTime * 0.8f;
                transform.localPosition = new Vector2(origin.x + (2.25f * Mathf.Cos(genericTheta)), origin.y);
                break;
            case Entities.Canon:
                if (!stateInitialized)
                {
                    transform.localPosition += 0.25f * Vector3.down;
                    CreditsEntity canonBase = Instantiate(entity, transform.parent);
                    canonBase.Spawn(Entities.CanonBase, transform.localPosition, -48);
                }
                break;
            case Entities.NonCanon:
                if (!stateInitialized)
                {
                    transform.localPosition += 0.25f * Vector3.down;
                    CreditsEntity nonCanonBase = Instantiate(entity, transform.parent);
                    nonCanonBase.Spawn(Entities.NonCanonBase, transform.localPosition, -48);
                }
                break;
            case Entities.Shellbreaker:
                if (!stateInitialized)
                {
                    CreditsEntity shellbreakerEyes = Instantiate(entity, transform.parent);
                    shellbreakerEyes.Spawn(Entities.ShellbreakerEye, transform.localPosition, -48);
                    float newTheta = 0f;
                    for (int i = 0; i < 6; i++)
                    {
                        CreditsEntity newHand = Instantiate(entity, transform.parent);
                        newHand.Spawn(Entities.ShellbreakerHand, transform.localPosition);
                        newHand.genericTheta = newTheta;
                        newTheta += PlayState.PI_OVER_THREE;
                    }
                }
                break;
            case Entities.ShellbreakerHand:
                genericTheta += Time.deltaTime * 4f;
                transform.localPosition = new Vector2(origin.x + (3f * Mathf.Sin(genericTheta)), origin.y + (3f * Mathf.Cos(genericTheta)));
                break;
            case Entities.Stompy:
                if (!stateInitialized)
                {
                    List<CreditsEntity> stompyParts = new();
                    for (int i = 0; i < 8; i++)
                        stompyParts.Add(Instantiate(entity, transform.parent));
                    stompyParts[0].Spawn(Entities.StompyFootL, transform.localPosition + new Vector3(-6.25f, -1f, 0));
                    stompyParts[1].Spawn(Entities.StompyFootR, transform.localPosition + new Vector3(6.25f, -1f, 0));
                    stompyParts[2].Spawn(Entities.StompyEyeL, transform.localPosition + new Vector3(-3.0625f, 4f, 0), -48);
                    stompyParts[3].Spawn(Entities.StompyEyeR, transform.localPosition + new Vector3(2.8125f, 4f, 0), -48);
                    stompyParts[4].Spawn(Entities.StompyEyelidL, transform.localPosition + new Vector3(-3.0625f, 4f, 0), -46);
                    stompyParts[5].Spawn(Entities.StompyEyelidR, transform.localPosition + new Vector3(2.8125f, 4f, 0), -46);
                    stompyParts[6].Spawn(Entities.StompyPupilL, transform.localPosition + new Vector3(-3.0625f, 4f, 0), -47);
                    stompyParts[7].Spawn(Entities.StompyPupilR, transform.localPosition + new Vector3(2.8125f, 4f, 0), -47);
                }
                break;
            case Entities.TheEnd:
                if (transform.position.y > PlayState.cam.transform.position.y)
                {
                    transform.position = new Vector2(transform.position.x, PlayState.cam.transform.position.y);
                    transform.parent.parent.GetComponent<Credits>().creditsDone = true;
                }
                break;
        }
        if (!stateInitialized && type != Entities.None)
        {
            stateInitialized = true;
            if (type != Entities.Stompy)
            {
                string animName = "Credits_" + System.Enum.GetName(typeof(Entities), (int)type).ToLower();
                int[] animData = PlayState.GetAnim(animName).frames;
                sprite.flipX = animData[0] == 1;
                sprite.flipY = animData[1] == 1;
                anim.Add(animName);
                anim.Play(animName);
            }
            else
                sprite.enabled = false;
        }
    }

    public float Spawn(Entities type, float x, float topY, int sortingOrder = -49)
    {
        this.type = type;
        float parsedHeight = entityHeights[(int)type] * PlayState.FRAC_16;
        transform.localPosition = new Vector2(x, topY - (parsedHeight * 0.5f));
        origin = transform.localPosition;
        sprite.sortingOrder = sortingOrder;
        return parsedHeight;
    }
    public float Spawn(Entities type, Vector2 pos, int sortingOrder = -49)
    {
        this.type = type;
        float parsedHeight = entityHeights[(int)type] * PlayState.FRAC_16;
        transform.localPosition = pos;
        origin = transform.localPosition;
        sprite.sortingOrder = sortingOrder;
        return parsedHeight;
    }
}
