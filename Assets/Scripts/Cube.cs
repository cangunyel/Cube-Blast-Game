using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;
public class Cube : Cell
{
    public string color;

    public Sprite tntSprite;

    public ParticleSystem tntParticleSystemPrefab;

    public ParticleSystem comboTntParticleSystemPrefab;


    public Cube()
    {
        //Cell attributes

        IsInteractable = false;
        DoesFall = true;

    }

    public void Initialize(Tuple<int, int> position, string color)
    {
        base.Initialize(position);

        // Cube-specific initialization
        this.color = color;
    }


    public void Interact(List<List<Cube>> connectedGroups)
    {
        //Get required data
        List<Cube> connectedCubes = GetGroupContainingCube(this, connectedGroups);
        int GridWidth = LevelManager.Instance.levelData.grid_width;
        int GridHeight = LevelManager.Instance.levelData.grid_height;

        if (connectedCubes.Count >= 5)
        {//if cube can be turn to TNT
            this.SwitchToTnt();
            connectedCubes.Remove(this);//removing this cube from connectedCubes since it is tnt now
            CheckSurroundingObstacles(this, GridWidth, GridHeight);//Surrounding objects of clicked cell
        }

        foreach (Cube cube in connectedCubes)
        {
            cube.Blast();//Blast each connected cells            
            CheckSurroundingObstacles(cube, GridWidth, GridHeight);//Surrounding objects of connected cells
        }

        LevelManager.Instance.surroundingObstacles = LevelManager.Instance.surroundingObstacles.Distinct().ToList();//Remove if there is duplicates
        AffectSurroundings();//Afffect unique surroundings of group

    }

    private void AffectSurroundings()
    {
        foreach (GameObject obstacle in LevelManager.Instance.surroundingObstacles)
        {
            obstacle.GetComponent<Obstacle>().TakeDamage();//Call TakeDamage for each obstacle
        }

        LevelManager.Instance.surroundingObstacles.Clear();//Clear the list for next iteration
    }

    public void Blast()
    {
        transform.DOPunchScale(0.3F*Vector3.one,0.5F,1,0).OnComplete(
        ()=>{ActivateParticles();//Activate particle effect sytem
        Destroy(this.gameObject);//Destroy the object
        }
        );
        LevelManager.Instance.UpdateGrid(this.GetPositionI(), this.GetPositionJ(), "null");//Set its position null
        

    }
    private void CheckSurroundingObstacles(Cube currentCube, int gridWidth, int gridHeight) // vase and box only
    {
        // Get the current position
        int i = currentCube.GetPositionI();
        int j = currentCube.GetPositionJ();

        // Check each surrounding position within boundaries
        if (j + 1 < gridHeight) // Above
        {
            string aboveName = "(" + i + " " + (j + 1) + ")";

            CheckIfObstacles(aboveName);
        }

        if (j - 1 >= 0) // Below
        {
            string belowName = "(" + i + " " + (j - 1) + ")";
            CheckIfObstacles(belowName);
        }

        if (i - 1 >= 0) // Left
        {
            string leftName = "(" + (i - 1) + " " + j + ")";
            CheckIfObstacles(leftName);
        }

        if (i + 1 < gridWidth) // Right
        {
            string rightName = "(" + (i + 1) + " " + j + ")";
            CheckIfObstacles(rightName);
        }
    }
    //Helper Function for CheckSurroundingObstacles
    private void CheckIfObstacles(string objectName)
    {
        // Find the GameObject
        GameObject obj = GameObject.Find(objectName);

        if (obj != null && ((obj.GetComponent<Box>() != null) || (obj.GetComponent<Vase>() != null)))//fallable obstacles 
        {
            LevelManager.Instance.surroundingObstacles.Add(obj); //Add to the surroundingObstacles list
        }
    }


    private void SwitchToTnt()
    {//eğer komşu sayısı 5ten fazlaysa tnt sprite renderı explosive geçirecekler
        //getting current GameObject and its position
        GameObject tempGameObject = this.gameObject;
        Tuple<int, int> cellPosition = tempGameObject.GetComponent<Cube>().position;

        //Remove Cube component
        Destroy(tempGameObject.GetComponent<Cube>());

        //Initialize TNT componenet
        TNT tntComponent = tempGameObject.AddComponent<TNT>();
        tntComponent.Initialize(cellPosition);
        SpriteRenderer spriteRenderer = tempGameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = tntSprite;
        tempGameObject.GetComponent<TNT>().particleSystemPrefab = tntParticleSystemPrefab;
        tempGameObject.GetComponent<TNT>().comboTntParticleSystemPrefab = comboTntParticleSystemPrefab;

        LevelManager.Instance.UpdateGrid(this.GetPositionI(), this.GetPositionJ(), "t");//update grid

    }

    //Returns list of cubes in the group
    private List<Cube> GetGroupContainingCube(Cube cube, List<List<Cube>> connectedGroups)
    {
        foreach (List<Cube> group in connectedGroups)
        {
            if (group.Contains(cube))
            {
                return group;
            }
        }
        return null;
    }
}
