using System;
using System.Collections.Generic;
using UnityEngine;

public class BTUIUtils{
	//returns the windowObject
	public static GameObject PrepareWindow(GameObject canvas, string windowName){
		if(canvas == null || windowName == null){
			return null;
		}
		
		Transform windowTransform = canvas.transform.Find(windowName);
		if(windowTransform == null){
			return null;
		}
		
		GameObject window = windowTransform.gameObject;
		Transform titleTransform = windowTransform.Find("Title");
		if(titleTransform == null){
			return null;
		}
		
		GameObject title = titleTransform.gameObject;
		BTUIDragHandler dragHandler = title.AddComponent<BTUIDragHandler>();
		dragHandler.Initialize(window.GetComponent<RectTransform>(), canvas.GetComponent<Canvas>());
		
		return window;
	}
	
	public static GameObject FindGameObjectAtPath(GameObject root, string path){
		if(root == null || path == null){
			return null;
		}
		
		Transform temp = FindTransformAtPath(root.transform, path);
		if(temp != null){
			return temp.gameObject;
		}
		return null;
	}
	
	public static Transform FindTransformAtPath(Transform root, string pathString){
		if(root == null || pathString == null){
			return null;
		}
		
		string[] paths = pathString.Split('/');
		
		Transform result = root;
		foreach(string path in paths){
			if(result == null){
				return null;
			}
			result = result.Find(path);
		}
		return result;
	}
}