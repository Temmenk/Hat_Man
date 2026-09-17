using UnityEngine;
using Photon.Pun;

public class HatPickup : MonoBehaviour
{
    private bool isPickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp)
            return;

        PlayerController playerController =
            other.GetComponentInParent<PlayerController>();

        if (playerController == null)
            return;

        // Only the local player who touched the hat triggers the win
        if (!playerController.photonView.IsMine)
            return;

        isPickedUp = true;

        // Tell everyone who won
        GameManager.instance.photonView.RPC(
            "WinGame",
            RpcTarget.All,
            playerController.id
        );

    }
}