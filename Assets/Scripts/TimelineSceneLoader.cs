/*
2026-11-12 AI-Tag
This was created with the help of Assistant, a Unity Artificial Intelligence product.
*/
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayableDirector))]
public sealed class TimelineSceneLoader : MonoBehaviour
{
    [Header("Timeline")]
    [Tooltip("Director cuya parada provoca el cambio de escena.")]
    [SerializeField] private PlayableDirector _director;

    [Header("Escena de destino")]
    [Tooltip("Nombre de la escena incluida en el build, sin .unity. " +
             "También admite su ruta completa para evitar nombres ambiguos.")]
    [SerializeField] private string _nextSceneName;

    private bool _isLoading;

    // Facilita la configuración al añadir el componente desde el editor.
    private void Reset()
    {
        _director = GetComponent<PlayableDirector>();
    }

    private void Awake()
    {
        // Obtener la referencia una sola vez, no en Update.
        if (_director == null)
            _director = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        if (_director != null)
            _director.stopped += OnDirectorStopped;
    }

    private void OnDisable()
    {
        // Evita suscripciones duplicadas y referencias pendientes.
        if (_director != null)
            _director.stopped -= OnDirectorStopped;
    }

    private void OnDirectorStopped(PlayableDirector stoppedDirector)
    {
        // Evita iniciar varias cargas por eventos repetidos.
        if (_isLoading || stoppedDirector != _director)
            return;

        if (string.IsNullOrWhiteSpace(_nextSceneName))
        {
            Debug.LogError(
                "TimelineSceneLoader: configura la escena de destino.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(_nextSceneName))
        {
            Debug.LogError(
                $"TimelineSceneLoader: la escena '{_nextSceneName}' no está " +
                "disponible. Comprueba la lista de escenas del build.", this);
            return;
        }

        _isLoading = true;

        // Single sustituye la escena actual.
        // La activación sucede automáticamente al terminar la carga.
        SceneManager.LoadSceneAsync(_nextSceneName, LoadSceneMode.Single);
    }
}
