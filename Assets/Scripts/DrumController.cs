using UnityEngine;
using UnityEngine.InputSystem;

public class DrumController : MonoBehaviour
{
    private AudioSource kick;
    private AudioSource snare;
    private AudioSource hiHat1;
    private AudioSource tom1;
    private AudioSource tom2;
    private AudioSource crash;
    private AudioSource floorTom;

    private DrumVisual kickVis;
    private DrumVisual snareVis;
    private DrumVisual hiHat1Vis;
    private DrumVisual tom1Vis;
    private DrumVisual tom2Vis;
    private DrumVisual crashVis;
    private DrumVisual floorTomVis;

    void Start()
    {
        kick     = GameObject.Find("Kick").GetComponent<AudioSource>();
        snare    = GameObject.Find("Snare drum").GetComponent<AudioSource>();
        hiHat1   = GameObject.Find("HiHat2").GetComponent<AudioSource>();
        tom1     = GameObject.Find("Tom1").GetComponent<AudioSource>();
        tom2     = GameObject.Find("Tom2").GetComponent<AudioSource>();
        crash    = GameObject.Find("CrashCymbal").GetComponent<AudioSource>();
        floorTom = GameObject.Find("FloorTom").GetComponent<AudioSource>();

        kickVis     = GameObject.Find("Kick").GetComponent<DrumVisual>();
        snareVis    = GameObject.Find("Snare drum").GetComponent<DrumVisual>();
        hiHat1Vis   = GameObject.Find("HiHat2").GetComponent<DrumVisual>();
        tom1Vis     = GameObject.Find("Tom1").GetComponent<DrumVisual>();
        tom2Vis     = GameObject.Find("Tom2").GetComponent<DrumVisual>();
        crashVis    = GameObject.Find("CrashCymbal").GetComponent<DrumVisual>();
        floorTomVis = GameObject.Find("FloorTom").GetComponent<DrumVisual>();

    AudioSettings.outputSampleRate = 48000;
    kick.Play(); kick.Stop();
    snare.Play(); snare.Stop();
    hiHat1.Play(); hiHat1.Stop();
    tom1.Play(); tom1.Stop();
    tom2.Play(); tom2.Stop();
    crash.Play(); crash.Stop();
    floorTom.Play(); floorTom.Stop();
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)        Hit(kick, kickVis);
        if (Keyboard.current.sKey.wasPressedThisFrame)            Hit(snare, snareVis);
        if (Keyboard.current.kKey.wasPressedThisFrame)            Hit(hiHat1, hiHat1Vis);
        if (Keyboard.current.rKey.wasPressedThisFrame)     Hit(tom1, tom1Vis);
        if (Keyboard.current.tKey.wasPressedThisFrame)     Hit(tom2, tom2Vis);
        if (Keyboard.current.qKey.wasPressedThisFrame)            Hit(crash, crashVis);
        if (Keyboard.current.yKey.wasPressedThisFrame)            Hit(floorTom, floorTomVis);
    }

    void Hit(AudioSource audio, DrumVisual visual)
    {
        if (audio != null && audio.clip != null)
            audio.PlayOneShot(audio.clip);

        if (visual != null)
            visual.Hit();
    }
}