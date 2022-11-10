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
    public bool done;

    private float mvol = 1.0f;
    public float musicfade;

    private PlayState.GameState oldgs;

    void Start() {
        textbox.text = text;
        oldgs = PlayState.gameState;
    }

    void Update() {
        if (play != playcheck) {
            if (play) {
                audio.Play();
                oldgs = PlayState.gameState;
                PlayState.gameState = PlayState.GameState.error;
            }
            playcheck = play;
        }
        if (musicfade != mvol) {
            mvol = Mathf.Clamp(musicfade, 0.0f, 1.0f);
            musicfade = mvol;
            PlayState.fader = mvol;
            MainMenu.music.volume = mvol;
        }
        if (Control.Pause()) {
            anim.SetBool("done", true);
        }
        if (done) {
            PlayState.gameState = oldgs;
            PlayState.fader = 1.0f;
            Destroy(gameObject);
        }
    }
}
