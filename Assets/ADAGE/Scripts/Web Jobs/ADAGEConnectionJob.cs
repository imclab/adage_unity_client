using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;

public class ADAGEConnectionJob: WebJob
{	
	private string email;
	private string password;
		
	public ADAGEConnectionJob(string email, string password)
	{
		this.email = email;
		this.password = password;
	}
	
	public override void Main(WorkerPool boss = null) 
	{	
		if(!ADAGE.Online)
		{
			//string json = JsonMapper.ToJson();
			
			
			
			/*if(webMessage.error == WebErrorCode.Error)
	        {
	            status = webMessage.extendedError;
				//The server is down - off the user the option to play offline
				if(status.Contains("503"))
				{
					UserManager.isOnline = false;
					state = AuthStatus.NO_NET;
				}
				if(status.Contains("401"))
				{
					status = "PLAYER NOT FOUND!";
					state = AuthStatus.AUTHENTICATION_COMPLETE;
					UserManager.isOnline = false;
					yield return 0;
				}
				if(status.Contains("500"))
				{
					status = "PLAYER NOT FOUND!";
					state = AuthStatus.AUTHENTICATION_COMPLETE;
					UserManager.isOnline = false;
					yield return 0;
				}
				if(status.Contains("Invalid email"))
				{
					status = "PLAYER NOT FOUND!";
					state = AuthStatus.AUTHENTICATION_COMPLETE;
					UserManager.isOnline = false;
					yield return 0;
				}
				//if we are trying to sign in silently and get ANY error just abort the login
				if(silentSignin)
				{
					UserManager.isOnline = false;
					state = AuthStatus.NO_NET;
				}
	            yield break;
	        }	*/
		}				
				
		if(boss != null)
			boss.CompleteJob(this);
    }

/*status = "Trying to login...";
WebMessage webMessage = new WebMessage();
Debug.Log(status);
yield return StartCoroutine(webMessage.Post(loginURL, 
                                            "email", loginName,
                                            "password", loginPassword));

// deserialize the login JSON text
//
try 
{
	Debug.Log(webMessage.www.text);
	UserManager.userInfo = JsonMapper.ToObject<UserData>(webMessage.www.text);
	UserManager.isOnline = true;
	UserManager.isLoggedIn = true;
	UserManager.playerName = loginName;
}
catch(JsonException)
{
    if(webMessage.error == WebErrorCode.None)
	{
		UserManager.isOnline = false;
		state = AuthStatus.NO_NET;
		yield break;
	}
}*/
}