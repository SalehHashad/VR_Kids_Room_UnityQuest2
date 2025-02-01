using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomReview : MonoBehaviourPun
{
    [SerializeField] GameObject roomReviewImage;
    [SerializeField] float waitingToCloseImage = 5f;
    public void OpenImageRPC()
    {
        print("OpenImageRPC");
        photonView.RPC("ViewImage", RpcTarget.All);
    }
    [PunRPC]
    void ViewImage()
    {
        roomReviewImage.SetActive(true);
        StartCoroutine(closingImage());
    }
    IEnumerator closingImage()
    {
        yield return new WaitForSeconds(waitingToCloseImage);
        roomReviewImage.SetActive(false);
    }
}
