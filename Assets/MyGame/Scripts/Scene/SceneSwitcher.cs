using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public int gameSceneIndex = 1;
    public string menuSceneName = "MainMenu";
    public int menuSceneIndex = 0;

    public void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            SceneManager.LoadScene(gameSceneIndex);
    }

    public void LoadMenuScene()
    {
        if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
        else
            SceneManager.LoadScene(menuSceneIndex);
    }
}