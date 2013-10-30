using UnityEngine;
using System;
using System.Collections;

public class ADAGEUser 
{
	public ADAGEUser(){
		playerName = "EMPTY";
		username = "EMPTY";
		email = "EMPTY@EMPTY.COM";
		adageId = "EMPTY";
		adageAccessToken = "EMPTY";
		adageRefreshToken = "EMPTY";
		fbAccessToken = "EMPTY";
		fbExpiresAt = new DateTime();
		adageExpiresAt = new DateTime();
	}
	public string playerName {get; set;} //what to display on the UI
	public string username {get; set; } //what the unique user name is. With FB username can be different from playerName
	public string email {get; set;}
	public string adageId {get; set;}
    public string adageAccessToken { get; set; }
	public string adageRefreshToken { get; set; }
	public string fbAccessToken {get; set; }
	public string facebookId {get; set;}
	
	
	public DateTime fbExpiresAt {get; set; }
	public DateTime adageExpiresAt {get; set; }
	
	//Is this a valid user
	public bool valid()
	{
		return !playerName.Equals("EMPTY") && !adageAccessToken.Equals("EMPTY");	
	}

}
