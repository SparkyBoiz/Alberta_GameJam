using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void QuitGame()
    {
        GameManager.Instance.Quit();
    }
}
