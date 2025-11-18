# UnityTest-WinterWolf-IECGame

Task1: Re-skin
- Folder: Resources/prefabs
- Each item is a prefab that contains a SpriteRenderer component. To change its skin, just replace the Sprite in that component with the fish skin.

Task2: Change gameplay
- Folder: 
 + Board: add Tray.cs
 + Controller: add TrayController.cs, adjust GameManager.cs, BoardController.cs
 + UI: add UIPanelGameWin.cs
- Flow:
 + GameManager: call LoadGame -> BoardController.StartGame(), TrayController.StartGame()
 + BoardController.StartGame() -> create new: m_board = new Board(transform, gameSettings) -> Board.Fill()
 + BoardController.Update() (update when player clicks) -> call TrayController.TryAddItemToTray() : m_tray.AddItemToTray(item)
 + Full tray -> m_gameManager.LoseGame(); -> UIPanelGameOver
 + TrayCollapseCoroutine(): same type of 3 items -> Explode item, Clear cell, Shift items left
 + FindMatchesAndCollapse(): Win: if (m_board.IsBoardEmpty()){ m_gameManager.WinGame();} -> UIPanelGameWin
- What i done: 
 + change gameplay: creat Tray with size that config in GameSetting, TrayController with functions (base on Board and BoardController), adjust Board, BoardController, comment parts that are belong to old gameplay
 + add UIPanelGameWin (base on UIPanelGameOver), split GAME_OVER into GAME_WIN and GAME_LOSE, lose when tray is full, win when board is empty (win may be not work well like expected)
 + change some related files (GameSetting, UIMainManager..)

 Task3: Add new gameplay
 I am not done yet, i am sorry about that, i do not have enough time and ability to do this task. But i will try my best to do this task after my final exams at university (i have final exams in 19,20,23 and may be 25 if i want to retake exam to improve grade)
