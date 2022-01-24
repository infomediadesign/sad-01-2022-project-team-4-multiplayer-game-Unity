using System.Collections;
using System.Collections.Generic;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

public class ScoketTest : MonoBehaviour
{
    private QSocket socket;

    void Start () {
        Debug.Log ("start");
        socket = IO.Socket ("http://127.0.0.1:5858");

        socket.On (QSocket.EVENT_CONNECT, () => {
            Debug.Log ("Connected");
            socket.Emit ("chat", "test");
        });

        socket.On ("chat", data => {
            Debug.Log ("data : " + data);
        });

        socket.Connect();
    }

    private void OnDestroy () {
        socket.Disconnect ();
    }
}
