using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProgramManager : MonoBehaviour
{

    public GameObject dfaPrefab;
    public GameObject statePrefab;
    public TextMeshPro currentCharacter;
    public TextMeshProUGUI currentWordMesh;
    public TextMeshProUGUI wordsToTestMesh;
    public TextMeshProUGUI errorTextMesh;
    public GameObject errorPanel;
    public RawImage haltEffect;
    public Image runImage;
    public Sprite runSprite;
    public Sprite blankSprite;
    

    public List<string> wordsToTest;
    public int currentWordIndex = 0;
    public string currentWord;

    [Serializable]
    public class Transition
    {
        public string character;
        public List<string> from;
        public List<string> to;
    }

    [Serializable]
    public class JsonDfaTransitions
    {
        public Transition[] transitions;
    }

    [Serializable]
    public class JsonDfa
    {
        public List<string> states;
        public string initial;
        public List<string> final;
        public List<string> alphabet;
        public List<string> strings;
        public List<Transition> transitions;
    }


    public static ProgramManager instance;

    public string inputFile = "";
    public string inputJson = "";
    public string outputFile;
    public Dfa dfa;
    public List<State> states;

    /// <summary>
    /// Initialize singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
        else if (instance != this)
        {
            Destroy(this);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        // initialize variables
        currentCharacter.text = "";
        currentWordMesh.text = "";
        currentWord = "";
        wordsToTest = new List<string>();
        states = new List<State>();


        // determine whether to load DFA from file or directly from FileSaver object
        if (FileSaver.instance.loadFromPath != "")
        {
            inputFile = FileSaver.instance.loadFromPath;
        }
        else
        {
            inputJson = FileSaver.instance.jsonDFA;
        }

        // read the input file/text as a JSON object representing a DFA
        JsonDfa jsonDfa = ReadUserInput();

        if (jsonDfa != null)
        {
            UpdateWordPanel();
            // attempt to build the DFA from the JSON format
            if (!BuildDfa(jsonDfa))
            {
                dfa = null;
            }
        }
    }

    /// <summary>
    /// Reads the input file/text as a JSON object representing a DFA
    /// </summary>
    /// <returns></returns>
    public JsonDfa ReadUserInput()
    {
        JsonDfa jsonDfa;
        string json;

        // get the json string from the provided file or text
        if (File.Exists(inputFile))
        {
            using (StreamReader reader = new StreamReader(inputFile))
            {
                json = reader.ReadToEnd();
            }
        }
        else if (inputJson != "")
        {
            json = inputJson;
        }
        else
        {
            StartCoroutine(DisplayError("Failed to read provided DFA"));
            return null;
        }

        // variables which keep track of where the transitions section of the JSON start
        int transitionsStart = json.IndexOf("\"transitions");
        string jsonBeginning = json.Substring(0, transitionsStart);
        string jsonEnd = json.Substring(transitionsStart + "\"transitions\":".Length);

        try
        {
            jsonDfa = JsonUtility.FromJson<JsonDfa>(jsonBeginning);
            // add the provided test strings to a list of words to test
            for (int i = 0; i < jsonDfa.strings.Count; i++)
            {
                wordsToTest.Add(jsonDfa.strings[i]);
            }
            currentWord = wordsToTest[0];

            // read the transitions
            var myObject = JsonUtility.FromJson<JsonDfaTransitions>("{\"transitions\":" + jsonEnd + "}");
            jsonDfa.transitions = new List<Transition>(myObject.transitions);
                    
        }
        catch (Exception)
        {
            StartCoroutine(DisplayError("The provided DFA is not in the proper format."));
            return null;
        }
        
        if (jsonDfa != null)
        {
            return jsonDfa;
        }
        
        
        return null;
    }

    /// <summary>
    /// Builds the DFA from the provided DFA in json format
    /// </summary>
    /// <param name="jsonDfa">DFA in json format</param>
    /// <returns></returns>
    private bool BuildDfa(JsonDfa jsonDfa)
    {

        // create the dfa GameObject
        dfa = Instantiate(dfaPrefab).GetComponent<Dfa>();

        string[] alphabet = jsonDfa.alphabet.ToArray();
        
        // create all of the state objects for this DFA
        for (int i = 0; i < jsonDfa.states.Count; i++)
        {
            State state = Instantiate(statePrefab).GetComponent<State>();
            state.name = jsonDfa.states[i];
            states.Add(state);
            
        }

        // ensure that test strings only contain characters in the alphabet
        for (int inputStringIndex = 0; inputStringIndex < jsonDfa.strings.Count; inputStringIndex++)
        {  
            for (int charIndex = 0; charIndex < jsonDfa.strings[inputStringIndex].Length; charIndex++)
            {
                if (!alphabet.Contains(jsonDfa.strings[inputStringIndex][charIndex].ToString()))
                {
                    StartCoroutine(DisplayError("Invalid DFA format. Test strings contain characters not in alphabet."));
                    return false;
                }
            }
        }

        // initialize each state
        for (int i = 0; i < states.Count; i++)
        { 
            
            bool isInitial = false;
            bool isFinal = false;
            if (jsonDfa.initial.Equals(states[i].name))
            {
                isInitial = true;
            }
            if (jsonDfa.final.Contains(states[i].name))
            {
                isFinal = true;
            }
            states[i].Initialize(isFinal, isInitial);


            List<string> charactersWithoutTransitions = new List<string>(alphabet);

            // read transitions
            for (int j = 0; j < jsonDfa.transitions.Count; j++)
            {
                if (!alphabet.Contains(jsonDfa.transitions[j].character))
                {
                    StartCoroutine(DisplayError("Invalid DFA format. Transition for character not in alphabet."));
                    return false;
                }

                // get the transition from this state
                int fromIndex = jsonDfa.transitions[j].from.IndexOf(states[i].name);
                if (fromIndex == -1) 
                {
                    StartCoroutine(DisplayError(
                        string.Format("Invalid DFA format. State {0} does not have a transition for every character.", states[i].name)));
                    return false; 
                }

                // get the destination of this transition
                string toStateName = jsonDfa.transitions[j].to[fromIndex];
                State toState = states.Find(x => x.name == toStateName);
                if (toState == null) 
                {
                    StartCoroutine(DisplayError(
                        string.Format("Invalid DFA format. Invalid state in transitions for character \'{0}\'", jsonDfa.transitions[j].character)));
                    return false;
                }

                // create the transition
                charactersWithoutTransitions.Remove(jsonDfa.transitions[j].character);
                states[i].AddPath(toState, jsonDfa.transitions[j].character);
            }

            if (charactersWithoutTransitions.Count > 0)
            {
                StartCoroutine(DisplayError("Invalid DFA format. Not all characters have transitions."));
                return false;
            }

        }
        return true;

    }

    /// <summary>
    /// Updates the panel of test strings to highlight the string currently being tested
    /// </summary>
    public void UpdateWordPanel()
    {
        wordsToTestMesh.text = "";
        for (int i = 0; i < wordsToTest.Count; i++)
        {
            if (i == currentWordIndex)
            {
                wordsToTestMesh.text += "<color=yellow>\"" + wordsToTest[currentWordIndex] + "\"</color>\n";
            }
            else
            {
                wordsToTestMesh.text += "\"" + wordsToTest[i] + "\"\n";
            }
        }
    }

    /// <summary>
    /// Sets the current word to be the next test string
    /// </summary>
    public void MoveToNextWprd()
    {
        currentWordIndex = (currentWordIndex + 1) % wordsToTest.Count;
        currentWord = wordsToTest[currentWordIndex];
        UpdateWordPanel();
    }

    /// <summary>
    /// Runs DFA on the current input test string
    /// </summary>
    public void RunDfa()
    {
        // ensure that the DFA exists and is not currently running
        if (runImage.sprite == runSprite && dfa != null)
        {
            currentWordMesh.text = currentWord;
            runImage.sprite = blankSprite;
            dfa.Read(currentWord);
        }
    }

    /// <summary>
    /// Runs animation of character along a curved path
    /// </summary>
    /// <param name="characterIndex">the index of the character in the current word</param>
    /// <param name="curvedPath">the curved path to move along</param>
    /// <returns></returns>
    public IEnumerator AnimateTransition(int characterIndex, CurvedLineRenderer curvedPath)
    {
        // Update the current transitioning character
        currentCharacter.text = currentWord[characterIndex].ToString();

        // Update the highlighted character in the provided input
        UpdateCurrentWordPosition(characterIndex);

        // initialize local variables
        float elapsedTime = 0;
        int nextPointIndex = 1;
        List<float> cumulativeDistances = new List<float>();
        float totalPathLength = 0f;

        // calculate the length of the curved path between states
        for (int i = 1; i < curvedPath.smoothedPoints.Length; i++)
        {
            totalPathLength += Vector3.Distance(curvedPath.smoothedPoints[i],
                                            curvedPath.smoothedPoints[i-1]);
        }

        // animation loop
        while (elapsedTime < 2f)
        {
            try
            {
                // ensure that there are at least two points to move between on the path
                if (curvedPath.smoothedPoints.Length > 1)
                {

                    float percentageOfTheWayDone = elapsedTime / 2f;

                    // calculate how far along we should be on the path
                    float distanceToCover = (percentageOfTheWayDone * totalPathLength);

                    // calculate the point on the curve that we should be at in order to cover the necessary distance
                    while (cumulativeDistances.Sum() < distanceToCover)
                    {
                        // add the distance between the next two points to the cumulative distances
                        cumulativeDistances.Add(Vector3.Distance(curvedPath.smoothedPoints[nextPointIndex],
                                                curvedPath.smoothedPoints[nextPointIndex - 1]));
                        nextPointIndex++;

                        // ensure that the nextPointIndex is within the bounds of the points on the curve
                        nextPointIndex = Math.Min(nextPointIndex, curvedPath.smoothedPoints.Length - 1);
                    }

                    // set the position for the character (move it back on the z-axis to place it in front of states and lines)
                    currentCharacter.GetComponent<RectTransform>().position = curvedPath.smoothedPoints[nextPointIndex]
                                                                            + (Vector3.back * 10);
                }
            }
            catch (Exception)
            {
                //an index out of bounds exception can occur if we are updating the path during the animation
            }

            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Updates the highlighted character in the current word to match the current transitioning character
    /// </summary>
    /// <param name="characterIndex">the index of the character to highlight</param>
    public void UpdateCurrentWordPosition(int characterIndex)
    {
        currentWordMesh.text = "";
        for (int i = 0; i < currentWord.Length; i++)
        {
            if (i == characterIndex)
            {
                currentWordMesh.text += "<color=yellow>" + currentWord[characterIndex].ToString() + "</color>";
            }
            else
            {
                currentWordMesh.text += currentWord[i];
            }

        }
    }

    /// <summary>
    /// Flashes green or red depending on whether the DFA accepted or rejected the input
    /// </summary>
    /// <param name="isAccept">whether the DFA accepted the input</param>
    /// <returns></returns>
    public IEnumerator FlashHaltEffect(bool isAccept)
    {
        haltEffect.gameObject.SetActive(true);
        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            if (isAccept)
            {
                haltEffect.color = new Color(0f, 1f, 0f, .2f - .2f * (elapsedTime / 1f));
            }
            else
            {
                haltEffect.color = new Color(1f, 0f, 0f, .2f - .2f * (elapsedTime / 1f));
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        haltEffect.gameObject.SetActive(false);
        runImage.sprite = runSprite;

    }

    /// <summary>
    /// Temporarily displays an error message at the bottom of the screen
    /// </summary>
    /// <param name="errorMessage">the message to display</param>
    /// <returns></returns>
    public IEnumerator DisplayError(string errorMessage)
    {
        runImage.sprite = runSprite;
        if (errorPanel.activeSelf)
        {
            // don't do anything since an error is already being displayed
            yield return null;
        }
        else
        {
            errorTextMesh.text = errorMessage;
            errorPanel.SetActive(true);
            float elapsedTime = 0f;
            while (elapsedTime < 7f)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            errorPanel.SetActive(false);
        }
    }

    public void ExitToStartMenu()
    {
        FileSaver.instance.jsonDFA = "";
        FileSaver.instance.loadFromPath = "";
        SceneManager.LoadScene(0);
    }

}
