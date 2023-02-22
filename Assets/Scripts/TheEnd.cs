using UnityEngine;

public class TheEnd : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
            PlayerPrefs.Save();
            Application.OpenURL(webplayerQuitURL);
#else
            Application.Quit();
#endif
        }
    }
}
