using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController: MonoBehaviour{

	//Primitive types
	public LayerMask collisionMask;
	public int horizontalRayCount = 4, verticalRayCount = 4;
	public const float skinWidth = .015f;
	
	[HideInInspector]
	public float horizontalRaySpacing, verticalRaySpacing;

	//Structs or Classes
	public RayCastOrigins raycastOrigins;

	[HideInInspector]
	public BoxCollider2D col;

	public virtual void Start(){
		col = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();
	}

	public void CalculateRaySpacing(){
		Bounds bounds = col.bounds;
		bounds.Expand(skinWidth * -2);
		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
	
	public void UpdateRaycastOrigins(){
		Bounds bounds = col.bounds;
		bounds.Expand(skinWidth * -2);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
	}
	
	public struct RayCastOrigins{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
