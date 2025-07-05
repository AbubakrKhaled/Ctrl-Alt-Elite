using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void close_game()
    {
        Application.Quit();
    }

    public void start_game()
    {
        SceneManager.LoadScene("Sewer");
    }
}
