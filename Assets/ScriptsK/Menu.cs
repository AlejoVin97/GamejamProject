using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] int numeroScene;
    [SerializeField] 
    GameObject menu,settings,menuGame;

    public void PlayGame()
    {
        SceneManager.LoadScene(numeroScene);
    }

    public void Settings()
    {
        menu.SetActive(false);
        settings.SetActive(true);
    }

    public void MainMenu()
    {
        menu.SetActive(true);
        settings.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void CloseMenu()
    {
        menuGame.SetActive(false);
    }
}
