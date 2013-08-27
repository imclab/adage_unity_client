using System;
using System.Reflection;

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
}