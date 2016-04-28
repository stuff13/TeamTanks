using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		// GetComponent<Rigidbody>().AddForce(GameObject.Find("Gun").transform.forward * 800);
	}

	void OnCollisionEnter()
	{
		Instantiate(Resources.Load("ExplosionMobile"), transform.position, transform.rotation);

		// add force to object if we hit it


		Destroy(gameObject);
	}
}
