using UnityEngine;

public class PCPlayerController : MonoBehaviour
{

    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float rotationSpeed = 20.0f;
    [SerializeField] private GameObject gun = null;
    [SerializeField] private float gunSpeed = 5.0f;

    private float _baseTime = 0.0f;

	void Update ()
	{
	    if (GameManager.IsServer)
	    {
	        float movement = Input.GetAxis("Forward");
	        float rotation = Input.GetAxis("Right");

	        if (movement != 0.0f)
	        {
	            transform.position += transform.forward * movement * speed * Time.deltaTime;
	        }

	        if (rotation != 0.0f)
	        {
	            transform.Rotate(Vector3.up * rotation * rotationSpeed * Time.deltaTime);
	        }

	        // figure out movement
	        // report it to network manager
	        // move player
	    }
	    else
	    {
            var targetQ = Quaternion.LookRotation(GameManager.Instance.Head.Gaze.direction);
            gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, targetQ, Time.deltaTime * gunSpeed);

        }
    }

    void FixedUpdate()
    {
        if (Time.deltaTime - _baseTime > 0.1)
        {
            _baseTime += 0.1f;
            if (GameManager.IsServer)
            {
                NetworkController.Instance.UpdateObjectLocations(gameObject);
            }
            else
            {
               // NetworkController.Instance.UpdateObjectLocations(gun);
            }
        }
    }
}
