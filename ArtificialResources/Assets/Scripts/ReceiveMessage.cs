using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReceiveMessage : MonoBehaviour
{
    public AppWindow appCode;
    public string newName;

    [SerializeField] private Button choose;

    [ContextMenu("ReceiveMessages")]
    private void ReceiveMessages()
    {
        appCode = FindObjectOfType<AppWindow>();

        appCode.appText.text = newName;
        Instantiate(choose.gameObject, appCode.transform);
    }
}
