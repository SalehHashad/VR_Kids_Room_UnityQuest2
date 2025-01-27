using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageConditions : MonoBehaviour
{
    private void Start()
    {
        Debug.Log(GameManager.instance.IsArabicApp());
    }
    
}
