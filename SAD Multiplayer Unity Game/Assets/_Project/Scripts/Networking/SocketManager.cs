using System;
using System.Collections.Generic;
using _Project.Scripts.Player;
using _Project.Scripts.UI;
using _Project.Scripts.Utils;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

namespace _Project.Scripts.Networking
{
    public class SocketManager : MonoBehaviour
    {
        private static SocketManager _instance = null;
        private QSocket socket;
        private UnityMainThreadDispatcher _unityMainThreadDispatcher;

        [SerializeField] private string playerName;
        [SerializeField] private string playerID;

        private Dictionary<string, GameObject> playersDictionary = new Dictionary<string, GameObject>();

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private UIManager mainMenuManager;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            mainMenuManager = FindObjectOfType<UIManager>();
            _unityMainThreadDispatcher = UnityMainThreadDispatcher.GetInstance();
            playerPrefab = Resources.Load<GameObject>("Player");
        }

        public static SocketManager GetInstance()
        {
            return _instance;
        }

        public void SetPlayerName(string playerName)
        {
            this.playerName = playerName;
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public void SetPlayerID(string playerID)
        {
            this.playerID = playerID;
        }

        public string GetPlayerID()
        {
            return playerID;
        }

        public void Connect()
        {
            socket = IO.Socket ("http://127.0.0.1:3000");
            SetupSocketEvents();
        }

        private void SetupSocketEvents()
        {
            socket.On (QSocket.EVENT_CONNECT, () => {
                Debug.Log ("Connected");
                socket.Emit("setPlayerName", playerName);
                //socket.Emit ("chat", "test");
            });

            socket.On(QSocket.EVENT_DISCONNECT, reason =>
            {
                Debug.Log($"Socket Disconnected due to {reason}");
            });

            socket.On ("chat", data => {
                Debug.Log ("data : " + data);
            });

            socket.On("init", playerID =>
            {
                this.playerID = (string)playerID;
            });

            socket.On("spawnPlayer", player =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    Player playerData = JsonUtility.FromJson<Player>(player.ToString());
                    AddPlayer(playerData);
                    mainMenuManager.SetUIHolderState(false);
                });
            });

            socket.On("playerDisconnected", playerID =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    string stringPlayerID = playerID.ToString();
                    RemovePlayer(stringPlayerID);
                });
            });
        }

        private void AddPlayer(Player p)
        {
            GameObject playerGO = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerGO.name = $"{p.userName} : {p.id}";
            PlayerSetup playerSetup = playerGO.GetComponent<PlayerSetup>();
            playerSetup.SetupPlayer(p.id.Equals(this.playerID));
            playersDictionary.Add(p.id, playerGO);
        }

        private void RemovePlayer(string stringPlayerID)
        {
            GameObject playerGO = playersDictionary[stringPlayerID];
            Destroy(playerGO);
            playersDictionary.Remove(stringPlayerID);
        }

        private void OnDestroy()
        {
            socket?.Disconnect();
        }
    }
    [Serializable]
    public class Player
    {
        public string id;
        public string userName;
    }
}


