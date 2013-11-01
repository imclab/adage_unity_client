using UnityEngine;
using System.Collections;
using System;

public class ReturnData
{
	public string data;
}

public class Test : MonoBehaviour 
{
	
	void OnGui()
	{
		if(GUILayout.Button("Get Data"))
		{
			ADAGEDownloadJob<ReturnData> download = new ADAGEDownloadJob<ReturnData>("/data/heatmap");
			download.AddParameter("gameName", "FairPlay");
			
			DateTime date1 = new DateTime(2013, 9, 2);
			
			download.AddParameter("since", date1.ToUniversalTime().ToString());
			download.AddParameter("key", "PathWorldClick");
			download.AddParameter("schema", "Beta-Build-04-08-2013");
						
			ADAGE.GetData<ReturnData>(download);
		}
		
		if(GUILayout.Button ("Screenshot"))
		{
			ADAGEScreenshot shot = new ADAGEScreenshot("main");
			ADAGE.LogData<ADAGEScreenshot>(shot);
		}
	}
	
	private void OnComplete(ReturnData data)
	{
		Debug.Log("");
	}
}
