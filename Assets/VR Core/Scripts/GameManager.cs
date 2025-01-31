using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Language { Arabic, English }

public class GameManager : MonoBehaviourPun
{
    public static GameManager instance { get; private set; }

    [Header("Language Settings")]
    public Language language = Language.Arabic;

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public bool IsArabicApp() => language == Language.Arabic;
    public void UpdateLanguage(bool isArabic)
    {
        if (isArabic)
            photonView.RPC("SetArabic", RpcTarget.AllBuffered);
        else
            photonView.RPC("SetEnglish", RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void SetArabic()
    {
        language = Language.Arabic;
        Debug.Log("Language set to Arabic.");
        photonView.RPC("LoadNextScene", RpcTarget.AllBuffered, SceneManager.GetActiveScene().buildIndex + 1);
    }
    [PunRPC]
    public void SetEnglish()
    {
        language = Language.English;
        Debug.Log("Language set to English.");

        photonView.RPC("LoadNextScene", RpcTarget.AllBuffered, SceneManager.GetActiveScene().buildIndex+1);
    }

    [PunRPC]
    void LoadNextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
