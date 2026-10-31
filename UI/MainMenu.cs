using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    Button newGameBtn;
    Button continueGameBtn;
    Button quitGameBtn;

    PlayableDirector director;

    void Awake() 
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueGameBtn = transform.GetChild(2).GetComponent<Button>();
        quitGameBtn = transform.GetChild(3).GetComponent<Button>();

        newGameBtn.onClick.AddListener(PlayTimeline);
        continueGameBtn.onClick.AddListener(ContinueGame);
        quitGameBtn.onClick.AddListener(QuitGame);

        director = FindObjectOfType<PlayableDirector>();
        director.stopped += NewGame;
    }

    void PlayTimeline()
    {
        director.Play();
    }

    void NewGame(PlayableDirector obj)
    {
        PlayerPrefs.DeleteAll();
        // 转换场景
        SceneController.Instance.TransitionToFirstLevel();
    }

    void ContinueGame()
    {
        // 转换场景，读取数据
        SceneController.Instance.TransitionToLoadGame();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
