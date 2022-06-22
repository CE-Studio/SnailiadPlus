using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chirpy1Generator : MonoBehaviour
{
    private const float BASE_TIMEOUT = 7;
    private readonly float[] TIMEOUTS = new float[] { 0.60153f, 0.48509f, 0.70037f, 0.66276f, 0.70802f, 0.79541f, 0.62043f, 0.5796f,
        0.99605f, 0.15058f, 0.72121f, 0.86851f, 0.64371f, 0.76708f, 0.89401f, 0.52828f, 0.72309f, 0.15963f, 0.15116f, 0.1799f, 0.27829f,
        0.40878f, 0.92538f, 0.45074f, 0.18865f, 0.59797f, 0.4318f, 0.94098f, 0.23463f, 0.29221f, 0.59734f, 0.34877f, 0.81676f, 0.57617f,
        0.14883f, 0.16094f, 0.14123f, 0.57931f, 0.85924f, 0.22828f, 0.63834f, 0.10387f, 0.54746f, 0.24897f, 0.11105f, 0.49748f, 0.54746f,
        0.19405f, 0.79792f, 0.36023f, 0.53726f, 0.78544f, 0.60425f, 0.83512f, 0.01696f, 0.10451f, 0.01513f, 0.78678f, 0.51617f, 0.24251f };

    private float timeout = 0;
    private int listPos = 0;

    private GameObject bird;

    void Awake()
    {
        listPos = Mathf.RoundToInt(Mathf.Abs(transform.position.x * 23 - transform.position.y * 17) % TIMEOUTS.Length);
        timeout = TIMEOUTS[listPos] * BASE_TIMEOUT;
        bird = Resources.Load<GameObject>("Objects/Enemies/Chirpy (blue)");
        GetComponent<SpriteRenderer>().enabled = false;
    }

    void Update()
    {
        if (PlayState.gameState != "Game")
            return;

        timeout -= Time.deltaTime;
        if (timeout < 0)
        {
            listPos = (listPos + 1) % TIMEOUTS.Length;
            timeout = TIMEOUTS[listPos] * BASE_TIMEOUT;
            float spawnX = Random.Range(0f, 1f) > 0.5f ? PlayState.cam.transform.position.x + 13 : PlayState.cam.transform.position.x - 13;
            if (Mathf.Abs(PlayState.player.transform.position.x - spawnX) < 4.6875f)
                return;
            float spawnY = PlayState.cam.transform.position.y + (Random.Range(-1f, 1f) * 7.5f);
            GameObject newBird = Instantiate(bird, new Vector2(spawnX, spawnY), Quaternion.identity, transform.parent);
            newBird.GetComponent<Chirpy1>().spawnedViaGenerator = true;
        }
    }
}
