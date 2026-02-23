
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private string _key = "6";
    public void StartSceneIsZero()
    {
        SceneManager.LoadScene(0);
    }
    public void StartSceneIsOne()
    {
        if(PlayerPrefs.GetString(_key) == _key)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            PlayerPrefs.SetString(_key, _key);          
            SceneManager.LoadScene(6);
        }
    }
    public void StartSceneTwo()
    {
        SceneManager.LoadScene(2);
    }
    public void SceneReset()
    {
        PlayerPrefs.DeleteAll();
    }
    public void Quit()
    {
        Application.Quit();
    }
}
