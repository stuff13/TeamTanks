﻿using UnityEngine;
using System.Collections;

public class PCPlayerController : MonoBehaviour
{

    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float rotationSpeed = 2.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    float movement = Input.GetAxis("Forward");
	    float rotation = Input.GetAxis("Right");

        if ( movement != 0.0f)
	    {
	        transform.position += transform.forward * movement * speed * Time.deltaTime;
	    }

	    if (rotation != 0.0f)
	    {
	        transform.Rotate(Vector3.up * rotation * rotationSpeed * Time.deltaTime);
	    }
	}
}
