using System;
using UnityEngine;

public class ADAGETrackContextException : Exception
{
    public ADAGETrackContextException(Type badType)
    {
        string message = string.Format("ADAGE ERROR: Method 'ADAGE.LogData' cannot be used to track progression object '{0}'. Please use the ADAGE.LogContext method", badType.ToString());
		Debug.LogWarning(message);
		//throw new Exception(message);
    }	
}

public class ADAGEStartContextException : Exception
{
    public ADAGEStartContextException(string id)
    {
        string message = string.Format("ADAGE ERROR: Cannot start tracking the progress of {0} because it is already being tracked", id);
		throw new Exception(message);
    }
}

public class ADAGEEndContextException : Exception
{
    public ADAGEEndContextException(string id)
    {
        string message = string.Format("ADAGE ERROR: Cannot stop tracking the progress of {0} because it isn't being tracked", id);
		throw new Exception(message);
    }
}