using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;
using System.Text.RegularExpressions;

// Token: 0x02000147 RID: 327
public class MemoryProfiler
{
	// Token: 0x06000867 RID: 2151
	public static void ForceMemoryCleanUp()
	{
		foreach (SpawnPool spawnPool in UnityEngine.Object.FindObjectsOfType(typeof(SpawnPool)) as SpawnPool[])
		{
			for (int i = spawnPool._prefabPools.Count - 1; i >= 0; i--)
			{
				PrefabPool prefabPool = spawnPool._prefabPools[i];
				int j = 0;
				int count = prefabPool.despawned.Count;
				while (j < count)
				{
					if (prefabPool.despawned[j] != null)
					{
						UnityEngine.Object.Destroy(prefabPool.despawned[j].gameObject);
					}
					j++;
				}
				prefabPool._despawned.Clear();
				if (prefabPool.spawned.Count <= 0)
				{
					spawnPool.prefabs._prefabs.Remove(prefabPool.prefab.name);
					spawnPool._prefabPools.RemoveAt(i);
				}
			}
		}
		RsResourceManager.UnloadUnusedAssets(true);
	}

	// Token: 0x06000868 RID: 2152
	public void RenderGUI()
	{
		if (this.boxReflectFontStyle == null)
		{
			this.boxReflectFontStyle = new GUIStyle(GUI.skin.box);
			this.boxReflectFontStyle.alignment = TextAnchor.MiddleLeft;
		}
		GUILayoutOption guilayoutOption = GUILayout.ExpandWidth(false);
		if (this.mCollapsed)
		{
			if (GUILayout.Button("MemProfiler", new GUILayoutOption[]
			{
				guilayoutOption
			}))
			{
				this.mCollapsed = false;
			}
			return;
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("Close", new GUILayoutOption[]
		{
			guilayoutOption
		}))
		{
			this.CloseProfiler();
		}
		if (GUILayout.Button("Collapse", new GUILayoutOption[]
		{
			guilayoutOption
		}))
		{
			this.mCollapsed = true;
		}
		GUILayout.Space(20f);
		if (GUILayout.Button("SnapShot", Array.Empty<GUILayoutOption>()))
		{
			this.TakeSnapShot();
		}
		if (GUILayout.Button("Refresh", Array.Empty<GUILayoutOption>()))
		{
			this.TakeLightSnapShot();
		}
		GUILayout.Space(20f);
		if (GUILayout.Button(" Log ", new GUILayoutOption[]
		{
			guilayoutOption
		}))
		{
			this.OutputToLog();
		}
		GUILayout.EndHorizontal();
		this.RenderGUI_Stats();
	}

	// Token: 0x06000869 RID: 2153
	private void CloseProfiler()
	{
		if (this._OwnerGO != null)
		{
			UnityEngine.Object.Destroy(this._OwnerGO);
		}
	}

	// Token: 0x0600086A RID: 2154
	private void RenderGUI_StatsViewTab(string Name, MemoryProfiler.StatsViewTab Mode)
	{
		Color backgroundColor = GUI.backgroundColor;
		if (this.mStatsViewIndex != Mode)
		{
			GUI.backgroundColor = Color.gray;
		}
		if (GUILayout.Button(Name, Array.Empty<GUILayoutOption>()))
		{
			this.mStatsViewIndex = Mode;
		}
		GUI.backgroundColor = backgroundColor;
	}

