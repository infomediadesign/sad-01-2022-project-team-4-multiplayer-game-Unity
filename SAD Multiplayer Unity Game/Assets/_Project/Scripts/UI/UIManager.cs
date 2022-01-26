using _Project.Scripts.Networking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private GameObject uiHolder;

        public void OnConnectClick()
        {
            string playerName = playerNameInputField.text;
            if(string.IsNullOrEmpty(playerName))
            {
                Debug.LogError("Please Enter a player name!");
                return;
            }

            //Remove extra trailing spaces from name
            playerName = playerName.Trim();
        
            //Store this name Somewhere and Switch Scene
            SocketManager.GetInstance().SetPlayerName(playerName);
            SocketManager.GetInstance().Connect();
        }

        public void SetUIHolderState(bool state)
        {
            uiHolder.SetActive(state);
        }
    }
}
