using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;
using Ionic.Zlib;

public class ADAGEUploadFileJob : WebJob
{
	private string authToken;
	private string data;
	
	public string Path{ get; protected set; }
	public int Status;
	public string Output = "Nothing";
		
	public ADAGEUploadFileJob(string token, string filepath, string text)
	{
		
		this.Path = filepath;
		this.data = text;
		
		if(Application.isEditor || Debug.isDebugBuild)
		{
			if(ADAGE.Staging)
			{
				this.url = ADAGE.stagingURL;	
			}
			else
			{
				this.url = ADAGE.developmentURL;	
			}
		}
		else
		{
			this.url = ADAGE.productionURL;	
		}		
	}
	
	public override void Main(WorkerPool boss = null) 
	{			
		if(ADAGE.Online)
		{
			byte[] compressed = GZipStream.CompressString(data);
			
			request = new HTTP.Request("Post", this.url + "/data_collector", compressed);
			request.AddHeader("Content-Type", "application/jsonrequest");
			request.AddHeader("Content-Encoding", "gzip");
			request.AddHeader("Authorization", "Bearer " + ADAGE.AuthenticationParameters["adage_access_token"]);
			Debug.Log(request.GetHeader("Authorization"));
			//request.AddParameter("auth_token", authToken);
			request.Send();
			
			//Error Handling - Server constantly sending 404
			Status = request.response.status;
			Output = request.response.Text;
		}
		else
		{
			//not sure	
			Status = 404;
		}
		
		if(boss != null)
			boss.CompleteJob(this);
    }
}

public class ADAGEUploadJob : WebJob
{
	private UploadWrapper data;
	
	public int Status;
	public string Output = "Nothing";
		
	public ADAGEUploadJob(UploadWrapper data)
	{
		//this.data = data;	
		this.data = new UploadWrapper(data);
		
		if(Application.isEditor)
		{
			if(ADAGE.Staging)
			{
				this.url = ADAGE.stagingURL;	
			}
			else
			{
				this.url = ADAGE.developmentURL;	
			}
		}
		else
		{
			this.url = ADAGE.productionURL;	
		}		
	}
	
	public override void Main(WorkerPool boss = null) 
	{	
		string json = JsonMapper.ToJson(this.data);
		
		Debug.Log("TRYING TO PUSH DATA: ADAGE online: " + ADAGE.Online);
		if(ADAGE.Online)
		{
			request = new HTTP.Request("Post", this.url + "/data_collector");
			request.AddHeader("Content-Type", "application/jsonrequest");
			request.AddHeader("Authorization", "Bearer " + ADAGE.user.adageAccessToken);
			request.SetText(json);
			request.Send();

			Debug.Log(request.uri + " " + json);
			
			//Error Handling - Server constantly sending 404
			Status = request.response.status;
			Output = json;
		}
		else
		{
			//not sure	
			Status = 404;
		}
		
		if(boss != null)
			boss.CompleteJob(this);
    }
	
	public UploadWrapper GetData()
	{
		return data;	
	}
}