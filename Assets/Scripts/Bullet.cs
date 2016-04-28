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

	    int maxId = NetworkController.Instance.GameCatalog.Keys.Max(x => x);
        NetworkController.Instance.GameCatalog.Add(maxId+1, gameObject);
    }

	void OnCollisionEnter(Collision col)
	{
		Instantiate(Resources.Load("ExplosionMobile"), transform.position, transform.rotation);

		Destroy(gameObject);
	}
}
