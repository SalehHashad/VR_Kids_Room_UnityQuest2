using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioIntructions : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip arabicClip;
    [SerializeField] AudioClip englishClip;

    private void Start()
    {
        StartCoroutine(PlayingAudioInstructions());
    }

    IEnumerator PlayingAudioInstructions()
    {
        if(GameManager.instance.IsArabicApp())
            audioSource.clip = arabicClip;
        else
            audioSource.clip = englishClip;

        yield return new WaitForSeconds(.5f);

        audioSource.Play();

        while (audioSource.isPlaying)
        {
            yield return null;
        }

        print("Instructions Finished");
        this.gameObject.SetActive(false);
    }
}
