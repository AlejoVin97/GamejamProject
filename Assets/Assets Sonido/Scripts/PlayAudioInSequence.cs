using UnityEngine;
using UnityEngine.Audio;

public class PlayAudioInSequence : MonoBehaviour
{
    //Elemento que reproduce los sonidos, se debe juntar en el canal de sfx
    private AudioSource audioOutput;
    //Sonidos a reproducir
    public AudioClip audioOne;
    public AudioClip audioTwo;
    private bool audioOnePlayed = false;//Determina cual audio debe reproducirse, iniciando por audioOne. Si ya se reprodujo audioOne, se debe reproducir audioTwo despues
    float timer = 0.0000f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        audioOutput = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Prueba - si no se ha terminado de reproducir el sonido actual, esperar a que termine de reproducirse.
        if (timer > 0.000f)
        {
            timer -= Time.deltaTime;
        }
        else 
        {//si no hay un sonido reproduciendose (o se ha terminado de reproducir el sonido actual), reproducir el siguiente sonido y esperar a que termine de reproducirse
            timer = PlayNextAudio();
        }
    }

    
    //Reproduce el siguiente sonido a reproducir entre los 2, iniciando por audioOne. Retorna la duracion del sonido actual, para manejo de esperas antes de volver a llamar esta funcion
    public float PlayNextAudio()
    {
        // Segun el ultimo sonido que se reprodujo, se asigna el otro a la salida de audio y se cambia la bandera para reconocer el siguiente a reproducir
        if (audioOnePlayed)
        {
            audioOutput.clip = audioTwo;
            audioOnePlayed = false;
        }
        else
        {
            audioOutput.clip = audioOne;
            audioOnePlayed = true;
        }

        //Se reproduce el sonido actual
        audioOutput.Play();

        //
        return audioOutput.clip.length;
                
    }
}
