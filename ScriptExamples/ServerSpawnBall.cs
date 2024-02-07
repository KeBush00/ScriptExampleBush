using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet;

/// <summary>
/// This script simply spawns a ball gameobject on server start
/// </summary>
public class ServerSpawnBall : NetworkBehaviour
{
[Header("Log Settings")]
    [SerializeField]
    bool _showLog;

    [SerializeField]
    GameObject _ball; //the gameobject of the ball set in the inspector

    [SerializeField]
    GameObject _ballSpawner;// in scene gameobject that determines position of ball when spawned

    // Start is called before the first frame update
    public  override void OnStartServer()
    {
        base.OnStartServer();
        Logger.Log("spawning ball",_showLog);
        GameObject spawnedBall = Instantiate(_ball,_ballSpawner.transform.position,Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(spawnedBall);
    }


}

