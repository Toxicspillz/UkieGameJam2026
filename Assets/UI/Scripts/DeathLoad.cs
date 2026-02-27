using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathLoad : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        SceneManager.LoadScene(2);

    }
}
