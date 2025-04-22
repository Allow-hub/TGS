using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject MoviePlayer;

    private bool MoviePlaying = false;
    private bool MovieFlag = true;

    private InputAction _pressAnyKeyAction =
        new InputAction(type: InputActionType.PassThrough, binding: "*/<Button>", interactions: "Press");


    private void OnEnable() => _pressAnyKeyAction.Enable();
    private void OnDisable() => _pressAnyKeyAction.Disable();
    void Start()
    {
        MoviePlayer.gameObject.SetActive(false);
        Invoke("playMovie", 5.0f);
    }

    void Update()
    {
        if (MoviePlaying == false)
        {
            if (MovieFlag == false)
            {
                Invoke("playMovie", 5.0f);
                MovieFlag = true;
            }

            if (_pressAnyKeyAction.triggered)
            {
                SceneManager.LoadScene("InGame");
            }
        }
        else
        {
            if (_pressAnyKeyAction.triggered)
            {
                MoviePlayer.gameObject.SetActive(false);
                MoviePlaying = false;
                MovieFlag = false;
            }
        }
    }

    void playMovie()
    {
        if (MoviePlaying == false)
        {
            MoviePlayer.gameObject.SetActive(true);
            MoviePlaying = true;
        }
    }
}
