using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Scriptable Objects/ParticleSpriteCollection", order = 1)]
public class ParticleSpriteCollection : ScriptableObject
{
    public Sprite blank;
    public Sprite[] bubble;
    public Sprite[] star;
}
