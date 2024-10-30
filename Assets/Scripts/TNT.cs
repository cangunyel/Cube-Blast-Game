using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;


public class TNT : Cell
{
    public int explosionRadius;
    private bool hasExploded = false;

    public ParticleSystem comboTntParticleSystemPrefab;

    public TNT()
    {
        //Cell attributes
        IsInteractable = true;
        DoesFall = true;

        //TNT attributes
        explosionRadius = 5;
    }


    public void Interact()
    {
        //Get data
        int GridWidth = LevelManager.Instance.levelData.grid_width;
        int GridHeight = LevelManager.Instance.levelData.grid_height;

        List<GameObject> surroundingTnts = CheckSurroundingTNT(this, GridWidth, GridHeight);//Is there any tnt in explosion radius

        if ((surroundingTnts.Count == 0))
        {//solo tnt
            this.Explode(this);
        }
        else
        {//if there is another tnt in range 
            this.explosionRadius = 7;
            this.GetComponent<TNT>().particleSystemPrefab = comboTntParticleSystemPrefab;//switch to combo particle effect
            this.Explode(this);
        }
    }

    private List<GameObject> CheckSurroundingTNT(TNT currentTNT, int gridWidth, int gridHeight)
    {
        // Get the current position
        int i = currentTNT.GetPositionI();
        int j = currentTNT.GetPositionI();

        // Create a list to hold the surrounding TNT GameObjects
        List<GameObject> surroundingTNTs = new List<GameObject>();

        // Check each surrounding position within boundaries
        if (j + 1 < gridHeight) // Above
        {
            string aboveName = "(" + i + " " + (j + 1) + ")";

            CheckIfTNT(aboveName, surroundingTNTs);
        }

        if (j - 1 >= 0) // Below
        {
            string belowName = "(" + i + " " + (j - 1) + ")";
            CheckIfTNT(belowName, surroundingTNTs);
        }

        if (i - 1 >= 0) // Left
        {
            string leftName = "(" + (i - 1) + " " + j + ")";
            CheckIfTNT(leftName, surroundingTNTs);
        }

        if (i + 1 < gridWidth) // Right
        {
            string rightName = "(" + (i + 1) + " " + j + ")";
            CheckIfTNT(rightName, surroundingTNTs);
        }

        return surroundingTNTs; // Return the list of surrounding TNT
    }

    //Helper Function for CheckSurroundingTNT
    private void CheckIfTNT(string objectName, List<GameObject> tntList)
    {
        // Find the GameObject 
        GameObject obj = GameObject.Find(objectName);

        // Check if the TNT exists
        if (obj != null && obj.GetComponent<TNT>() != null)
        {
            tntList.Add(obj); //add to tntList
        }
    }
    public void Explode(TNT currentTNT)
    {
        ActivateParticles();//activate particle system

        // Prevent re-explosion of the same TNT
        if (hasExploded) return;

        hasExploded = true;

        //Get position and grid data
        int marginNumber = (explosionRadius - 1) / 2;
        int currentTNTi = currentTNT.GetPositionI();
        int currentTNTj = currentTNT.GetPositionJ();
        int width = LevelManager.Instance.levelData.grid_width;
        int height = LevelManager.Instance.levelData.grid_height;

        //Set the range of explosion (I get max and mins to stay within grid)
        int iCoverLowerLimit = Math.Max((currentTNTi - marginNumber), 0);
        int iCoverUpperLimit = Math.Min((currentTNTi + marginNumber), width);
        int jCoverLowerLimit = Math.Max((currentTNTj - marginNumber), 0);
        int jCoverUpperLimit = Math.Min((currentTNTj + marginNumber), height);

        // Create a list to store TNTs that need to explode
        List<TNT> tntToExplode = new List<TNT>();

        //Iterate explosion area
        for (int i = iCoverLowerLimit; i <= iCoverUpperLimit; i++)
        {
            for (int j = jCoverLowerLimit; j <= jCoverUpperLimit; j++)
            {
                //Get the gameobject at the position
                string cellName = "(" + i + " " + j + ")";
                GameObject currentCell = GameObject.Find(cellName);

                if (currentCell == null) continue;  // Skip if cell not found

                if (CheckIsCube(currentCell))//if cube, blast
                {
                    currentCell.GetComponent<Cube>().Blast();
                }
                else if (CheckIsObstacle(currentCell))//if obstacle, take damage
                {
                    currentCell.GetComponent<Obstacle>().TakeDamage();
                }
                else if (CheckIsTNT(currentCell))//if tnt, exlpode
                {
                    TNT tnt = currentCell.GetComponent<TNT>();
                    if (!tnt.hasExploded)  // Only add if not already exploded
                    {
                        tntToExplode.Add(tnt);
                    }
                }
            }
        }

        // Trigger other TNT explosions after the loop
        foreach (TNT tnt in tntToExplode)
        {
            tnt.Explode(tnt);
        }
        // Update grid
        transform.DOPunchScale(0.3F*Vector3.one,0.5F,1,0).OnComplete(
        ()=>{
            Destroy(gameObject);

        }
        );
        LevelManager.Instance.UpdateGrid(currentTNTi, currentTNTj, "null");

    }

    //Helper Functions
    private bool CheckIsCube(GameObject currentGameObject)
    {
        return (currentGameObject.GetComponent<Cube>() != null);
    }
    private bool CheckIsObstacle(GameObject currentGameObject)
    {
        return (currentGameObject.GetComponent<Obstacle>() != null);
    }
    private bool CheckIsTNT(GameObject currentGameObject)
    {
        return (currentGameObject.GetComponent<TNT>() != null);
    }

}
