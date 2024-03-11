using System.Collections.Generic;
using UnityEngine;

public class LevelReader : MonoBehaviour {
    private List<Level> _levels = new List<Level>();
    public int numberOfLevels = 10;
    [SerializeField]private TextAsset textAsset;

    public List<Level> Levels => _levels;
    
    void Awake() {
        LoadJsonFiles();
    }

    void LoadJsonFiles() {
        
        for (int i = 1; i <= numberOfLevels; i++)
        {
            if (i <= 9)
            {
                textAsset = Resources.Load<TextAsset>("level_0" + i);
            }
            else
            {
                textAsset = Resources.Load<TextAsset>("level_" + i);
            }

            string jsonString = textAsset.text;
            var levelData = JsonUtility.FromJson<Level>(jsonString);
            _levels.Add(levelData);
            //Debug.Log("LEVEL NO: " + _levels[i-1].level_number);
        }
    }
    public List<Level> GetAllLevels()
    {
        return _levels;
    }
    /*
    for (int i = 0; i < numberOfLevels; i++)
    {
        _levels.Add(new Level());
        _levels[i] = JsonUtility.FromJson<Level>(textAsset[i].text);
    }*/
}