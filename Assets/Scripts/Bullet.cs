using UnityEngine;
using System.Collections;
using System.Linq;

public class Bullet : MonoBehaviour
{
    private float Impulse = 1000;

	// Use this for initialization
	void Start () 
	{
		GetComponent<Rigidbody>().AddForce(GameObject.Find("Gun").transform.forward * Impulse);
	    NetworkController.Instance.InsertObject(gameObject);
    }

	void OnCollisionEnter(Collision col)
	{
		Instantiate(Resources.Load("ExplosionMobile"), transform.position, transform.rotation);

		NetworkController.Instance.RemoveObject(gameObject);
		Destroy(gameObject);
	}
}
