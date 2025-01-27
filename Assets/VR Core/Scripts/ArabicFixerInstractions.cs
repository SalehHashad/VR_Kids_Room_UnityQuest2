using ArabicSupport;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArabicFixerInstractions : MonoBehaviour
{
    public void ArabicFixerThreeD(string arabicString)
    {
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();

        string fixedText = ArabicFixer.Fix(arabicString);

        gameObject.GetComponent<TextMesh>().text = fixedText;

        Debug.Log(fixedText);
    }
}
