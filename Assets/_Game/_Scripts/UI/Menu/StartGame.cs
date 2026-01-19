
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void StartSceneIsZero()
    {
        SceneManager.LoadScene(0);
    }
    public void StartSceneIsOne()
    {
        SceneManager.LoadScene(1);
    }
    public void StartSceneTwo()
    {
        SceneManager.LoadScene(2);
    }
}
