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
    [SerializeField] private TextMeshProUGUI errorMessagePanelTextMeshProUGUI;
    
    [SerializeField] private TextMeshProUGUI gameCodeTextMeshProUGUI;
    [SerializeField] private TextMeshProUGUI roomPlayersCountTextMeshProUGUI;
    public static Action<string> onGameErrorMessage;
    public static Action<bool, GameUI> onUpdatePlayerInput;

    private HashSet<GameUI> gameUIsEnabled = new HashSet<GameUI>();

    [SerializeField] private ChatManager _chatManager;
    [SerializeField] private GameObject leaveRoomButtonGO;

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
        gameCodeTextMeshProUGUI.gameObject.SetActive(true);
        
        roomPlayersCountTextMeshProUGUI.SetText($"Players: {currentPlayers}/{maxPlayers}");
        roomPlayersCountTextMeshProUGUI.gameObject.SetActive(true);
            
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
        errorMessagePanelTextMeshProUGUI.SetText(errorMessage);
        errorMessagePanel.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);
            
        errorMessagePanel.SetActive(false);
        errorMessagePanelTextMeshProUGUI.SetText("");
    }

    public void OnLeaveRoomClick()
    {
        SocketManager.GetInstance().Disconnect();
        _chatManager.ClearMessages();
        _chatManager.HideChatUI();
    }

}

public enum GameUI
{
    ChatPanel,
    TaskPanel
}
