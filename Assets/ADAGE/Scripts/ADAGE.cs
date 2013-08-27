using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;

public class UploadWrapper
{
	[SkipSerialization]
	public int Count
	{
		get
		{
			if(data == null)
				data = new List<ADAGEData>();
			
			return data.Count;		
		}
	}
	
	[SkipSerialization]
	public ADAGEData[] Items
	{
		get
		{
			if(data == null)
				data = new List<ADAGEData>();
			
			return data.ToArray();					
		}
	}	
	
	public List<ADAGEData> data;
	
	public UploadWrapper()
	{
		data = new List<ADAGEData>();
	}	
	
	public UploadWrapper(UploadWrapper copy)
	{
		data = new List<ADAGEData>();
		foreach(ADAGEData element in copy.Items)
		{
			data.Add(element);	
		}
	}
	
	public void Add(ADAGEData newData)
	{
		if(data == null)
			data = new List<ADAGEData>();
		
		if(data.Count == 0 || data[data.Count] != newData)
		{
			data.Add(newData);	
		}
	}
	
	public void Add(UploadWrapper copy)
	{
		foreach(ADAGEData element in copy.Items)
		{
			Add(element);	
		}
	}
	
	public void Clear()
	{
		if(data == null)
			data = new List<ADAGEData>();
		else
			data.Clear();
	}
}

public class ADAGE : MonoBehaviour
{
	public static string VERSION = "drunken_dolphin";
	
	public static readonly string productionURL = "https://adage.gameslearningsociety.org";
	public static readonly string developmentURL = "http://ada.dev.eriainteractive.com";
	public static readonly string stagingURL = "https://adage.gameslearningsociety.org";
		
	public static bool Staging
    {
		get
		{
			return instance.staging;
		}
    }	
	
	public static bool Online
    {
		get
		{
			return instance.isOnline;
		}
    }	
	
	public static ADAGEGameInfo GameInfo
	{
		get
		{
			return instance.gameInfo;	
		}
	}
	
	public static Dictionary<string, string> AuthenticationParameters
	{
		get 
		{
			return new Dictionary<string, string> 
			{ 
				//{ "auth_token", "7FiB9FPAxyrMGwgKiUU3" } 
				{ "auth_token", "SsTx2yApqSnev1tsXiSX" }
			};
		}
	}
	
	public static ADAGEVirtualContext VirtualContext
	{
		get
		{
			return instance.vContext;
		}
	}
	
	public static ADAGEPositionalContext PositionalContext
	{
		get
		{
			return instance.pContext;
		}
	}
	
	public bool staging = false;
	public ADAGEGameInfo gameInfo;
	public int pushRate = 5;
	
	[HideInInspector]
	public string dataPath = "";
	
	private static ADAGE instance;

	private bool isOnline = false;
	private ADAGEUser user;
	private string currentSession;
	
	private WorkerPool threads;	
	private UploadWrapper dataWrapper;
	private UploadWrapper localWrapper;
	private float lastPush = 0;  
	
	private ADAGEVirtualContext vContext;
	private ADAGEPositionalContext pContext;
	
	public static void LogData<T>(T data) where T:ADAGEData
	{
		if(ReflectionUtils.CompareType(data.GetType(), (typeof(ADAGEContext))))
		{
			string message = string.Format("ADAGE WARNING: Method 'ADAGE.LogData' should not be used to track progression object '{0}'. Please use the ADAGE.LogContext method", data.GetType().ToString());
			Debug.LogWarning(message);	
		}
		instance.AddData<T>(data);
	}
	
	public static void LogContext<T>(T data) where T:ADAGEContext
	{
		instance.AddContext<T>(data);	
	}
	
	public static void UpdatePositionalContext(Transform transform)
	{
		UpdatePositionalContext(transform.position, transform.eulerAngles);
	}

	public static void UpdatePositionalContext(Vector3 pos, Vector3 rot = new Vector3())
	{
		if(instance.pContext == null)
			instance.pContext = new ADAGEPositionalContext();
		
		instance.pContext.setPosition(pos.x, pos.y, pos.z);
		instance.pContext.setRotation(rot.x, rot.y, rot.z);
	}
	
	public void Awake()
	{
		if(instance != null)
			Debug.LogWarning("You have multiple copies of the ADAGE object running. Overriding...");
		instance = this;
		
		threads = new WorkerPool(3);
		dataWrapper = new UploadWrapper();
		
		vContext = new ADAGEVirtualContext(Application.loadedLevelName);
		
		if(dataPath == "")
		{
			dataPath = "ADAGE/Data/";	
		}
		else
		{
			if(dataPath.Substring(dataPath.Length-1) != "/")
			{
				dataPath += "/";
			}
		}
	}
	
	public void Start()
	{	
		/*Vector3 jungle = new Vector3(0, 158, 96);
		Vector3 forest = new Vector3(49, 120, 115);
		Vector3 hunter = new Vector3(53, 94, 59);
		Debug.Log (jungle.magnitude);
		Debug.Log (forest.magnitude);
		Debug.Log (hunter.magnitude);*/
		ADAGEStartGame s = new ADAGEStartGame();
		ADAGEStartGame s2 = new ADAGEStartGame();
		Debug.Log(s);
		
		//Start the session
		currentSession = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
		
		//Temp
		isOnline = true;
		
		//Push any local data to the server
		PushLocalToOnline();
				
		//Temp
		UpdatePositionalContext(Vector3.zero,Vector3.zero);
		ADAGEStartGame log_start = new ADAGEStartGame();
		LogData<ADAGEStartGame>(log_start);
	}
	
