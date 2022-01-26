using _Project.Scripts.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.MainMenu
{
    public class MainMenuManager : MonoBehaviour
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
            //Basically loads the next scene
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void SetUIHolderState(bool state)
        {
            uiHolder.SetActive(state);
        }
    }
}
