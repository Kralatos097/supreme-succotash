using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private bool launchGame = false;
    [SerializeField] private bool quitGame = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (launchGame == true)
        {
            LaunchGameOnTrigger();
        }
        else if(quitGame == true)
        {
            QuitOnTrigger();
        }
    }

    void LaunchGameOnTrigger()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    void QuitOnTrigger()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
    }
}
