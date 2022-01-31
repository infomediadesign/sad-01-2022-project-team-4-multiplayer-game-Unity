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
    public static Action<string> onGameErrorMessage;

    [SerializeField] private ChatManager _chatManager;

    private void OnEnable()
    {
        onGameErrorMessage += DisplayErrorMessage;
    }

    private void OnDisable()
    {
        onGameErrorMessage -= DisplayErrorMessage;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && SocketManager.GetInstance().currentScene == SceneName.GameScene)
        {
            _chatManager.ToggleChatUI();
        }
    }

    public void SetGameCodeToUI(string gameCode)
    {
        gameCodeTextMeshProUGUI.SetText("Game Code : " + gameCode);
        gameCodeTextMeshProUGUI.gameObject.SetActive(true);
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
}
