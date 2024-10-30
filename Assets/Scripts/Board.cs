using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
public class Board : MonoBehaviour
{

    public float gridScalerX = 1.4F;
    public float gridScalerY = 1.6F;
    public int width;
    public int height;
    public GameObject cubeNoColorPrefab;
    public GameObject vaseNoDamagePrefab;
    public GameObject tntPrefab;
    public GameObject stonePrefab;
    public GameObject boxPrefab;
    public GameObject gameManagerGameObject;


    void Start()
    {
        Setup();
    }

    //Setting up the board
    private void Setup()
    {
        SetGridSize();
        SetBoard();
    }


    ////Set board objects size by setting its sprite size
    void SetSpriteDimensions()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Vector2 newSize = new Vector2(width * gridScalerX + 0.5F, height * gridScalerY + 0.5F);

        spriteRenderer.size = newSize;  // Set the size directly

    }
    private void SetGridSize()
    {//Get grid information from LevelManager and set grid size

        width = LevelManager.Instance.levelData.grid_width;
        height = LevelManager.Instance.levelData.grid_height;

        SetSpriteDimensions();//set sprite size
    }

    //Instantiate cells on the board according to levelData
    private void SetBoard()
    {
        float startPositionX = -((width*gridScalerX) /2f) + gridScalerX/2f;
        float startPositionY= -(height*gridScalerY)/2+ gridScalerY/2f-3;
        // Iterate through the grid
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Get the cell string from level data
                string gridItem = LevelManager.Instance.levelData.grid[j * width + i];

                // Assign a random color if grid item is "rand"
                if (gridItem == "rand")
                {
                    gridItem = GetRandomColor();
                    LevelManager.Instance.UpdateGrid(i, j, gridItem);
                }

                // Calculate the position of the cell in the game world
                Vector2 cellPosition = new Vector2(((i * gridScalerX) + startPositionX), ((j * gridScalerY) + startPositionY));

                //Create null Cell
                GameObject currentCell = null;
                Tuple<int, int> cellCoords = new Tuple<int, int>(i, j);

                // Instantiate and initialize according to grid
                switch (gridItem)
                {
                    case "b":
                    case "g":
                    case "r":
                    case "y": // Cube
                        currentCell = Instantiate(cubeNoColorPrefab, cellPosition, Quaternion.identity);
                        Cube cube = currentCell.GetComponent<Cube>();
                        cube.Initialize(cellCoords, gridItem);
                        break;

                    case "t": // TNT
                        currentCell = Instantiate(tntPrefab, cellPosition, Quaternion.identity);
                        TNT tnt = currentCell.GetComponent<TNT>();
                        tnt.Initialize(cellCoords);
                        break;

                    case "bo": // Box
                        currentCell = Instantiate(boxPrefab, cellPosition, Quaternion.identity);
                        Box box = currentCell.GetComponent<Box>();
                        box.Initialize(cellCoords);
                        LevelManager.Instance.numberOfBox++;
                        break;

                    case "s": // Stone
                        currentCell = Instantiate(stonePrefab, cellPosition, Quaternion.identity);
                        Stone stone = currentCell.GetComponent<Stone>();
                        stone.Initialize(cellCoords);
                        LevelManager.Instance.numberOfStone++;
                        break;

                    case "v": // Vase
                        currentCell = Instantiate(vaseNoDamagePrefab, cellPosition, Quaternion.identity);
                        Vase vase = currentCell.GetComponent<Vase>();
                        vase.Initialize(cellCoords);
                        LevelManager.Instance.numberOfVase++;
                        break;

                    default:
                        Debug.LogError("Corrupted Level Config JSON");
                        break;
                }


                if (currentCell != null)
                {
                    // Set cell name 
                    currentCell.name = "(" + i + " " + j + ")";

                    // Assign sprite and particle system based on the grid item
                    SpriteRenderer spriteRenderer = currentCell.GetComponent<SpriteRenderer>();
                    Sprite[] defaultSprites = gameManagerGameObject.GetComponent<GameManager>().defaultSprites;
                    Dictionary<string, int> indexer = LevelManager.Instance.stringToIndex;

                    if (indexer.ContainsKey(gridItem))
                    {
                        spriteRenderer.sprite = defaultSprites[indexer[gridItem]];
                        currentCell.GetComponent<Cell>().particleSystemPrefab = gameManagerGameObject.GetComponent<GameManager>().particleList[indexer[gridItem]];
                    }
                }
            }
        }

        // Disable UI elements if no Box, Vase, or Stone remain
        DisableUIIfEmpty("Box", LevelManager.Instance.numberOfBox, ref gameManagerGameObject.GetComponent<GameManager>().CheckBox);
        DisableUIIfEmpty("Vase", LevelManager.Instance.numberOfVase, ref gameManagerGameObject.GetComponent<GameManager>().CheckVase);
        DisableUIIfEmpty("Stone", LevelManager.Instance.numberOfStone, ref gameManagerGameObject.GetComponent<GameManager>().CheckStone);
    }

    // Helper method to disable UI 
    private void DisableUIIfEmpty(string uiElementName, int itemCount, ref bool checkFlag)
    {
        if (itemCount < 1)
        {
            GameObject uiObject = GameObject.Find(uiElementName);
            if (uiObject != null)
            {
                uiObject.SetActive(false);
            }
            checkFlag = false;
        }
    }


    //Helper Function
    private string GetRandomColor()
    {
        // Choose a random color from the list
        string[] colors = { "r", "g", "b", "y" }; // Red, Green, Blue, Yellow
        int randomIndex = UnityEngine.Random.Range(0, colors.Length);
        return colors[randomIndex];
    }

}
