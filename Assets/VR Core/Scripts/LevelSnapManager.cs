using ArabicSupport;
using Photon.Pun;
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
    [SerializeField] private AudioClip englishSuccessAudioClip, arabicSuccessAudioClip;
    [SerializeField] private AudioClip englishFailedAudioClip, arabicFailedAudioClip;
    [SerializeField] private AudioClip englishPassAudioClip, arabicPassAudioClip;
    [SerializeField] private AudioClip englishInstructions, arabicInstructions;

    [Header("Score Settings")]
    [SerializeField] private int totalScore = 0;
    [SerializeField] private int requiredScore = 0;
    [SerializeField] private TextMeshProUGUI scoreTMP, screenScoreTMP;
    [SerializeField] private List<ModelsData> modelsData = new();

    [Header("Image View Settings")]
    [SerializeField] private Image referenceImage;
    [SerializeField] private TextMeshProUGUI viewsRemainingText;
    [SerializeField] private int remainingViews;
    private float viewDuration;
    private bool isImageVisible = false;
    private Coroutine hideImageCoroutine;

    private ShowImage_Tag showImage;
    private ScoreButton_Tag showScoreButton;

    [Header("Next Level")]
    [SerializeField] private int waitingForNextLevelSeconds = 5;
    [SerializeField] private GameObject nextLevelPanel;
    [SerializeField] private TextMeshProUGUI nextLevelInSecondsTMP;

    public UnityEvent onLevelComplete;
    public UnityEvent<int> onScoreUpdated;

    private void Awake() => InitializeObjects();

    private void Start()
    {
        InitializeLevelSettings();
        referenceImage?.gameObject.SetActive(false);
        UpdateViewsText();
    }

    private void InitializeObjects()
    {
        showImage = FindObjectOfType<ShowImage_Tag>(true);
        showScoreButton = FindObjectOfType<ScoreButton_Tag>(true);
    }

    private void InitializeLevelSettings()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        (requiredScore, remainingViews, viewDuration) = currentLevel switch
        {
            1 => (6, 4, 10f),
            2 => (18, 3, 8f),
            3 => (27, 2, 6f),
            4 => (51, 1, 5f),
            _ => (0, 0, 0f)
        };

        if (currentLevel == 1) PlayGameInstructions();
        photonView.RPC("UpdateScoreUI", RpcTarget.AllBuffered, totalScore);
    }

    private void PlayGameInstructions()
    {
        audioSource.clip = GameManager.instance.IsArabicApp() ? arabicInstructions : englishInstructions;
        audioSource.Play();
    }

    public void ShowReferenceImage()
    {
        if (remainingViews <= 0 || isImageVisible || referenceImage == null) return;

        showImage.gameObject.SetActive(false);
        showScoreButton.gameObject.SetActive(false);
        isImageVisible = true;
        remainingViews--;
        UpdateViewsText();

        referenceImage.gameObject.SetActive(true);
        if (hideImageCoroutine != null) StopCoroutine(hideImageCoroutine);
        hideImageCoroutine = StartCoroutine(HideImageAfterDelay());
    }

    private IEnumerator HideImageAfterDelay()
    {
        yield return new WaitForSeconds(viewDuration);
        referenceImage.gameObject.SetActive(false);
        showImage.gameObject.SetActive(true);
        showScoreButton.gameObject.SetActive(true);
        isImageVisible = false;
    }

    private void UpdateViewsText()
    {
        if (viewsRemainingText)
            viewsRemainingText.text = $"Remaining Views: {remainingViews}";
    }

    public void HandleCorrectSnap(string objectTag)
    {
        photonView.RPC("CorrectAnswer", RpcTarget.Others, objectTag);
    }

    [PunRPC]
    private void CorrectAnswer(string objectTag)
    {
        var snapPoint = modelsData.Find(sp => sp.ObjetcTag == objectTag && !sp.IsMatched);
        if (snapPoint == null)
        {
            Debug.Log($"No matching snap point found for object tag: {objectTag}");
            return;
        }

        PlayAudio(GameManager.instance.IsArabicApp() ? arabicSuccessAudioClip : englishSuccessAudioClip);
        totalScore++;
        snapPoint.IsMatched = true;
        onScoreUpdated?.Invoke(totalScore);
        photonView.RPC("UpdateScoreUI", RpcTarget.AllBuffered, totalScore);
    }

    public void HandleFailedAnswer()
    {
        PlayAudio(GameManager.instance.IsArabicApp() ? arabicFailedAudioClip : englishFailedAudioClip);
    }

    private void CheckLevelCompletion()
    {
        if (totalScore < requiredScore) return;
        onLevelComplete?.Invoke();
        StartCoroutine(PassTheLevel());
    }

    private IEnumerator PassTheLevel()
    {
        yield return new WaitForSeconds(GetAudioClipLength(GameManager.instance.IsArabicApp() ? arabicInstructions : englishSuccessAudioClip));
        PlayAudio(GameManager.instance.IsArabicApp() ? arabicPassAudioClip : englishPassAudioClip);
        yield return new WaitForSeconds(GetAudioClipLength(GameManager.instance.IsArabicApp() ? arabicPassAudioClip : englishPassAudioClip));
        StartCoroutine(NextLevel());
    }

    private float GetAudioClipLength(AudioClip clip) => clip ? clip.length : 0;

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private IEnumerator NextLevel()
    {
        nextLevelPanel.SetActive(true);
        for (float remainingTime = waitingForNextLevelSeconds; remainingTime > 0; remainingTime--)
        {
            nextLevelInSecondsTMP.text = GameManager.instance.IsArabicApp()
                ? $"تهانينا، ستصل إلى المستوى التالي خلال {Mathf.CeilToInt(remainingTime)} ثانية."
                : $"Congratulations, you will be next level in {Mathf.CeilToInt(remainingTime)} seconds.";
            yield return new WaitForSeconds(1f);
        }
        photonView.RPC("LoadNextLevel", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            nextSceneIndex = 0; // Restart to main menu
        PhotonNetwork.LoadLevel(nextSceneIndex);
    }

    [PunRPC]
    private void UpdateScoreUI(int score)
    {
        scoreTMP.text = score.ToString();
        screenScoreTMP.text = $"Score: {score}/{requiredScore}";
        CheckLevelCompletion();
    }
}
