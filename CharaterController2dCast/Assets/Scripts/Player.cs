using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
		float moveSpeed = 6;
		Vector2 velocity;

		Controller2D controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D> ();
    }

    // Update is called once per frame
    void Update()
    {
				Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

				float targetVelocityX = input.x * moveSpeed;

				velocity.x = targetVelocityX;

				controller.Move (velocity * Time.deltaTime);
    }
}