	public void Update()
	{
		CheckThreads();
	}
	
	public void FixedUpdate()
	{
		float elapsedTime = Time.time - lastPush;
		if((elapsedTime > pushRate) && dataWrapper.Count != 0)
		{
			lastPush = Time.time;
			AddLogJob();
		}
	}
	
	public void OnLevelWasLoaded(int level) 
	{
		vContext.level = Application.loadedLevelName;
	}
	
	public void OnApplicationQuit() 
	{
		ADAGEQuitGame log_quit = new ADAGEQuitGame();
		LogData<ADAGEQuitGame>(log_quit);
		
		//Stops workers from handling threads
		threads.FireWorkers();
		List<Job> remainingJobs = threads.GetRemainingJobs();
		Debug.Log ("Remaining Jobs: " + remainingJobs.Count);
		foreach(Job job in remainingJobs)
		{
			job.Main();	
			OnComplete(job);
		}
		
		if(localWrapper != null && localWrapper.Count > 0)
		{
			//Do local write
			string outgoingData = JsonMapper.ToJson(localWrapper);
			string path = Application.dataPath + dataPath + ADAGE.AuthenticationParameters["auth_token"] + "/";
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			
			System.IO.File.AppendAllText(path + currentSession + ".data", outgoingData);
		}
	}
	
	private void CheckThreads()
	{
		if(threads.jobsComplete())
		{
			Job output = Queue.Synchronized(threads.completedJobs).Dequeue() as Job;
			if(output.OnComplete != null)
				output.OnComplete(output);
		}
	}
	
	private void AddData<T>(T data) where T:ADAGEData
	{
		if(data.GetType().IsSubclassOf(typeof(ADAGEEventData)))
			(data as ADAGEEventData).Update();
		
		data.gameName = ADAGE.GameInfo.name;
		data.gameVersion = ADAGE.GameInfo.version;
		data.timestamp = System.DateTime.Now.ToUniversalTime().ToString();
		data.session_token = currentSession;
		data.key = data.GetType().ToString();
		
		data.ada_base_types = new List<string>();
		Type curType = data.GetType().BaseType;
		while(curType != typeof(System.Object))
		{
			data.ada_base_types.Add(curType.ToString());
			curType = curType.BaseType;
		}		
		
		dataWrapper.Add(data);
	}
	
	private void AddLogJob()
	{				
		ADAGEUploadJob job = new ADAGEUploadJob(dataWrapper);
		job.OnComplete = OnComplete;
		threads.AddJob(job);
		
		dataWrapper.Clear();
	}
	
	private void OnComplete(Job job)
	{
		ADAGEUploadJob upload = (job as ADAGEUploadJob);
	
		if(upload.Status != 302)  //NEEDS TO CHANGE TO 200 SOMEDAY
		{
			Debug.Log ("Adding Data to Local Wrapper");
			
			if(localWrapper == null)
				localWrapper = new UploadWrapper();
			
			localWrapper.Add(upload.GetData());
		}
		
		Debug.Log (upload.Status);	
		Debug.Log (upload.Output);	
	}
	
	private void OnLocalUploadComplete(Job job)
	{
		ADAGEUploadFileJob upload = (job as ADAGEUploadFileJob);
		
		if(upload.Status == 200)
		{
			File.Delete(upload.Path);	
		}
		
		Debug.Log (upload.Status);
		Debug.Log (upload.Output);	
	}
	
	private void AddContext<T>(T data) where T:ADAGEContext
	{
		if(data.GetType().IsAssignableFrom(typeof(ADAGEContextStart)))
		{
			StartContext(data as ADAGEContextStart);
		}
		else if(data.GetType().IsAssignableFrom(typeof(ADAGEContextEnd)))
		{
			EndContext(data as ADAGEContextEnd);			
		}		
		
		AddData<T>(data);
	}
	
	private void StartContext(ADAGEContextStart data)
	{		
		vContext.Add(data.name);
	}

	private void EndContext(ADAGEContextEnd data)
	{		
		vContext.Remove(data.name);
	}
	
	private void PushLocalToOnline()
	{	
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + dataPath);
        DirectoryInfo[] diArr = di.GetDirectories();
		
		foreach(DirectoryInfo token in diArr)
		{
			string[] files = System.IO.Directory.GetFiles(token.ToString(), "*.data");
			Debug.Log("Local File Count For " + token.Name + ": " + files.Length);
			for(int i=files.Length-1; i >= 0; i--)
			{
				Debug.Log("Reading File: " + files[i]);
				
				string outgoingData = File.ReadAllText(files[i]);
								
				ADAGEUploadFileJob job = new ADAGEUploadFileJob(token.Name, token.ToString(), outgoingData);
				job.OnComplete = OnLocalUploadComplete;
				threads.AddJob(job);		
			}
		}
	}
}