using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class errorcontrol : MonoBehaviour {

    public static string text;
    public UnityEngine.UI.Text textbox;
    public AudioSource audio;
    public Animator anim;

    private bool playcheck = false;
    public bool play;

    private float mvol = 1.0f;
    public float musicfade;

    void Start() {
        textbox.text = text;
    }

    void Update() {
        if (play != playcheck) {
            if (play) {
                audio.Play();
            }
            playcheck = play;
        }
        if (musicfade != mvol) {
            mvol = Mathf.Clamp(musicfade, 0.0f, 1.0f);
            musicfade = mvol;
            PlayState.fader = mvol;
            MainMenu.music.volume = mvol;
        }
    }
}
