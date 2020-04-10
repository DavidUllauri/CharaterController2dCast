using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Controller2D : MonoBehaviour
{
		ContactFilter2D contactFilter;

		const float _skinWidth = 0.015f;

		Collider2D _collider;
		public CollisionInfo collisions;
		RaycastHit2D [] hitBuffer = new RaycastHit2D [16];
		List<RaycastHit2D> hitBufferList = new List<RaycastHit2D> (16);

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider2D> ();

				contactFilter.SetLayerMask (Physics2D.GetLayerCollisionMask (gameObject.layer));
				contactFilter.useLayerMask = true;
    }

		public void Move (Vector2 velocity) {
				collisions.Reset ();

				if (velocity.x != 0)
				{
						HorizontalCollisions (ref velocity);
				}
				if (velocity.y != 0) {
						VerticalCollisions (ref velocity);
				}

				transform.Translate (velocity);
		}

		void VerticalCollisions (ref Vector2 velocity) {
				float directionY;
				float castLength;
				int hitCount;

				directionY = Mathf.Sign (velocity.y);
				castLength = Mathf.Abs (velocity.y) + _skinWidth;
				hitCount = _collider.Cast (directionY * Vector2.up, contactFilter, hitBuffer, castLength);

				hitBufferList.Clear ();

				for (int i=0; i < hitCount; i++) {
						hitBufferList.Add (hitBuffer[i]);
				}

				foreach (RaycastHit2D hit in hitBufferList) {
						velocity.y = (hit.distance - _skinWidth) * directionY;
						castLength = hit.distance;

						collisions.below = (directionY == -1);
						collisions.above = (directionY == 1);
				}
		}

		void HorizontalCollisions (ref Vector2 velocity) {
				float directionX;
				float castLength;
				int hitCount;

				directionX = Mathf.Sign (velocity.x);
				castLength = Mathf.Abs (velocity.x) + _skinWidth;
				hitCount = _collider.Cast (directionX * Vector2.right, contactFilter, hitBuffer, castLength);

				hitBufferList.Clear ();

				for (int i=0; i < hitCount; i++) {
						hitBufferList.Add (hitBuffer[i]);
				}

				foreach (RaycastHit2D hit in hitBufferList) { 
						velocity.x = (hit.distance - _skinWidth) * directionX;
						castLength = hit.distance;

						collisions.left = (directionX == -1);
						collisions.right = (directionX == 1);
				}
		}

		public struct CollisionInfo {
				public bool above, below;
				public bool left, right;

				public void Reset() {
						above = below = false;
						left = right = false;
				}

				public override string ToString () {
						return string.Format (
								"(controller: a:{0} b:{1} l:{2} r{3})",
								above,
								below,
								left,
								right);
				}
		}
}
