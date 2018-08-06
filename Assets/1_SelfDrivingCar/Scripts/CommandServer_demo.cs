using System;
using UnityEngine;
using System.Collections.Generic;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.SceneManagement;

public class CommandServer_demo : MonoBehaviour
{
    public Boolean RandomInitPosition;
    public CarRemoteControl CarRemoteControl;
    public Camera NorthCamera;
    public Camera SouthCamera;
    public Camera EastCamera;
    public Camera WestCamera;
    private SocketIOComponent _socket;
    private CarController _carController;
    private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    void Awake() {
        int width = 160;
        int height = 120;
        NorthCamera.targetTexture = new RenderTexture(width, height, 24);
        SouthCamera.targetTexture = new RenderTexture (width, height, 24);
        EastCamera.targetTexture = new RenderTexture (width, height, 24);
        WestCamera.targetTexture = new RenderTexture (width, height, 24);
        _carController = CarRemoteControl.GetComponent<CarController>();
        _carController.RandomInitPosition = RandomInitPosition;
    }

    // Use this for initialization
    void Start()
    {
        _socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        _socket.On("open", OnOpen);
        _socket.On ("reset", OnReset);
        _socket.On ("steer", OnSteer);
        stopWatch.Start();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnOpen(SocketIOEvent obj)
    {
        Debug.Log("Connection Open");
    }

    void OnReset(SocketIOEvent obj)
    {
        // UnityMainThreadDispatcher.Instance ().Clear ();
        SceneManager.LoadScene(SceneManager.GetActiveScene ().name);
        Debug.Log("Reseting simulator ...");
    }

    void OnSteer(SocketIOEvent obj)
    {
        stopWatch.Stop ();
        Debug.Log("Steering data event ... at " + stopWatch.ElapsedMilliseconds);
        stopWatch.Start ();
        JSONObject jsonObject = obj.data;
        CarRemoteControl.SteeringAngle = float.Parse(jsonObject.GetField("steering_angle").str);
        CarRemoteControl.Acceleration = float.Parse(jsonObject.GetField("throttle").str);
        var steering_bias = 1.0f * Mathf.Deg2Rad;
        CarRemoteControl.SteeringAngle += steering_bias;
        EmitTelemetry(obj);
    }

    void EmitTelemetry(SocketIOEvent obj)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // Collect Data from the Car
            Dictionary<string, JSONObject> data = new Dictionary<string, JSONObject>();
            data["steering_angle"] = new JSONObject(_carController.CurrentSteerAngle);
            data["throttle"] = new JSONObject(_carController.AccelInput);
            data["speed"] = new JSONObject(_carController.CurrentSpeed);
            data["cte"] = new JSONObject(0.0f);
            Dictionary<string, string> images = new Dictionary<string, string>();
            images["camera0"] = Convert.ToBase64String(CameraHelper.CaptureFrame(NorthCamera));
            images["camera1"] = Convert.ToBase64String(CameraHelper.CaptureFrame(EastCamera));
            images["camera2"] = Convert.ToBase64String(CameraHelper.CaptureFrame(SouthCamera));
            images["camera3"] = Convert.ToBase64String(CameraHelper.CaptureFrame(WestCamera));
            data["images"] = new JSONObject(images);
            stopWatch.Stop();
            Debug.Log("Sending frame at " + stopWatch.ElapsedMilliseconds);
            _socket.Emit("telemetry", new JSONObject(data));
            stopWatch.Start();
        });
    }
}
