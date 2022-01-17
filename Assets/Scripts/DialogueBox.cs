using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    public Animator anim;
    public AudioSource sfx;

    public GameObject cam;
    public GameObject player;
    public GameObject portrait;
    public TextMesh dialogueText;
    public TextMesh dialogueShadow;
    public Transform roomText;

    public AudioClip dialogue0;

    private float camPos = 0;
    private float portraitPos = 0;
    private int dialogueType = 0;     // 1 = Item popup, 2 = single-page dialogue, 3 = involved multi-page dialogue
    private int boxState = 0;
    private int pointer = 0;          // This pointer looks at what page of text it's looking at
    private bool buttonDown = false;
    private bool active = false;
    private bool playSound = true;
    private bool forcedClosed = false;
    private Vector2 roomTextOrigin;
    private List<SpriteRenderer> portraitParts = new List<SpriteRenderer>();
    private bool forceDownPosition;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
        cam = transform.parent.gameObject;
        portrait = transform.Find("Portrait").gameObject;
        portrait.SetActive(false);
        player = GameObject.FindWithTag("Player");
        dialogueText = transform.Find("Text").gameObject.GetComponent<TextMesh>();
        dialogueShadow = transform.Find("Shadow").gameObject.GetComponent<TextMesh>();
        roomText = GameObject.Find("View/Minimap Panel/Room Name Parent").transform;
        roomTextOrigin = roomText.localPosition;

        portraitParts.Add(portrait.transform.GetChild(0).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(1).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(2).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(3).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(4).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(5).GetComponent<SpriteRenderer>());
    }

    void Update()
    {
        if (dialogueType != 3)
        {
            if (player.transform.position.y > cam.transform.position.y + 0.125f || forceDownPosition)
            {
                if (active)
                    camPos = Mathf.Lerp(transform.localPosition.y, -4.5f, 7 * Time.deltaTime);
                else
                    camPos = -4.5f;
            }
            else
            {
                if (active)
                    camPos = Mathf.Lerp(transform.localPosition.y, 4.5f, 7 * Time.deltaTime);
                else
                    camPos = 4.5f;
            }
        }
        else
        {
            camPos = -4.5f;
        }

        if (dialogueType != 1)
        {
            transform.localPosition = new Vector2(0, camPos);
        }
        else
        {
            transform.localPosition = Vector2.zero;
        }

        portrait.transform.localPosition = new Vector2(-10, portraitPos);
        portraitPos = Mathf.Lerp(portraitPos, 3, 7 * Time.deltaTime);

        if (dialogueType == 2 && boxState == 0)
        {
            if (player.transform.position.y > cam.transform.position.y + 0.125f)
            {
                camPos = transform.localPosition.y - 4.5f;
            }
            else
            {
                camPos = transform.localPosition.y + 4.5f;
            }
        }

        if (active && dialogueType == 2 && player.transform.position.y < cam.transform.position.y + 0.125f)
            roomText.localPosition = new Vector2(Mathf.Lerp(roomText.localPosition.x, roomTextOrigin.x + 5, 8 * Time.deltaTime), roomTextOrigin.y);
        else
            roomText.localPosition = new Vector2(Mathf.Lerp(roomText.localPosition.x, roomTextOrigin.x, 8 * Time.deltaTime), roomTextOrigin.y);
    }

    public void RunBox(int type, int speaker, List<string> text, List<Color32> colors = null, List<int> stateList = null, bool facingLeft = false)
    {
        boxState = 0;
        pointer = 0;
        IEnumerator cor = Box(type, speaker, text, colors, stateList, facingLeft);
        StartCoroutine(cor);
    }

    public IEnumerator Box(int type, int speaker, List<string> text, List<Color32> colors = null, List<int> stateList = null, bool facingLeft = false)
    {
        active = true;
        forcedClosed = false;
        while (active)
        {
            switch (boxState)
            // Case 0 = dialogue box opens
            // Case 1 = initalization of text
            // Case 2 = waiting for a button press to advance text
            // Case 3 = dialogue box closes
            // Case 4 = static box for single-page dialogue
            {
                case 0:
                    anim.SetBool("isActive", true);
                    boxState = 1;
                    dialogueType = type;
                    playSound = true;
                    yield return new WaitForSeconds(0.075f);
                    if (type == 3)
                    {
                        portrait.SetActive(true);
                    }
                    portraitPos = 1;
                    break;
                case 1:
                    if (type == 3)
                    {
                        if (stateList[pointer] != 0)
                        {
                            for (int i = 0; i < portraitParts.Count - 1; i++)
                                portraitParts[i].color = colors[(i + 1) * stateList[pointer] - 1];
                            for (int i = 0; i < portraitParts.Count - 1; i++)
                            {
                                portraitParts[i].enabled = true;
                                if (facingLeft)
                                    portraitParts[i].flipX = true;
                                else
                                    portraitParts[i].flipX = false;
                            }
                            portraitParts[5].enabled = false;
                        }
                        else if (stateList[pointer] == 0)
                        {
                            for (int i = 0; i < portraitParts.Count - 1; i++)
                                portraitParts[i].enabled = false;
                            portraitParts[5].enabled = true;
                            if (facingLeft)
                                portraitParts[5].flipX = false;
                            else
                                portraitParts[5].flipX = true;
                        }
                    }

                    if (type == 1)
                    {
                        dialogueText.text = text[pointer];
                        dialogueShadow.text = text[pointer];
                        pointer++;
                        boxState = 2;
                        if (pointer == 1)
                        {
                            yield return new WaitForSeconds(4);
                        }
                        else
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < text[pointer].Length; i++)
                        {
                            if (forcedClosed)
                            {
                                break;
                            }
                            dialogueText.text += text[pointer][i];
                            dialogueShadow.text = dialogueText.text;
                            if (text[pointer][i] != ' ' && playSound)
                            {
                                sfx.PlayOneShot(dialogue0);
                            }
                            playSound = !playSound;
                            if (Input.GetAxisRaw("Speak") == 0 && buttonDown)
                            {
                                buttonDown = false;
                            }
                            if (Input.GetAxisRaw("Speak") == 1 && !buttonDown && type == 3)
                            {
                                buttonDown = true;
                                dialogueText.text = text[pointer];
                                dialogueShadow.text = text[pointer];
                                break;
                            }
                            yield return new WaitForFixedUpdate();
                        }
                        if (type == 2)
                        {
                            boxState = 4;
                        }
                        else if (type == 3)
                        {
                            pointer++;
                            boxState = 2;
                        }
                    }
                    yield return new WaitForEndOfFrame();
                    break;
                case 2:
                    anim.SetBool("canContinue", true);
                    if (Input.GetAxisRaw("Speak") == 0 && buttonDown)
                    {
                        buttonDown = false;
                    }
                    if (Input.GetAxisRaw("Speak") == 1 && !buttonDown)
                    {
                        buttonDown = true;
                        anim.SetBool("canContinue", false);
                        if (pointer == text.Count)
                        {
                            boxState = 3;
                            yield return new WaitForEndOfFrame();
                        }
                        else
                        {
                            if (type == 3)
                            {
                                dialogueText.text = "";
                                dialogueShadow.text = "";
                            }
                            boxState = 1;
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    yield return new WaitForEndOfFrame();
                    break;
                case 3:
                    CloseBox();
                    yield return new WaitForEndOfFrame();
                    break;
                case 4:
                    yield return new WaitForEndOfFrame();
                    break;
                default:
                    yield return new WaitForEndOfFrame();
                    break;
            }
            if (boxState == 3 && type == 1)
            {
                PlayState.activeMus.clip = PlayState.areaMus;
                PlayState.activeMus.time = PlayState.playbackTime;
                PlayState.activeMus.volume = 0;
                PlayState.activeMus.Play();
                PlayState.gameState = "Game";
                StartCoroutine(nameof(ReturnMusicVol));
            }
        }
    }

    public IEnumerator ReturnMusicVol()
    {
        while (PlayState.activeMus.volume < 1)
        {
            PlayState.activeMus.volume += 0.025f * PlayState.musicVol;
            yield return new WaitForFixedUpdate();
        }
    }

    public void CloseBox()
    {
        if (dialogueType == 3)
            forceDownPosition = true;
        forcedClosed = true;
        active = false;
        pointer = 0;
        dialogueText.text = "";
        dialogueShadow.text = "";
        anim.SetBool("isActive", false);
        portrait.SetActive(false);
        PlayState.paralyzed = false;
        dialogueType = 0;
    }

    public void DeactivateForceDown()
    {
        forceDownPosition = false;
    }
}
