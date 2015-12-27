using UnityEngine;
using System.Collections;


public class Controller2D: RaycastController{

	//Primitive types
	float maxClimbAngle = 80;
	float maxDescendAngle = 75;

	//Structs or Classes
	public CollisionInfo collisions;

	public override void Start(){
		base.Start();

	}

	public void Move(Vector3 v, bool standingOnPlatform = false){
		UpdateRaycastOrigins();
		collisions.Reset();
		collisions.velocityOld = v;
		if(v.y < 0) DescendSlope(ref v);
		if(v.x != 0) HorizontalCollisions(ref v);
		if(v.y != 0) VerticalCollisions(ref v);
		transform.Translate(v);
		if(standingOnPlatform){
			collisions.below = true;
		}
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
				if(hit.distance == 0){
					continue;
				}
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if(i == 0 && slopeAngle <= maxClimbAngle){
					if(collisions.descendingSlope){
						collisions.descendingSlope = false;
						v = collisions.velocityOld;
					}
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
		float directionX = Mathf.Sign(v.x);
		Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
		if(hit){
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if(slopeAngle != 0 && slopeAngle <= maxDescendAngle){
				if(Mathf.Sign(hit.normal.x) == directionX){
					if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(v.x)){
						float moveDistance = Mathf.Abs(v.x);
						float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						v.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(v.x);
						v.y -= descendVelocityY;
						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = collisions.below = true;
					}
				}
			}
		}
	}



	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;
		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;

		public void Reset(){
			above = below = left = right = false;
			climbingSlope = descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
