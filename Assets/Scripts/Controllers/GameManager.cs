using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_WIN, //added
        GAME_LOSE,//added
    }


    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;
    private TrayController m_trayController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        // create board & tray
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_trayController = new GameObject("TrayController").AddComponent<TrayController>();

        m_boardController.StartGame(this, m_gameSettings);
        m_trayController.StartGame(this, m_gameSettings);

        if (m_levelCondition != null)
        {
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }

        State = eStateGame.GAME_STARTED;
    }


    //public void GameOver()
    //{
    //    StartCoroutine(WaitBoardController());
    //}

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }

        if (m_trayController)
        {
            m_trayController.Clear();  
            Destroy(m_trayController.gameObject); 
            m_trayController = null;
        }
    }


    private IEnumerator WaitBoardController(eStateGame finalState)
    {
        while (m_boardController.IsBusy)
            yield return null;

        yield return new WaitForSeconds(1f);

        State = finalState;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= WinGame;
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }

    public void LoseGame()
    {
        StartCoroutine(WaitBoardController(eStateGame.GAME_LOSE));
    }

    public void WinGame()
    {
        StartCoroutine(WaitBoardController(eStateGame.GAME_WIN));
    }

}
