using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {
	[SerializeField] private Transform projectile = null;
	[SerializeField] Transform spawnPoint = null;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Cardboard.SDK.VRModeEnabled && Cardboard.SDK.Triggered)
		{
		    Fire(spawnPoint.position, spawnPoint.rotation);
		}
	}

    public GameObject Fire(Vector3 position, Quaternion rotation)
    {
        Vector3 spawnVector = new Vector3(position.x, position.y, position.z);
        Transform bulletTransform = Instantiate(projectile, spawnVector, rotation) as Transform;
        return bulletTransform != null ? bulletTransform.gameObject : null;
    }
}
