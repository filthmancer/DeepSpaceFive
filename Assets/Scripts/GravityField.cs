using UnityEngine;
using System.Collections;

public class GravityField : MonoBehaviour {
	//power of gravity pull
	public AnimationCurve GravityCurve;
	public float Mass = 1.0F;
	public GameObject Model;
	//Radiuses of gravity pull
	public float FarRadius = 5.0F, NearRadius = 1.0F;

	[SerializeField]
	private SphereCollider _far, _near;
	public SphereCollider Far{
		get{return _far;}
	}
	public SphereCollider Near{
		get{return _near;}
	}

	private Transform trans;

	// Use this for initialization
	void Start () {

		trans = this.transform;
		if(!_far)
		{
			GameObject f = (GameObject) Instantiate(GameManager.instance.FieldObj);//new GameObject("Far Collider");
			f.transform.localScale = new Vector3(FarRadius*2, FarRadius*2, 0.05F);
			f.transform.SetParent(trans);
			f.transform.position = trans.position + Vector3.down;
			_far = f.GetComponent<SphereCollider>();
		}
		//_far.radius = FarRadius;
		_far.isTrigger = true;

		if(!_near)
		{
			GameObject n = (GameObject) Instantiate(GameManager.instance.DeathObj);
			n.transform.localScale = new Vector3(NearRadius*2, NearRadius*2, 0.05F);
			n.transform.SetParent(trans);
			n.transform.position = trans.position;
			_near = n.GetComponent<SphereCollider>();
		}
		//_near.radius = NearRadius;
		//_near.isTrigger = true;
	}

	void Update()
	{
		Model.transform.Rotate(0, 0.4F,0);
		for(int i = 0; i < GameManager.Players.Length; i++)
		{
			if(GameManager.Players[i] == null) continue;
			float dist = Vector3.Distance(trans.position, GameManager.Players[i].transform.position);
			if(dist < FarRadius)
			{
				GameManager.Players[i].SetField(this);
			}

		}
	}

	public float PowerAtDist(Vector3 pos)
	{
		float dist = RadialDist(pos);//Vector3.Distance(pos, trans.position);
		return GravityCurve.Evaluate(dist) * Mass;
	}

	public float RadialDist(Vector3 pos)
	{
		float d = Vector3.Distance(pos, trans.position);
		return (d-NearRadius) / (FarRadius - NearRadius);
	}

	public float SolarPower(Vector3 pos)
	{
		float dist = RadialDist(pos);
		return (GameManager.SolarRate * Mass) / dist;
	}


}
