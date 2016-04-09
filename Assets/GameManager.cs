using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	public static GameManager Instance;
	[SerializeField] private GameObject mainCamera = null;
	[SerializeField] private GameObject turretCamera = null;
	[SerializeField] private GameObject tank = null;
	[SerializeField] private GameObject turret = null;
	[SerializeField] private GameObject gun = null;
	[SerializeField] private float maxTurretRotation = 120.0f;
	[SerializeField] private float maxGunAngle = 50.0f;   // angle from horizon
	[SerializeField] private float turretSpeed = 2.0f;
	[SerializeField] private float gunSpeed = 5.0f;
	private CardboardHead head;
	private Quaternion currentTurretRotation;
	private Quaternion currentGunRotation;

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
		if(SystemInfo.operatingSystem.Contains("Windows"))
		{
			mainCamera.SetActive(true);
		}
		else
		{
			Cardboard.Create();
			turretCamera.SetActive(true);
			head = turretCamera.GetComponent<StereoController>().Head;

			currentTurretRotation = turret.transform.rotation;
			currentGunRotation = gun.transform.rotation;
		}
	}

	void Update () 
	{
		Vector3 gaze = head.Gaze.direction; // or head.GetComponent<Camera>().transform.forward; 
		// RotateTurret(gaze);	
		RotateGun(gaze);
		// 			  > angular speed, rotate by angular speed

//		if(Vector3.Angle(planeGaze, tank.transform.forward) > maxTurretRotation)
//		{
//			// we need to clamp the rotation to 120
//			float currentTurretRotation = Vector3.Angle(turret.transform.forward, tank.transform.forward);
//			turret.transform.Rotate(turret.transform.up, (120 - currentTurretRotation) * turretSpeed * Time.deltaTime, Space.Self);
//		}
//		else
//		{
//			Quaternion qPlaneGaze = Quaternion.LookRotation(planeGaze, turret.transform.up);
//			turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, qPlaneGaze, Time.deltaTime);
//		}
//
//		float targetGunAngleFromUp = Vector3.Angle(turret.transform.up, gaze); 	// this is to ensure all desired angles are from 0 => 180
//		float clampedTargetGunAngleFromUp = Mathf.Clamp(targetGunAngleFromUp, 90 - maxGunAngle, 90);
////		Quaternion.
//  
//
//		float currentGunAngle = Vector3.Angle(gun.transform.forward, turret.transform.up);
//		gun.transform.Rotate(gun.transform.right, (clampedTargetGunAngleFromUp - currentGunAngle) * gunSpeed * Time.deltaTime);
//
//		Debug.Log("gun.transform.right = " + gun.transform.right);
//		Debug.Log("delta = " + (clampedTargetGunAngleFromUp - currentGunAngle));


//		Debug.Log("current gun angle = " + currentGunAngle);
//		Debug.Log("targetGunAngle = " + targetGunAngleFromUp);
//		Debug.Log("clampedTargetGunAngle = " + clampedTargetGunAngleFromUp);

//		float baseAngle = HorizontalDegreesBetweenVectors(tank.transform.forward, head.Gaze.direction);
//		Debug.Log("baseAngle = " + baseAngle);
//		baseAngle = Mathf.Clamp(baseAngle, -maxTurretRotation, maxTurretRotation);
//		float currentDegreesFromBase = HorizontalDegreesBetweenVectors(tank.transform.forward, turret.transform.forward);
//		float baseRotationRequired = baseAngle - currentDegreesFromBase;
//		turret.transform.Rotate(Vector3.up * baseRotationRequired * turretSpeed * Time.deltaTime);
//
		// update direction of gun
//		float baseRayMagnitude = new Vector3(head.Gaze.direction.x, 0, head.Gaze.direction.z).magnitude;	// save this for later
//		// Atan gives 0 degrees straight up, we want 0 degrees straight out
//		float upAngle = 90 - ConvertToDegrees(Mathf.Atan(baseRayMagnitude / head.Gaze.direction.y));	
//		upAngle = Mathf.Clamp(upAngle, 0, maxGunAngle);
//		float currentGunAngle = DegreesBetweenVectors(turret.transform.forward, gun.transform.forward);
//		float upRotationRequired = upAngle - currentGunAngle;
//		gun.transform.Rotate(turret.transform.right, upRotationRequired * gunSpeed * Time.deltaTime);
	}

	private void RotateTurret(Vector3 gaze)
	{
		// project to plane
		Vector3 planeGaze = Vector3.ProjectOnPlane(gaze, turret.transform.up);
		var planeGazeQ = Quaternion.LookRotation(planeGaze);
		turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, planeGazeQ, Time.deltaTime * turretSpeed);

