using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;
using UnityEngine.UI;
[System.Serializable]
public class GameData
{
    public int currentLevel;
}

public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;


}

public class LevelManager : MonoBehaviour
{
    // Make it a singleton
    public static LevelManager Instance { get; private set; }

    public LevelData levelData;
    private string levelFilePath;
    private string saveFilePath;
    public int currentLevel = 1;

    public bool isPlaying=true;
    [SerializeField] public TextMeshProUGUI currentLevelText;

    public int numberOfBox = 0;
    public int numberOfVase = 0;
    public int numberOfStone = 0;
    public List<GameObject> surroundingObstacles;
    public Dictionary<string, int> stringToIndex = new Dictionary<string, int>(){
            { "b", 0 },//blue
            { "g", 1 },//green
            { "r", 2 },//red
            { "y", 3 },//yellow
            {"t",4},//tnt
            {"bo",5},//box
            {"s",6},//stone
            {"v",7}//vase
        };


    private void Awake()
    {
        // If there is no instance, make this the instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // If an instance already exists, destroy this one
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        saveFilePath = Application.persistentDataPath + "/gameData.json";
        LoadCurrentLevel();
    }



    public void LoadLevelScene()
    {
        if (currentLevel <= 10)
        {
            isPlaying=true;
            string currentLevelName = "Levels";//Same scene for all levels
            SceneManager.LoadSceneAsync(currentLevelName);
            GetLevelData();
        }
    }
    public void LoadMainScene()
    {
        // Load the Main Menu scene asynchronously
        SceneManager.LoadScene("MainMenu");

        if (currentLevel <= 10)
            StartCoroutine(LoadMainSceneCoroutine());// I used coroutine bcs while loading the scene objects should wait for each other to acces
    }


    private IEnumerator LoadMainSceneCoroutine()
    {
        // Wait until the scene is fully loaded
        while (SceneManager.GetActiveScene().name != "MainMenu")
        {
            yield return null;
        }

        // Find the button and add the listener
        Button levelButton = GameObject.Find("LevelButton").GetComponent<Button>();
        if (levelButton != null)
        {
            levelButton.onClick.AddListener(LoadLevelScene);
            Debug.Log("LevelButton listener added successfully.");
        }

        // Load the current level data 
        LoadCurrentLevel();
    }

    //This function increment level data and save level information to local file and load the main menu
    public void GetNextLevel()
    {
        currentLevel++;
        SaveCurrentLevel();//Save level data locally
        LoadMainScene();//Get back to Main Menu
    }

    public void SaveCurrentLevel()
    {
        // GameData object to hold the level information
        GameData data = new GameData();
        data.currentLevel = currentLevel;

        // Convert the GameData object to a JSON string 
        string json = JsonUtility.ToJson(data, true);

        // Write the JSON to saveFilePath
        File.WriteAllText(saveFilePath, json);

        // Log a message 
        Debug.Log("Level " + currentLevel + " saved to " + saveFilePath);
    }



    public void LoadCurrentLevel()
    {
        //Get current level data from saved file
        string json = File.ReadAllText(saveFilePath);
        GameData data = JsonUtility.FromJson<GameData>(json);
        currentLevel = data.currentLevel;

        //Get textmeshpro on the button
        TextMeshProUGUI currentLevelText = GameObject.Find("currentLevelText").GetComponent<TextMeshProUGUI>();
        
        //Set text on the button
        if (currentLevel <= 10)
        {

            currentLevelText.text = "Level " + currentLevel;
        }
        else
        {
            currentLevelText.text = "Finished";
        }
    }

    //Update LevelData from level config files
    private void GetLevelData()
    {
        string levelName;

        if (currentLevel < 10)
        {//One digit
            levelName = "level_0" + currentLevel;
        }

        else
        {//More thone one digit
            levelName = "level_" + currentLevel;
        }

        //Get level data from levelFilePath
        levelFilePath = Path.Combine(Application.streamingAssetsPath, "Levels/" + levelName + ".json");
        levelData = ReadJsonFile(levelFilePath);
    }

    private LevelData ReadJsonFile(string path)
    {
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonContent);
            return levelData;
        }
        else
        {
            Debug.LogError("JSON file not found: " + path);
            return null;
        }
    }

    //This function isn't related to level. However i still add it here because i want only my levelmanager to be singleton
    public void UpdateGrid(int i, int j, string newValue)
    {
        levelData.grid[j * levelData.grid_width + i] = newValue;
    }
}