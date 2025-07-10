using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptInputfield : MonoBehaviour
{
    public AssessmentMode assessmentMode;
    public int index;
    // Start is called before the first frame update
    void Start()
    {
        TMP_InputField inputField = GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener((text) => HandleInput(text, index));
            return;
        }
        TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener((value) => HandleDropdown(dropdown.options[value].text, index));
        }
    }

    private void HandleDropdown(string text, int index)
    {
        if (index == 1)
        {
            assessmentMode.SetValue(text, "Checkpoint");
        }
        else if (index == 2)
        {
            assessmentMode.SetValue(text, "Resolution");
        }
        else if (index == 3)
        {
            assessmentMode.SetValue(text, "Controlnet");
        }
    }

    private void HandleInput(string text, int index)
    {
        if(index == 6)
        {
            assessmentMode.SetValue(text, "MainPrompt");
        }
        else if(index == 7)
        {
            assessmentMode.SetValue(text, "KeyPrompt");
        }
        else
        {
            assessmentMode.SetValue(text, (index - 1).ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
