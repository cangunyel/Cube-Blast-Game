using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
public class GameManager : MonoBehaviour
{

    public GameObject boardGameObject;
    public Sprite[] defaultSprites;
    public Sprite[] tntSprites;
    public int width;
    public int height;
    public GameObject cubeNoColorPrefab;
    public bool isAnimating;
    [SerializeField] TextMeshProUGUI move_left;
    [SerializeField] TextMeshProUGUI boxCount;
    [SerializeField] TextMeshProUGUI vaseCount;
    [SerializeField] TextMeshProUGUI stoneCount;
    public GameObject CheckPrefab;
    public bool CheckBox = true;
    public bool CheckVase = true;
    public bool CheckStone = true;

    public GameObject winPopUpPrefab;

    public GameObject loosePopUpPrefab;
    public ParticleSystem[] particleList;
    public List<List<Cube>> connectedGroups;
    private bool[,] visited;
    private List<List<int>> nullIndicesByColumn;
    // Start is called before the first frame update
    void Start()
    {
        InitializeUI();
        connectedGroups = new List<List<Cube>>();
        SetGridSize();  //Initialize grid size
        BeforeNextMove(); // Start the first iteration of game

    }



    private void InitializeUI()
    {
        move_left.text = LevelManager.Instance.levelData.move_count.ToString();
        boxCount.text = LevelManager.Instance.numberOfBox.ToString();
        vaseCount.text = LevelManager.Instance.numberOfVase.ToString();
        stoneCount.text = LevelManager.Instance.numberOfStone.ToString();
    }
    IEnumerator WaitForSec(float time)
    {
        yield return new WaitForSeconds(time);
    }

    private void SetGridSize()
    {//Get grid information from LevelManager and set grid size
        width = LevelManager.Instance.levelData.grid_width;
        height = LevelManager.Instance.levelData.grid_height;
    }

    //Analyze the board
    public void BeforeNextMove()
    {
        FindConnectedGroups();
        IterateGroups();
    }

    //Perform actions according to clicked object
    public IEnumerator AfterMove()
    {
        yield return new WaitForSeconds(0.5f);
        DetectNulls();//Detect nulls after the move
        List<int> newCubesInColumn = FillNulls();//Fill detected nulls by moving object and return list of needed cells per column

        // Wait for all falling animations to complete
        while (isAnimating)
        {
            yield return new WaitForSeconds(0.1f);
        }

        CreateNewCubes(newCubesInColumn);//Create new cubes and move them to null positions after falling cells

        // Wait for new cube animations to complete
        while (isAnimating)
        {
            yield return new WaitForSeconds(0.1f);
        }

        CheckGameStatus();//Check if the game ends or not
        BeforeNextMove();//Set board ready for a next move
    }

    public void PopupReturnMainMenu()
    {
        LevelManager.Instance.LoadMainScene();
    }

    public void PopupTryAgain()
    {
        LevelManager.Instance.LoadLevelScene();
    }

    private void CheckGameStatus()
    {
        // Update the UI 
        int remainingCountNumber = LevelManager.Instance.levelData.move_count;
        boxCount.text = LevelManager.Instance.numberOfBox.ToString();
        vaseCount.text = LevelManager.Instance.numberOfVase.ToString();
        stoneCount.text = LevelManager.Instance.numberOfStone.ToString();

        // Check if all boxes are destroyed
        if (boxCount.text == "0" && CheckBox)
        {
            HandleObjectDestruction("BoxCount", "BoxCheck", ref CheckBox);
        }

        // Check if all stones are destroyed
        if (stoneCount.text == "0" && CheckStone)
        {
            HandleObjectDestruction("StoneCount", "StoneCheck", ref CheckStone);
        }

        // Check if all vases are destroyed
        if (vaseCount.text == "0" && CheckVase)
        {
            HandleObjectDestruction("VaseCount", "VaseCheck", ref CheckVase);
        }

        // Update the remaining move count
        move_left.text = remainingCountNumber.ToString();

        // Check for win or loss conditions
        if (boxCount.text == "0" && stoneCount.text == "0" && vaseCount.text == "0")
        {
            Debug.Log("You win");
            ShowPopUpWithDelay(winPopUpPrefab, true);
        }
        else if (remainingCountNumber < 1)
        {
            Debug.Log("You lose");
            ShowPopUpWithDelay(loosePopUpPrefab, false);
        }
        else
        {
            Debug.Log("Remaining Move Count: " + remainingCountNumber);
        }
    }

