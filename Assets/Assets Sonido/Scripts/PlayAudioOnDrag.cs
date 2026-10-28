using UnityEngine;
using UnityEngine.InputSystem;

public class PlayAudioOnDrag : MonoBehaviour
{
    //Elemento que reproduce los sonidos, se debe juntar en el canal de sfx. Debe de tener un clip asociado de antemano. Se debe asociar a el canal de sfx en el editor
    private AudioSource audioOutput;
    private float playTime = 0.000f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Se revisa si se hizo click sobre el GameObject. Se devuelve si se hizo click en el objeto
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
