using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProgramManager : MonoBehaviour
{

    public GameObject dfaPrefab;
    public GameObject statePrefab;
    
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

    public string inputFile = "sampelDfa.json.txt";
    public string outputFile;
    public Dfa dfa;

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
        JsonDfa jsonDfa = ReadUserInput();
        if (jsonDfa != null)
        {
            BuildDfa(jsonDfa);
        }
    }

    public JsonDfa ReadUserInput()
    {
        
        if (File.Exists(Application.dataPath + "/" + inputFile))
        {
           JsonDfa jsonDfa = null;
           using (StreamReader reader = new StreamReader(Application.dataPath + "/" + inputFile))
           {
                string json = reader.ReadToEnd();
                int transitionsStart = json.IndexOf("\"transitions");
                string jsonBeginning = json.Substring(0, transitionsStart);
                Debug.Log(jsonBeginning);
                string jsonEnd = json.Substring(transitionsStart + "\"transitions\":".Length);
                Debug.Log(jsonEnd);
                //Debug.Log(json);
                try
                {
                    jsonDfa = JsonUtility.FromJson<JsonDfa>(jsonBeginning);
                    var myObject = JsonUtility.FromJson<JsonDfaTransitions>("{\"transitions\":" + jsonEnd + "}");
                    jsonDfa.transitions = new List<Transition>(myObject.transitions);
                    
                }
                catch (Exception e)
                {
                    Debug.Log(string.Format("Failed to read file {0} with exception {1}", Application.dataPath + "/" + inputFile, e));
                }
           }
            if (jsonDfa != null)
            {
                //Debug.Log("States: " + string.Join(",", jsonDfa.states));
                //Debug.Log("Initial: " + jsonDfa.initial);
                //Debug.Log("Final: " + string.Join(",", jsonDfa.final));
                //Debug.Log("Alphabet: " + string.Join(",", jsonDfa.alphabet));
                //Debug.Log("Strings: " + string.Join(",", jsonDfa.strings));
                //Debug.Log("Transitions: " + string.Join(",", jsonDfa.transitions));
                return jsonDfa;

            }
        }
        else
        {
            Debug.Log(string.Format("Can't find file at path {0}", Application.dataPath + "/" + inputFile));
        }
        return null;
    }


    private bool BuildDfa(JsonDfa jsonDfa)
    {

        dfa = Instantiate(dfaPrefab).GetComponent<Dfa>();
        List<State> states = new List<State>();
        for (int i = 0; i < jsonDfa.states.Count; i++)
        {
            State state = Instantiate(statePrefab).GetComponent<State>();
            state.name = jsonDfa.states[i];
            states.Add(state);
        }
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
        return true;

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
