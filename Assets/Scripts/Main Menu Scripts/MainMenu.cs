using UnityEngine;
using UnityEngine.SceneManagement;


// simple main menu screen to just sttart the game
public class MainMenu : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayGame();
        }
    }

    public void PlayGame()
    {

        SceneManager.LoadScene("Turn Based");
    }
}
