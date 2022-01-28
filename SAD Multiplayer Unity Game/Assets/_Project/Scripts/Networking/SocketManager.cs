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

        [SerializeField] private bool isLocal = false;
        [SerializeField] private string playerName;
        [SerializeField] private string playerID;

        private Dictionary<string, GameObject> playersDictionary = new Dictionary<string, GameObject>();

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private UIManager uiManager;

        private Action onConnectAction;

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
            uiManager = FindObjectOfType<UIManager>();
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
            //socket = IO.Socket ("http://127.0.0.1:3000");
            socket = IO.Socket (isLocal ?"http://127.0.0.1:7779":"http://3.67.177.79:7779");
            //socket.Emit("chat", "Hello From the other side");
            SetupSocketEvents();
        }

        private void SetupSocketEvents()
        {
            socket.On (QSocket.EVENT_CONNECT, () => {
                Debug.Log ("Connected");
                onConnectAction?.Invoke();
                //socket.Emit("setPlayerName", playerName);
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
                    uiManager.SetUIHolderState(false);
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

            socket.On("roomJoined", (roomID) =>
            {
                string roomCode = roomID.ToString();
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    uiManager.SetRoomCode(roomCode);
                });
            });

            socket.On("playerPositionUpdate", playerObject =>
            {
                Player playerData = JsonUtility.FromJson<Player>(playerObject.ToString());
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    var playerID = playerData.id;
                    if (playerID.Equals(this.playerID))
                    {
                        return;
                    }
                    GameObject playerGO = playersDictionary[playerID];
                    playerGO.GetComponent<CharacterController>().enabled = false;
                    playerGO.transform.position = new Vector3(playerData.position.x, playerData.position.y,
                        playerData.position.z);
                    playerGO.GetComponent<CharacterController>().enabled = true;
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

        public void HostGame()
        {
            onConnectAction = () =>
            {
                socket.Emit("hostRoom", playerName);
            };
            Connect();
        }

        public void JoinGame(string roomName)
        {
            onConnectAction = () =>
            {
                socket.Emit("joinRoom", roomName, playerName);
            };
            Connect();
        }

        public void SendPositionUpdate(Vector3 pos)
        {
            socket.Emit("updatePlayerPosition", playerID, pos.x, pos.y, pos.z);
        }
    }
    [Serializable]
    public class Player
    {
        public string id;
        public string userName;
        public Position position;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }
}