    private void HandleObjectDestruction(string countObjectName, string checkObjectName, ref bool checkFlag)
    {
        // Disable the count UI 
        GameObject countObject = GameObject.Find(countObjectName);
        countObject.SetActive(false);

        //create the check mark for destroyed objects
        Vector2 checkPosition = new Vector2(0, 0);
        GameObject checkObject = Instantiate(CheckPrefab, checkPosition, Quaternion.identity);
        checkObject.name = checkObjectName;

        checkObject.transform.SetParent(countObject.transform.parent);
        checkObject.transform.localPosition = new Vector2(10, -10); // Adjust positioning

        Renderer checkRenderer = checkObject.GetComponent<Renderer>();
        checkRenderer.sortingOrder = 1;

        checkFlag = false; // No more checks for this object type
    }

    private void ShowPopUpWithDelay(GameObject popUpPrefab, bool isWin)
    {
        GameObject popUpObject = openPopUp(popUpPrefab);
        
        if (isWin)
        {
            
            StartCoroutine(WaitForUserClick(popUpObject));
        }
    }


    private GameObject openPopUp(GameObject prefab)
    {
        LevelManager.Instance.isPlaying=false;
        Vector2 centerPosition = new Vector2(0, 0);
        GameObject canvas = GameObject.Find("Canvas2");
        GameObject popUpObject = Instantiate(prefab, centerPosition, Quaternion.identity);
        popUpObject.transform.SetParent(canvas.transform, false);

        return popUpObject;
    }
    private IEnumerator WaitForUserClick(GameObject winPopUp)
    {
        // Wait until the darkened screen clicked
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // Once clicked, disable popup
        winPopUp.SetActive(false);


        //Move to the next level here
        LevelManager.Instance.GetNextLevel();
    }

