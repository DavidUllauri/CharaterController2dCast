﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
		float jumpHeight = 4;
		float timeToJumpApex = 0.4f;
		float moveSpeed = 6;

		float gravity;
		Vector2 velocity;

		Controller2D controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D> ();

				gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
    }

    // Update is called once per frame
    void Update()
    {
				float targetVelocityX;
				Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

				targetVelocityX = input.x * moveSpeed;
				velocity.x = targetVelocityX;
				velocity.y += gravity * Time.deltaTime;
				controller.Move (velocity * Time.deltaTime);
	}
}
