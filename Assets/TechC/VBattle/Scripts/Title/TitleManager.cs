using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
namespace TechC{

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject MoviePlayer;
    [SerializeField] private float ChangeTime = 5.0f; // 修正: 型をfloatに変更

    private bool MoviePlaying = false;

    private InputAction _pressAnyKeyAction =
        new InputAction(type: InputActionType.PassThrough, binding: "*/<Button>", interactions: "Press");

    private void OnEnable() => _pressAnyKeyAction.Enable();
    private void OnDisable() => _pressAnyKeyAction.Disable();

    void Start()
    {
        MoviePlayer.gameObject.SetActive(false);
        StartCoroutine(PlayMovieWithDelay(ChangeTime)); // Coroutineを使用
    }

    void Update()
    {
        if (_pressAnyKeyAction.triggered)
        {
            if (MoviePlaying)
            {
                StopMovie();
            }
            else
            {
                SceneManager.LoadScene("InGame");
            }
        }
    }

    /// <summary>
    /// Coroutineで遅延処理を実現
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator PlayMovieWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayMovie();
    }

    /// <summary>
    /// 映像再生を開始
    /// </summary>
    void PlayMovie()
    {
        if (!MoviePlaying)
        {
            MoviePlayer.gameObject.SetActive(true);
            MoviePlaying = true;
        }
    }

    /// <summary>
    /// 映像再生を停止
    /// </summary>
    void StopMovie()
    {
        MoviePlayer.gameObject.SetActive(false);
        MoviePlaying = false;
        StartCoroutine(PlayMovieWithDelay(ChangeTime)); // 次の再生をスケジュール
    }
}
}