using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Controller2D : Raycast2D {


    public CollisionInfo collisions;
    public struct collidingObject
    {
        public collidingObject(string direction, Transform obj_)
        {
            dir = direction;
            obj = obj_;
        }
        public string dir;
        public Transform obj;
    }
    public HashSet<collidingObject> objs;
    [HideInInspector]
    public Vector2 playerInput;



    public void Move (Vector3 velocity)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        objs = new HashSet<collidingObject>();
        if (velocity.x != 0)
            HorizontalCollisions(ref velocity);
        if (velocity.y != 0)
            VerticalCollisions(ref velocity);

        transform.Translate(velocity);
    }

    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
                string dir = (collisions.left)?"left":"right";
                collidingObject obj = new collidingObject(dir, hit.transform);
                if(!objs.Contains(obj))
                {
                    objs.Add(obj);
                }
            }
        }
    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,  Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
                string dir = (collisions.below)?"down":"up";
                collidingObject obj = new collidingObject(dir, hit.transform);
                if(!objs.Contains(obj))
                {
                    objs.Add(obj);
                }

            }
        }
    }

    public HashSet<collidingObject> GetObjects()
    {
        return objs;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
	
}