    //Find connected cells and 
    private void FindConnectedGroups()
    {
        visited = new bool[width, height];//keeping visited list for not revisiting 
        connectedGroups.Clear();  // Clear existing groups before finding new ones
        Dictionary<string, int> spriteIndexer = LevelManager.Instance.stringToIndex;//string to index mapping   

        //Itarate the grid
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                string potentialColor = LevelManager.Instance.levelData.grid[j * width + i];//Get its color from grid

                if (!visited[i, j] && CheckIsCube(potentialColor))  // Only process non visited Cube objects
                {
                    List<Cube> currentGroup = new List<Cube>();
                    DFS(i, j, potentialColor, currentGroup); //search in dfs way

                    if (currentGroup.Count > 1)//it consist a group
                    {
                        connectedGroups.Add(currentGroup);
                    }
                    else
                    {//If cube is not connected any cubes it is uninteractable
                        GetCubeByPosition(i, j).IsInteractable = false;
                    }
                }
                if (CheckIsCube(potentialColor))
                {//if it is cube
                    //set its sprite back to default since it might unconnect from its group
                    string cubeName = GetNameByPosition(i, j);
                    string color = GetCubeByPosition(i, j).color;//get back to default sprite 
                    SpriteRenderer spriteRenderer = GameObject.Find(cubeName).GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = defaultSprites[spriteIndexer[color]];
                }
                if (CheckIsTnt(potentialColor))
                {//if it is cube
                    //set its explosion radius to default since it might unconnect from near tnt
                    string tntName = GetNameByPosition(i, j);
                    GameObject currentTnt = GameObject.Find(tntName);
                    currentTnt.GetComponent<TNT>().explosionRadius = 5;
                }


            }
        }
    }
    //Helper Function for FindConnected Groups
    private void DFS(int i, int j, string color, List<Cube> currentGroup)
    {
        // Check if the indices are within bounds and the cell is not visited
        if (i < 0 || i >= width || j < 0 || j >= height || visited[i, j])
        {
            return;
        }
        string potentialColor = LevelManager.Instance.levelData.grid[j * width + i];
        // Check colors match
        if (!(CheckIsCube(potentialColor)) || potentialColor != color)
        {
            return;
        }

        // Mark the cell as visited 
        visited[i, j] = true;


        Cube cubeComponent = GetCubeByPosition(i, j);
        currentGroup.Add(cubeComponent);

        // Explore neighboring cells
        DFS(i + 1, j, color, currentGroup);
        DFS(i - 1, j, color, currentGroup);
        DFS(i, j + 1, color, currentGroup);
        DFS(i, j - 1, color, currentGroup);
    }
    //Iterate groups for setting interactability and tntStates    
    private void IterateGroups()
    {
        Dictionary<string, int> spriteIndexer = LevelManager.Instance.stringToIndex;

        for (int groupIndex = 0; groupIndex < connectedGroups.Count; groupIndex++)
        {

            List<Cube> group = connectedGroups[groupIndex];
            string color = group[0].color;  // All cubes in group have same color

            if (group.Count >= 2)
            {//if it is interactable
                foreach (Cube cube in group)
                {
                    cube.IsInteractable = true;
                }
                if (group.Count >= 5)
                {//if it is eligible to construct a tnt

                    foreach (Cube cube in group)//Set the tnt sprites of all members of the group
                    {
                        SpriteRenderer spriteRenderer = cube.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = tntSprites[spriteIndexer[color]];
                    }
                }
            }
        }
    }

    //When a gameObject is clicked this function determines what will happen
    public bool TriggerAction(GameObject clickedCell)
    {

        if (clickedCell.GetComponent<Cell>().IsInteractable == true)
        {//blastable or explodable cell
            if (LevelManager.Instance.levelData.move_count > 0)
            {//Decrease remaining moves if the games is not over
                LevelManager.Instance.levelData.move_count--;
            }
            if (clickedCell.GetComponent<Cube>() != null)
            {//If interacted object is cube
                clickedCell.GetComponent<Cube>().Interact(connectedGroups);
            }
            else
            {//If interacted object is tnt
                clickedCell.GetComponent<TNT>().Interact();
            }
            StartCoroutine(AfterMove());  // Changed this line
            return true;
        }
        else
        {//solo cell
            Debug.Log("Invalid Tap");
            return false;
        }
    }

    //Itarate the grid and find nulls after blast or a explosion
    private void DetectNulls()
    {
        //Keep the nullIndicesByColumn for later checking null status of each column
        nullIndicesByColumn = new List<List<int>>();

        for (int i = 0; i < width; i++)
        {
            nullIndicesByColumn.Add(new List<int>());

        }
        //Itarate grid
        for (int j = 0; j < height; j++)  // rows
        {
            for (int i = 0; i < width; i++)  // columns
            {
                if (LevelManager.Instance.levelData.grid[j * width + i] == "null")
                {
                    nullIndicesByColumn[i].Add(j);
                }
            }
        }

    }

    //Fill nulls occured after blast or a explosion
    private List<int> FillNulls()
    {
        int columnIndex = 0;//i

        List<int> newCubesInColumn = new List<int>();////newly added cube number per columns 
        List<Tuple<int, int>> objectsWillMove = new List<Tuple<int, int>>();

        //Explore nulls column by column since I fill it vertically
        foreach (var nullListInColumn in nullIndicesByColumn)//nullListInColumn i
        {

            if (nullListInColumn.Count != 0)//If there is null in that column
            {

                int moveAmount = 0;   // Number of rows cell need to fall
                int newCubeNumber = 0;  // New cubes to come from above
                int currentPos = 0;   //Start from one cell above null

                // Check cells starting from 0 to height
                while (currentPos < height)
                {
                    string CellString = LevelManager.Instance.levelData.grid[currentPos * width + columnIndex];

                    if (CellString == "null")//if currrent cell is null
                    {
                        moveAmount++;//moveamount will increase to replace that null
                        newCubeNumber++;//newcubenumber will increase bcs column lack of cubes
                        currentPos++;
                    }
                    else if (CellString != "s" && CellString != "bo")//if currrent cell is a fallable object
                    {
                        // Found a movable cell
                        if (moveAmount > 0)
                        {//if seen nulls before 
                            objectsWillMove.Add(Tuple.Create(currentPos, moveAmount));//move the cell number of nulls
                        }
                        currentPos++;
                    }
                    else//if currrent cell is static
                    {
                        newCubeNumber = 0;//set newCubeNumber to 0 since nulls below it wont be fill with new cubes
                        moveAmount = 0;//set moveamount to 0 since static object will not move and also it wont let the objects above it pass
                        currentPos++;
                    }
                }
                newCubesInColumn.Add(newCubeNumber); //Add column's will be newly added cube number 

                foreach (var moveData in objectsWillMove)
                {
                    // Calculate source and target positions
                    int sourceY = moveData.Item1;
                    int targetY = sourceY - moveData.Item2;

                    // Move the cell down
                    string cellToMove = LevelManager.Instance.levelData.grid[sourceY * width + columnIndex];

                    // Assign the moved cell to the target position
                    LevelManager.Instance.levelData.grid[targetY * width + columnIndex] = cellToMove;

                    // Mark the source cell as null
                    LevelManager.Instance.levelData.grid[sourceY * width + columnIndex] = "null";

                    // Update gameobject position
                    UpdateCellVisual(sourceY, targetY, columnIndex, 0.2F);
                }
                objectsWillMove.Clear();//Clear for next iteration

            }
            else
            {
                newCubesInColumn.Add(0);
            }
            columnIndex++;
        }
        return newCubesInColumn;
    }


    // Updates the position and visual of a cell
    private void UpdateCellVisual(int sourceY, int targetY, int columnIndex, float speed)
    {
        // Set true to prevent other interactions
        isAnimating = true;

        // Adjust positioning
        float gridScalerX = boardGameObject.GetComponent<Board>().gridScalerX;
        float gridScalerY = boardGameObject.GetComponent<Board>().gridScalerY;
        float startPositionX = -((width*gridScalerX) /2f) + gridScalerX/2f;
        float startPositionY= -(height*gridScalerY)/2+ gridScalerY/2f-3;

        // Calculate the source and target positions 
        Vector2 sourcePosition = new Vector2(((columnIndex * gridScalerX) + startPositionX), ((sourceY * gridScalerY) + startPositionY));
        Vector2 targetPosition = new Vector2(((columnIndex * gridScalerX) + startPositionX), ((targetY * gridScalerY) + startPositionY));

        // Find the GameObject 
        GameObject cellObject = GameObject.Find("(" + columnIndex + " " + sourceY + ")");
        if (cellObject != null)
        {
            // Start the coroutine to move and update
            StartCoroutine(MoveAndUpdate(cellObject, sourcePosition, targetPosition, speed, columnIndex, targetY));

            // Update the name of the GameObject for its new position
            cellObject.name = "(" + columnIndex + " " + targetY + ")";
        }
    }

    // Coroutine to smoothly move the cell and update its properties
    IEnumerator MoveAndUpdate(GameObject obj, Vector3 beginPos, Vector3 endPos, float time, int columnIndex, int targetY)
    {
        float elapsedTime = 0f; // Track the elapsed time for the movement

        // Gradually move the object 
        while (elapsedTime < time)
        {
            obj.transform.position = Vector3.Lerp(beginPos, endPos, elapsedTime / time); // Interpolate the position based on time
            elapsedTime += Time.deltaTime; // Increase the elapsed time
            yield return null;
        }


        obj.transform.position = endPos;

        // Update the postion
        obj.GetComponent<Cell>().position = new Tuple<int, int>(columnIndex, targetY);

        // Set isAnimating to false to remove the block
        isAnimating = false;
    }


    //Create new cubes to fill blasted cubes and move them 
    private void CreateNewCubes(List<int> newCubesInColumn)
    {
        isAnimating = true;// Set true to prevent other interactions
        int columnNumber = 0; // X-coordinate

        // Dictionary to map color names to indices
        Dictionary<string, string> colorToIndex = new Dictionary<string, string>()
    {
        { "blue", "b" },
        { "green", "g" },
        { "red", "r" },
        { "yellow", "y" }
    };

    //Adjust positioning
    float gridScalerX = boardGameObject.GetComponent<Board>().gridScalerX;
    float gridScalerY = boardGameObject.GetComponent<Board>().gridScalerY;
    float startPositionX = -((width*gridScalerX) /2f) + gridScalerX/2f;
    float startPositionY= -(height*gridScalerY)/2+ gridScalerY/2f-3;

        // Loop through each column and the number of new cubes to create in that column
        foreach (int newCubeNumber in newCubesInColumn)
        {
            for (int x = 0; x < newCubeNumber; x++)
            {
                // Determine positions
                int targetY = height - newCubeNumber + x;
                int sourceY = height + x + 1;//spawn cubes from above

                // Calculate the position based on grid scaling
                Vector2 tempPosition = new Vector2(((columnNumber * gridScalerX) + startPositionX), ((sourceY * gridScalerY) + startPositionY));

                // Instantiate a new cube 
                GameObject currentCell = Instantiate(cubeNoColorPrefab, tempPosition, Quaternion.identity);
                Cube cubeComponent = currentCell.GetComponent<Cube>();

                // Update the GameObject name to reflect its grid position
                currentCell.name = "(" + columnNumber + " " + sourceY + ")";

                // Assign a random color and its sprite
                SpriteRenderer spriteRenderer = currentCell.GetComponent<SpriteRenderer>();
                int randomIndex = UnityEngine.Random.Range(0, 4);
                spriteRenderer.sprite = defaultSprites[randomIndex];

                // Initialize the cube component
                string color = colorToIndex[spriteRenderer.sprite.name];
                Tuple<int, int> cellPosition = new Tuple<int, int>(columnNumber, targetY);
                cubeComponent.Initialize(cellPosition, color);

                // Assign particle system based on the selected color
                currentCell.GetComponent<Cell>().particleSystemPrefab = particleList[randomIndex];

                // Animate the cube's movement from source to target
                UpdateCellVisual(sourceY, targetY, columnNumber, (sourceY - targetY) * 0.1f);

                // Update the grid data in LevelManager
                LevelManager.Instance.UpdateGrid(columnNumber, targetY, color);
            }

            columnNumber++;
        }
    }



    //Helper Functions
    public bool CheckIsCube(string gridCode)
    {
        return (gridCode == "g" || gridCode == "y" || gridCode == "b" || gridCode == "r");
    }

    public bool CheckIsTnt(string gridCode)
    {
        return (gridCode == "t");
    }


    public string GetNameByPosition(int i, int j)
    {
        string cellName = "(" + i + " " + j + ")";
        return cellName;
    }


    private Cube GetCubeByPosition(int i, int j)
    {
        string cubeName = GetNameByPosition(i, j);
        GameObject currentCube = GameObject.Find(cubeName);

        if (currentCube == null)
        {
            return null;
        }

        Cube cubeComponent = currentCube.GetComponent<Cube>();

        return cubeComponent;
    }


}
