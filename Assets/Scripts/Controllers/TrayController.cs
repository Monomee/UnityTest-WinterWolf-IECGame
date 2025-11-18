using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrayController : MonoBehaviour
{
    private Tray m_tray;
    private GameManager m_gameManager;
    private GameSettings m_gameSettings;
    private bool isGameOver;

    public event Action<Item> OnItemAdded = delegate { };
    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;
        m_gameManager.StateChangedAction += OnGameStateChange;
        m_tray = new Tray(this.transform, m_gameSettings);
        isGameOver = false;
    }
    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                isGameOver = false;
                break;
            case GameManager.eStateGame.PAUSE:
                // tray doesn't need active updates while paused, but keep state
                break;
            case GameManager.eStateGame.GAME_LOSE:
                isGameOver = true;
                break;
        }
    }
    public bool TryAddItemToTray(Item item)
    {
        if (isGameOver || item == null)
            return false;

        int placedIndex = m_tray.AddItemToTray(item);
        if (placedIndex == -1)
            return false;

        // 
        if (m_tray.IsTrayFull())
        {
            var matchesCheck = m_tray.GetContiguousMatches();
            if (matchesCheck == null || matchesCheck.Count == 0)
            {
                m_gameManager.LoseGame();
                return true;
            }
        }


        OnItemAdded?.Invoke(item);

        StartCoroutine(DelayedCheckForMatches());
        return true;
    }



    private IEnumerator DelayedCheckForMatches()
    {
        yield return new WaitForSeconds(0.22f);

        var matches = m_tray.GetContiguousMatches();
        if (matches != null && matches.Count > 0)
            StartCoroutine(TrayCollapseCoroutine(matches));
    }



    // Coroutine to explode matched groups in tray, shift, then check again for cascades
    private IEnumerator TrayCollapseCoroutine(List<List<Cell>> initialMatches)
    {
        // explode all initial matches
        foreach (var group in initialMatches)
        {
            foreach (var cell in group)
            {
                if (cell != null)
                {
                    cell.ExplodeItem();
                }
            }
        }

        // small delay to allow explode animations
        yield return new WaitForSeconds(0.2f);

        // free cells in those groups and compact left
        foreach (var group in initialMatches)
        {
            m_tray.ClearCells(group);
        }

        m_tray.ShiftTrayItemsLeft();

        // wait for move animations
        yield return new WaitForSeconds(0.2f);

        // check for new matches (cascade)
        var next = m_tray.GetContiguousMatches();
        if (next != null && next.Count > 0)
        {
            // recursively handle next cascade
            yield return StartCoroutine(TrayCollapseCoroutine(next));
        }
    }

    // Remove tray content (e.g. when level ends)
    internal void Clear()
    {
        if (m_tray != null)
            m_tray.ClearTray();
    }
}
