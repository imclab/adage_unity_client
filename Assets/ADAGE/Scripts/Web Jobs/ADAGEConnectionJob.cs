using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using LitJson;



public class ADAGEConnectionJob: WebJob
{	
	private string email;
	private string password;

	private string clientId = "";
	private string clientSecret = "";

	public int status = 0;
	public string response = "Nothing";
	
		
	public ADAGEConnectionJob(string clientId, string clientSecret, string email, string password)
	{
		this.email = email;
		this.password = password;
		this.clientId = clientId;
		this.clientSecret = clientSecret;

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
		
		//First we authorize the client 
		HTTP.Request request = new HTTP.Request ("GET", this.url + "/auth/authorize_unity");
		request.AddParameter("client_id", this.clientId);
		request.AddParameter("client_secret", this.clientSecret);
		request.AddParameter("email", this.email);
		request.AddParameter("password", this.password);
		request.AddParameter("grant_type", "password");

		// Add request headers
		request.AddHeader ("Content-Type", "application/x-www-form-urlencoded");
		
		// Send request
		request.Send();
		
		Debug.Log(request.uri);
			
	
		// Dump request response to debug console
		status = request.response.status;
		response = request.response.Text;
		Debug.Log (response);
			

		

		

						
				
		if(boss != null)
			boss.CompleteJob(this);
    }

}

public class ADAGERequestUserJob: WebJob
{	

	public int status;
	public string response = "Nothing";
	public ADAGEUserResponse userResponse;
	private string access_token; 
		
	public ADAGERequestUserJob(string access_token)
	{
		this.access_token = access_token;
	
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
		request = new HTTP.Request("Get", this.url + "/auth/unity_user.json");
		request.AddHeader("Content-Type", "application/jsonrequest");
		request.AddHeader("Authorization", "Bearer " + access_token);
		request.Send();

		Debug.Log(request.uri);
			
			
		status = request.response.status;
		response = request.response.Text;
		Debug.Log (response);
		if(status == 200) 
		{
			userResponse = JsonMapper.ToObject<ADAGEUserResponse>(request.response.Text);
			Debug.Log("Successfully requested ADAGE user info for " + userResponse.player_name ); 
		}
			
				
		if(boss != null)
			boss.CompleteJob(this);
    }

}


#if FACEBOOK_SUPPORT
public class ADAGEFacebookConnectionJob: WebJob
{	


	public int status;
	public string response = "Nothing";
	
	private string clientId = "";
	private string clientSecret = "";
	
	private ADAGE.FakebookAuthResponse cookie;
	
	
	

		
	public ADAGEFacebookConnectionJob(string clientId, string clientSecret, ADAGE.FakebookAuthResponse cookie)
	{
		this.clientId = clientId;
		this.clientSecret = clientSecret;
		this.cookie = cookie;
		
		
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
	
			
		HTTP.Request request = new HTTP.Request ("GET", this.url + "/auth/authorize_unity_fb");
			
		request.AddParameter("client_id", this.clientId);
		request.AddParameter("client_secret", this.clientSecret);
		request.AddParameter("grant_type", "fakebook");
		request.AddHeader("omniauth.auth", JsonMapper.ToJson(cookie));
			

		// Add request headers
		request.AddHeader ("Content-Type", "application/x-www-form-urlencoded");
		
		Debug.Log(request.uri);
		// Send request
		request.Send ();
		
			
	
			// Dump request response to debug console
			
		status = request.response.status;
		response = request.response.Text;
		Debug.Log ("RESPONSE ***********************************: " + response);
			
		


				
				
		if(boss != null)
			boss.CompleteJob(this);
    }

}
#endif