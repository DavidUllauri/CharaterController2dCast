using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

		public void Move (Vector2 velocity) {
				transform.Translate (velocity);
		}
}
