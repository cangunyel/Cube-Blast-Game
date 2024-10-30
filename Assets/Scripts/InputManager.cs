using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    Camera main_camera; // Reference to the main camera
    public GameObject gameManagerGameObject; // Reference to the GameManager object

    
    void Awake()
    {
        // Set main_camera to the camera tagged as "MainCamera"
        main_camera = Camera.main;
    }

    // Method that handles input events
    public void OnClick(InputAction.CallbackContext context)
    {   
        // Check if GameManager is not animating or not
        if (context.started && !gameManagerGameObject.GetComponent<GameManager>().isAnimating)
        {
            // Start the coroutine to handle the click action
            StartCoroutine(HandleClick());
        }
    }

    // Coroutine to handle the click logic
    private IEnumerator HandleClick()
    {
        if(LevelManager.Instance.isPlaying){
        // Hitted raycast
        var rayHit = Physics2D.GetRayIntersection(main_camera.ScreenPointToRay(Mouse.current.position.ReadValue()));
        
        // If no collider was hit by the ray, stop execution
        if (!rayHit.collider) yield break;

        // Trigger an action on the clicked object
        if(!gameManagerGameObject.GetComponent<GameManager>().TriggerAction(rayHit.collider.gameObject)){
            yield break; // If the clik was invalid
        }

        // call AfterMove to handle post-move operations
        gameManagerGameObject.GetComponent<GameManager>().AfterMove();
    }
    }
}
