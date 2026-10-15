using UnityEngine;
using UnityEngine.SceneManagement;

public class Sceneloade : MonoBehaviour
{
   public void Load(string sceneName)
    {
        SceneFader.LoadScene(sceneName);
    }

    public void Reload()
    {
        SceneFader.ReloadCurrent();
    }
}
