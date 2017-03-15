using UnityEngine;
using System.Collections;

public class DestroyCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter(Collider col)
	{
		PlayerControl cont = col.transform.gameObject.GetComponent<PlayerControl>();
		if(cont)
		{
			cont.Destroy();
		}
	}
}
