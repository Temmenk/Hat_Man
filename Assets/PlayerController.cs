using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;

    [Header("Bash")]
    public float bashRange = 2.0f;
    public float bashForce = 10.0f;
    public float bashUpForce = 3.0f;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public Camera playerCamera;

    [Header("Camera")]
    public float mouseSensitivity = 2.0f;
    public float minCameraAngle = -60.0f;
    public float maxCameraAngle = 60.0f;

    private float cameraPitch = 0.0f;

    // called when the player object is instantiated
    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        // give the first player the hat
        //if (id == 1)
           // GameManager.instance.GiveHat(id, true);

        // if this isn't our local player, disable physics as that's
        // controlled by the user and synced to all other clients
        if (!photonView.IsMine)
            {
                rig.isKinematic = true;
                playerCamera.gameObject.SetActive(false); 
            }
        else
        {
            playerCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            {
                
            }
        }
    }


    private void Update()
    {
        if (photonView.IsMine)
        {
            Move();
            RotateCamera();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();
            if(Input.GetKeyDown(KeyCode.B))
                TryBash();

            // track the amount of time we're wearing the hat
            if (hatObject.activeInHierarchy)
            {
                curHatTime += Time.deltaTime;
            }
        }

        // if I'm the game host, check to see if this player has won
        if (PhotonNetwork.IsMasterClient)
        {
            if (curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection =
            transform.right * x +
            transform.forward * z;

        moveDirection = moveDirection.normalized * moveSpeed;

        rig.linearVelocity = new Vector3(
            moveDirection.x,
            rig.linearVelocity.y,
            moveDirection.z
        );
    }

    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Turn the entire player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Look up/down with the camera
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minCameraAngle,
            maxCameraAngle
        );

        playerCamera.transform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void TryBash()
    {
        RaycastHit hit;

        if(Physics.Raycast(transform.position, transform.forward, out hit, bashRange))
        {
            PlayerController otherPlayer =
                hit.collider.GetComponentInParent<PlayerController>();
            
            if(otherPlayer != null && otherPlayer !=this)
            {
                Vector3 bashDirection =
                    transform.forward * bashForce + Vector3.up * bashUpForce;

                otherPlayer.photonView.RPC(
                    "ReceiveBash",
                    otherPlayer.photonPlayer,
                    bashDirection
                );
            }
        }
    }
    [PunRPC]
    void ReceiveBash(Vector3 force)
    {
        rig.AddForce(force, ForceMode.Impulse);
    }
    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (!photonView.IsMine)
    //         return;

    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         if (GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
    //         {
    //             if (GameManager.instance.CanGetHat())
    //             {
    //                 GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
    //             }
    //         }
    //     }
    // }

    // from IPunObservable - allows us to send and receive data
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curHatTime); // send info about this playercontroller to others
        }
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext(); // receive info about this playercontroller from others
        }
    }
}
