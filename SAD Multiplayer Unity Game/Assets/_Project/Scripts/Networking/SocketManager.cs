using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Player;
using _Project.Scripts.UI;
using _Project.Scripts.Utils;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        [SerializeField] private RoomConfig roomConfig;
        [SerializeField] private PlayerController myPlayerController;

        private Dictionary<string, Player> playerIDToPlayerDictionary = new Dictionary<string, Player>();
        private Dictionary<string, GameObject> playerIDToPlayerGameObjectDictionary = new Dictionary<string, GameObject>();

        public SceneName currentScene;
        
        [SerializeField] private AlwaysOnUIManager alwaysOnUIManager;

        private Action onConnectAction;
        public static bool isGameStarted = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            currentScene = arg0.name.Equals("GameScene") ? SceneName.GameScene : SceneName.MainMenu;
            if (currentScene == SceneName.MainMenu)
            {
                alwaysOnUIManager.MainMenuLoaded();
            }
        }

        private void Start()
        {
            _unityMainThreadDispatcher = UnityMainThreadDispatcher.GetInstance();
        }

        private void Update()
        {
            if (currentScene == SceneName.MainMenu)
            {
                return;
            }

            lock (playerIDToPlayerDictionary)
            {
                lock (playerIDToPlayerGameObjectDictionary)
                {
                    var idToPlayerDictionaryKeysList = playerIDToPlayerDictionary.Keys.ToList();
                    var idToGameObjectDictionaryKeysList = playerIDToPlayerGameObjectDictionary.Keys.ToList();
                    foreach (var pKey in idToPlayerDictionaryKeysList)
                    {
                        if (!playerIDToPlayerGameObjectDictionary.ContainsKey(pKey))
                        {
                            //Spawn Player
                            AddPlayer(playerIDToPlayerDictionary[pKey]);
                        }
                    }

                    foreach (var pKey in idToGameObjectDictionaryKeysList)
                    {
                        if (!playerIDToPlayerDictionary.ContainsKey(pKey))
                        {
                            //Destroy Player
                            RemovePlayer(pKey);
                        }
                    }
                }
            }
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
                socket.Emit("setPlayerName", this.playerName);
                onConnectAction?.Invoke();
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

            socket.On("roomJoined", (roomConfigObj) =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    roomConfig = JsonUtility.FromJson<RoomConfig>(roomConfigObj.ToString());
                    alwaysOnUIManager.SetRoomUI(roomConfig.roomName, roomConfig.playersInRoom,
                        roomConfig.maxAllowedPlayers);
                    StartCoroutine(LoadGameScene());
                });
            });

            socket.On("unableToJoinRoom", roomFailedToJoinMessage =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    AlwaysOnUIManager.onGameErrorMessage?.Invoke(roomFailedToJoinMessage.ToString());
                });
            });
            
            socket.On("addPlayer", player =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    Player playerData = JsonUtility.FromJson<Player>(player.ToString());
                    lock (playerIDToPlayerDictionary)
                    {
                        if (!playerIDToPlayerDictionary.ContainsKey(playerData.id))
                        {
                            playerIDToPlayerDictionary.Add(playerData.id, playerData);
                        }
                    }
                });
            });
            
            socket.On("removePlayer", playerID =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    string stringPlayerID = playerID.ToString();
                    lock (playerIDToPlayerDictionary)
                    {
                        if (playerIDToPlayerDictionary.ContainsKey(stringPlayerID))
                        {
                            playerIDToPlayerDictionary.Remove(stringPlayerID);
                        }
                    }
                });
            });

            socket.On("playerPositionUpdate", playerObject =>
            {
                Player playerData = JsonUtility.FromJson<Player>(playerObject.ToString());
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    var playerID = playerData.id;
                    if (playerIDToPlayerGameObjectDictionary.ContainsKey(playerID))
                    {
                        GameObject playerGO = playerIDToPlayerGameObjectDictionary[playerID];
                        playerGO.transform.position = new Vector3(playerData.position.x, playerData.position.y,
                        playerData.position.z);
                        playerGO.transform.eulerAngles = new Vector3(playerData.rotation.x, playerData.rotation.y,
                            playerData.rotation.z);
                    }
                });
            });

            socket.On("newChatMessageFromServer", chatMessage =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    ChatMessage chatMessageObj = JsonUtility.FromJson<ChatMessage>(chatMessage.ToString());
                    if (chatMessageObj != null && playerIDToPlayerDictionary.ContainsKey(chatMessageObj.playerID))
                    {
                        string messagePlayerName = playerIDToPlayerDictionary[chatMessageObj.playerID].userName;
                        ChatManager.onNewChatMessageReceived?.Invoke(messagePlayerName, chatMessageObj.message, 
                            chatMessageObj.playerID == playerID);
                    }
                });
            });
            
            socket.On("gameStarting", () =>
            {
                Debug.Log("Game Starting!");
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    alwaysOnUIManager.SetGameStartingPanelState(true);
                });
            });
            
            socket.On("gameStarted", () =>
            {
                Debug.Log("Game Started!");
                isGameStarted = true;
            });
            
            socket.On("gameOver", winnerPlayerID =>
            {
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    isGameStarted = false;
                    lock (playerIDToPlayerDictionary)
                    {
                        if (playerIDToPlayerDictionary.TryGetValue(winnerPlayerID.ToString(), out Player winnerPlayer))
                        {
                            string winnerName = winnerPlayer.userName;
                            if (!string.IsNullOrEmpty(winnerName))
                            {
                                Debug.Log("Game Over and Winner is " + winnerName);
                                alwaysOnUIManager.SetWinnerPanelState(winnerName);
                            }
                        }
                    }
                });

            });
        }

        private IEnumerator LoadGameScene()
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("GameScene");
            asyncOperation.allowSceneActivation = false;
            while (asyncOperation.progress < 0.9f)
            {
                yield return null;
            }

            asyncOperation.allowSceneActivation = true;
        }

        private void AddPlayer(Player p)
        {
            GameObject playerGO = Instantiate(ReferenceManager.GetInstance().playerPrefab, 
                Vector3.zero, Quaternion.identity);
            bool isLocalPlayer = p.id.Equals(playerID);
            playerGO.name = $"{p.userName} : {p.id}";
            PlayerSetup playerSetup = playerGO.GetComponent<PlayerSetup>();
            playerSetup.SetupPlayer(isLocalPlayer, p.modelIndex);
            if (isLocalPlayer)
            {
                myPlayerController = playerGO.GetComponent<PlayerController>();
            }
            playerIDToPlayerGameObjectDictionary.Add(p.id, playerGO);
            
            alwaysOnUIManager.SetRoomUI(roomConfig.roomName, playerIDToPlayerGameObjectDictionary.Count,
                roomConfig.maxAllowedPlayers);
        }

        private void RemovePlayer(string stringPlayerID)
        {
            GameObject playerGO = playerIDToPlayerGameObjectDictionary[stringPlayerID];
            Destroy(playerGO);
            playerIDToPlayerGameObjectDictionary.Remove(stringPlayerID);
        }

        private void OnDestroy()
        {
            socket?.Disconnect();
            SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
        }

        public void HostGame(int maxPlayerCount)
        {
            onConnectAction = () =>
            {
                socket.Emit("hostRoom", maxPlayerCount);
            };
            Connect();
        }

        public void JoinGame(string roomName)
        {
            onConnectAction = () =>
            {
                socket.Emit("joinRoom", roomName);
            };
            Connect();
        }

        public void SendPositionUpdate(Vector3 pos)
        {
            socket.Emit("updatePlayerPosition", pos.x, pos.y, pos.z);
        }
        
        public void SendRotationUpdate(Vector3 pos)
        {
            socket.Emit("updatePlayerRotation", pos.x, pos.y, pos.z);
        }

        public PlayerController GetLocalPlayerController()
        {
            return myPlayerController;
        }

        public void SendChatMessage(string myChatMessage)
        {
            socket.Emit("chatMessage", myChatMessage);
        }

        public void Disconnect()
        {
            isGameStarted = false;
            socket?.Disconnect();
            SceneManager.LoadScene("Main");
        }

        public void SendTaskFinished()
        {
            socket.Emit("taskFinished");
        }
    }
    [Serializable]
    public class Player
    {
        public string id;
        public string userName;
        public Position position;
        public Position rotation;
        public int modelIndex;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class ChatMessage
    {
        public string playerID;
        public string message;
    }

    [Serializable]
    public class RoomConfig
    {
        public string roomName;
        public int playersInRoom;
        public int maxAllowedPlayers;
    }

    public enum SceneName
    {
        MainMenu,
        GameScene
    }
}


