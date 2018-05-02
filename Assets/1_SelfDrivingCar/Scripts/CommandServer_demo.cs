using System;
using UnityEngine;
using System.Collections.Generic;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.SceneManagement;

public class CommandServer_demo : MonoBehaviour
{
	public CarRemoteControl CarRemoteControl;
	public Camera NorthCamera;
	public Camera SouthCamera;
	public Camera EastCamera;
	public Camera WestCamera;
	private SocketIOComponent _socket;
	private CarController _carController;
	private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

	void Awake() {
		int width = 320;
		int height = 160;
		NorthCamera.targetTexture = new RenderTexture(width, height, 24);
		SouthCamera.targetTexture = new RenderTexture (width, height, 24);
		EastCamera.targetTexture = new RenderTexture (width, height, 24);
		WestCamera.targetTexture = new RenderTexture (width, height, 24);
		_carController = CarRemoteControl.GetComponent<CarController>();
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
		SceneManager.LoadScene("SonyDemo");
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
				stopWatch.Stop();
				print("Attempting to Send frame " + stopWatch.ElapsedMilliseconds);
				stopWatch.Start();
				// Collect Data from the Car
				Dictionary<string, string> data = new Dictionary<string, string>();
				data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
				data["throttle"] = _carController.AccelInput.ToString("N4");
				data["speed"] = _carController.CurrentSpeed.ToString("N4");
				data["cte"] = "0.0";
				data["north"] = Convert.ToBase64String(CameraHelper.CaptureFrame(NorthCamera));
				data["south"] = Convert.ToBase64String(CameraHelper.CaptureFrame(SouthCamera));
				data["east"] = Convert.ToBase64String(CameraHelper.CaptureFrame(EastCamera));
				data["west"] = Convert.ToBase64String(CameraHelper.CaptureFrame(WestCamera));
					stopWatch.Stop();
				Debug.Log("Sending frame at " + stopWatch.ElapsedMilliseconds);
				_socket.Emit("telemetry", new JSONObject(data));
					stopWatch.Start();
		});
				
	}
}