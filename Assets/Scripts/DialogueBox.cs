using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    public Animator anim;
    public AudioSource sfx;

    public GameObject cam;
    public GameObject player;
    public TextMesh dialogueText;
    public TextMesh dialogueShadow;

    public AudioClip dialogue0;

    private float camPos = 0;
    private int dialogueType = 1;     // 1 = Item popup, 2 = single-page dialogue, 3 = involved multi-page dialogue
    private int boxState = 0;
    private int pointer = 0;          // This pointer looks at what page of text it's looking at
    private bool buttonDown = false;
    private bool active = false;
    private bool playSound = true;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
        cam = transform.parent.gameObject;
        player = GameObject.FindWithTag("Player");
        dialogueText = transform.Find("Text").gameObject.GetComponent<TextMesh>();
        dialogueShadow = transform.Find("Shadow").gameObject.GetComponent<TextMesh>();
    }

    void Update()
    {
        if (player.transform.position.y > cam.transform.position.y)
        {
            camPos = Mathf.Lerp(transform.position.y, -4.5f, 1f);
        }
        else
        {
            camPos = Mathf.Lerp(transform.position.y, 4.5f, 1f);
        }
        if (dialogueType != 1)
        {
            transform.localPosition = new Vector2(0, camPos);
        }
        else
        {
            transform.localPosition = Vector2.zero;
        }
    }

    public void RunBox(int type, int speaker, List<string> text)
    {
        boxState = 0;
        pointer = 0;
        IEnumerator cor = Box(type, speaker, text);
        StartCoroutine(cor);
    }

    public IEnumerator Box(int type, int speaker, List<string> text)
    {
        active = true;
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
                    yield return new WaitForSeconds(0.25f);
                    break;
                case 1:
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
                            dialogueText.text += text[pointer][i];
                            dialogueShadow.text = dialogueText.text;
                            if (text[pointer][i] != ' ' && playSound)
                            {
                                sfx.PlayOneShot(dialogue0);
                            }
                            playSound = !playSound;
                            yield return new WaitForFixedUpdate();
                        }
                        if (type == 2)
                        {
                            boxState = 4;
                        }
                        else if (type == 3)
                        {
                            boxState = 2;
                        }
                    }
                    yield return new WaitForEndOfFrame();
                    break;
                case 2:
                    anim.SetBool("canContinue", true);
                    if (Input.GetAxisRaw("Jump") == 0 && buttonDown)
                    {
                        buttonDown = false;
                    }
                    if (Input.GetAxisRaw("Jump") == 1 && !buttonDown)
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
                            boxState = 1;
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    yield return new WaitForEndOfFrame();
                    break;
                case 3:
                    active = false;
                    pointer = 0;
                    dialogueText.text = "";
                    dialogueShadow.text = "";
                    anim.SetBool("isActive", false);
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
                PlayState.mus.clip = PlayState.areaMus;
                PlayState.mus.time = PlayState.playbackTime;
                PlayState.mus.volume = 0;
                PlayState.mus.Play();
                PlayState.gameState = "Game";
                StartCoroutine(nameof(ReturnMusicVol));
            }
        }
    }

    public IEnumerator ReturnMusicVol()
    {
        while (PlayState.mus.volume < 1)
        {
            PlayState.mus.volume += 0.025f * PlayState.musicVol;
            yield return new WaitForFixedUpdate();
        }
    }

    public void CloseBox()
    {
        boxState = 3;
    }
}
