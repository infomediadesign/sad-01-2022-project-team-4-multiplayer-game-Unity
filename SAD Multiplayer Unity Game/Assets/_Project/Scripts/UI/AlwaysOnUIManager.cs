using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlwaysOnUIManager : MonoBehaviour
{
    [SerializeField] private GameObject errorMessagePanel;
    [SerializeField] private TextMeshProUGUI errorMessagePanelTextMeshProUGUI;
    
    [SerializeField] private TextMeshProUGUI gameCodeTextMeshProUGUI;
    public static Action<string> onGameErrorMessage;

    private void OnEnable()
    {
        onGameErrorMessage += DisplayErrorMessage;
    }

    private void OnDisable()
    {
        onGameErrorMessage -= DisplayErrorMessage;
    }

    public void SetGameCodeToUI(string gameCode)
    {
        gameCodeTextMeshProUGUI.SetText(gameCode);
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
