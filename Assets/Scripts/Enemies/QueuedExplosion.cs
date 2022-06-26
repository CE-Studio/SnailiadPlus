using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueuedExplosion : MonoBehaviour
{
    float lifeTime = 0;
    int size = 0;
    bool makeNoise = false;

    public void Spawn(float life, int s, bool loud)
    {
        lifeTime = life;
        size = s;
        makeNoise = loud;
        Destroy(GetComponent<SpriteRenderer>());
    }

    void Update()
    {
        if (PlayState.gameState != "Game")
            return;

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
            Destroy(gameObject);

        if (size == 1)
        {
            GenerateExplosions(3, 15, 5);
            GenerateExplosions(2, 17.5f, 4);
            GenerateExplosions(3, 19.375f, 4);
            GenerateExplosions(2, 22.5f, 4);
            GenerateExplosions(3, 26.875f, 4);
            GenerateExplosions(4, 26.875f, 2);
        }
        GenerateExplosions(2, 10, 5);
        GenerateExplosions(3, 7.5f, 2);
        GenerateExplosions(2, 5, 3);
        GenerateExplosions(3, 3.75f, 2);
        if (Random.Range(0, 21) > 17)
            GenerateExplosions(3, 8.125f, 1);
        if (makeNoise)
        {
            if (Random.Range(0f, 1f) > 0.2f)
                PlayState.PlaySound("Explode" + Random.Range(1, 5).ToString());
            if (Random.Range(0f, 1f) > 0.2f)
                PlayState.PlaySound("EnemyKilled" + Random.Range(1, 4).ToString());
        }
    }

    private void GenerateExplosions(int size, float distance, int count = 1)
    {
        for (int i = 0; i < count; i++)
            PlayState.RequestParticle(RandomAngle(distance), "explosion", new float[] { size }, false);
    }

    private Vector2 RandomAngle(float distance)
    {
        return transform.position + (Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector2.up * (Random.Range(0f, 1f) * distance));
    }
}
