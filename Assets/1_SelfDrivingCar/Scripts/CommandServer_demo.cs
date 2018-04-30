using UnityEngine;
using System.Collections.Generic;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using System;
using UnityEngine.SceneManagement;

public class CommandServer_demo : MonoBehaviour
{
	public CarRemoteControl CarRemoteControl;
	public Camera FrontFacingCamera;
	public Camera NorthCamera;
	public Camera SouthCamera;
	public Camera EastCamera;
	public Camera WestCamera;
	private SocketIOComponent _socket;
	private CarController _carController;

	// Use this for initialization
	void Start()
	{
		NorthCamera.targetTexture = new RenderTexture(320, 160, 24);
		SouthCamera.targetTexture = new RenderTexture (320, 160, 24);
		EastCamera.targetTexture = new RenderTexture (320, 160, 24);
		WestCamera.targetTexture = new RenderTexture (320, 160, 24);
		_socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
		_socket.On("open", OnOpen);
		_socket.On ("reset", OnReset);
		_socket.On("steer", OnSteer);
		_socket.On("manual", onManual);
		_carController = CarRemoteControl.GetComponent<CarController>();
		//wpt = new WaypointTracker_pid ();
	}

	// Update is called once per frame
	void Update()
	{
	}

	void OnOpen(SocketIOEvent obj)
	{
		Debug.Log("Connection Open");
		EmitTelemetry(obj);
	}

	// 
	void onManual(SocketIOEvent obj)
	{
        Debug.Log("Manual driving event ...");
		EmitTelemetry (obj);
	}

	void OnReset(SocketIOEvent obj)
	{
		SceneManager.LoadScene("LakeTrackAutonomous_pid");
		Debug.Log("Reseting simulator ...");
		EmitTelemetry (obj);
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
				Dictionary<string, string> data = new Dictionary<string, string>();
				//var cte = wpt.CrossTrackError (_carController);
				data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
				data["throttle"] = _carController.AccelInput.ToString("N4");
				data["speed"] = _carController.CurrentSpeed.ToString("N4");
				data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
				data["north"] = Convert.ToBase64String(CameraHelper.CaptureFrame(NorthCamera));
				data["south"] = Convert.ToBase64String(CameraHelper.CaptureFrame(SouthCamera));
				data["east"] = Convert.ToBase64String(CameraHelper.CaptureFrame(EastCamera));
				data["west"] = Convert.ToBase64String(CameraHelper.CaptureFrame(WestCamera));
				_socket.Emit("telemetry", new JSONObject(data));
			}
		});
				
	}
}