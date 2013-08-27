using System;

public abstract class Job {
	//public int id;
	public Action<Job> OnComplete = null;
	public abstract void Main(WorkerPool boss = null);
}

public abstract class WebJob : Job {
	public string url {get; set;}
	
	protected HTTP.Request request;
}