using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayAudioOnClick : MonoBehaviour
{
    //Elemento que reproduce los sonidos, se debe juntar en el canal de sfx. Debe de tener un clip asociado de antemano. Se debe asociar a el canal de sfx en el editor
    private AudioSource audioOutput;
    private float playTime=0.000f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioOutput = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Si se hizo click
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            if (playTime < 0.000f) { //se ha terminado de reproducir el sonido
                if (CheckClick()){ //y el click fue sobre el objeto,
                    audioOutput.Play();// se reproduce el sonido 
                    playTime = audioOutput.clip.length;//y hacemos que espere a que termine de reproducirse antes de que pueda reproducirse nuevamente
                }
                else//sino ha terminado de reproducirse el sonido
                {
                    playTime -= Time.deltaTime;//contamos cuanto le falta para terminar 
                }

            }
        }
        
    }

    //Se revisa si se hizo click sobre el GameObject, se reproduce el audio asociado por su canal. Se devuelve si se hizo click en el objeto
    private bool CheckClick()
    {
        
        Ray pointer;
        RaycastHit2D cursorHit;      
        
        pointer = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawRay(pointer.origin, pointer.direction * 20f, Color.red, 2f);
        cursorHit = Physics2D.GetRayIntersection(pointer);

        if (cursorHit.transform == transform)
        {
            return true;
        }
        else
        {
            return false;
        }
        
        
    }
}
