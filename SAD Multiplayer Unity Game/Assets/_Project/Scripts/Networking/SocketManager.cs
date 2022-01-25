using System;
using System.Collections.Generic;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

namespace _Project.Scripts.Networking
{
    public class SocketManager : MonoBehaviour
    {
        private static SocketManager _instance = null;
        private QSocket socket;

        [SerializeField] private string playerName;
        [SerializeField] private string playerID;

        //---------------------------------------------------------------
        // TODO : MAYBE WE NEED TO LOCK IT TO MAKE IT THREAD SAFE
        private Queue<Action> mainThreadActionQueue = new Queue<Action>();
        //---------------------------------------------------------------
        
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

        private void Update()
        {
            while (mainThreadActionQueue.Count > 0)
            {
                mainThreadActionQueue.Dequeue()?.Invoke();
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
                Player p = JsonUtility.FromJson<Player>(player.ToString());
                mainThreadActionQueue.Enqueue(() =>
                {
                    GameObject playerGO = new GameObject(p.id);
                });
            });
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


