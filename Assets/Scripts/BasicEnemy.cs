using UnityEngine;
using System.Collections;

public class BasicEnemy : MonoBehaviour {

	float jumpHeight = 4;
    float timeToJumpApex = .4f;
    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;
    float moveSpeed = 4;
    bool pull = false;
    Vector2 destination;
    PlayerController player;
    float currentSpeed = 0;

    //True == Right// False == left
    public bool facing = true;


    bool jumped;
    Vector2 velocity;
    float gravity;

    float velocityXSmoothing;

    Controller2D controller;

	public LayerMask mask = 0;
	public BoxCollider2D collider;
	// Use this for initialization
	void Start () {
        controller = GetComponent<Controller2D>();
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
    }

    // Update is called once per frame
    void Update () {

    	UpdateFacing();
    	Move();
       
    }

    void Move()
    {
    	if(pull)
    	{
    		destination = player.getPosition();
    		Vector2 currentPosition = new Vector2(this.transform.position.x, this.transform.position.y);
    		float distance = Vector2.Distance(currentPosition, destination);
    		float pullspeed = (player.getSpeed() * 1.25f > 10)?player.getSpeed() * 1.25f:10;
    		pullspeed = Mathf.Max(pullspeed, currentSpeed);
    		currentSpeed = pullspeed;

    		//float targetVelocityX = ((destination.x - currentPosition.x)/distance) * moveSpeed * 1.5f;
        	//velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        	velocity.x = ((destination.x - currentPosition.x)/distance) *pullspeed;
        	velocity.y = ((destination.y - currentPosition.y)/distance) *pullspeed;
        	//float targetVelocityY = ((destination.y - currentPosition.y)/distance) * moveSpeed * 1.5f;
        	//velocity.y = Mathf.SmoothDamp(velocity.y, targetVelocityY, ref velocityYSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        	if((destination - currentPosition).magnitude < 3)
       		{
            	Debug.Log("Done Grappling");
            	pull = false;
            	currentSpeed = 0; 
            	player.setPulling(false);
        	}	

    	}
    	else
    	{
	    	if (controller.collisions.below)
	        {
	            velocity.y = 0;
	            jumped = false;
	        }


	        float targetVelocityX = moveSpeed * ((facing)?1:-1);
	        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
	        velocity.y += gravity * Time.deltaTime;
	    }


        controller.Move(velocity * Time.deltaTime);
    }
    void UpdateFacing()
    {

        Vector2 start = this.transform.position;
        Vector2 dir = Vector2.down + ((facing)?Vector2.right:Vector2.left);


    	RaycastHit2D hit = Physics2D.Raycast(start, dir, 2f, mask);
    	
    	if(!hit)
    	{
    		facing = !facing;
    	}
    	
    	dir = ((facing)?Vector2.right:Vector2.left);
    	RaycastHit2D hit1 = Physics2D.Raycast(start, dir, 1f, mask);
    	if(hit1)
    	{
    		facing = !facing;
    	}
    	

    }

    public void setPull(PlayerController p)
    {
    	player = p;
    	pull = true;
    }

    public void stopPull()
    {
    	pull = false;
    }
}
