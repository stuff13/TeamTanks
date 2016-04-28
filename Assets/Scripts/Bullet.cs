using UnityEngine;
using System.Collections;
using System.Linq;

public class Bullet : MonoBehaviour
{
    private float Impulse = 1000;
    int id;

	// Use this for initialization
	void Start () 
	{
		GetComponent<Rigidbody>().AddForce(GameObject.Find("Gun").transform.forward * Impulse);

	    id = NetworkController.Instance.GameCatalog.Keys.Max(x => x)+1;
		NetworkController.Instance.GameCatalog.Add(id, gameObject);
    }

	void OnCollisionEnter(Collision col)
	{
		Instantiate(Resources.Load("ExplosionMobile"), transform.position, transform.rotation);

		NetworkController.Instance.GameCatalog.Remove(id);
		Destroy(gameObject);
	}
}
