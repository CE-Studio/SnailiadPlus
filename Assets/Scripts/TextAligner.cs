using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAligner : MonoBehaviour
{
    Transform cam;
    Vector2 originalPos;
    
    void Start()
    {
        cam = GameObject.Find("View").transform;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        float camTruePosX = cam.position.x;
        float camTruePosY = cam.position.y;
        float camSnappedPosX = Mathf.Round(cam.position.x * 16) * 0.0625f;
        float camSnappedPosY = Mathf.Round(cam.position.y * 16) * 0.0625f;
        float x = camSnappedPosX - camTruePosX;
        float y = camSnappedPosY - camTruePosY;

        transform.localPosition = new Vector2(originalPos.x + x, originalPos.y + y);
    }
}
