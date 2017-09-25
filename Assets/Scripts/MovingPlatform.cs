using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Controller2D controller;

    float direction = 1f;

    void Start ()
    {
        controller = GetComponent<Controller2D>();
    }
	    
	void Update ()
    {

        if (transform.position.x > 20)
        {
            direction = -1f;
        }   
        else if (transform.position.x < -20)
        {
            direction = 1f;
        }
        controller.Move(new Vector2(direction, 0));
	}
}
