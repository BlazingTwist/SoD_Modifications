using System;
using UnityEngine;

// Token: 0x020000F8 RID: 248
public class EREel : KAMonoBase
{
	// Token: 0x170000AF RID: 175
	// (get) Token: 0x06000672 RID: 1650 RVA: 0x00009F62 File Offset: 0x00008162
	private Transform mEelTransform
	{
		get
		{
			return this._RenderPath.transform;
		}
	}

	// Token: 0x06000673 RID: 1651 RVA: 0x00009F6F File Offset: 0x0000816F
	private void Start()
	{
		if (this._SndWaterSplash != null)
		{
			this._Channel.pClip = this._SndWaterSplash;
		}
		this.Initialize();
	}

	// Token: 0x06000674 RID: 1652 RVA: 0x0006B5BC File Offset: 0x000697BC
	public void Initialize()
	{
		this._RenderPath.gameObject.SetActive(this._Active);
		this.mEelTransform.position = this._StartPoint.position;
		this.mEelTransform.rotation = this._StartPoint.rotation;
		this.mRotFrom.eulerAngles = this.mEelTransform.rotation.eulerAngles;
		Vector3 eulerAngles = this.mEelTransform.rotation.eulerAngles;
		eulerAngles.x = -this.mEelTransform.rotation.eulerAngles.x;
		this.mRotTo.eulerAngles = eulerAngles;
		this._EelTargetable._Active = false;
		this._DragonFiringCSM.enabled = false;
		this.StartJumpIntervalTimer();
	}

	// Token: 0x06000675 RID: 1653 RVA: 0x0006B688 File Offset: 0x00069888
	private void Update()
	{
		if (base.gameObject == null || this.mIsDestroying)
		{
			return;
		}
		if (this._Active)
		{
			Animation eelAnim = this._EelAnim;
			if (!this.mPlayedPartForGoingAbove && this.MultiplyVectors(this.mEelTransform.position, Vector3.up) > this.MultiplyVectors(this._WaterHeightMarker.transform.position, Vector3.up))
			{
				this.PlaySplashEffects();
				eelAnim.CrossFade(this._LaunchAnimName, 0.3f);
				eelAnim[this._LaunchAnimName].wrapMode = WrapMode.Once;
				this.mPlayedPartForGoingAbove = true;
			}
			if (this.mPlayedPartForGoingAbove && !this.mPlayedPartForGoingBelow)
			{
				if (!this.mIdleAnimationPlayed && eelAnim.IsPlaying(this._LaunchAnimName) && eelAnim[this._LaunchAnimName].time + this._LauchCrossFadeTime > eelAnim[this._LaunchAnimName].length)
				{
					this.mIdleAnimationPlayed = true;
					eelAnim.CrossFade(this._IdleAnimName, this._LauchCrossFadeTime);
					eelAnim[this._IdleAnimName].wrapMode = WrapMode.Loop;
				}
				if (this.MultiplyVectors(this.mEelTransform.position, Vector3.up) < this.MultiplyVectors(this._WaterHeightMarker.transform.position, Vector3.up))
				{
					this.PlaySplashEffects();
					this.mPlayedPartForGoingBelow = true;
				}
			}
			if (this.mLamdaPosition < 1f)
			{
				this.mEelTransform.rotation = Quaternion.Lerp(this.mRotFrom, this.mRotTo, this.mLamdaPosition);
				this.mEelTransform.position = EREel.DoBezier(this._StartPoint.position, this.mControlPoint, this._EndPoint.position, this.mLamdaPosition);
				if (this.mLamdaPosition > 0.5f && this.mDelayTime < this._DelayOnTop)
				{
					this.mDelayTime += Time.deltaTime;
				}
				else
				{
					this.mLamdaPosition += this._Speed * Time.deltaTime;
				}
				if (this.mLamdaPosition >= 1f)
				{
					this.StartJumpIntervalTimer();
					this._EelTargetable._Active = false;
					this._DragonFiringCSM.enabled = false;
				}
			}
			if (this.mStartTimer)
			{
				this.mJumpTimer -= Time.deltaTime;
				if (this.mJumpTimer < 0f)
				{
					this.MakeAJump();
				}
			}
		}
	}

	// Token: 0x06000676 RID: 1654 RVA: 0x0006B8F0 File Offset: 0x00069AF0
	private void PlaySplashEffects()
	{
		if (this._WaterSplashEffects != null)
		{
			GameObject gameObject = this._WaterSplashEffects[UnityEngine.Random.Range(0, this._WaterSplashEffects.Length)];
			UnityEngine.Object.Instantiate<GameObject>(gameObject, this.mEelTransform.position, gameObject.transform.rotation).transform.parent = this._PrtParent.transform;
			SnChannel channel = this._Channel;
			if (channel == null)
			{
				return;
			}
			channel.Play();
		}
	}

