using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompyEye : Enemy
{
    public void StartFlash()
    {
        StartCoroutine(Flash(false));
    }
}
