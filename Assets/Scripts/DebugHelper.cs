using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHelper : MonoBehaviour
{
    public GameObject gameManagerGameObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayGrid(){
        Debug.Log("Grid Starts");
        int width=LevelManager.Instance.levelData.grid_width;
        int height=LevelManager.Instance.levelData.grid_height;
        string[] grid=LevelManager.Instance.levelData.grid;
        for(int i=0;i<width*height;i++){
            int y=i/width;
            int x=i-(y*width);
            Debug.Log("Position: "+x+" "+y+" Color "+grid[i]);
        }
    }




}
