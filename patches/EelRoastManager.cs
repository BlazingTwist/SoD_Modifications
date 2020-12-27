using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000FC RID: 252
public class EelRoastManager : KAMonoBase
{
	// Token: 0x06000682 RID: 1666 RVA: 0x0000A033 File Offset: 0x00008233
	private void Start()
	{
		this.SpawnEels();
	}

	// Token: 0x06000683 RID: 1667 RVA: 0x0006BCEC File Offset: 0x00069EEC
	private void SpawnEels()
	{
		if(BTDebugCamInputManager.GetConfigHolder().hackConfig != null && BTDebugCamInputManager.GetConfigHolder().hackConfig.eelRoast_spawnAllEels){
			foreach(EelRoastMarkerInfo markerInfo in this._EelRoastInfos){
				string randomEelPath = this.GetRandomEelPath(markerInfo);
				if(!string.IsNullOrEmpty(randomEelPath)){
					string[] array = randomEelPath.Split(new char[]
					{
						'/'
					});
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], new RsResourceEventHandler(this.ResourceEventHandler), typeof(GameObject), false, markerInfo);
				}else{
					UtDebug.Log("Eel Asset path is empty ");
				}
			}
		}else{
			if (this._EelRoastInfos == null || this._EelRoastInfos.Length < 1 || this._NoOfEelsToSpawn.Min < 1f || this._NoOfEelsToSpawn.Min > this._NoOfEelsToSpawn.Max)
			{
				return;
			}
			List<EelRoastMarkerInfo> list = new List<EelRoastMarkerInfo>(this._EelRoastInfos);
			int num = Mathf.Clamp(this._NoOfEelsToSpawn.GetRandomInt(), 1, this._EelRoastInfos.Length);
			for (int i = 0; i < num; i++)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				string randomEelPath = this.GetRandomEelPath(list[index]);
				if (!string.IsNullOrEmpty(randomEelPath))
				{
					string[] array = randomEelPath.Split(new char[]
					{
						'/'
					});
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], new RsResourceEventHandler(this.ResourceEventHandler), typeof(GameObject), false, list[index]);
				}
				else
				{
					UtDebug.Log("Eel Asset path is empty ");
				}
				list.RemoveAt(index);
			}
		}
	}

	// Token: 0x06000684 RID: 1668 RVA: 0x0006BDF8 File Offset: 0x00069FF8
	private void ResourceEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			if (inEvent == RsResourceLoadEvent.ERROR)
			{
				UtDebug.LogError("ERROR: CHEST MANAGER UNABLE TO LOAD RESOURCE AT: " + inURL);
			}
			return;
		}
		if (inObject == null || inUserData == null)
		{
			UtDebug.LogError("ERROR: Eel's inObject or inUserData is null");
			return;
		}
		EelRoastMarkerInfo eelRoastMarkerInfo = inUserData as EelRoastMarkerInfo;
		this.SetUpEel((GameObject)inObject, inURL, eelRoastMarkerInfo._SpawnNode);
	}

	// Token: 0x06000685 RID: 1669 RVA: 0x0006BE50 File Offset: 0x0006A050
	private string GetRandomEelPath(EelRoastMarkerInfo eelRoastMarkerInfo)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < eelRoastMarkerInfo._EelTypes.Length; i++)
		{
			num += eelRoastMarkerInfo._EelTypes[i]._SpawnChance;
		}
		int num3 = UnityEngine.Random.Range(0, num);
		for (int j = 0; j < eelRoastMarkerInfo._EelTypes.Length; j++)
		{
			if (eelRoastMarkerInfo._EelTypes[j]._SpawnChance != 0 && num3 >= num2 && num3 < num2 + eelRoastMarkerInfo._EelTypes[j]._SpawnChance)
			{
				return eelRoastMarkerInfo._EelTypes[j]._AssetPath;
			}
			num2 += eelRoastMarkerInfo._EelTypes[j]._SpawnChance;
		}
		return string.Empty;
	}

	// Token: 0x06000686 RID: 1670 RVA: 0x0006BEF0 File Offset: 0x0006A0F0
	private void SetUpEel(GameObject eelObject, string inURL, Transform spawnNode)
	{
		if (eelObject == null || spawnNode == null)
		{
			UtDebug.Log("Eel object or SpawnNode is null");
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(eelObject, spawnNode.position, spawnNode.rotation);
		string[] array = inURL.Split(new char[]
		{
			'/'
		});
		gameObject.name = array[2];
	}

	// Token: 0x06000687 RID: 1671 RVA: 0x00008726 File Offset: 0x00006926
	public EelRoastManager()
	{
	}

	// Token: 0x040005D0 RID: 1488
	public EelRoastMarkerInfo[] _EelRoastInfos;

	// Token: 0x040005D1 RID: 1489
	public MinMax _NoOfEelsToSpawn;
}
