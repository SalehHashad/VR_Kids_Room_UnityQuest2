using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhotonTransfromAndView : MonoBehaviour
{
    [ContextMenu("Add Photon View AND Photon Transform")]
    public void AddPhotonViewANDPhotonTransform()
    {
        PhotonView photonView = this.gameObject.AddComponent<PhotonView>();
        PhotonTransformView photonTransformView = this.gameObject.AddComponent<PhotonTransformView>();
        photonTransformView.m_UseLocal = false;
        photonTransformView.m_SynchronizePosition = true;
        photonTransformView.m_SynchronizeRotation = true;
        photonTransformView.m_SynchronizeScale = true;

        Debug.Log($"photonView & photonTransformView are added for {this}");

        DestroyImmediate(this);
    }

}
