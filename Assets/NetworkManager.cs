using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
// instance
public static NetworkManager instance;
void Awake ()
    {
// if an instance already exists and it's not this one - destroy us
    if(instance != null && instance != this)
        gameObject.SetActive(false);
    else
        {
    // set the instance
        instance = this;
        DontDestroyOnLoad(gameObject);
        }
    }
void Start()
    {
    // connect to photon server
    PhotonNetwork.ConnectUsingSettings();
    }
//attempt to creat a new room
public void CreateRoom(string roomName)
    {
    PhotonNetwork.CreateRoom(roomName);
    }
//attempt to join a room
public void JoinRoom(string roomName)
    {
    PhotonNetwork.JoinRoom(roomName);
    }
// changes scene using photon
[PunRPC]
public void ChangeScene (string sceneName)
    {
            PhotonNetwork.LoadLevel(sceneName);
    }
public override void OnConnectedToMaster()
    {
    Debug.Log("Connected to Master Server");
        CreateRoom("TestRoom");
    }
public override void OnCreatedRoom()
    {
        Debug.Log("Created room: " + PhotonNetwork.CurrentRoom.Name);
    }




    
}