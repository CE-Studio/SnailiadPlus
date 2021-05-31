using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemType = "Rainbow Wave";

    public Animator anim;
    public BoxCollider2D box;

    public bool countedInPercentage;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        box = GetComponent<BoxCollider2D>();

        switch (itemType)
        {
            case "Rainbow Wave":
                anim.SetInteger("itemType", 0);
                box.size = new Vector2(1.25f, 2f);
                break;
            default:
                anim.SetInteger("itemType", 0);
                box.size = new Vector2(1.25f, 2f);
                break;
        }
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            switch (itemType)
            {
                case "Rainbow Wave":
                    PlayState.hasRainbowWave = true;
                    collision.GetComponent<Player>().selectedWeapon = 0;
                    break;
                default:
                    break;
            }
            PlayState.RunItemPopup(itemType);
            Destroy(gameObject);
        }
    }
}
