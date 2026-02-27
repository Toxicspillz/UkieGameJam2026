using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryLoad : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        SceneManager.LoadScene(3);

    }
}
