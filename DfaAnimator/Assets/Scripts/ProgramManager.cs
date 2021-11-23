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

        //"transitions":[{"character":"a", "from":["q0","q1","q2"], "to":["q2","q1","q2"]},{"character":"b", "from":["q0","q1","q2"], "to":["q1","q2","q2"]}], 
        public List<string> states;
        public string initial;
        public List<string> final;
        public List<string> alphabet;
        public List<string> strings;
        public List<Transition> transitions;

    }


    public static ProgramManager instance;

    public string inputFile = "";
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
        inputFile = FileSaver.instance.loadFromPath;

        currentCharacter.text = "";
        currentWordMesh.text = "";
        currentWord = "";
        wordsToTest = new List<string>();
        states = new List<State>();
        JsonDfa jsonDfa = ReadUserInput();
        if (jsonDfa != null)
        {
            UpdateWordPanel();
            if (!BuildDfa(jsonDfa))
            {
                StartCoroutine(DisplayError(string.Format("The file {0} is not in the proper format", inputFile)));
            }
        }
    }

    public JsonDfa ReadUserInput()
    {
        
        if (File.Exists(inputFile))
        {
           JsonDfa jsonDfa = null;
           using (StreamReader reader = new StreamReader(inputFile))
           {
                string json = reader.ReadToEnd();
                int transitionsStart = json.IndexOf("\"transitions");
                string jsonBeginning = json.Substring(0, transitionsStart);
                Debug.Log(jsonBeginning);
                string jsonEnd = json.Substring(transitionsStart + "\"transitions\":".Length);
                Debug.Log(jsonEnd);

                try
                {
                    jsonDfa = JsonUtility.FromJson<JsonDfa>(jsonBeginning);
                    for (int i = 0; i < jsonDfa.strings.Count; i++)
                    {
                        wordsToTest.Add(jsonDfa.strings[i]);
                    }
                    currentWord = wordsToTest[0];
                    var myObject = JsonUtility.FromJson<JsonDfaTransitions>("{\"transitions\":" + jsonEnd + "}");
                    jsonDfa.transitions = new List<Transition>(myObject.transitions);
                    
                }
                catch (Exception e)
                {
                    StartCoroutine(DisplayError(string.Format("The file {0} is not in the proper format", inputFile)));
                    Debug.Log(string.Format("Failed to read file {0} with exception {1}", inputFile, e));
                }
           }
            if (jsonDfa != null)
            {
                return jsonDfa;
            }
        }
        else
        {
            StartCoroutine(DisplayError(string.Format("Can't find file at path {0}", inputFile)));
            Debug.Log(string.Format("Can't find file at path {0}", inputFile));
        }
        return null;
    }


    private bool BuildDfa(JsonDfa jsonDfa)
    {

        dfa = Instantiate(dfaPrefab).GetComponent<Dfa>();

        
        // create all of the state objects for this DFA
        for (int i = 0; i < jsonDfa.states.Count; i++)
        {
            State state = Instantiate(statePrefab).GetComponent<State>();
            state.name = jsonDfa.states[i];
            states.Add(state);
            
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

            for (int j = 0; j < jsonDfa.transitions.Count; j++)
            {
                int fromIndex = jsonDfa.transitions[j].from.IndexOf(states[i].name);
                if (fromIndex == -1) { return false; }
                string toStateName = jsonDfa.transitions[j].to[fromIndex];
                State toState = states.Find(x => x.name == toStateName);
                if (toState == null) { return false; }
                states[i].AddPath(toState, jsonDfa.transitions[j].character);
            }

        }
        dfa.Initialize();
        return true;

    }

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

    public void MoveToNextWord()
    {
        currentWordIndex = (currentWordIndex + 1) % wordsToTest.Count;
        currentWord = wordsToTest[currentWordIndex];
        UpdateWordPanel();
    }

    public void RunDfa()
    {
        if (runImage.sprite == runSprite)
        {
            currentWordMesh.text = currentWord;
            runImage.sprite = blankSprite;
            dfa.Read(currentWord);
        }
    }

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
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

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
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
