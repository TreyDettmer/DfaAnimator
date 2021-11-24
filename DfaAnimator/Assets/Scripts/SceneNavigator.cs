using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public TextMeshProUGUI errorTextMesh;
    public TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        
        if (SceneManager.GetActiveScene().buildIndex == 0) // at start menu scene
        {
            errorTextMesh.text = "";
            inputField.text = "goodDFA1.txt";
        }
        else // at custom DFA scene
        {
            // this was a pain to type out
            inputField.text = "{\n\t\"states\":[\"q0\",\"q1\",\"q2\"],\n\t \"initial\":\"q0\",\n\t  \"final\":[\"q1\"],\n\t \"alphabet\":[\"a\",\"b\"],\n\t \"strings\":[\"aba\",\"bab\",\"bbba\",\"\",\"b\"]\n}\n\"transitions\":[\n\t{\n\t\t \"character\":\"a\",\n\t\t \"from\":[\"q0\",\"q1\",\"q2\"],\n\t\t \"to\":[\"q2\",\"q1\",\"q2\"]\n\t},\n\t{\n\t\t \"character\":\"b\",\n\t\t \"from\":[\"q0\",\"q1\",\"q2\"],\n\t\t \"to\":[\"q1\",\"q2\",\"q2\"]\n\t}\n]";
        }
    }

    /// <summary>
    /// Loads input file then switches to the main scene
    /// </summary>
    public void LoadFile()
    {
        if (!File.Exists(Application.dataPath + "/" + inputField.text))
        {
            StartCoroutine(DisplayError(string.Format("No file exists at path {0}", Application.dataPath + "/" + inputField.text)));
        }
        else
        {
            FileSaver.instance.loadFromPath = Application.dataPath + "/" + inputField.text;
            SceneManager.LoadScene(2);
        }
            
    }

    public void LoadCustomDFAMenu()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Saves the user's "json" and switches to the main scene
    /// </summary>
    public void ReadJson()
    {
        if (inputField.text != "")
        {
            FileSaver.instance.jsonDFA = inputField.text;
            SceneManager.LoadScene(2);
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

    public void BackToStartMenu()
    {
        FileSaver.instance.jsonDFA = "";
        FileSaver.instance.loadFromPath = "";
        SceneManager.LoadScene(0);
    }
}
