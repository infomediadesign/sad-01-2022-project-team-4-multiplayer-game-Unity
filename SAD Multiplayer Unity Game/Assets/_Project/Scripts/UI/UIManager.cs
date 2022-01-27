using _Project.Scripts.Networking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TextMeshProUGUI roomCodeLabel;
        [SerializeField] private GameObject uiHolder;

        public void OnConnectClick()
        {
            SocketManager.GetInstance().Connect();
        }

        public void HostGameClick()
        {
            if (IsValidName())
            {
                SocketManager.GetInstance().HostGame();
            }
        }

        public void JoinGameClick()
        {
            if (IsValidName())
            {
                string roomName = roomNameInputField.text;
                if(string.IsNullOrEmpty(roomName))
                {
                    Debug.LogError("Please Enter a player name!");
                    return;
                }

                //Remove extra trailing spaces from name
                roomName = roomName.Trim();

                SocketManager.GetInstance().JoinGame(roomName);
            }
        }

        private bool IsValidName()
        {
            string playerName = playerNameInputField.text;
            if(string.IsNullOrEmpty(playerName))
            {
                Debug.LogError("Please Enter a player name!");
                return false;
            }

            //Remove extra trailing spaces from name
            playerName = playerName.Trim();
        
            SocketManager.GetInstance().SetPlayerName(playerName);
            return true;
        }

        public void SetUIHolderState(bool state)
        {
            uiHolder.SetActive(state);
        }

        public void SetRoomCode(string roomCode)
        {
            roomCodeLabel.SetText(roomCode);
        }
    }
}
