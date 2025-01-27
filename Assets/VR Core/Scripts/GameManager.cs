using UnityEngine;
using UnityEngine.SceneManagement;

public enum Language { Arabic, English }

public class GameManager : MonoBehaviour
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

    public void SetArabic()
    {
        language = Language.Arabic;
        Debug.Log("Language set to Arabic.");
        LoadNextScene();
    }

    public void SetEnglish()
    {
        language = Language.English;
        Debug.Log("Language set to English.");
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
