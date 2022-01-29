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

        private Dictionary<string, Player> playerIDToPlayerDictionary = new Dictionary<string, Player>();
        private Dictionary<string, GameObject> playerIDToPlayerGameObjectDictionary = new Dictionary<string, GameObject>();

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private UIManager uiManager;
        
        [SerializeField] private SceneName currentScene;

        private Action onConnectAction;

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
        }

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            _unityMainThreadDispatcher = UnityMainThreadDispatcher.GetInstance();
            playerPrefab = Resources.Load<GameObject>("Player");
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

            socket.On("roomJoined", (roomID) =>
            {
                string roomCode = roomID.ToString();
                _unityMainThreadDispatcher.AddActionToMainThread(() =>
                {
                    uiManager.SetRoomCode(roomCode);
                    StartCoroutine(LoadGameScene());
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
            GameObject playerGO = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerGO.name = $"{p.userName} : {p.id}";
            PlayerSetup playerSetup = playerGO.GetComponent<PlayerSetup>();
            playerSetup.SetupPlayer(p.id.Equals(this.playerID));
            playerIDToPlayerGameObjectDictionary.Add(p.id, playerGO);
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
            socket.Emit("updatePlayerPosition", pos.x, pos.y, pos.z);
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

    public enum SceneName
    {
        MainMenu,
        GameScene
    }
}


