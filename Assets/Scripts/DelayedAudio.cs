using System.Collections;
using UnityEngine;

// Reproduce un audio tras una espera. Sin clip asignado no hace nada.
[RequireComponent(typeof(AudioSource))]
public class DelayedAudio : MonoBehaviour
{
    [SerializeField] float delay = 8f;          // segundos de espera antes de sonar
    [SerializeField] float repeatEvery = 0f;    // 0 = suena una sola vez
    [SerializeField] bool playOnEnable = false; // arranca solo cuando el objeto se activa

    AudioSource source;
    Coroutine routine;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
    }

    void OnEnable() { if (playOnEnable) Play(); }
    void OnDisable() { Cancel(); }

    // Llamar desde un UnityEvent (clic del pajarito, evento del RouteBranch...)
    public void Play()
    {
        Cancel();
        routine = StartCoroutine(Run());
    }

    public void Cancel()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        if (source != null) source.Stop();
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(delay);
        while (true)
        {
            if (source.clip != null) source.Play();
            if (repeatEvery <= 0f) break;
            yield return new WaitForSeconds(repeatEvery);
        }
        routine = null;
    }
}