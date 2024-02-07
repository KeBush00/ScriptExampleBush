using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    /// <summary>
    /// This Script allows for the camera to soothly follow the player with some delay which is why the main functionality is included
    /// in LateUpdate
    /// </summary>
    [SerializeField]
    Transform _playerPosition;

    [SerializeField]
    Camera playerCamera;

    [Header("Camera Settings")]
    [SerializeField]
    Vector3 _offset; // the intital offset the camera will have to give some distance from the player
    [SerializeField]
    float panFollowCutOff; //how far to follow the player position for left and right movement
    [SerializeField]
    float zoomInCutOff;//how far to zoom in
    [SerializeField]
    float angle; //the x angle of the camera for up and down adjustments
    [SerializeField]
    private float smoothZTime; // higher number means longer to track player
    [SerializeField]
    private float smoothXTime; // higher number means longer to track player
    [SerializeField]
    private float smoothResetTime;// the time it takes for the camera to go to normal position
    [SerializeField]
    bool _testbool;
    [SerializeField]
    bool _facingNorth;// is the gameobject facing north?
    //Vector3 smoothedPosition;

    Vector3 targetPosition;//What is being tracked


    Vector3 _currentVelocity = Vector3.zero;

   
    void Start()
    {
        playerCamera = Camera.main;
        playerCamera.transform.eulerAngles = new Vector3(angle, playerCamera.transform.eulerAngles.y, playerCamera.transform.eulerAngles.z);
        playerCamera.transform.position = _offset;
    }

    
    void LateUpdate() { 
        if(base.IsOwner && base.IsClientOnly){
            
            transform.eulerAngles = new Vector3(angle, playerCamera.transform.eulerAngles.y, playerCamera.transform.eulerAngles.z);

            Vector3 targetPosition = _playerPosition.position + _offset;
            //targetPosition.x = targetPosition.x / 1.15f;
            Vector3 smoothedZPosition = Vector3.SmoothDamp(playerCamera.transform.position, targetPosition, ref _currentVelocity, smoothZTime);
            Vector3 smoothedXPosition = Vector3.SmoothDamp(playerCamera.transform.position, targetPosition, ref _currentVelocity, smoothXTime);
        

            playerCamera.transform.position = new Vector3(playerCamera.transform.position.x, _offset.y, smoothedZPosition.z);
            playerCamera.transform.position = new Vector3(smoothedXPosition.x, _offset.y, playerCamera.transform.position.z);
            playerCamera.transform.rotation = Quaternion.Euler(playerCamera.transform.rotation.eulerAngles.x,0f, playerCamera.transform.rotation.eulerAngles.z);
 
            //Camera Mirror Not Necessary till 5v5 or at least playable other side of court

        }
    }

    public bool getFaceNorth()//Getter
    {
        return _facingNorth;
    }
}
