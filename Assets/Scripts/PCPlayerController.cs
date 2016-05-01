using Assets.Scripts;

using UnityEngine;

public class PCPlayerController : MonoBehaviour
{

    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float rotationSpeed = 20.0f;
    [SerializeField] private GameObject gun = null;
    [SerializeField] private float gunSpeed = 5.0f;
	[SerializeField] private float networkFrameTime = 0.2f;

    private float amountOfTimeSinceLastUpdate = 0;
    private GameObject objectToUpdate;

    private const float epsilon = 0.0001f;

    private const int TankId = 1;

    void Start()
    {
        objectToUpdate = GameManager.IsServer ? gameObject : gun;
    }

    void Update ()
	{
        amountOfTimeSinceLastUpdate += Time.deltaTime;

        if (GameManager.IsServer)
	    {
	        float movement = Input.GetAxis("Forward");
	        float rotation = Input.GetAxis("Right");

	        if (Mathf.Abs(movement) > epsilon)
	        {
	            transform.position += transform.forward * movement * speed * Time.deltaTime;
	        }

	        if (Mathf.Abs(rotation) > epsilon)
	        {
	            transform.Rotate(Vector3.up * rotation * rotationSpeed * Time.deltaTime);
	        }
	    }
	    else
	    {
            var targetQ = Quaternion.LookRotation(GameManager.Instance.Head.Gaze.direction);
            gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, targetQ, Time.deltaTime * gunSpeed);

            // adjust tank position
            ObjectHistory tankHistory = NetworkController.Instance.GameCatalog[TankId];
            transform.position = Vector3.Lerp(transform.position, tankHistory.TargetPosition, amountOfTimeSinceLastUpdate);
            transform.rotation = Quaternion.Slerp(transform.rotation, tankHistory.TargetRotation, amountOfTimeSinceLastUpdate);
        }

        if (amountOfTimeSinceLastUpdate > networkFrameTime)
        {
            NetworkController.Instance.UpdateObjectLocations(objectToUpdate);
            amountOfTimeSinceLastUpdate = 0;
        }
	}

}
