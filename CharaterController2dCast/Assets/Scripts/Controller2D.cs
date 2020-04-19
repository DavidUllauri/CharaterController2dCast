using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Controller2D : MonoBehaviour
{
		ContactFilter2D contactFilter;

		const float _skinWidth = 0.015f;
		int rayCount = 3;

		float maxClimbAngle = 80;
		float maxDescendAngle = 80;

		float horizontalRaySpacing;
		float verticalRaySpacing;

		Collider2D _collider;
		RaycastOrigins raycastOrigins;
		public CollisionInfo collisions;
		RaycastHit2D [] hitBuffer = new RaycastHit2D [16];
		List<RaycastHit2D> hitBufferList = new List<RaycastHit2D> (16);

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider2D> ();

				contactFilter.SetLayerMask (Physics2D.GetLayerCollisionMask (gameObject.layer));
				contactFilter.useLayerMask = true;
				CalculateRaySpacing ();
    }

		public void Move (Vector2 velocity) {
				UpdateColliderBounds ();
				collisions.Reset ();

				if (velocity.y < 0) {
						DescendSlope (ref velocity);
				}
				if (velocity.x != 0) {
						HorizontalCollisions (ref velocity);
				}
				if (velocity.y != 0) {
						VerticalCollisions (ref velocity);
				}

				transform.Translate (velocity);
		}

		void HorizontalCollisions (ref Vector2 velocity) {
				float directionX;
				float castLength;
				int hitCount;

				directionX = Mathf.Sign (velocity.x);
				castLength = Mathf.Abs (velocity.x) + _skinWidth;
				hitCount = _collider.Cast (directionX * Vector2.right, contactFilter, hitBuffer, castLength);

				for (int i=0; i < rayCount; i++) {
						Vector2 rayOrigin;

						rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
						rayOrigin += Vector2.up * (horizontalRaySpacing * i);
						Debug.DrawRay (rayOrigin, Vector2.right * directionX * castLength * 3, Color.red);
				}

				hitBufferList.Clear ();

				for (int i=0; i < hitCount; i++) {
						hitBufferList.Add (hitBuffer[i]);
				}
				
				foreach (RaycastHit2D hit in hitBufferList) { 
						float slopeAngle;

						slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

						if (slopeAngle <= maxClimbAngle) {
								float distanceToSlopeStart;

								if(collisions.descendingSlope) {
										collisions.descendingSlope = false;
										velocity = collisions.velocityOld;
								}

								distanceToSlopeStart = 0;
								if (slopeAngle != collisions.slopeAngleOld) {
										distanceToSlopeStart = hit.distance - _skinWidth;
										velocity.x -= distanceToSlopeStart * directionX;
								}
								ClimbSlope (ref velocity, slopeAngle);
								velocity.x += distanceToSlopeStart * directionX;
						}

						if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
						{
								if (collisions.climbingSlope)
								{
										velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
								}

								collisions.left = (directionX == -1) && (hit.point.x < gameObject.transform.position.x);
								collisions.right = (directionX == 1) && (hit.point.x > gameObject.transform.position.x);

								if (collisions.left || collisions.right) {
										velocity.x = (hit.distance - _skinWidth) * directionX;
										castLength = hit.distance;
								}
						}
				}
		}

		void VerticalCollisions (ref Vector2 velocity) {
				int directionY;
				float castLength;
				int hitCount;

				directionY = (int)Mathf.Sign (velocity.y);
				castLength = Mathf.Abs (velocity.y) + _skinWidth;
				hitCount = _collider.Cast (directionY * Vector2.up, contactFilter, hitBuffer, castLength);

				for (int i=0; i < rayCount; i++) {
						Vector2 rayOrigin;

						rayOrigin = (directionY == -1) ?raycastOrigins.bottomLeft : raycastOrigins.topLeft;
						rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
						Debug.DrawRay (rayOrigin, Vector2.up * directionY * castLength * 2, Color.red);
				}

				hitBufferList.Clear ();

				for (int i=0; i < hitCount; i++) {
						hitBufferList.Add (hitBuffer[i]);
				}

				foreach (RaycastHit2D hit in hitBufferList) {
						if (collisions.climbingSlope)
						{
								velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
						}
						
						collisions.below = (directionY == -1) && (hit.point.y < gameObject.transform.position.y);
						collisions.above = (directionY == 1) && (hit.point.y > gameObject.transform.position.y);

						if (collisions.below || collisions.above)
						{
								velocity.y = (hit.distance - _skinWidth) * directionY;
								castLength = hit.distance;

								collisions.below = (directionY == -1);
						}

						if (collisions.climbingSlope) {
								float directionX;
								float slopeAngle;

								directionX = Mathf.Sign (velocity.x);
								castLength = Mathf.Abs (velocity.x) + _skinWidth;

								slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
								if (slopeAngle != collisions.slopeAngle) {
										velocity.x = (hit.distance - _skinWidth) * directionX;
										collisions.slopeAngle = slopeAngle;
								}
								collisions.below = true;
						}
				}
		}

		void ClimbSlope (ref Vector2 velocity, in float slopeAngle) {
				float moveDistance = Mathf.Abs (velocity.x);
				float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

				if (velocity.y <= climbVelocityY) {
						velocity.y = climbVelocityY;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);

						collisions.below = true;
						collisions.climbingSlope = true;
						collisions.slopeAngle = slopeAngle;
				}
		}

		void DescendSlope (ref Vector2 velocity) {
				float directionX = Mathf.Sign (velocity.x);
				int hitCount = _collider.Cast (-Vector2.up, contactFilter, hitBuffer, Mathf.Infinity);

				hitBufferList.Clear();

				for (int i=0; i < hitCount; i++) {
						hitBufferList.Add (hitBuffer[i]);
				}

				foreach (RaycastHit2D hit in hitBufferList) {
						float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

						if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
								bool isDescendingSlope = Mathf.Sign (hit.normal.x) == directionX;
								if (isDescendingSlope) {
										if (hit.distance - _skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x)) {
												float moveDistance;
												float descendVelocityY;

												moveDistance = Mathf.Abs (velocity.x);
												descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

												velocity.x	= Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
												velocity.y	-= descendVelocityY;

												collisions.slopeAngle = slopeAngle;
												collisions.descendingSlope = true;
												collisions.below = true;
										}
								}
						}
				}
		}

		void UpdateColliderBounds () {
				Bounds bounds = _collider.bounds;
				// bounds.Expand (_skinWidth * -2);

				raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
				raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
				raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
				raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
		}

		void CalculateRaySpacing () {
				Bounds bounds = _collider.bounds;
				bounds.Expand (_skinWidth * -2);
				rayCount = 3;

				rayCount = Mathf.Clamp (rayCount, 2, int.MaxValue);

				horizontalRaySpacing = bounds.size.y / (rayCount - 1);
				verticalRaySpacing = bounds.size.x / (rayCount - 1);
		}

		struct RaycastOrigins {
				public Vector2 topLeft, topRight;
				public Vector2 bottomLeft, bottomRight;
		}

		public struct CollisionInfo {
				public bool above, below;
				public bool left, right;

				public bool climbingSlope;
				public bool descendingSlope;

				public float slopeAngle, slopeAngleOld;
				public Vector2 velocityOld;

				public void Reset() {
						above = below = false;
						left = right = false;

						climbingSlope = false;
						descendingSlope = false;

						slopeAngleOld = slopeAngle;
						slopeAngle = 0;
				}

				public override string ToString () {
						return string.Format (
								"(controller: a:{0}, b:{1}, l:{2}, r:{3})",
								above,
								below,
								left,
								right);
				}
		}
}