	// Token: 0x06000677 RID: 1655 RVA: 0x00009F96 File Offset: 0x00008196
	private void StartJumpIntervalTimer()
	{
		this.mLamdaPosition = 1f;
		this.mJumpTimer = UnityEngine.Random.Range(1f, this._JumpIntervalTime);
		this.mStartTimer = true;
	}

	// Token: 0x06000678 RID: 1656 RVA: 0x0006B95C File Offset: 0x00069B5C
	private void MakeAJump()
	{
		this._TopPoint = (this._StartPoint.position + this._EndPoint.position) / 2f + this._StartPoint.up * UnityEngine.Random.Range(this._MinHeight, this._MaxHeight);
		this.mControlPoint = EREel.DoBezierReverse(this._StartPoint.position, this._TopPoint, this._EndPoint.position);
		this.mStartTimer = (this.mPlayedPartForGoingAbove = (this.mPlayedPartForGoingBelow = (this.mIdleAnimationPlayed = false)));
		this.mEelTransform.LookAt(this._TopPoint);
		this.mDelayTime = (this.mLamdaPosition = 0f);
		this._EelTargetable._Active = true;
		this._DragonFiringCSM.enabled = true;
	}

	// Token: 0x06000679 RID: 1657 RVA: 0x0006BA40 File Offset: 0x00069C40
	public void OnEelHit()
	{
		if(BTDebugCamInputManager.GetConfigHolder().hackConfig != null && BTDebugCamInputManager.GetConfigHolder().hackConfig.eelRoast_infiniteEels){
			SanctuaryManager.pCurPetInstance.UpdateMeter(SanctuaryPetMeterType.HAPPINESS, this._PetHappiness);
			if (this._EelBlastColors != null && this._EelBlastColors.Length != 0 && this._EelBlastEffectObj != null)
			{
				int num = UnityEngine.Random.Range(0, this._EelBlastColors.Length);
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._EelBlastEffectObj, this.mEelTransform.position, this._EelBlastEffectObj.transform.rotation);
				gameObject.transform.parent = this._PrtParent.transform;
				ParticleSystem.MainModule main = gameObject.GetComponent<ParticleSystem>().main;
				main.startColor = this._EelBlastColors[num];
				if (this._EelHit3DScore != null)
				{
					Vector3 position = SanctuaryManager.pCurPetInstance.GetHeadPosition() + this._HappinessTextDragonOffset;
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this._EelHit3DScore.gameObject, position, this._EelHit3DScore.transform.rotation);
					TargetHit3DScore component = gameObject2.GetComponent<TargetHit3DScore>();
					component.mDisplayScore = (int)this._PetHappiness;
					component.mDisplayText = this._PetHappinessText._Text;
					gameObject2.transform.parent = this._PrtParent.transform;
				}
			}
		}else{
			if (this.mIsDestroying)
			{
				return;
			}
			this._Active = false;
			this._RenderPath.gameObject.SetActive(false);
			this.mIsDestroying = true;
			SanctuaryManager.pCurPetInstance.UpdateMeter(SanctuaryPetMeterType.HAPPINESS, this._PetHappiness);
			if (this._EelBlastColors != null && this._EelBlastColors.Length != 0 && this._EelBlastEffectObj != null)
			{
				int num = UnityEngine.Random.Range(0, this._EelBlastColors.Length);
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._EelBlastEffectObj, this.mEelTransform.position, this._EelBlastEffectObj.transform.rotation);
				gameObject.transform.parent = this._PrtParent.transform;
				ParticleSystem.MainModule main = gameObject.GetComponent<ParticleSystem>().main;
				main.startColor = this._EelBlastColors[num];
				if (this._EelHit3DScore != null)
				{
					Vector3 position = SanctuaryManager.pCurPetInstance.GetHeadPosition() + this._HappinessTextDragonOffset;
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this._EelHit3DScore.gameObject, position, this._EelHit3DScore.transform.rotation);
					TargetHit3DScore component = gameObject2.GetComponent<TargetHit3DScore>();
					component.mDisplayScore = (int)this._PetHappiness;
					component.mDisplayText = this._PetHappinessText._Text;
					gameObject2.transform.parent = this._PrtParent.transform;
				}
			}
			if (base.gameObject != null)
			{
				UnityEngine.Object.Destroy(base.gameObject, this._DestroyDelay);
			}
		}
	}

	// Token: 0x0600067A RID: 1658 RVA: 0x0006BBB8 File Offset: 0x00069DB8
	public static Vector3 DoBezier(Vector3 Start, Vector3 Control, Vector3 End, float lamda)
	{
		if (lamda <= 0f)
		{
			return Start;
		}
		if (lamda >= 1f)
		{
			return End;
		}
		return (1f - lamda) * (1f - lamda) * Start + 2f * lamda * (1f - lamda) * Control + lamda * lamda * End;
	}

	// Token: 0x0600067B RID: 1659 RVA: 0x00009FC0 File Offset: 0x000081C0
	public static Vector3 DoBezierReverse(Vector3 Start, Vector3 Mid, Vector3 End)
	{
		return (Mid - 0.25f * Start - 0.25f * End) / 0.5f;
	}

	// Token: 0x0600067C RID: 1660 RVA: 0x00009FED File Offset: 0x000081ED
	private float MultiplyVectors(Vector3 a, Vector3 b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z;
	}

	// Token: 0x0600067D RID: 1661 RVA: 0x0006BC18 File Offset: 0x00069E18
	public EREel()
	{
	}

	// Token: 0x040005A4 RID: 1444
	public bool _Active;

	// Token: 0x040005A5 RID: 1445
	public Animation _EelAnim;

	// Token: 0x040005A6 RID: 1446
	public Transform _RenderPath;

	// Token: 0x040005A7 RID: 1447
	public GameObject _PrtParent;

	// Token: 0x040005A8 RID: 1448
	public AudioClip _SndWaterSplash;

	// Token: 0x040005A9 RID: 1449
	public GameObject _WaterHeightMarker;

	// Token: 0x040005AA RID: 1450
	public GameObject[] _WaterSplashEffects;

	// Token: 0x040005AB RID: 1451
	public Color[] _EelBlastColors;

	// Token: 0x040005AC RID: 1452
	public GameObject _EelBlastEffectObj;

	// Token: 0x040005AD RID: 1453
	public TargetHit3DScore _EelHit3DScore;

	// Token: 0x040005AE RID: 1454
	public EREelTargetable _EelTargetable;

	// Token: 0x040005AF RID: 1455
	public DragonFiringCSM _DragonFiringCSM;

	// Token: 0x040005B0 RID: 1456
	public SnChannel _Channel;

	// Token: 0x040005B1 RID: 1457
	public Transform _StartPoint;

	// Token: 0x040005B2 RID: 1458
	public Transform _EndPoint;

	// Token: 0x040005B3 RID: 1459
	public string _LaunchAnimName = "Launch";

	// Token: 0x040005B4 RID: 1460
	public string _IdleAnimName = "Idle";

	// Token: 0x040005B5 RID: 1461
	public float _Speed;

	// Token: 0x040005B6 RID: 1462
	public float _DelayOnTop;

	// Token: 0x040005B7 RID: 1463
	public float _DestroyDelay = 1f;

	// Token: 0x040005B8 RID: 1464
	public float _LauchCrossFadeTime = 0.3f;

	// Token: 0x040005B9 RID: 1465
	public float _JumpIntervalTime = 3f;

	// Token: 0x040005BA RID: 1466
	public float _MinHeight;

	// Token: 0x040005BB RID: 1467
	public float _MaxHeight;

	// Token: 0x040005BC RID: 1468
	public float _PetHappiness;

	// Token: 0x040005BD RID: 1469
	public LocaleString _PetHappinessText = new LocaleString("Happiness");

	// Token: 0x040005BE RID: 1470
	public Vector3 _HappinessTextDragonOffset;

	// Token: 0x040005BF RID: 1471
	private bool mIsDestroying;

	// Token: 0x040005C0 RID: 1472
	private Vector3 mControlPoint = Vector3.zero;

	// Token: 0x040005C1 RID: 1473
	private float mLamdaPosition;

	// Token: 0x040005C2 RID: 1474
	private float mDelayTime;

	// Token: 0x040005C3 RID: 1475
	private bool mPlayedPartForGoingAbove;

	// Token: 0x040005C4 RID: 1476
	private bool mPlayedPartForGoingBelow;

	// Token: 0x040005C5 RID: 1477
	private bool mIdleAnimationPlayed;

	// Token: 0x040005C6 RID: 1478
	private Vector3 _TopPoint;

	// Token: 0x040005C7 RID: 1479
	private Quaternion mRotFrom;

	// Token: 0x040005C8 RID: 1480
	private Quaternion mRotTo;

	// Token: 0x040005C9 RID: 1481
	private bool mStartTimer;

	// Token: 0x040005CA RID: 1482
	private float mJumpTimer;
}
