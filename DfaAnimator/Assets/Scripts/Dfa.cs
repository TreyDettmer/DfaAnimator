using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dfa : MonoBehaviour
{

    public float animationDelay;
    public bool isSimulating = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize()
    {

    }

    public void Read(string word)
    {
        if (!isSimulating)
        {
            isSimulating = true;
            StartCoroutine(ShowTransition(word));
        }
    }


    private IEnumerator ShowTransition(string word)
    {
        
        State currentState = null;
        currentState = ProgramManager.instance.states.Find(state => state.isInitial == true);
        State nextState = null;
        if (currentState == null)
        {
            StartCoroutine(ProgramManager.instance.DisplayError("DFA has no initial state."));
            Debug.LogError("DFA has no initial state.");

        }
        else
        {
            for (int letterIndex = 0; letterIndex < word.Length; letterIndex++)
            {
                // read the letter
                nextState = currentState.Read(word[letterIndex].ToString());
                // show transition from currentState to nextState
                if (nextState == null)
                {
                    StartCoroutine(ProgramManager.instance.DisplayError("Got a null state."));
                    Debug.LogError("Got a null state.");
                }
                CurvedLineRenderer curvedLineRenderer = currentState.GetLineRenderer(nextState);
               
                StartCoroutine(ProgramManager.instance.AnimateTransition(letterIndex, curvedLineRenderer));
                yield return new WaitForSeconds(2f);

                currentState = nextState;
            }
            StartCoroutine(ProgramManager.instance.FlashHaltEffect(currentState.isAccepting));
            FileSaver.instance.SaveOutput(word, currentState.isAccepting);
        }
        isSimulating = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
