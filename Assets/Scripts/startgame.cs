using UnityEngine;
using UnityEngine.SceneManagement;

// Simple start-screen controller that opens the main training scene.
public class StartGame : MonoBehaviour
{
    // Triggered by the Start button on the first screen.
    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
