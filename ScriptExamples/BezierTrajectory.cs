using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using static UnityEngine.GraphicsBuffer;
/// <summary>
/// This script handles the launch trajectory of the ball when shot also haws a fuction to calculate the value of the accuracy stat of the
/// determine likelyhood fo miss or make
/// </summary>
public class BezierTrajectory : NetworkBehaviour
{
    [Header("Log Settings")]
    [SerializeField]
    bool _showLog;
    //Variables/Script References assigned in inspector//
    [SerializeField]
    GameObject _playerRoot;

    [SerializeField]
    GameObject _gMgr;

    [SerializeField]
    TeamGameStateHandler teamGameScript;

    [SerializeField]
    PossHandler possScript;

    [SerializeField]
    ShotFeedBackUI feedbackScript;

    [SerializeField]
    PlayerActionStates actionStatesScript;

    [SerializeField]
    ShotTypeHandler typeScript;

    [SerializeField]
    StatSheet playerStats;

    [SerializeField]                                                                                                                                         
    ShotCharge chargeScript;

    [SerializeField]
    GameObject _Bball;

    [SerializeField]

    Vector3 _endpointVector;

    [SerializeField]
    float _currentLerpTime;

    [SerializeField]
    int randomizeMissLocale;

    [SerializeField]
    GameObject _scoreZone;

    [SerializeField]
    GameObject _endPoint;

    [SerializeField]
    Vector3 _rebEndPoint;

    [SerializeField]
    float _3pointRangePenalty;

    [SerializeField]
    bool _trajLock;

    [SerializeField]
    GameObject[] _rimMarkers; 

    [Header("Various Shot Settings")]

    [SerializeField]
    float _threeStatSensitivity;
    [SerializeField]
    float _threeChargeSensitivity;
    [SerializeField]
    float _twoStatSensitivity;
    [SerializeField]
    float _twoChargeSensitivity;
    [SerializeField]
    float _layStatSensitivity;
    [SerializeField]
    float _layChargeSensitivity;
    [SerializeField]
    float _postStatSensitivity;
    
    [SerializeField]
    
    float _height = 3;
    [SerializeField]
    bool _debug;
    [SerializeField]
    float _trueAcc = 0;
    [SerializeField]
    float _gravity = -9.8f; // determines spped of shot
    [SerializeField]
    float _targetDistance;
    [SerializeField]
    int _shotangle;
    [SerializeField]
    float _flightDuration;
    [SerializeField]
    float _elapsedTime;
    [SerializeField]
    float Vx;
    [SerializeField]
    float Vy;
    [SerializeField]
    float projectile_Velocity;

    void Start()
    {
        //initial assignments udring start runtime
        _gMgr = GameObject.FindGameObjectWithTag("GM");
        teamGameScript = _gMgr.GetComponent<TeamGameStateHandler>();
        _endPoint = null;
        _height = 3;
         _rimMarkers = GameObject.FindGameObjectsWithTag ("RimMark");
        _scoreZone = GameObject.FindGameObjectWithTag("SZone");
        Logger.Log("Num of total objects:" + _rimMarkers.Length, _showLog);
        _Bball = GameObject.FindGameObjectWithTag("ball");
        Physics.gravity = Vector3.up * _gravity;

    }


    void FixedUpdate()
    {
        //checks if the player is shooting to start process
        if (base.IsOwner && actionStatesScript.getShootState().getIsState())
        {
            Debug.Log("this is shooter");
            PhysicsLaunch();
            //TransRigidSync();
        }
    }
    // Launches the ball with plhysics once the ball is in the launched state and sends command to server to tell the client that it is launched
    void PhysicsLaunch() {
        
        if (_Bball.GetComponent<BallLaunchState>().getLaunchState() && !_trajLock) {
            _Bball.GetComponent<Rigidbody>().isKinematic = false;
            Debug.Log("running bezier on this client");
            //_Bball.GetComponent<BallPhysics>().ServerRPCBallGravityToggle(true);
            _Bball.GetComponent<Rigidbody>().useGravity = true;
            _Bball.GetComponent<BallPhysics>().ServerRPCBallColliderTriggerToggle(false);
            _Bball.GetComponent<BallLaunchState>().ServerRpcStopBallLaunch();
            
            Launch();
        }
    }

    [ObserversRpc]
    void ObserversRpcToggleTrigger()
    {
        _Bball.GetComponent<SphereCollider>().isTrigger = false;
    }

    
    public void Launch()
    {
            
            _trajLock = true;
            Rigidbody ballrb = _Bball.GetComponent<Rigidbody>();
            ballrb.velocity = CalculateLaunchData().initialVelocity;
        

    }

