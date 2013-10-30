using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// THE data class. Every chunk of logged data must inherit from this in some way.
/// If you are adding more fields to a child class of ADAGEData, you should also implement the
/// InitFromJSON method so that your object can be recreated from JSON.
/// </summary>
public class ADAGEData
{
	public string gameName {get; set;}
	public string gameVersion {get; set;}
	public string ADAVersion = ADAGE.VERSION;
	public string timestamp { get; set; }
	public string session_token { get; set;}
	public List<string> ada_base_types { get; set;}
	public string key { get; set; } 
	
	
	public static ADAGEData CreateFromJSON(string json)
	{	
		ADAGEData baseData = LitJson.JsonMapper.ToObject<ADAGEData>(json);
		
		if(baseData.key != null && baseData.key != "")
		{
			Type theType = ReflectionUtils.FindType(baseData.key);
			if(theType != null)
			{
				ADAGEData output = Activator.CreateInstance(theType) as ADAGEData;
				output.Copy(baseData);
				output.InitFromJSON(json);
				return output;
			}
		}	
		
		return null;			
	}
	
	/*public override bool Equals(object obOther)
	{
		if (null == obOther)
			return false;
		if (object.ReferenceEquals(this, obOther)
			return true;
		if (this.GetType() != obOther.GetType())
			return false;
		
		# private method to compare members.
		return CompareMembers(this, obOther as ADAGEData);
	}*/
	
	public void Copy(ADAGEData data)
	{
		this.gameName = data.gameName;
		this.gameVersion = data.gameVersion;
		this.ADAVersion = data.ADAVersion;
		this.timestamp = data.timestamp;
		this.session_token = data.session_token;
		this.ada_base_types = data.ada_base_types;
		this.key = data.key;	
		
	}
	
	//TODO: Can we reflect here?
	public virtual void InitFromJSON(string input){}
}

public class ADAGEStartGame : ADAGEPlayerEvent {}
public class ADAGEQuitGame : ADAGEPlayerEvent {}

public class ADAGEVirtualContext
{
	public string level;  //The name of the level(map, scene, stage, etc)
	private List<string> active_units; //Names of all the currently active game units. This can be used as a flat list of tags for processing actions within the scope of the units
	
	public ADAGEVirtualContext()
	{
		active_units = new List<string>();
		level = "";
	}
	
	public ADAGEVirtualContext(string curLevel)
	{
		active_units = new List<string>();
		level = curLevel;
	}
	
	public void Add(string id)
	{
		if(IsTracking(id))
		{
			throw new ADAGEStartContextException(id);	
		}
				
		active_units.Add(id);
	}
	
	public void Remove(string id)
	{
		if(!IsTracking(id))
		{
			throw new ADAGEStartContextException(id);	
		}
			
		active_units.Remove(id);
	}
	
	public bool IsTracking(string id)
	{
		if(active_units == null)
			active_units = new List<string>();
		
		return active_units.Contains(id);
	}
}

public class ADAGEPositionalContext
{
	public float x;
	public float y;
	public float z;
	//Euler angles for rotation
	public float rotx;
	public float roty;
	public float rotz;
	
	public ADAGEPositionalContext()
	{
		x = 0f;
		y = 0f;
		z = 0f;
		rotx = 0f;
		roty = 0f;
		rotz = 0f;
	}
	
	public void setPosition(float iX, float iY, float iZ)
	{
		x = iX;
		y = iY;
		z = iZ;
	}

	public void setRotation(float iX, float iY, float iZ)
	{
		rotx = iX;
		roty = iY;
		rotz = iZ;
	}
}


public class ADAGEContext : ADAGEData
{
	public string name; //Should be unique
	public string parent_name; //This can be left blank if there is no parent for this unit
	
	public ADAGEContext()
	{
		name = "";
		parent_name = "";		
	}
}

public class ADAGEContextStart : ADAGEContext 
{
	public ADAGEContextStart() : base()
	{}
}

public class ADAGEContextEnd : ADAGEContext 
{
	public ADAGEContextEnd() : base()
	{}
}

public abstract class ADAGEEventData : ADAGEData
{
	public abstract void Update();
}

/// <summary>
/// Used for logging player events in the world. This is different than normal ADAGEData logging in that this event
/// is capable of updating it's virtual and positional context fields before being logged.
/// </summary>
public class ADAGEPlayerEvent : ADAGEGameEvent
{
	public ADAGEPositionalContext positional_context;
	
	public ADAGEPlayerEvent() : base()
	{
		positional_context = new ADAGEPositionalContext();
	}
	
	public override void Update()
	{
		base.Update();
		positional_context = ADAGE.PositionalContext;
	}	
}

/// <summary>
/// Used for logging game events. This is different than normal ADAGEData logging in that this event
/// is capable of updating it's virtual context field before being logged.
/// </summary>
public class ADAGEGameEvent : ADAGEEventData
{
	public ADAGEVirtualContext virtual_context;
	
	public ADAGEGameEvent()
	{
		virtual_context = new ADAGEVirtualContext();	
	}
	
	public override void Update()
	{
		virtual_context = ADAGE.VirtualContext;		
	}
}