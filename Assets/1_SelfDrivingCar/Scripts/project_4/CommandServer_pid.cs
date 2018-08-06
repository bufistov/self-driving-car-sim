﻿using UnityEngine;
using System.Collections.Generic;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using System;
using UnityEngine.SceneManagement;

public class CommandServer_pid : MonoBehaviour
{
    public CarRemoteControl CarRemoteControl;
    public Camera FrontFacingCamera;
    private SocketIOComponent _socket;
    private CarController _carController;
    private WaypointTracker_pid wpt;

    // Use this for initialization
    void Start()
    {
        _socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        _socket.On("open", OnOpen);
        _socket.On ("reset", OnReset);
        _socket.On("steer", OnSteer);
        _socket.On("manual", onManual);
        _carController = CarRemoteControl.GetComponent<CarController>();
        _carController.RandomInitPosition = false;
        wpt = new WaypointTracker_pid ();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnOpen(SocketIOEvent obj)
    {
        Debug.Log("Connection Open");
    }

    //
    void onManual(SocketIOEvent obj)
    {
        Debug.Log("Manual driving event ...");
        EmitTelemetry (obj);
    }

    void OnReset(SocketIOEvent obj)
    {
        SceneManager.LoadScene("LakeTrack");
        Debug.Log("Reseting simulator ...");
    }

    void OnSteer(SocketIOEvent obj)
    {
        Debug.Log("Steering data event ...");
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
            print("Attempting to Send...");
            // send only if it's not being manually driven
            if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
                _socket.Emit("telemetry", new JSONObject());
            } else {
                // Collect Data from the Car
                Dictionary<string, JSONObject> data = new Dictionary<string, JSONObject>();
                var cte = wpt.CrossTrackError (_carController);
                data["steering_angle"] = new JSONObject(_carController.CurrentSteerAngle);
                data["throttle"] = new JSONObject(_carController.AccelInput);
                data["speed"] = new JSONObject(_carController.CurrentSpeed);
                data["cte"] = new JSONObject(cte);
                Dictionary<string, string> images = new Dictionary<string, string>();
                images["camera0"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
                data["images"] = new JSONObject(images);
                _socket.Emit("telemetry", new JSONObject(data));
            }
        });
    }
}