    // Credit to Sebsatian Lague method from Youtube for kinematic equations that determines lauch trajectory with a drawpath function implemented
    LaunchData CalculateLaunchData()
    {
        Rigidbody ballrb = _Bball.GetComponent<Rigidbody>();
        Debug.Log("Times:");
        float displacementY = _scoreZone.transform.position.y - ballrb.position.y;
        Vector3 displacementXZ = new Vector3(_scoreZone.transform.position.x - ballrb.position.x, 0, _scoreZone.transform.position.z - ballrb.position.z);
        float time = Mathf.Sqrt(-2 * _height / _gravity) + Mathf.Sqrt(2 * (displacementY - _height) / _gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * _gravity * _height);
        Vector3 velocityXZ = displacementXZ / time;
        Debug.Log(velocityXZ + velocityY * -Mathf.Sign(_gravity));
        return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(_gravity), time);
    }
    
    struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }

    }

    void DrawPath()
    {
        LaunchData launchData = CalculateLaunchData();
        Vector3 previousDrawPoint = _Bball.transform.position;

        int resolution = 30;
        for (int i = 1; i <= resolution; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * _gravity * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = _Bball.transform.position + displacement;
            Debug.DrawLine(previousDrawPoint, drawPoint, Color.green);
            previousDrawPoint = drawPoint;
        }
    }

    [ServerRpc]
    void sendBackBallPosition(Vector3 ballPosiiton) {
        _Bball.transform.position = ballPosiiton;
    }

    /*Server Only fuction that determines the accuracy of the shot depending on player stats,opposing contest value, 
     * the type of shot, if the shots is a 2 or 3 , and
    other factors*/
    [Server]
    public void returnCalcEndPoint(float charge, int shotScore, int shotMod, float contest)
    {

        Logger.Log("contest" + contest, _showLog);

        switch (shotScore)
        {


            case 2:
                switch (shotMod)
                {
                    case -1:
                    case 1:
                        Logger.Log("Layup: 2pt", _showLog);
                        _trueAcc = (charge * _layChargeSensitivity) + playerStats.getLayupVal().getBaseValue() * _layStatSensitivity + contest;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;
                    case 2:
                        Logger.Log("Set:2pt", _showLog);
                        _trueAcc = (charge * _twoChargeSensitivity) + playerStats.getTwoPointVal().getBaseValue() * _twoStatSensitivity + contest;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;
                    case 3:
                        Logger.Log("Drift:2pt", _showLog);
                        _trueAcc = (charge * _twoChargeSensitivity) + playerStats.getTwoPointVal().getBaseValue() * _twoStatSensitivity + contest;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;
                    case 4:
                        Logger.Log("Fade:2pt", _showLog);
                        _trueAcc = (charge * _twoChargeSensitivity) + playerStats.getTwoPointVal().getBaseValue() * _twoStatSensitivity + contest;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;

                    case 7:
                        Logger.Log("PostFade:2pt", _showLog);
                        _trueAcc = (charge * _twoChargeSensitivity) + playerStats.getTwoPointVal().getBaseValue() * _twoStatSensitivity / 10 + playerStats.getPostValue().getBaseValue() * _postStatSensitivity + contest;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;
                }
                break;
            case 3:
                switch (shotMod)
                {
                    case 2:
                        Logger.Log("Set:3pt", _showLog);
                        _trueAcc = (((charge * _threeChargeSensitivity) + playerStats.getThreePointVal().getBaseValue() * _threeStatSensitivity) + contest) - _3pointRangePenalty;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;
                    case 3:
                        Logger.Log("Drift:3pt", _showLog);
                        _trueAcc = (((charge * _threeChargeSensitivity) + playerStats.getThreePointVal().getBaseValue() * _threeStatSensitivity) + contest) - _3pointRangePenalty;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;
                    case 4:
                        Logger.Log("Fade:3pt", _showLog);
                        _trueAcc = (((charge * _threeChargeSensitivity) + playerStats.getThreePointVal().getBaseValue() * _threeStatSensitivity) + contest) - _3pointRangePenalty;

                        Logger.Log("Accuracy:" + _trueAcc, _showLog);
                        break;
                }
                break;
        }

        if (chargeScript.getFailedShotStatus())
        {
            _trueAcc = 5;
        }

        if (chargeScript.getFailedShotStatus() && typeScript.getOutOf3RangeStatus())
        {
            _trueAcc = 1;
        }

        if (_trueAcc < 0)
        {
            _trueAcc = 0;
        }

        if (Random.value * 100 <= _trueAcc)
        {


            _endPoint = _scoreZone;
            ObserversRPCSyncEndpoint(_scoreZone);
            Logger.Log("ScoreZone", _showLog);
            
        }
        else
        {
            
            _endPoint = calcRimTarget();
            ObserversRPCSyncEndpoint(_endPoint);
            
            

        }

    }

    GameObject calcRimTarget(){
         randomizeMissLocale = Random.Range(0, _rimMarkers.Length);
         return _rimMarkers[randomizeMissLocale];
    }
    //Gives value to single client
    [TargetRpc]
    public void TargetRPCSyncRebEndpoint(NetworkConnection conn, Vector3 reboundpoint)
    {
        Logger.Log("running rebound endpint rpc on client", _showLog);
        _rebEndPoint = reboundpoint;
    }
    //Gives value to all clients
    [ObserversRpc]
    public void ObserversRPCSyncEndpoint(GameObject endpoint)
    {
        Logger.Log("Setting the endopint to:" + endpoint,_showLog);
        _endPoint = endpoint;
    }


    public void set3PointRangePenalty(int val)
    {
        _3pointRangePenalty = val;
    }

    public void setCurrentLerpTime(float counter)
    {
        _currentLerpTime = counter;
    }

    public void setTrajLock(bool status) {
        _trajLock = status;
    }
    public int getTrueAcc()
    {
        return (int)_trueAcc;
    }
}