//		// 2. figure out angle of rotation
//		float angleOfRotation = Vector3.Angle(tank.transform.forward, planeGaze);
//		float sign = Vector3.Cross(tank.transform.forward, planeGaze).y > 0 ? 1 : -1;	// up result is rotating to right
//		// 3. clamp
//		angleOfRotation = Mathf.Clamp(angleOfRotation, 0, maxTurretRotation) * sign;
//		Debug.Log("angleOfRotation = " + angleOfRotation);
//
//
//		// 3.5 delta = clamped value - current value
////		Quaternion angleOfRotationQ = Quaternion.LookRotation(angleOfRotation);
//		float currentAngle = Quaternion.Angle(Quaternion.LookRotation(tank.transform.forward), currentTurretRotation);
//		Debug.Log("currentAngle = " + currentAngle);
//		float delta = angleOfRotation - currentAngle; //Quaternion.Angle(angleOfRotationQ, currentTurretRotation);
//		Debug.Log("delta = " + delta);
//
//		// 4 if delta < angular speed, rotate by delta
//		// 			  > angular speed, rotate by angular speed
//		float amountToRotate = Mathf.Abs(delta) > turretSpeed * Time.deltaTime ? turretSpeed * Time.deltaTime * sign : delta;
//		Debug.Log("amountToRotate = " + amountToRotate);
//		turret.transform.rotation *= Quaternion.AngleAxis(amountToRotate, turret.transform.up);

		// 4. adjust current position
//		currentTurretRotation = turret.transform.localRotation;
	}

	private void RotateGun(Vector3 target)
	{
//		float baseRayMagnitude = new Vector3(target.x, 0, target.z).magnitude;	// save this for later
////		// Atan gives 0 degrees straight up, we want 0 degrees straight out
//		float upAngle = 90 - Mathf.Atan(baseRayMagnitude / target.y) * Mathf.Rad2Deg;	

		var targetQ = Quaternion.LookRotation(target);
		gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, targetQ, Time.deltaTime * gunSpeed);

// 		upAngle = Mathf.Clamp(upAngle, 0, maxGunAngle);
// 		float currentGunAngle = DegreesBetweenVectors(turret.transform.forward, gun.transform.forward);
//		float upRotationRequired = upAngle - currentGunAngle;
//		gun.transform.Rotate(turret.transform.right, upRotationRequired * gunSpeed * Time.deltaTime);

		// 2. figure out angle of rotation
		// 3. clamp


		// 3.5 delta = clamped value - current value
		// 4 if delta < angular speed, rotate by delta
		// 			  > angular speed, rotate by angular speed

		// 4. adjust current position

	}

	public static float ConvertToRadians(float degrees)
	{
		return degrees * Mathf.PI / 180.0f;
	}

	public static float ConvertToDegrees(float radians)
	{
		return radians * 180.0f / Mathf.PI;
	}

	private float HorizontalDegreesBetweenVectors(Vector3 main, Vector3 other)
	{
		Vector3 mainHRay = new Vector3(main.x, 0, main.z);		
		Vector3 otherHRay = new Vector3(other.x, 0, other.z);  

		return DegreesBetweenVectors(mainHRay, otherHRay);
	}

	private float DegreesBetweenVectors(Vector3 main, Vector3 other)
	{
		return Vector3.Angle(other, main);
	}
}
