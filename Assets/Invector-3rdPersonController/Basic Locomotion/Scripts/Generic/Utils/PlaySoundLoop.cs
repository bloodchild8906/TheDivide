using Invector;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundLoop : MonoBehaviour
{
    public AudioClip[] clips;
    public bool playOnAwake;
    public AnimationCurve volumeCurve;
    public AnimationCurve pitchCurve;
    public AnimationCurve frequencyCurve;
    protected AudioSource source;
    [vReadOnly]
    public float currentCurveTime;
    bool isPlaying;

    public void ChangeCurveTime(float curveTime)
    {
        currentCurveTime = curveTime;
    }

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        if (source != null && playOnAwake) { Play(); }
    }

    public void Play()
    {
        isPlaying = true;
        StopAllCoroutines();
        if (gameObject.activeInHierarchy)
        {
            currentCurveTime = 0;
            StartCoroutine(PlayRoutine());
        }
    }

    public void Stop()
    {
        isPlaying = false;
    }

    IEnumerator PlayRoutine()
    {
        float time = 0;
        while (isPlaying)
        {
            float _frequency = frequencyCurve.Evaluate(currentCurveTime);
            if (time < _frequency)
            {
                time += Time.deltaTime;
            }
            else
            {
                time = 0;
                PlayOnShot();
            }
            yield return null;
        }
    }

    protected virtual void PlayOnShot()
    {
        if (source)
        {
            float _frequency = frequencyCurve.Evaluate(currentCurveTime);
            float _volume = volumeCurve.Evaluate(currentCurveTime);
            float _pitch = pitchCurve.Evaluate(currentCurveTime);

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
                source.volume = _volume;
                source.pitch = _pitch;
                source.PlayOneShot(clip);
            }
        }
    }

}
