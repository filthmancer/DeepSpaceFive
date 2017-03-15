using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour {

	private Transform trans;
	public string Name;
	public Color TailColor;
	// Use this for initialization
	void Start () {
		trans = this.transform;
		VelocityParticles.startColor = TailColor;
		Fuel.Max = Fuel.Current;
	}

	private Vector3 lastvel;
	[SerializeField]
	private Vector3 actual_vel;

	public PlayerProps Properties;
	public FloatProps Charge
	{
		get{return Properties._Charge;}
	}

	public FloatProps Fuel
	{
		get{return Properties._Fuel;}
	}
	private VelProps Gravity
	{
		get{return Properties._Gravity;}
	}
	private VelProps Control
	{
		get{return Properties._Control;}
	}

	public ParticleSystem VelocityParticles;
	// Update is called once per frame
	void Update () {
		
		for(int i = 0; i < Fields.Count; i++)
		{
			float d = Fields[i].RadialDist(trans.position);
			if(d >= 1.0F)
			{
				Fields.RemoveAt(i);
				i--;
			}
			else 
			{

				Fuel.Current = Mathf.Clamp(Fuel.Current+Fields[i].SolarPower(trans.position), 0.0F, Fuel.Max);
			}
			//
		} 
		Fuel.Current  = Mathf.Clamp(Fuel.Current + Fuel.RechargeRate, 0.0F, Fuel.Max);
		if(Input.GetButtonDown("Destruct_" + Name)) Destroy();
		Velocity();
	}



	public List<GravityField> Fields = new List<GravityField>();
	//public GravityField Field;
	public void SetField(GravityField g)
	{
		if(!Fields.Contains(g)) 	Fields.Add(g);
		//if(Field == null || Field.PowerAtDist(trans.position) < g.PowerAtDist(trans.position))
		//{
		//	Field = g;
		//}
		
	}

	public void ClearField(GravityField g)
	{
		Fields.Remove(g);
		//if(Field == g) 
		//{
		//	Field = null;
		//}
	}

	public float ChargeThreshold = 0.3F;
	private void Velocity()
	{
		Vector2 vel = new Vector2(Input.GetAxis("Horizontal_" + Name),Input.GetAxis("Vertical_" + Name));

		//if(GameManager.Players[0] == this) print(vel);
		if(Vector3.Distance(vel, Vector3.zero) > 0.2F)
		{
			if(Fuel.Current > Fuel.DecayRate*2 && vel != Vector2.zero)
			{
				Fuel.Current = Mathf.Clamp(Fuel.Current-Fuel.DecayRate, 0.0F, 10.0F);
				VelocityParticles.enableEmission = true;
				Quaternion look = Quaternion.LookRotation(-vel);
				Quaternion current = Quaternion.Slerp(VelocityParticles.transform.rotation, look, Time.deltaTime * 5);
				VelocityParticles.transform.rotation = Quaternion.Euler(current.eulerAngles.x, current.eulerAngles.y, 0);
			}
			else VelocityParticles.enableEmission = false;
		}
		else VelocityParticles.enableEmission = false;

		

		ControlMove(vel);

		Gravity.Velocity = GravityMove();

		actual_vel = Vector3.zero;
		actual_vel += (Control.Velocity.normalized * Control.SpeedInput) + (Control.Velocity.normalized * Charge.Current/150.0F);
		actual_vel += Gravity.Velocity * Gravity.SpeedInput;

		float totalspeed = Mathf.Abs(actual_vel.x) + Mathf.Abs(actual_vel.z);
		if(totalspeed > ChargeThreshold) Charge.Current += Charge.RechargeRate;
		else if(Charge.Current > 0.0F) Charge.Current -= Charge.DecayRate;
		else Charge.Current = 0.0F;
		

		Debug.DrawRay(transform.position, actual_vel.normalized*5, TailColor);
		trans.position += actual_vel;
		lastvel = actual_vel;
	}


	public Vector3 GravityMove()
	{
		if(Fields.Count == 0)
		{
			return Gravity.Velocity;
		} 
		
		foreach(GravityField g in Fields)
		{
			Vector3 initvel = (g.transform.position - trans.position).normalized * g.PowerAtDist(trans.position);
		//Add Gravity from field
			Vector3 pullvel = initvel;
			Gravity.x = Mathf.Clamp(Gravity.x + pullvel.x * Gravity.SpeedInput, -Gravity.SpeedMax, Gravity.SpeedMax);
			Gravity.y = Mathf.Clamp(Gravity.y + pullvel.z * Gravity.SpeedInput, -Gravity.SpeedMax, Gravity.SpeedMax);

		//BUT! Also push the player away if they will cross too close to the field
			Ray pushray = new Ray(transform.position, actual_vel);
			Vector3 velradialpoint = pushray.GetPoint(g.RadialDist(trans.position));
			if(Vector3.Distance(velradialpoint, g.transform.position) < g.NearRadius)
			{
				//Gravity.x *= 0.8F;
				//Gravity.y *= 0.8F;

				//float push_x = 0.0F;
				//float push_y = 0.0F;
				//Gravity.x = Mathf.Clamp(Gravity.x + push_x, -Gravity.SpeedMax, Gravity.SpeedMax);
				//Gravity.y = Mathf.Clamp(Gravity.y + push_y, -Gravity.SpeedMax, Gravity.SpeedMax);
			}
			
		}
		
		return new Vector3(Gravity.x, 0, Gravity.y);
	}

	public void SetControlVel(Vector3 v, float spd)
	{
		Control.x = v.x * spd;
		Control.y = v.z * spd;
		Control.Velocity = v * spd;
	}
	
	public void ControlMove(Vector2 move)
	{
		Control.x = Mathf.Clamp(Control.x + move.x * Control.SpeedInput, -Control.SpeedMax, Control.SpeedMax);
		Control.y = Mathf.Clamp(Control.y + move.y * Control.SpeedInput, -Control.SpeedMax, Control.SpeedMax);
		Control.Velocity = new Vector3(Control.x, 0, Control.y);
	}

	public void Destroy()
	{
		GameManager.instance.Destroy(this);
	}

	public void OnTriggerEnter(Collider c)
	{
		if(c.tag == "Destroy") Destroy();
	}

}


[System.Serializable]
public class PlayerProps
{
	public FloatProps _Charge;
	public FloatProps _Fuel;
	public VelProps _Gravity;
	public VelProps _Control;
}

[System.Serializable]
public class FloatProps
{
	public float Current = 0.0F;
	public float Max  = 0.0F;
	public float DecayRate = 0.01F;
	public float RechargeRate = 0.01F;
}

[System.Serializable]
public class VelProps
{
	public Vector3 Velocity;
	public float x, y;
	public float SpeedInput;
	public float SpeedMax;
}
