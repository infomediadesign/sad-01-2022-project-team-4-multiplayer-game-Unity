using System;
using System.Collections;
using _Project.Scripts.Networking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TMP_InputField maxPlayerInputField;

        public void HostGameClick()
        {
            if (IsValidName())
            { 
                string maxPlayerCountString = maxPlayerInputField.text;
                maxPlayerCountString = string.IsNullOrEmpty(maxPlayerCountString) ? "" : maxPlayerCountString.Trim();
                int maxPlayerCount = 0;
                if(string.IsNullOrEmpty(maxPlayerCountString) || !int.TryParse(maxPlayerCountString, out maxPlayerCount))
                {
                    AlwaysOnUIManager.onGameErrorMessage?.Invoke("Please Enter max allowed players!");
                    return;
                }

                SocketManager.GetInstance().HostGame(maxPlayerCount);
            }
        }

        public void JoinGameClick()
        {
            if (IsValidName())
            {
                string roomName = roomNameInputField.text;
                if(string.IsNullOrEmpty(roomName))
                {
                    AlwaysOnUIManager.onGameErrorMessage?.Invoke("Please Enter a room name!");
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
                AlwaysOnUIManager.onGameErrorMessage?.Invoke("Please Enter a player name!");
                return false;
            }

            //Remove extra trailing spaces from name
            playerName = playerName.Trim();
        
            SocketManager.GetInstance().SetPlayerName(playerName);
            return true;
        }
    }
}
