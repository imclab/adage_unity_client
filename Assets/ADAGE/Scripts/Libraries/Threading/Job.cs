using System;
using System.Collections.Generic;

public abstract class Job {
	//public int id;
	public Action<Job> OnComplete = null;
	public abstract void Main(WorkerPool boss = null);
}

public abstract class WebJob : Job {
	public string url {get; set;}
	
	protected HTTP.Request request;
	protected Dictionary<string, string> parameters;
	
	public void AddParameter(string name, string value)
	{
		if(parameters == null)
			parameters = new Dictionary<string, string>();
		
		parameters[name] = value;
	}
	
	public void SendRequest()
	{
		if(request == null)
			throw new Exception(string.Format("Attempting to send a web request that is null"));
		
		if(parameters == null)
			parameters = new Dictionary<string, string>();
		
		foreach(string key in parameters.Keys)
		{
			request.AddParameter(key, parameters[key]);	
		}
		
		request.Send();
	}
}