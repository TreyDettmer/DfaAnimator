using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public TextMeshProUGUI errorTextMesh;
    public TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        errorTextMesh.text = "";
        inputField.text = "sampleDfa.json.txt";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadFile()
    {
        if (!File.Exists(Application.dataPath + "/" + inputField.text))
        {
            StartCoroutine(DisplayError(string.Format("No file exists at path {0}", Application.dataPath + "/" + inputField.text)));
        }
        else
        {
            FileSaver.instance.loadFromPath = Application.dataPath + "/" + inputField.text;
            SceneManager.LoadScene(1);
        }
            
    }

    public IEnumerator DisplayError(string errorMessage)
    {
        
        if (errorTextMesh.text != "")
        {
            // don't do anything since an error is already being displayed
            yield return null;
        }
        else
        {
            errorTextMesh.text = errorMessage;
            float elapsedTime = 0f;
            while (elapsedTime < 7f)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            errorTextMesh.text = "";
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
