using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ReflectionUtils {
	public static bool	TypeContainsAttributes( Type type, params Type[] attributes ) {
		Attribute[] typeAttribs = type.GetCustomAttributes( true ) as Attribute[];
		foreach( Attribute typeAttrib in typeAttribs ) {
			foreach( Type argAttrib in attributes ) {
				if( typeAttrib.GetType() == argAttrib ) {
					return true;
				}
			}
		}

		return false;
	}
	
	public static Type	FindType( string typeName ) {
		if( string.IsNullOrEmpty(typeName) ) {
			return null;
		}

		foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach(Type t in a.GetTypes())
			{
				if(t.Name == typeName)
				{
					return t;
				}
			}
		}

		return null;
	}
	
	//Returns true if a is a child of or is the same as b
	public static bool CompareType(Type a, Type b)
	{
		bool subclass = a.IsSubclassOf(b);
		bool same = (a == b);
		return subclass || same;	
	}
	
	public static Dictionary<string, System.Type> GetChildTypes<T>()
	{
		return GetChildTypes(typeof(T));
	}
	
	public static Dictionary<string, System.Type> GetChildTypes(System.Type baseType)
	{
		List<Type> types = Assembly.GetAssembly(baseType).GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
		Dictionary<string, System.Type> output = new Dictionary<string, System.Type>();
		foreach(Type child in types)
		{
			output.Add(child.ToString(), child);
		}
		return output;
	}
}