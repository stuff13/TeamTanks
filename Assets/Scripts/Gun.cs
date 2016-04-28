using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {
	[SerializeField] private Transform projectile = null;
	[SerializeField] float bulletSpeed = 10;
	[SerializeField] Transform spawnPoint = null;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Cardboard.SDK.VRModeEnabled && Cardboard.SDK.Triggered)
		{
			// fire
			Vector3 spawnVector = new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z);
			Instantiate(projectile, spawnVector, spawnPoint.rotation);
		}
	}
}
