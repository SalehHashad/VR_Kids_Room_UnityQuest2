using UnityEngine;
using Photon.Pun;

public class SceneLoader : MonoBehaviourPun
{
    public static SceneLoader instance;
    private void OnEnable()
    {
        instance = this;
    }
    [PunRPC]
    public void LoadSceneByIndex(int buildIndex)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(buildIndex);
        }
        else
        {
            Debug.LogWarning("Only the MasterClient can load scenes.");
        }
    }
    [PunRPC]
    public void LoadSceneByIndex(string buildString)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(buildString);
        }
        else
        {
            Debug.LogWarning("Only the MasterClient can load scenes.");
        }
    }

    public void RequestSceneLoad(int buildIndex)
    {
        photonView.RPC("LoadSceneByIndex", RpcTarget.AllBuffered, buildIndex);
    }
}
