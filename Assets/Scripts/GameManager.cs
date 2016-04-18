using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour 
{
	public static GameManager Instance;
	[SerializeField] private GameObject mainCamera = null;
	[SerializeField] private GameObject turretCamera = null;
    //	[SerializeField] private GameObject turret = null;
    //	[SerializeField] private float maxTurretRotation = 120.0f;
    //	[SerializeField] private float maxGunAngle = 50.0f;   // angle from horizon
    //	[SerializeField] private float turretSpeed = 2.0f;

	private bool onApple = false;

    public static bool IsServer { get { return !Instance.onApple; } }
    public CardboardHead Head { get; set; }

    void Start () 
	{
		if(Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this.gameObject);
		}

		Debug.Log(SystemInfo.operatingSystem);
	    onApple = !SystemInfo.operatingSystem.Contains("Windows");

	    if (onApple)
	    {
	        Cardboard.Create();
	        turretCamera.SetActive(true);
	        mainCamera.SetActive(false);
            Head = turretCamera.GetComponent<StereoController>().Head;
	        GetComponent<Cardboard>().VRModeEnabled = true;
	    }
	    else
	    {
	        mainCamera.SetActive(true);
	        turretCamera.SetActive(false);
	        GetComponent<Cardboard>().VRModeEnabled = false;
	    }
	}

	void Update () 
	{
	}

}
