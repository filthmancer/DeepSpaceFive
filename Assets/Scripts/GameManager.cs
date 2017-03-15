using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	void Awake()
	{
		instance = this;
	}

	public static PlayerControl [] Players
	{
		get{return instance._Players;}
	}
	[SerializeField]
	private PlayerControl [] _Players;

	private int CreatePlayers = 1;

	public Camera MainCam;
	public PlayerControl PlayerObj;
	public GameObject FieldObj, DeathObj;

	public Color [] Colors;
	public Image [] FuelCounters;
	public Image [] ChargeCounters;

	public GameObject Grid;

	public static float SolarRate = 0.01F;

	public GravityField InitField, Field2;
	// Use this for initialization
	void Start () {
		_Players = new PlayerControl[CreatePlayers];
		for(int i = 0; i < _Players.Length; i++)
		{
			Players[i] = CreatePlayerInOrbit(InitField);
			Players[i].TailColor = Colors[i];
			Players[i].Name = "p" + (i+1);
		}
		
	}
	Vector3 CamOffset = new Vector3(0, 54, -38);
	// Update is called once per frame
	void Update () {
		var materialProperty = new MaterialPropertyBlock();
		Vector4[] vecArray= new Vector4[] {
			new Vector4(InitField.transform.position.x,
						InitField.Mass,
						InitField.transform.position.z, 0.0F),
			new Vector4(Field2.transform.position.x,
						Field2.Mass,
						Field2.transform.position.z, 0.0F)
		};

		materialProperty.SetVectorArray("planets", vecArray);
		Grid.GetComponent<Renderer> ().SetPropertyBlock (materialProperty);
		Vector3 CamPos = Vector3.zero;
		for(int i = 0; i < _Players.Length; i++)
		{
			if(_Players[i] != null)
			CamPos = Vector3.Lerp(CamPos, _Players[i].transform.position, 0.5F);

			FuelCounters[i].fillAmount = _Players[i].Fuel.Current / 10.0F;
			FuelCounters[i].color = Colors[i];
			ChargeCounters[i].fillAmount = _Players[i].Charge.Current / 100.0F;
		}

		CamPos += CamOffset;
		//MainCam.transform.position = Vector3.Lerp(MainCam.transform.position, CamPos, Time.deltaTime);
	}

	public void Destroy(PlayerControl c)
	{
		int num = 0;
		for(int i = 0; i < _Players.Length; i++)
		{
			if(_Players[i] == c) 
			{
				num = i;
			}
		}

		Destroy(c.gameObject);
		Players[num] = CreatePlayerInOrbit(InitField);
		Players[num].TailColor = Colors[num];
		Players[num].Name = "p"+(num+1);
	}

	public PlayerControl CreatePlayerInOrbit(GravityField f)
	{
		PlayerControl p =  (PlayerControl) Instantiate(PlayerObj);

		float dist = Random.Range(f.NearRadius * 3, f.FarRadius*0.7F);
		Vector3 vel = Utility.RandomVectorInclusive(1,1,1).normalized;
		Vector3 pos = f.transform.position + (vel * dist);
		pos.y = 0.0F;
		p.transform.position = pos;

		Vector3 pvel = f.transform.position - p.transform.position;
		pvel.Normalize();
		Vector3 bvel = Vector3.up;


		Vector3 dotdir = Vector3.Cross(pvel, bvel).normalized;
		p.SetControlVel(dotdir, 80);
		return p;
	}
}
