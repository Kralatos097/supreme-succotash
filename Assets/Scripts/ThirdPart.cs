using System.Collections.Generic;
using UnityEngine;

public class ThirdPart : MonoBehaviour
{
    [SerializeField] private bool ResetPlayerPref;
    [SerializeField] private bool MainMenu;
    [SerializeField] private List<GameObject> ToActivate = new List<GameObject>();
    [SerializeField] private List<GameObject> ToDesactivate = new List<GameObject>();
    private void Start()
    {
        if(ResetPlayerPref) PlayerPrefs.DeleteAll();
        
        if (PlayerPrefs.GetInt("ThirdPart") == 1 && !ResetPlayerPref)
        {
            Debug.Log("Part 3 en cours");
        }

        if (MainMenu && PlayerPrefs.GetInt("ThirdPart") == 1)
        {
            foreach (var a in ToActivate)
            {
                a.gameObject.SetActive(true);
            }
            foreach (var a in ToDesactivate)
            {
                a.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Active la partie 3");
            PlayerPrefs.SetInt("ThirdPart", 1);
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
            Application.OpenURL(webplayerQuitURL);
#else
            Application.Quit();
#endif
        }
    }
}
