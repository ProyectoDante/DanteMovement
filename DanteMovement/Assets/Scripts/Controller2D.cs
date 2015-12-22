using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	//Primitive types
	public int horizontalRayCount, verticalRayCount;
	public LayerMask collisionMask;
	private const float skinWidth = .015f;
	private float horizontalRaySpacing, verticalRaySpacing;
	float maxClimbAngle = 80;
	float maxDescendAngle = 75;

	//Structs or Classes
	public CollisionInfo collisions;
	private RayCastOrigins raycastOrigins;
	private BoxCollider2D col;
	

	void Start(){
		col = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();
	}

	public void Move(Vector3 v){
		UpdateRaycastOrigins();
		collisions.Reset();
		if(v.y < 0) DescendSlope(ref v);
		if(v.x != 0) HorizontalCollisions(ref v);
		if(v.y != 0) VerticalCollisions(ref v);
		transform.Translate(v);
	}

	void HorizontalCollisions(ref Vector3 v){
		float directionX = Mathf.Sign(v.x);
		float rayLength = Mathf.Abs(v.x) + skinWidth;

		for(int i = 0; i < horizontalRayCount; i++){
			Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			Debug.DrawRay(rayOrigin, Vector2.right * rayLength * directionX, Color.red);
			if(hit){
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if(i == 0 && slopeAngle <= maxClimbAngle){
					float distanceToSlopeStart = 0;
					if(slopeAngle != collisions.slopeAngleOld){
						distanceToSlopeStart = hit.distance - skinWidth;
						v.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref v, slopeAngle);
					v.x += distanceToSlopeStart * directionX;
				}

				if(!collisions.climbingSlope || slopeAngle > maxClimbAngle){
					v.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;
					if(collisions.climbingSlope){
						v.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(v.x);
					}
					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector3 v){
		float directionY = Mathf.Sign(v.y);
		float rayLength = Mathf.Abs(v.y) + skinWidth;

		for(int i = 0; i < verticalRayCount; i++){
			Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + v.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			Debug.DrawRay(rayOrigin, Vector2.up * rayLength * directionY, Color.red);
			if(hit){
				v.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;
				if(collisions.climbingSlope){
					v.x = v.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(v.x);
				}
				collisions.above = directionY == 1;
				collisions.below = directionY == -1;
			}
		}

		if(collisions.climbingSlope){
			float directionX = Mathf.Sign(v.x);
			rayLength = Mathf.Abs(v.x) + skinWidth;
			Vector2 rayOrigin = (directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * v.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			if(hit){
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if(slopeAngle != collisions.slopeAngle){
					v.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope(ref Vector3 v, float angle){
		float moveDistance = Mathf.Abs(v.x);
		float climbVelocityY = Mathf.Sin(angle * Mathf.Deg2Rad) * moveDistance;
		if(v.y <= climbVelocityY){
			v.y = climbVelocityY;
			v.x = Mathf.Cos(angle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(v.x);
			collisions.below = collisions.climbingSlope = true;
			collisions.slopeAngle = angle;
		}
	}

	void DescendSlope(ref Vector3 v){

	}

	void CalculateRaySpacing(){
		Bounds bounds = col.bounds;
		bounds.Expand(skinWidth * -2);
		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	void UpdateRaycastOrigins(){
		Bounds bounds = col.bounds;
		bounds.Expand(skinWidth * -2);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
	}

	struct RayCastOrigins{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;
		public bool climbingSlope;
		public float slopeAngle, slopeAngleOld;

		public void Reset(){
			above = below = left = right = false;
			climbingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