	// Token: 0x0600086B RID: 2155
	private void RenderGUI_Stats()
	{
		this.GUI_BeginContents();
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		this.RenderGUI_StatsViewTab("Current Stats", MemoryProfiler.StatsViewTab.CURRENT_STATS);
		this.RenderGUI_StatsViewTab("Current Objects", MemoryProfiler.StatsViewTab.CURRENT_OBJECTS);
		this.RenderGUI_StatsViewTab("Dif Stats", MemoryProfiler.StatsViewTab.DIF_STATS);
		this.RenderGUI_StatsViewTab("Dif Objects", MemoryProfiler.StatsViewTab.DIF_OBJECTS);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Filter:", new GUILayoutOption[]
		{
			GUILayout.Width(40f)
		});
		this.mFilter_Text = GUILayout.TextField(this.mFilter_Text, new GUILayoutOption[]
		{
			GUILayout.ExpandWidth(true)
		});
		if (GUILayout.Button("X", new GUILayoutOption[]
		{
			GUILayout.Width(20f)
		}))
		{
			this.mFilter_Text = "";
		}
		this.mFilter_ShowDependencies = GUILayout.Toggle(this.mFilter_ShowDependencies, "Dependencies", new GUILayoutOption[]
		{
			GUILayout.ExpandWidth(false)
		});
		MemoryProfiler.reflectOnProperties = GUILayout.Toggle(MemoryProfiler.reflectOnProperties, "reflectOnProperties", new GUILayoutOption[]{
			GUILayout.ExpandWidth(false)
		});
		GUILayout.EndHorizontal();
		switch (this.mStatsViewIndex)
		{
		case MemoryProfiler.StatsViewTab.CURRENT_STATS:
			this.RenderGUI_Stats(this.mStats_Current1);
			break;
		case MemoryProfiler.StatsViewTab.CURRENT_OBJECTS:
			this.RenderGUI_List(this.mList_Snapshot);
			break;
		case MemoryProfiler.StatsViewTab.DIF_STATS:
			this.RenderGUI_Stats(this.mStats_Dif1);
			break;
		case MemoryProfiler.StatsViewTab.DIF_OBJECTS:
			this.RenderGUI_List(this.mList_Differences);
			break;
		}
		this.GUI_EndContents();
	}

	// Token: 0x0600086D RID: 2157
	private void RenderGUI_Stats(List<KeyValuePair<string, int>> mStats)
	{
		this.mScrollViewPos_Stats = GUILayout.BeginScrollView(this.mScrollViewPos_Stats, new GUILayoutOption[]
		{
			GUILayout.ExpandHeight(true)
		});
		TextAnchor alignment = GUI.skin.label.alignment;
		GUI.skin.label.alignment = TextAnchor.MiddleRight;
		int i = 0;
		int count = mStats.Count;
		while (i < count)
		{
			KeyValuePair<string, int> keyValuePair = mStats[i];
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label((keyValuePair.Value < 1048576) ? ((keyValuePair.Value / 1024).ToString() + " Kb") : ((keyValuePair.Value / 1048576).ToString("0.00") + " Mb"), new GUILayoutOption[]
			{
				GUILayout.Width(80f)
			});
			if (GUILayout.Button(keyValuePair.Key, Array.Empty<GUILayoutOption>()))
			{
				this.mFilter_Text = keyValuePair.Key;
				MemoryProfiler.StatsViewTab statsViewTab = this.mStatsViewIndex;
				if (statsViewTab != MemoryProfiler.StatsViewTab.CURRENT_STATS)
				{
					if (statsViewTab == MemoryProfiler.StatsViewTab.DIF_STATS)
					{
						this.mStatsViewIndex = MemoryProfiler.StatsViewTab.DIF_OBJECTS;
					}
				}
				else
				{
					this.mStatsViewIndex = MemoryProfiler.StatsViewTab.CURRENT_OBJECTS;
				}
			}
			GUILayout.EndHorizontal();
			i++;
		}
		GUI.skin.label.alignment = alignment;
		GUILayout.EndScrollView();
	}

	// Token: 0x0600086E RID: 2158
	public void GUI_BeginContents()
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(4f);
		GUILayout.BeginHorizontal(new GUILayoutOption[]
		{
			GUILayout.ExpandHeight(true)
		});
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Space(2f);
	}

	// Token: 0x0600086F RID: 2159
	public void GUI_EndContents()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
	}

	// Token: 0x06000870 RID: 2160
	public void TakeSnapShot()
	{
		this.TakeLightSnapShot();
		this.mList_LastSnapshot.Clear();
		this.mList_LastSnapshot.Capacity = this.mList_Snapshot.Count;
		int i = 0;
		int count = this.mList_Snapshot.Count;
		while (i < count)
		{
			this.mList_LastSnapshot.Add(this.mList_Snapshot[i]);
			i++;
		}
	}

	// Token: 0x06000871 RID: 2161
	private int IndexOfObjInArray(ref UnityEngine.Object[] Objs, UnityEngine.Object Obj)
	{
		int i = 0;
		int num = Objs.Length;
		while (i < num)
		{
			if (Objs[i] == Obj)
			{
				return i;
			}
			i++;
		}
		return -1;
	}

	// Token: 0x06000872 RID: 2162
	public void TakeLightSnapShot()
	{
		Dictionary<string, List<MemoryProfiler.ObjMemDef>> dictionary = new Dictionary<string, List<MemoryProfiler.ObjMemDef>>();
		int i = 0;
		int count = this.mList_LastSnapshot.Count;
		while (i < count)
		{
			this.AddLastSnapShotElement(ref dictionary, this.mList_LastSnapshot[i]);
			i++;
		}
		this.mList_Differences.Clear();
		this.mList_Snapshot.Clear();
		int num = 0;
		this.mStats_Current.Clear();
		this.mStats_Dif.Clear();
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
		int j = 0;
		int num2 = array.Length;
		while (j < num2)
		{
			UnityEngine.Object @object = array[j];
			int num3 = (int)Profiler.GetRuntimeMemorySizeLong(@object);
			num += num3;
			string text = @object.GetType().ToString();
			if (text.StartsWith("UnityEngine."))
			{
				text = text.Substring(12);
			}
			int num4 = 0;
			this.mStats_Current.TryGetValue(text, out num4);
			this.mStats_Current[text] = num4 + num3;
			MemoryProfiler.ObjMemDef objMemDef = new MemoryProfiler.ObjMemDef(num3, text, @object.name, this.HasADependantInTheList(@object, ref array), @object);
			this.mList_Snapshot.Add(objMemDef);
			if (!this.RemoveLastSnapShotElement(ref dictionary, objMemDef))
			{
				this.mList_Differences.Add(objMemDef);
				num4 = 0;
				this.mStats_Dif.TryGetValue(text, out num4);
				this.mStats_Dif[text] = num4 + num3;
			}
			j++;
		}
		this.mStats_Dif1.Clear();
		this.mStats_Current1.Clear();
		foreach (KeyValuePair<string, int> item in this.mStats_Dif)
		{
			this.mStats_Dif1.Add(item);
		}
		this.mStats_Dif1.Sort((KeyValuePair<string, int> v1, KeyValuePair<string, int> v2) => v2.Value - v1.Value);
		foreach (KeyValuePair<string, int> item2 in this.mStats_Current)
		{
			this.mStats_Current1.Add(item2);
		}
		this.mStats_Current1.Sort((KeyValuePair<string, int> v1, KeyValuePair<string, int> v2) => v2.Value - v1.Value);
		this.mStats_Dif.Clear();
		this.mStats_Current.Clear();
		this.mList_Snapshot.Sort((MemoryProfiler.ObjMemDef p1, MemoryProfiler.ObjMemDef p2) => p2.CompareTo(ref p1));
		this.mList_Differences.Sort((MemoryProfiler.ObjMemDef p1, MemoryProfiler.ObjMemDef p2) => p2.CompareTo(ref p1));
	}

	// Token: 0x06000873 RID: 2163
	private bool HasADependantInTheList(UnityEngine.Object Obj, ref UnityEngine.Object[] Objs)
	{
		GameObject gameObject = Obj as GameObject;
		return gameObject == null || gameObject.transform.parent != null;
	}

	// Token: 0x06000876 RID: 2166
	private void OutputToLog()
	{
	}

	// Token: 0x06000877 RID: 2167
	public MemoryProfiler()
	{
		this.mStats_Dif1 = new List<KeyValuePair<string, int>>();
		this.mStats_Dif = new Dictionary<string, int>();
		this.mList_Differences = new List<MemoryProfiler.ObjMemDef>();
		this.mList_LastSnapshot = new List<MemoryProfiler.ObjMemDef>();
		this.mScrollViewPos_Stats = new Vector2(0f, 0f);
		this.mFilter_Text = "";
		this.mFilter_ShowDependencies = true;
	}

	// Token: 0x06007E13 RID: 32275
	private static MemoryProfiler.ObjectContentInfo buildContentInfo(object instance)
	{
		if (instance == null)
		{
			return null;
		}
		Debug.LogWarning("building contentInfo for object: " + instance.ToString());
		List<object> objectHistory = new List<object>();
		return new MemoryProfiler.ObjectContentInfo(instance.GetType().Name, "", instance.ToString())
		{
			contentInfo = MemoryProfiler.buildContentInfoRecursive(instance, 0, objectHistory)
		};
	}

	// Token: 0x06007EFC RID: 32508
	private static List<MemoryProfiler.ObjectContentInfo> buildContentInfoRecursive(object value, int depth, List<object> objectHistory)
	{
		if (value == null)
		{
			return null;
		}
		if (depth > 15)
		{
			return null;
		}
		if (objectHistory.Contains(value))
		{
			return null;
		}
		objectHistory.Add(value);
		Type type = value.GetType();
		if (type.IsPrimitive)
		{
			return null;
		}
		if (typeof(string).IsInstanceOfType(value))
		{
			return null;
		}
		if(typeof(DateTime).IsInstanceOfType(value)){
			return null;
		}
		List<MemoryProfiler.ObjectContentInfo> result = new List<MemoryProfiler.ObjectContentInfo>();
		try
		{
			if (type.IsEnum)
			{
				foreach (string enumName in type.GetEnumNames())
				{
					result.Add(new MemoryProfiler.ObjectContentInfo("Enum", type.Name, enumName));
				}
			}
			else if (typeof(Array).IsAssignableFrom(type))
			{
				Array array = (Array)value;
				int length = array.GetLength(0);
				for (int index = 0; index < length; index++)
				{
					object arrayValue = array.GetValue(index);
					MemoryProfiler.ObjectContentInfo arrayContentInfo = new MemoryProfiler.ObjectContentInfo(arrayValue.GetType().Name, "arr_" + index, arrayValue);
					result.Add(arrayContentInfo);
					arrayContentInfo.contentInfo = MemoryProfiler.buildContentInfoRecursive(arrayValue, depth + 1, objectHistory);
				}
			}
			else if(type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>)).Any())
			{
				IDictionary<object, object> dictionary = (IDictionary<object, object>)value;
				foreach(KeyValuePair<object, object> kvp in dictionary){
					MemoryProfiler.ObjectContentInfo enumerableContentInfo = new MemoryProfiler.ObjectContentInfo(kvp.Key.GetType().Name, "dict[" + kvp.Key.ToString() + "]", kvp.Value);
					result.Add(enumerableContentInfo);
					enumerableContentInfo.contentInfo = MemoryProfiler.buildContentInfoRecursive(kvp.Value, depth + 1, objectHistory);
				}
			}
			else if (type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)).Any())
			{
				IEnumerable<object> enumerable = (IEnumerable<object>) value;
				int quasiIndex = 0;
				foreach(object item in enumerable){
					MemoryProfiler.ObjectContentInfo enumerableContentInfo = new MemoryProfiler.ObjectContentInfo(item.GetType().Name, "enumerable~" + quasiIndex, item);
					result.Add(enumerableContentInfo);
					enumerableContentInfo.contentInfo = MemoryProfiler.buildContentInfoRecursive(item, depth + 1, objectHistory);
					quasiIndex++;
				}
			}else{
				foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
				{
					object fieldValue = fieldInfo.GetValue(value);
					MemoryProfiler.ObjectContentInfo fieldContentInfo = new MemoryProfiler.ObjectContentInfo(fieldInfo.FieldType.Name, fieldInfo.Name, fieldValue);
					result.Add(fieldContentInfo);
					fieldContentInfo.contentInfo = MemoryProfiler.buildContentInfoRecursive(fieldValue, depth + 1, objectHistory);
				}
				if(MemoryProfiler.reflectOnProperties){
					foreach(PropertyInfo propertyInfo in type.GetProperties()){
						if(propertyInfo.CanRead){
							ParameterInfo[] parameters = propertyInfo.GetIndexParameters();
							if(parameters.GetLength(0) != 0){
								ObjectContentInfo indexedPropContentInfo = new ObjectContentInfo(propertyInfo.PropertyType.Name, propertyInfo.Name, "[can't reflect on indexed property! listing parameters instead]");
								result.Add(indexedPropContentInfo);
								List<ObjectContentInfo> parameterInfo = new List<ObjectContentInfo>();
								foreach(ParameterInfo parameter in parameters){
									string defaultParamValue = parameter.HasDefaultValue ? parameter.DefaultValue.ToString() : "none";
									parameterInfo.Add(new ObjectContentInfo(parameter.ParameterType.Name, parameter.Name, "default: " + defaultParamValue));
								}
								indexedPropContentInfo.contentInfo = parameterInfo;
							}else{
								object propertyValue = propertyInfo.GetValue(value);
								if (propertyValue != null && !propertyValue.GetType().IsAssignableFrom(type))
								{
									MemoryProfiler.ObjectContentInfo propContentInfo = new MemoryProfiler.ObjectContentInfo(propertyInfo.PropertyType.Name, propertyInfo.Name, propertyValue);
									result.Add(propContentInfo);
									propContentInfo.contentInfo = MemoryProfiler.buildContentInfoRecursive(propertyValue, depth + 1, objectHistory);
								}
							}
						}
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("reflection usage failed, error: " + e.ToString());
			return new List<ObjectContentInfo>(){new ObjectContentInfo("error", "encountered error", e)};
		}
		return result;
	}

	// Token: 0x06007FEE RID: 32750
	private void RenderGUI_List(List<MemoryProfiler.ObjMemDef> mListDef)
	{
		this.mScrollViewPos_Stats = GUILayout.BeginScrollView(this.mScrollViewPos_Stats, new GUILayoutOption[]
		{
			GUILayout.ExpandHeight(true)
		});
		float lineHeight = GUI.skin.button.lineHeight + (float)GUI.skin.button.margin.top + (float)GUI.skin.button.padding.top + (float)GUI.skin.button.padding.bottom;
		int viewportStartLine = (int)(this.mScrollViewPos_Stats.y / lineHeight);
		int viewportEndLine = viewportStartLine + (int)((float)Screen.height / lineHeight);
		int extraSpacingLines = 0;
		int renderedLines = 0;
		TextAnchor alignment = GUI.skin.label.alignment;
		GUI.skin.label.alignment = TextAnchor.MiddleRight;
		int count = mListDef.Count;
		bool doReflect = false;
		string filterText = "";
		string[] reflectPath = null;
		if (!string.IsNullOrEmpty(this.mFilter_Text))
		{
			doReflect = this.mFilter_Text.EndsWith("@reflect");
			if (doReflect)
			{
				reflectPath = this.mFilter_Text.Replace("@reflect", "").ToLowerInvariant().Split(new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries);
			}
			else
			{
				filterText = this.mFilter_Text.ToLowerInvariant();
			}
		}
		for (int i = 0; i < count; i++)
		{
			MemoryProfiler.ObjMemDef objMemDef = mListDef[i];
			if (doReflect)
			{
				if (objMemDef._Name.ToLowerInvariant().Equals(reflectPath[0]) || objMemDef._ObjType.ToLowerInvariant().Equals(reflectPath[0]))
				{
					this.RenderContentInfoRecursive(objMemDef.GetContentInfo(), 0, 20f, ref renderedLines, viewportStartLine, viewportEndLine, ref extraSpacingLines, lineHeight, reflectPath);
				}
			}
			else if ((this.mFilter_ShowDependencies || !objMemDef._IsADependency) && (filterText.Equals("") || objMemDef._Name.ToLowerInvariant().Contains(filterText) || objMemDef._ObjType.ToLowerInvariant().Contains(filterText)))
			{
				if (renderedLines > 0 && (renderedLines < viewportStartLine || renderedLines > viewportEndLine))
				{
					extraSpacingLines++;
				}
				else
				{
					if (extraSpacingLines > 0)
					{
						GUILayout.Space((float)extraSpacingLines * lineHeight);
						extraSpacingLines = 0;
					}
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label((objMemDef._Size < 1048576) ? ((objMemDef._Size / 1024).ToString() + " Kb") : ((objMemDef._Size / 1048576).ToString("0.00") + " Mb"), new GUILayoutOption[]
					{
						GUILayout.Width(80f)
					});
					if(GUILayout.Button("reflect on type", new GUILayoutOption[]{GUILayout.ExpandWidth(false)})){
						this.mFilter_Text = objMemDef._ObjType + "@reflect";
					}
					if(GUILayout.Button("reflect on name", new GUILayoutOption[]{GUILayout.ExpandWidth(false)})){
						this.mFilter_Text = objMemDef._Name + "@reflect";
					}
					if (GUILayout.Button(objMemDef._ObjType, new GUILayoutOption[]{GUILayout.Width(200f)})){
						this.mFilter_Text = objMemDef._ObjType;
					}
					if (GUILayout.Button(objMemDef._Name, Array.Empty<GUILayoutOption>())){
						this.mFilter_Text = objMemDef._Name;
					}
					GUILayout.EndHorizontal();
				}
				renderedLines++;
			}
		}
		GUI.skin.label.alignment = alignment;
		if (extraSpacingLines > 0)
		{
			GUILayout.Space((float)extraSpacingLines * lineHeight);
		}
		GUILayout.EndScrollView();
	}

	// Token: 0x06007FF6 RID: 32758
	private void AddLastSnapShotElement(ref Dictionary<string, List<MemoryProfiler.ObjMemDef>> LastSnapshot, MemoryProfiler.ObjMemDef ObjDef)
	{
		List<MemoryProfiler.ObjMemDef> list = null;
		if (!LastSnapshot.TryGetValue(ObjDef._Name, out list))
		{
			list = new List<MemoryProfiler.ObjMemDef>();
			LastSnapshot[ObjDef._Name] = list;
		}
		list.Add(ObjDef);
	}

	// Token: 0x06007FF7 RID: 32759
	private bool RemoveLastSnapShotElement(ref Dictionary<string, List<MemoryProfiler.ObjMemDef>> LastSnapshot, MemoryProfiler.ObjMemDef ObjDef)
	{
		List<MemoryProfiler.ObjMemDef> list = null;
		if (!LastSnapshot.TryGetValue(ObjDef._Name, out list))
		{
			return false;
		}
		int num = list.FindIndex((MemoryProfiler.ObjMemDef p) => ObjDef._ObjType == p._ObjType);
		if (num < 0)
		{
			return false;
		}
		list.RemoveAt(num);
		return true;
	}

	// Token: 0x0600806E RID: 32878
	private void RenderContentInfoRecursive(MemoryProfiler.ObjectContentInfo content, int depth, float spaceWidth, ref int renderedLines, int viewPortStartLine, int viewPortEndLine, ref int skippedLines, float lineHeight, string[] reflectPath)
	{
		if (content == null)
		{
			return;
		}
		if (renderedLines == 0 || (renderedLines <= viewPortEndLine && renderedLines >= viewPortStartLine))
		{
			if (skippedLines > 0)
			{
				GUILayout.Space(lineHeight * (float)skippedLines);
				skippedLines = 0;
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Space(spaceWidth * (float)depth);
			if (content.contentInfo != null)
			{
				if (GUILayout.Button(content.isFolded ? ">" : "v", new GUILayoutOption[]
				{
					GUILayout.Width(20f)
				}))
				{
					content.isFolded = !content.isFolded;
				}
			}
			else
			{
				GUILayout.Box("o", new GUILayoutOption[]
				{
					GUILayout.Width(20f)
				});
			}
			GUILayout.Box(content.typeText, this.boxReflectFontStyle, new GUILayoutOption[]
			{
				GUILayout.Width(200f)
			});
			GUILayout.Box(content.nameText, this.boxReflectFontStyle, new GUILayoutOption[]
			{
				GUILayout.Width(200f)
			});
			GUILayout.Box(content.valueText, this.boxReflectFontStyle, Array.Empty<GUILayoutOption>());
			GUILayout.EndHorizontal();
		}
		else
		{
			skippedLines++;
		}
		renderedLines++;
		if (content.contentInfo != null && !content.isFolded)
		{
			string pathTypeString = null;
			string pathNameString = null;
			if(reflectPath.Length > (depth + 1)) {
				string pathString = reflectPath[depth + 1];
				if(pathString.EndsWith("@type")) {
					pathTypeString = pathString.Substring(0, pathString.Length - "@type".Length);
				} else {
					pathNameString = pathString;
				}

				if(string.Equals(pathTypeString, "*")) {
					pathTypeString = null;
				}
				if(string.Equals(pathNameString, "*")) {
					pathNameString = null;
				}
			}
			foreach (MemoryProfiler.ObjectContentInfo innerContent in content.contentInfo)
			{
				if(pathTypeString != null && !InputPartiallyMatches(pathTypeString, innerContent.typeString)) {
					continue;
				}
				if(pathNameString != null && !InputPartiallyMatches(pathNameString, innerContent.nameString)) {
					continue;
				}
				this.RenderContentInfoRecursive(innerContent, depth + 1, spaceWidth, ref renderedLines, viewPortStartLine, viewPortEndLine, ref skippedLines, lineHeight, reflectPath);
			}
		}
	}

	/// <summary>
	/// checks whether the inputString contains characters in the same sequence as the targetString
	/// </summary>
	/// <param name="input"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public static bool InputPartiallyMatches(string input, string target) {
		char[] inputChars = input.ToCharArray();
		int inputCharsLength = inputChars.GetLength(0);
		char[] targetChars = target.ToCharArray();
		int targetCharsLength = targetChars.GetLength(0);
		int targetIndex = 0;
		for(int inputIndex = 0; inputIndex < inputCharsLength; inputIndex++) {
			char inputChar = char.ToUpperInvariant(inputChars[inputIndex]);
			while(inputChar != char.ToUpperInvariant(targetChars[targetIndex])) {
				targetIndex++;
				if(targetIndex >= targetCharsLength) {
					// unable to find matching character in target
					return false;
				}
			}
			targetIndex++;
			if(targetIndex >= targetCharsLength && (inputIndex + 1) < inputCharsLength) {
				// reached end of target string, but still have input chars to check
				return false;
			}
		}
		return true;
	}

	// Token: 0x04000784 RID: 1924
	private List<KeyValuePair<string, int>> mStats_Current1 = new List<KeyValuePair<string, int>>();

	// Token: 0x04000785 RID: 1925
	private Dictionary<string, int> mStats_Current = new Dictionary<string, int>();

	// Token: 0x04000787 RID: 1927
	private List<KeyValuePair<string, int>> mStats_Dif1;

	// Token: 0x04000788 RID: 1928
	private Dictionary<string, int> mStats_Dif;

	// Token: 0x0400078B RID: 1931
	public GameObject _OwnerGO;

	// Token: 0x0400078C RID: 1932
	private Vector2 mScrollViewPos_Stats;

	// Token: 0x0400078D RID: 1933
	private MemoryProfiler.StatsViewTab mStatsViewIndex;

	// Token: 0x0400078E RID: 1934
	private string mFilter_Text;

	private static bool reflectOnProperties = false;

	// Token: 0x0400078F RID: 1935
	private bool mFilter_ShowDependencies;

	// Token: 0x04000790 RID: 1936
	private bool mCollapsed;

	// Token: 0x040083A9 RID: 33705
	private GUIStyle boxReflectFontStyle;

	// Token: 0x04008516 RID: 34070
	private List<MemoryProfiler.ObjMemDef> mList_Snapshot = new List<MemoryProfiler.ObjMemDef>();

	// Token: 0x04008519 RID: 34073
	private List<MemoryProfiler.ObjMemDef> mList_Differences;

	// Token: 0x0400851A RID: 34074
	private List<MemoryProfiler.ObjMemDef> mList_LastSnapshot;

	// Token: 0x02000148 RID: 328
	private class ObjMemDef
	{
		// Token: 0x06007D1E RID: 32030
		public ObjMemDef(int Size, string ObjType, string Name, bool IsADependency, object instance)
		{
			this._Size = Size;
			this._ObjType = ObjType;
			this._Name = Name;
			this._IsADependency = IsADependency;
			this.instance = instance;
			this.contentLoaded = false;
			this.contentInfo = null;
		}

		// Token: 0x06007EE0 RID: 32480
		public MemoryProfiler.ObjectContentInfo GetContentInfo()
		{
			if (!this.contentLoaded)
			{
				this.contentInfo = MemoryProfiler.buildContentInfo(this.instance);
				this.contentLoaded = true;
			}
			return this.contentInfo;
		}

		// Token: 0x06007FFE RID: 32766
		public int CompareTo(ref MemoryProfiler.ObjMemDef other)
		{
			if (this._Size != other._Size)
			{
				return this._Size - other._Size;
			}
			if (this._IsADependency != other._IsADependency)
			{
				return (this._IsADependency ? 1 : 0) - (other._IsADependency ? 1 : 0);
			}
			int num = string.Compare(this._Name, other._Name);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this._ObjType, other._ObjType);
		}

		// Token: 0x04000791 RID: 1937
		public int _Size;

		// Token: 0x04000792 RID: 1938
		public bool _IsADependency;

		// Token: 0x04000793 RID: 1939
		public string _ObjType;

		// Token: 0x04000794 RID: 1940
		public string _Name;

		// Token: 0x04008306 RID: 33542
		private MemoryProfiler.ObjectContentInfo contentInfo;

		// Token: 0x040083DB RID: 33755
		private object instance;

		// Token: 0x040083DC RID: 33756
		private bool contentLoaded;
	}

	// Token: 0x02000149 RID: 329
	private enum StatsViewTab
	{
		// Token: 0x04000796 RID: 1942
		CURRENT_STATS,
		// Token: 0x04000797 RID: 1943
		CURRENT_OBJECTS,
		// Token: 0x04000798 RID: 1944
		DIF_STATS,
		// Token: 0x04000799 RID: 1945
		DIF_OBJECTS
	}

	// Token: 0x02001525 RID: 5413
	private class ObjectContentInfo
	{
		// Token: 0x06007E00 RID: 32256
		public ObjectContentInfo(string typeName, string displayName, object value)
		{
			this.typeString = typeName;
			this.nameString = displayName;
			this.typeText = "type: " + typeName;
			this.nameText = "name: " + displayName;
			this.valueText = ((value == null) ? "null" : value.ToString());
			this.isFolded = false;
		}

		// Token: 0x040082F4 RID: 33524
		public List<MemoryProfiler.ObjectContentInfo> contentInfo;

		public string typeString;
		public string typeText;

		public string nameString;
		public string nameText;

		// Token: 0x04008428 RID: 33832
		public string valueText;

		// Token: 0x0400846C RID: 33900
		public bool isFolded;
	}
}
