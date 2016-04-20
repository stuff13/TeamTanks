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
    static int mainThreadId;

    public static bool IsServer { get { return !Instance.onApple; } }
    public CardboardHead Head { get; set; }

    void Start () 
	{
        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        if (Instance == null)
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
	        mainCamera.SetActive(false);
            Head = turretCamera.GetComponent<StereoController>().Head;
	        turretCamera.GetComponent<Camera>().enabled = true;
            turretCamera.GetComponent<CardboardHead>().enabled = true;
            turretCamera.GetComponent<AudioListener>().enabled = true;
            GetComponent<Cardboard>().VRModeEnabled = true;
	    }
	    else
	    {
	        mainCamera.SetActive(true);
	        turretCamera.GetComponent<Camera>().enabled = false;
	        turretCamera.GetComponent<CardboardHead>().enabled = false;
            turretCamera.GetComponent<AudioListener>().enabled = false;
            GetComponent<Cardboard>().VRModeEnabled = false;
	    }
	}

	void Update () 
	{
	}


    // If called in the non main thread, will return false;
    public static bool IsMainThread
    {
        get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
    }
}
