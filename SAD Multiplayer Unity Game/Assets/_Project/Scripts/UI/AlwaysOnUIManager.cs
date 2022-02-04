using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Networking;
using _Project.Scripts.UI;
using TMPro;
using UnityEngine;

public class AlwaysOnUIManager : MonoBehaviour
{
    [SerializeField] private GameObject errorMessagePanel;
    [SerializeField] private GameObject errorMessagePanelBgGameObject;
    [SerializeField] private TextMeshProUGUI errorMessagePanelTextMeshProUGUI;
    
    [SerializeField] private TextMeshProUGUI gameCodeTextMeshProUGUI;
    [SerializeField] private GameObject gameCodeHolder;
    [SerializeField] private TextMeshProUGUI roomPlayersCountTextMeshProUGUI;
    [SerializeField] private GameObject roomPlayerCountHolder;
    public static Action<string> onGameErrorMessage;
    public static Action<bool, GameUI> onUpdatePlayerInput;

    private HashSet<GameUI> gameUIsEnabled = new HashSet<GameUI>();

    [SerializeField] private ChatManager _chatManager;
    [SerializeField] private GameObject leaveRoomButtonGO;
    [SerializeField] private GameObject gameStartedPanel;
    [SerializeField] private GameObject gameStartedPanelBgGameObject;
    [SerializeField] private TextMeshProUGUI gameStartedTextMeshProUGUI;
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private GameObject winnerPanelBgGameObject;
    [SerializeField] private TextMeshProUGUI winnerNameTextMeshProUGUI;

    private void Awake()
    {
        leaveRoomButtonGO.SetActive(false);
    }

    private void OnEnable()
    {
        onGameErrorMessage += DisplayErrorMessage;
        onUpdatePlayerInput += ONUpdatePlayerInput;
    }

    private void OnDisable()
    {
        onGameErrorMessage -= DisplayErrorMessage;
        onUpdatePlayerInput -= ONUpdatePlayerInput;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && SocketManager.GetInstance().currentScene == SceneName.GameScene)
        {
            _chatManager.ToggleChatUI();
        }
    }

    public void SetRoomUI(string gameCode, int currentPlayers, int maxPlayers)
    {
        gameCodeTextMeshProUGUI.SetText("Game Code : " + gameCode);
        gameCodeHolder.SetActive(true);
        
        roomPlayersCountTextMeshProUGUI.SetText($"Players: {currentPlayers}/{maxPlayers}");
        roomPlayerCountHolder.SetActive(true);
            
        leaveRoomButtonGO.SetActive(true);
    }
    
    private void ONUpdatePlayerInput(bool arg1, GameUI arg2)
    {
        if (arg1)
        {
            gameUIsEnabled.Add(arg2);
            SocketManager.GetInstance().GetLocalPlayerController().SetLockInput(true);
        }
        else
        {
            gameUIsEnabled.Remove(arg2);
            if (gameUIsEnabled.Count == 0)
            {
                SocketManager.GetInstance().GetLocalPlayerController().SetLockInput(false);
            }
        }
    }
    
    private void DisplayErrorMessage(string errorMessage)
    {
        StartCoroutine(DisplayErrorMessageRoutine(errorMessage));
    }

    private IEnumerator DisplayErrorMessageRoutine(string errorMessage)
    {
        errorMessagePanelBgGameObject.transform.localScale = Vector3.zero;
        errorMessagePanelTextMeshProUGUI.SetText(errorMessage);
        errorMessagePanel.SetActive(true);
        errorMessagePanelBgGameObject.LeanScale(new Vector3(1, 1, 1), .5f).setEaseOutBack();

        yield return new WaitForSecondsRealtime(2f);

        errorMessagePanelBgGameObject.LeanScale(Vector3.zero, .5f).setEaseInBack().
            setOnComplete(() =>
        {
            errorMessagePanel.SetActive(false);
            errorMessagePanelTextMeshProUGUI.SetText("");
        });

    }

    public void OnLeaveRoomClick()
    {
        SocketManager.GetInstance().Disconnect();
        MainMenuLoaded();
    }

    public void SetGameStartingPanelState(bool state)
    {
        if (state)
        {
            gameStartedPanelBgGameObject.transform.localScale = Vector3.zero;
            gameStartedTextMeshProUGUI.transform.localScale = Vector3.zero;
            gameStartedPanel.SetActive(true);
            StartCoroutine(GameStartingSequenceRoutine());
        }
        else
        {
            gameStartedPanel.SetActive(false);
        }
    }

    private IEnumerator GameStartingSequenceRoutine()
    {
        gameStartedPanelBgGameObject.LeanScale(new Vector3(1, 1, 1), .5f).setEaseOutBack();
        for (int i = 10; i > 0; i--)
        {
            gameStartedTextMeshProUGUI.SetText($"Game Starting in \n {i}");
            gameStartedTextMeshProUGUI.transform.LeanScale(new Vector3(1, 1, 1), .5f).setEaseOutBack().
            setOnComplete(
            () =>
            {
                gameStartedTextMeshProUGUI.transform.LeanScale(Vector3.zero, .5f).setEaseInBack();
            });

            if (i == 1)
            {
                break;
            }
            yield return new WaitForSecondsRealtime(1f);
        }

        gameStartedPanelBgGameObject.transform.LeanScale(Vector3.zero, .5f).setEaseInBack().setOnComplete(() =>
        {
            gameStartedPanel.SetActive(false);
        });
    }

    public void SetWinnerPanelState(string winnerName)
    {
        winnerPanelBgGameObject.transform.localScale = Vector3.zero;
        winnerNameTextMeshProUGUI.SetText($"Winner is \n {winnerName}");
        winnerPanel.SetActive(true);
        winnerPanelBgGameObject.LeanScale(new Vector3(1, 1, 1), .5f).setEaseOutBack();
        ONUpdatePlayerInput(true, GameUI.WinnerPanel);
    }

    public void OnCloseWinnerPanelClick()
    {
        winnerPanelBgGameObject.LeanScale(Vector3.zero, .5f).setEaseInBack().setOnComplete(() =>
        {
            winnerPanel.SetActive(false);
            ONUpdatePlayerInput(false, GameUI.WinnerPanel);
        });
    }

    public void MainMenuLoaded()
    {
        _chatManager.ClearMessages();
        _chatManager.HideChatUI();
        winnerPanel.SetActive(false);
        gameStartedPanel.SetActive(false);
        StopAllCoroutines();
        gameCodeHolder.SetActive(false);
        roomPlayerCountHolder.SetActive(false);
        errorMessagePanel.SetActive(false);
        leaveRoomButtonGO.SetActive(false);
        gameUIsEnabled.Clear();
    }
}

public enum GameUI
{
    ChatPanel,
    TaskPanel,
    WinnerPanel
}
