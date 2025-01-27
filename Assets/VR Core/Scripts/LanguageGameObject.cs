using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageGameObject : MonoBehaviour
{
    [SerializeField] Language language;
    private void OnEnable()
    {
        if (language == Language.Arabic)
            this.GetComponent<PointableUnityEventWrapper>().WhenSelect.AddListener(OnArabicSelected);
        else
            this.GetComponent<PointableUnityEventWrapper>().WhenSelect.AddListener(OnEnglishSelected);
    }

    private void OnArabicSelected(PointerEvent arg0)
    {
        GameManager.instance.SetArabic();
    }
    private void OnEnglishSelected(PointerEvent arg0)
    {
        GameManager.instance.SetEnglish();
    }
}
