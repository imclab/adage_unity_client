#define FACEBOOK_SUPPORT 
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;

public class ADAGEAccessTokenResponse
{
	public string access_token;
}

public class ADAGEUserResponse
{
	public string provider;
	public string uid;
	public string player_name;
	public string email;
}

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
		
		if(data.Count == 0 || data[data.Count - 1] != newData)
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
	//public static readonly string developmentURL = "http://10.129.22.43:3000";
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
	
	public static bool UserIsValid
    {
		get
		{
			return user.valid();
		}
    }	
	
	public static ADAGEGameInfo GameInfo
	{
		get
		{
			return instance.gameInfo;	
		}
	}
	

	public static Dictionary<string, string> AuthenticationParameters = new Dictionary<string, string>();


	
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
	public string appToken;
	public string appSecret;
	
	public string devToken = "foo";
	public string devSecret = "bar";
	
	[HideInInspector]
	public string dataPath = "";
	
	private static ADAGE instance;

	private bool isOnline = false;
	public static ADAGEUser user = new ADAGEUser();
	private string currentSession;
	private string statusMessage = "Offline";
	public static string userNameField = "";
	public static string passwordField = "";
	private WorkerPool threads;	
	private UploadWrapper dataWrapper;
	private UploadWrapper localWrapper;
	private float lastPush = 0;  
	
	private ADAGEVirtualContext vContext;
	private ADAGEPositionalContext pContext;
	

	private Dictionary<string, ADAGECamera> cameras;

	
	public static void LogData<T>(T data) where T:ADAGEData
	{
		if(ReflectionUtils.CompareType(data.GetType(), (typeof(ADAGEContext))))
		{
			string message = string.Format("ADAGE WARNING: Method 'ADAGE.LogData' should not be used to track progression object '{0}'. Please use the ADAGE.LogContext method", data.GetType().ToString());
			Debug.LogWarning(message);	
		}
		else if(ReflectionUtils.CompareType(data.GetType(), (typeof(ADAGEScreenshot))))
		{
			ADAGEScreenshot shotData = data as ADAGEScreenshot;
			if(instance.cameras != null && instance.cameras.ContainsKey(shotData.cameraName))
			{
				ADAGECamera cam = instance.cameras[shotData.cameraName];
				if(cam.camera != null)
				{
					shotData.shot = instance.TakeScreenshot(cam.camera);
				}
				else
				{
					string message = string.Format("ADAGE WARNING: Cannot log screenshot from source '{0}' because no Unity3D camera is present.", shotData.cameraName);
					Debug.LogWarning(message);	
				}
			}
			else
			{
				string message = string.Format("ADAGE WARNING: Cannot log screenshot from source '{0}' because the camera has not been registered with ADAGE. Did you add the ADAGECamera object to the camera?", shotData.cameraName);
				Debug.LogWarning(message);	
			}
		}
		
		instance.AddData<T>(data);
	}
	
	public static void GetData<T>(WebJob job)
	{
		
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

	
	public static void LoginPlayer(string playerName, string password)
	{
		//instance.AddConnectionJob(playerName, password);
		instance.AddConnectionJob("ztest", "zisnogood");
	}
	
	public static void RegisterPlayer(string playerName, string password, string passwordConfirm)
	{
		
	}
	
	//This will create an anonymous yet unique guest login
	public static void ConnectAsGuest()
	{
		
	}
	
	public static string GetStatusMessage()
	{
		return instance.statusMessage;	
	}

#if FACEBOOK_SUPPORT
	//Structure returned by the FB login call
	public class FBLoginResponse
	{
		public bool is_logged_in;
		public string user_id;
		public string access_token;
	}
	
	//Structure returned by the call to get /me from the FB graph API
	public class FBProfileInfo
	{
		public string id;
		public int timezone;
		public string username;
		public string link;
		public string locale;
		public string last_name;
		public string email;
		public bool verified;
		public string gender;
		public string name;
		public string first_name;
		public DateTime updated_time;
	}
	
	//Since currently I can't get the real full auth response from the FB SDK
	//construct a fake one!
	public class FakebookInfo
	{
		public string username;
		public string email;
	}
	public class FakebookCredentials
	{
		public string token;
		public string expires_at;
	}
	public class FakebookAuthResponse
	{
		public FakebookInfo raw_info;
		public FakebookInfo info;
		public string uid;
		public string provider = "facebook";
		public FakebookCredentials credentials;
		
		public FakebookAuthResponse()
		{
			raw_info = new FakebookInfo();
			info = new FakebookInfo();
			credentials = new FakebookCredentials();
		}
	}
	
	
	
	public static void ConnectToFacebook()
	{
		instance.BeginFacebookAuth();	
	}
#endif
	
	
	public static void AddCamera(ADAGECamera camera)
	{
		if(instance.cameras == null)
			instance.cameras = new Dictionary<string, ADAGECamera>();
		
		instance.cameras[camera.cameraName] = camera;
	}
	
	public void Awake()
	{
		if(instance != null) 
		{
			Debug.LogWarning("You have multiple copies of the ADAGE object running. Overriding...");
		}
		DontDestroyOnLoad(this);
		instance = this;
		
		threads = new WorkerPool(3);
		dataWrapper = new UploadWrapper();
		cameras = new Dictionary<string, ADAGECamera>();
		
		vContext = new ADAGEVirtualContext(Application.loadedLevelName);
		
		if(dataPath == "")
		{
			dataPath = "/ADAGE/Data/";	
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

		//Start the session
		currentSession = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
		
		//Temp
		isOnline = false;
		
	
		//if we are in the editor we are going to use the dev server credentials for this app
		if(Application.isEditor)
		{
			appToken = devToken;
			appSecret = devSecret;
		}
		//LoginPlayer("", "");
		//ConnectToFacebook();
				
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
		Debug.Log("ONLINE: " + isOnline);
		if((elapsedTime > pushRate) && dataWrapper.Count != 0 && isOnline)
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
			string path = Application.dataPath + dataPath + ADAGE.AuthenticationParameters["adage_access_token"] + "/";
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
		data.gameVersion = appToken;
		if(Application.isEditor)
		{
			data.gameVersion = ADAGE.GameInfo.development_version;
		}
		data.timestamp = convertDateTimeToEpoch(System.DateTime.Now.ToUniversalTime()).ToString();
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
	
	//This looks to see if a player was previously logged in and will try to reconnect them if 
	private void SearchForPreviousPlayer()
	{
		
		//if this is a web build then look for credentials passed in from the website
		if(Application.isWebPlayer)
		{
			Application.ExternalCall("GetAccessToken");
		}
		else
		{
			LoadUserInfo();
		}
		
		
		
		
	}
	
	private void SaveUserInfo()
	{
		string userData = JsonMapper.ToJson(user);
		string path = Application.dataPath + "session.info";
		System.IO.File.WriteAllText(path, userData);
		
	}
	
	private void LoadUserInfo()
	{
		if(File.Exists(Application.dataPath + "session.info"))
		{
			string sessionInfo = File.ReadAllText(Application.dataPath + "session.info");	
			user = JsonMapper.ToObject<ADAGEUser>(sessionInfo);
		}
		
	}
	
	private void ListenForAccessToken(string incoming){
		if(incoming != "invalid")
		{
			user.adageAccessToken = incoming;
			//try to connect and get user info
			AddUserRequestJob();
			
		}
	}
	
	private void AddConnectionJob(string name, string password)
	{
		statusMessage = "Connecting...";
		ADAGEConnectionJob job = new ADAGEConnectionJob(appToken, appSecret, name, password);
		job.OnComplete = OnConnectionComplete;
		threads.AddJob(job);

	}
		
	private void AddUserRequestJob()
	{
		statusMessage = "Requesting player info...";
		Debug.Log(statusMessage);
		//Now make a call to get the User info
		ADAGERequestUserJob rjob = new ADAGERequestUserJob(user.adageAccessToken);
		rjob.OnComplete = OnRequestUserComplete;
		threads.AddJob(rjob);

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

	
		if(upload.Status != 201) 
		{
			Debug.Log ("Adding Data to Local Wrapper");
			
			if(localWrapper == null)
				localWrapper = new UploadWrapper();
			
			localWrapper.Add(upload.GetData());
		}
		
		Debug.Log (upload.Status);	
		Debug.Log (upload.Output);	
	}

	private void OnConnectionComplete(Job job)
	{
		ADAGEConnectionJob connection = (job as ADAGEConnectionJob);
		Debug.Log(connection.status);
		
		if(connection.status != 200) 
		{
			Debug.Log("What we have here is a FAILURE to authenticate!");
			statusMessage = "Could not connect";
			return;
		}
		
		ADAGEAccessTokenResponse accessResponse = JsonMapper.ToObject<ADAGEAccessTokenResponse>(connection.response);
		user.adageAccessToken = accessResponse.access_token;
		Debug.Log (accessResponse.access_token);
		DebugEx.Log("Successfully authenticated with ADAGE."); 

		
		AddUserRequestJob();
	}
	
	private void OnRequestUserComplete(Job job)
	{
		ADAGERequestUserJob connection = (job as ADAGERequestUserJob);

		if(connection.status != 200)  
		{
			Debug.Log("Request adage User failed!");
			return;
		}

		Debug.Log(connection.status);
		user.playerName = connection.userResponse.player_name;
		user.adageId = connection.userResponse.uid;
		
		isOnline = true;
		statusMessage = user.playerName;
		//SaveUserInfo();
		PushLocalToOnline();
	}
	
	private void OnLocalUploadComplete(Job job)
	{
		ADAGEUploadFileJob upload = (job as ADAGEUploadFileJob);
		
		if(upload.Status == 201)
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
	

#if FACEBOOK_SUPPORT
	private void BeginFacebookAuth()
	{
		Debug.Log("called FB Init");
		FB.Init(OnFBInitComplete);	
		
		
	}
	
	private void OnFBInitComplete()
	{
		Debug.Log("FB Init complete...Login starting");
		FB.Login("email", OnFBLoginComplete);
		
	}
	
	private void OnFBLoginComplete(FBResult response)
	{
		Debug.Log("FB Access token: " + FB.AccessToken);
		Debug.Log("FB Uid: " + FB.UserId);
		Debug.Log("FB response: " + response.Text);
		
		FBLoginResponse info =  JsonMapper.ToObject<FBLoginResponse>(response.Text);
		user.fbAccessToken = info.access_token;
		
		FB.API("/me", Facebook.HttpMethod.GET, OnFBUserInfo);
		//FB.GetAuthResponse(OnAuthResponse);
	}
	
	private void OnFBUserInfo(FBResult response)
	{
		Debug.Log("FB User Info... ");
		Debug.Log("FB response: " + response.Text);
		
		FBProfileInfo info = JsonMapper.ToObject<FBProfileInfo>(response.Text);
		Debug.Log("Look a valid email account! " + info.email);
		user.email = info.email;
		user.playerName = info.name;
		user.username = info.username;
		user.facebookId = info.id;
		//user.adageExpiresAt = 
		FBAuthWithAdage();
		
		
	}
	
	private void OnFBConnectionComplete(Job job)
	{
		ADAGEFacebookConnectionJob connection = (job as ADAGEFacebookConnectionJob);
		Debug.Log(connection.status);
		
		if(connection.status != 200) 
		{
			Debug.Log("What we have here is a FAILURE to authenticate!");
			statusMessage = "Could not connect";
			return;
		}
		
		ADAGEAccessTokenResponse accessResponse = JsonMapper.ToObject<ADAGEAccessTokenResponse>(connection.response);
		user.adageAccessToken = accessResponse.access_token;
		Debug.Log (accessResponse.access_token);
		DebugEx.Log("Successfully authenticated with ADAGE."); 

		
		AddUserRequestJob();
	}
	
	private void OnAuthResponse(FBResult response)
	{
		Debug.Log("FB auth response " + response.Text);	
	}
	
	
	//This function takes info supplied by the iOS FB Oauth and constructs an Oauth like response 
	//that is sent to ADAGE to authenticate the user. This path will create an ADAGE user for 
	//the FB email if none already exists.
	private void FBAuthWithAdage()
	{
		FakebookAuthResponse cookie = new FakebookAuthResponse();
		cookie.credentials.token = user.fbAccessToken;
		cookie.credentials.expires_at = "";
		cookie.info.email = user.email;
		cookie.raw_info.username = user.username;
		cookie.uid = user.facebookId;
		statusMessage = "Connection...";
		Debug.Log("Starting Auth with ADAGE");
		ADAGEFacebookConnectionJob job = new ADAGEFacebookConnectionJob(appToken, appSecret, cookie);
		job.OnComplete = OnFBConnectionComplete;
		threads.AddJob(job);
		
	}

#endif
	
	public static long convertDateTimeToEpoch(DateTime time)
	{
   		DateTime epoch = new DateTime(1970, 1, 1);

    	TimeSpan ts = time - epoch;
    	return (long) ts.Ticks/ 10;
	}

	

	private byte[] TakeScreenshot(Camera cam)
	{
		Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		
		// Initialize and render
		RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
		cam.targetTexture = rt;
		cam.Render();
		RenderTexture.active = rt;
		 
		// Read pixels
		tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height), 0, 0);
		 
		// Clean up
		cam.targetTexture = null;
		RenderTexture.active = null; 
		DestroyImmediate(rt);
		
		return tex.EncodeToPNG();
	}

}