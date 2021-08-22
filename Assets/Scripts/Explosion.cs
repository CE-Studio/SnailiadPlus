using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public bool isActive = false;

    public void ResetExpl()
    {
        transform.position = Vector2.zero;
        GetComponent<Animator>().Play("Explosion blank", 0, 0);
        isActive = false;
    }
}
