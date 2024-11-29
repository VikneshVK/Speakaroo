using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPDFManager : MonoBehaviour
{
    public  void OpenPdfUrl(string Url)
    {
        Application.OpenURL(Url);
    }
}
