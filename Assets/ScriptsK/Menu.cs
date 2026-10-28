using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] int numeroScene;
    [SerializeField] 
    GameObject menu,settings,menuGame,menuGame2;

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
    public void MenuGame()
    {
        menuGame2.SetActive(true);
        Time.timeScale = 0; 
    }
    public void CloseMenuGame()
    {
        menuGame2.SetActive(false);
        Time.timeScale = 1;
    }
}
