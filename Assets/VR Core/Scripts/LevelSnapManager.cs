using ArabicSupport;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class LevelSnapManager : MonoBehaviourPun
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip englishSuccessAudioClip;
    [SerializeField] private AudioClip arabicSuccessAudioClip;
    [SerializeField] private AudioClip englishFailedAudioClip;
    [SerializeField] private AudioClip arabicFailedAudioClip;
    [SerializeField] private AudioClip englishPassAudioClip;
    [SerializeField] private AudioClip arabicPassAudioClip;
    [SerializeField] private AudioClip englishInstructions;
    [SerializeField] private AudioClip arabicInstructions;

    [Header("Score Settings")]
    [SerializeField] private int totalScore = 0;
    [SerializeField] private int requiredScore = 0;
    [SerializeField] private TextMeshProUGUI ScoreTMP;
    [SerializeField] private TextMeshProUGUI ScreenScoreTMP;
    [SerializeField] List<ModelsData> modelsData = new List<ModelsData>();

    [Header("Image View Settings")]
    [SerializeField] private Image referenceImage;
    [SerializeField] private TextMeshProUGUI viewsRemainingText;
    public int remainingViews;
    private float viewDuration;
    private bool isImageVisible = false;
    private Coroutine hideImageCoroutine;

    private ShowImage_Tag showImage;
    private ScoreButton_Tag showScoreButton;

    [Space]
    [Header("Next Level")]
    public int WaitingForNextLevelPerSeconds = 5;
    public GameObject nextLevelPanel;
    public TextMeshProUGUI nextLevelInSecondsTMP;
    [Space]
    public UnityEvent onLevelComplete;
    public UnityEvent<int> onScoreUpdated;


    private void Awake()
    {
        InitializeObjects();
    }

    private void InitializeObjects()
    {
        showImage = FindObjectOfType<ShowImage_Tag>(true);
        showScoreButton = FindObjectOfType<ScoreButton_Tag>(true);
    }
    private void Start()
    {
        InitializeLevelSettings();
        ScoreTMP.text = totalScore.ToString();

        if (referenceImage != null)
        {
            referenceImage.gameObject.SetActive(false);
        }
        UpdateViewsText();
    }
    private void InitializeLevelSettings()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        switch (currentLevel)
        {
            case 1:
                PlayGameInstractions();
                requiredScore = 6;
                remainingViews = 4;
                viewDuration = 10f;
                break;
            case 2:
                // here you can also add the audio intro for theis level like "This is level two you need to grab 24 object and so on ..."
                requiredScore = 18;
                remainingViews = 3;
                viewDuration = 8f;
                break;
            case 3:
                requiredScore = 27;
                remainingViews = 2;
                viewDuration = 6f;
                break;
            case 4:
                requiredScore = 51;
                remainingViews = 1;
                viewDuration = 5f;
                break;
            default:
                requiredScore = 0;
                remainingViews = 0;
                viewDuration = 0f;
                break;
        }
        
        photonView.RPC("UpdateScoreUI",RpcTarget.AllBuffered, totalScore);
    }

    private void PlayGameInstractions()
    {
        if (GameManager.instance.IsArabicApp())
            audioSource.clip = arabicInstructions;
        else
            audioSource.clip = englishInstructions;
        audioSource.Play();
    }

    public void ShowReferenceImage()
    {
        if (remainingViews > 0 && !isImageVisible && referenceImage != null)
        {
            showImage.gameObject.SetActive(false);
            showScoreButton.gameObject.SetActive(false);
            isImageVisible = true;
            remainingViews--;
            UpdateViewsText();

            referenceImage.gameObject.SetActive(true);

            if (hideImageCoroutine != null)
            {
                StopCoroutine(hideImageCoroutine);
            }

            hideImageCoroutine = StartCoroutine(HideImageAfterDelay());
        }
    }

    private IEnumerator HideImageAfterDelay()
    {
        yield return new WaitForSeconds(viewDuration);
        if (referenceImage != null)
        {
            referenceImage.gameObject.SetActive(false);
            showImage.gameObject.SetActive(true);
            showScoreButton.gameObject.SetActive(true);
        }
        isImageVisible = false;
    }

    private void UpdateViewsText()
    {
        if (viewsRemainingText != null)
        {
            viewsRemainingText.text = $"Remaining Views: {remainingViews}";
        }
    }
    [ContextMenu("Make Correct Answer")]
    public void testCorrctAnswer()
    {
        if (englishSuccessAudioClip != null && audioSource != null)
        {
            if(!GameManager.instance.IsArabicApp())
                audioSource.PlayOneShot(englishSuccessAudioClip);
            else
                audioSource.PlayOneShot(arabicSuccessAudioClip);
        }

        totalScore += 1;
        //snapPoint.IsMatched = true;
        onScoreUpdated?.Invoke(totalScore);
        photonView.RPC("UpdateScoreUI", RpcTarget.AllBuffered, totalScore);
        Debug.Log("Your score is : " + totalScore);
        CheckLevelCompletion();
    }

    public void HandleCorrectSnap(string objectTag)
    {
        var snapPoint = modelsData.Find(sp => sp.ObjetcTag == objectTag && !sp.IsMatched);

        if (snapPoint != null)
        {
            if (englishSuccessAudioClip != null && audioSource != null)
            {
                if (!GameManager.instance.IsArabicApp())
                    audioSource.PlayOneShot(englishSuccessAudioClip);
                else
                    audioSource.PlayOneShot(arabicSuccessAudioClip);
            }

            totalScore += 1;
            snapPoint.IsMatched = true;
            onScoreUpdated?.Invoke(totalScore);
            photonView.RPC("UpdateScoreUI", RpcTarget.AllBuffered, totalScore);
            Debug.Log("Your score is : " + totalScore);
        }
        else
        {
            Debug.Log("No matching snap point found for object tag: " + objectTag);
        }
    }

    public void HandleFailedAnswer()
    {
        if(!GameManager.instance.IsArabicApp())
            audioSource.PlayOneShot(englishFailedAudioClip);
        else
            audioSource.PlayOneShot(arabicFailedAudioClip);
    }

    private void CheckLevelCompletion()
    {
        if (totalScore >= requiredScore)
        {
            onLevelComplete?.Invoke();
            Debug.Log("You have pass this level ");
            StartCoroutine(PassTheLevel());
        }
    }

    IEnumerator PassTheLevel()
    {
        if (!GameManager.instance.IsArabicApp())
            yield return new WaitForSeconds(englishSuccessAudioClip.length);
        else
            yield return new WaitForSeconds(arabicInstructions.length);


        if (!GameManager.instance.IsArabicApp())
            audioSource.PlayOneShot(englishPassAudioClip);
        else    
            audioSource.PlayOneShot(arabicPassAudioClip);


        if (!GameManager.instance.IsArabicApp())
            yield return new WaitForSeconds(englishPassAudioClip.length);
        else
            yield return new WaitForSeconds(arabicPassAudioClip.length);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        StartCoroutine(NextLevel());
    }
    [ContextMenu("Next Level")]
    IEnumerator NextLevel()
    {
        Debug.Log("NextLevel");
        nextLevelPanel.SetActive(true);

        // Countdown loop
        float remainingTime = WaitingForNextLevelPerSeconds;
        while (remainingTime > 0)
        {
            if (!GameManager.instance.IsArabicApp())
            {
                nextLevelInSecondsTMP.gameObject.SetActive(true);
                nextLevelInSecondsTMP.text = $"congratulations, you will be next level in {Mathf.CeilToInt(remainingTime)} seconds.";
            }
            else
                FindObjectOfType<ArabicFixerInstractions>().ArabicFixerThreeD($"تهانينا، ستصل إلى المستوى التالي خلال  {Mathf.CeilToInt(remainingTime)} ثانية.");

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        // Use RPC to sync scene loading
        photonView.RPC("LoadNextLevel", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (nextSceneIndex >= totalScenes)
            nextSceneIndex = 0; // Go back to main menu

        PhotonNetwork.LoadLevel(nextSceneIndex);
    }


    [PunRPC]
    void UpdateScoreUI(int totalScore)
    {

        ScoreTMP.text = totalScore.ToString();
        ScreenScoreTMP.text = $@"Score: {totalScore}/{requiredScore}";
        Debug.Log("RPC TEST");
        CheckLevelCompletion();
    }
}