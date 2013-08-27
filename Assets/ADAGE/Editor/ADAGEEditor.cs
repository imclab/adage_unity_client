using UnityEngine;
using UnityEditor;
 
[CustomEditor(typeof(ADAGE))]
public class ADAGEEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();
		GUILayout.BeginHorizontal();
		{
			EditorGUILayout.TextField("Data Path", (target as ADAGE).dataPath);
			if(GUILayout.Button("Select"))
			{
				ADAGE curTarget = (target as ADAGE);
				string oldPath = curTarget.dataPath;
				curTarget.dataPath = EditorUtility.OpenFolderPanel("Select ADAGE Data Directory", "", "");
				
				string appDataPath = Application.dataPath;
				bool longer = (curTarget.dataPath.Length >= appDataPath.Length);
				if(!longer || (longer && curTarget.dataPath.Substring(0,appDataPath.Length) != appDataPath))
				{
					EditorUtility.DisplayDialog("ADAGE Data Path Error", "You must select a path that is in the project path", "OK");
					curTarget.dataPath = oldPath;
				}
				else
				{
					curTarget.dataPath = curTarget.dataPath.Substring(appDataPath.Length);	
				}
			}
		}
		GUILayout.EndHorizontal();
	}
}