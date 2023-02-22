using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnTrigger : MonoBehaviour
{
    [SerializeField] private string SceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneName);
        }
    }
}
