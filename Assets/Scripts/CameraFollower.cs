using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    Transform player;
    Camera mainCam;
    public BoxCollider2D currentBound;
    BoxCollider2D closestBound;
    public List<BoxCollider2D> bounds;
    float YOFFSET;
    const float XOFFSET = 13.3f / 2f;
    const float SMOOTHCAMVAR = 5f;

    private void Awake()
    {
        // Get references to global members
        player = GameObject.Find("Player").GetComponent<Transform>();
        mainCam = Camera.main;
    }

    private void Start()
    {
        YOFFSET = mainCam.orthographicSize;

        // Get all the gameobjects with the CameraBond tag
        GameObject[] camBonds = GameObject.FindGameObjectsWithTag("CameraBond");

        // Get the BoxCollider2D component of each and store them in a List
        for (int i = 0; i < camBonds.Length; ++i)
        {
            bounds.Add(camBonds[i].GetComponent<BoxCollider2D>());
        }
    }
    private void LateUpdate()
    {
        // If there was no camera bound found yet
        if (currentBound == null)
            return;
        
        // Calculate the positions of the edge of the camera relative to the player
        float playerLeftEdge = player.position.x - XOFFSET;
        float playerRightEdge = player.position.x + XOFFSET;

        // Calculate the current bounds left and right edge again... (maybe return them with the BoxCollider2D in GetCurrentBound()?)
        float boundLeftEdge = currentBound.transform.position.x + currentBound.offset.x - currentBound.size.x / 2f;
        float boundRightEdge = currentBound.transform.position.x + currentBound.offset.x + currentBound.size.x / 2f;
        Vector3 newPos;

        // Should stop moving more towards left
        if (playerLeftEdge <= boundLeftEdge) {
            newPos = Vector3.Lerp(mainCam.transform.position, new Vector3(boundLeftEdge + XOFFSET, currentBound.transform.position.y, -10f), Time.deltaTime * SMOOTHCAMVAR);
        }
        // Should stop moving more towards right
        else if (playerRightEdge >= boundRightEdge)
        {
            newPos = Vector3.Lerp(mainCam.transform.position, new Vector3(boundRightEdge - XOFFSET, currentBound.transform.position.y, -10f), Time.deltaTime * SMOOTHCAMVAR);
        }
        else
        {
            float newY;
            float newX;

            // If the player is currently transitioning between two bounds (apply interpolation)
            if (mainCam.transform.position.y != currentBound.transform.position.y)
            {
                newY = Mathf.Lerp(mainCam.transform.position.y, currentBound.transform.position.y, Time.deltaTime * SMOOTHCAMVAR);
                newX = Mathf.Lerp(mainCam.transform.position.x, player.position.x, Time.deltaTime * SMOOTHCAMVAR * 1.5f);
            }
            else
            {
                newY = currentBound.transform.position.y;
                newX = player.position.x;
            }
            newPos = new Vector3(newX, newY, -10f);
        }
        // Apply the new camera position
        mainCam.transform.position = newPos;
    }

    private void FixedUpdate()
    {
        currentBound = GetCurrentBound(player);
    }

    BoxCollider2D GetCurrentBound(Transform player) // Gets the camera bound of which the player is currently within
    {
        // Loop through all Camera bounds
        for (int i = 0; i < bounds.Count; ++i)
        {
            // Udregn positionerne af det nuv�rende bound's kanter
            float bPlusX = bounds[i].transform.position.x + bounds[i].size.x / 2 + bounds[i].offset.x;
            float bNegX = bounds[i].transform.position.x + bounds[i].offset.x - bounds[i].size.x / 2;
            float bPlusY = bounds[i].transform.position.y + bounds[i].size.y / 2 + bounds[i].offset.y;
            float bNegY = bounds[i].transform.position.y + bounds[i].offset.y - bounds[i].size.y / 2;
            
            // Check if player is inside the dimensions of the currently checkin camera bound
            if ((player.position.x >= bNegX && player.position.x <= bPlusX) && (player.position.y >= bNegY && player.position.y <= bPlusY))
                return bounds[i];
        }

        // If the player is not able to fit in any bound just return the players transform
        return player.GetComponent<BoxCollider2D>();
    }
}
