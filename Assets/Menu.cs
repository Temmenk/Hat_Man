using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button joinRoomButton;

    [Header("Lobby Screen")]
    public TextMeshProUGUI playerListText;
    public Button startGameButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // disable the buttons at the start as we're not connected to the server yet
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

        // called when we connect to the master server
        // enable the "Create Room" and "Join Room" buttons
        public override void OnConnectedToMaster ()
        {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        }
    void SetScreen(GameObject screen)
    {
        // deactivcate all screens
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        // enable the requested screen
        screen.SetActive(true);
    }
    public void OnCreateRoomButton (TMP_InputField roomNameInput)
    {
        // create a new room with the name from the input field
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }
    public void OnJoinRoomButton (TMP_InputField roomNameInput)
    {
        // join the room with the name from the input field
        NetworkManager.instance.JoinRoom(roomNameInput.text);
    }
    public void OnPlayerNameUpdate(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }
    public override void OnJoinedRoom ()
    {
        SetScreen(lobbyScreen);
        // since there's now a new player in the lobby, tell everyone to update the lobby UI
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }
    [PunRPC]
    public void UpdateLobbyUI ()
    {
        playerListText.text = "";
        // display all the players currently in the lobby
        foreach(Player player in PhotonNetwork.PlayerList)
            {
                playerListText.text += player.NickName + "\n";
            }
        // only the host can start the game
        if(PhotonNetwork.IsMasterClient)
            startGameButton.interactable = true;
        else
             startGameButton.interactable = false;
    }
    public override void OnPlayerLeftRoom (Player otherPlayer)
    {
    // we don't RPC it like when we join the lobby
    // that's because OnJoinRoom is only called for the client who just joined
    // OnPlayerLeftRoom gets called for all clients in the room, so we don't need to RPC it
    
        UpdateLobbyUI();
    }
    public void OnLeaveLobbyButton ()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }
    public void OnStartGameButton ()
    {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
}