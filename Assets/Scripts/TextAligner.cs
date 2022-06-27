using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAligner : MonoBehaviour
{
    public Vector2 originalPos;
    public Vector2 posOffset;
    
    void Start()
    {
        originalPos = transform.localPosition;
        posOffset = new Vector2(
            Mathf.Round(transform.position.x * 16) * 0.0625f - transform.position.x,
            Mathf.Round(transform.position.y * 16) * 0.0625f - transform.position.y
            );
    }

    void Update()
    {
        transform.localPosition = originalPos;
        transform.position = new Vector2(
            Mathf.Round((transform.position.x + posOffset.x) * 16) * 0.0625f - 0.01f,
            Mathf.Round((transform.position.y + posOffset.y) * 16) * 0.0625f
            );
    }
}
