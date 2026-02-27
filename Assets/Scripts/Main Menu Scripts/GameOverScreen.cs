using UnityEngine;
using UnityEngine.SceneManagement;

// simple game over screen meant to either restart ot quit the game

public class GameOverScreen : MonoBehaviour
{
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.J))
        {
            RestartGame();
        }


        if (Input.GetKeyDown(KeyCode.K))
        {
            GoToMainMenu();
        }
    }

    
    public void RestartGame()
    {

        if (GameSession.gs != null)
        {
            Destroy(GameSession.gs.gameObject);
        }


        SceneManager.LoadScene("Turn Based");
    }

    
    public void GoToMainMenu()
    {

        if (GameSession.gs != null)
        {
            Destroy(GameSession.gs.gameObject);
        }


        SceneManager.LoadScene("MainMenu");
    }
}
