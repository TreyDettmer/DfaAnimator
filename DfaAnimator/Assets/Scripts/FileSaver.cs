using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileSaver : MonoBehaviour
{
    public static FileSaver instance;
    public string loadFromPath = "";
    public List<(string, bool)> results;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else if (instance != this)
        {
            Destroy(this);
        }

    }

    public void SaveOutput(string _word, bool _result)
    {
        results.Add((_word, _result));
        List<string> output = new List<string>();
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].Item2)
            {
                output.Add(string.Format("Input: \"{0}\"  Result: Accept", results[i].Item1));
            }
            else
            {
                output.Add(string.Format("Input: \"{0}\"  Result: Reject", results[i].Item1));
            }
        }
        File.WriteAllLines(loadFromPath.Substring(0, loadFromPath.IndexOf(".txt")) + "_output.txt", output.ToArray());

    }

    


    // Start is called before the first frame update
    void Start()
    {
        results = new List<(string, bool)>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
