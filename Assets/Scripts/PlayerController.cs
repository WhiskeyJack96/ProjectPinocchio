using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	//Left Right Movement
	float moveSpeed = 12;
	Controller2D controller;

    float accelerationTimeAirborne = 0.5f;
    float accelerationTimeGrounded = 0.2f;
    float velocityXSmoothing;
    float velocityYSmoothing;
    Vector2 velocity;

    //Jumping
    int numberOfJumps = 1;

    KeyCode jump = KeyCode.Space;
    //adjust the minimum jump height
    public float jumpHeight = 3;
    //adjust this to adjust the floatiness of holding down space, be careful though make it to low and the player will fly off screen
    public float extraHeightFactor = 50;
    float timeToJumpApex = .4f;
    float gravity;
    float jumpVelocity;

    //Grapple
    Transform grapplePoint;
    //Adjust to adjust the distance that one can grapple things
    public float grappleLength=100;
    //Ignore
    public LayerMask mask;
    float grappleTime = .5f;
    float grappleVelocity;
    float stopGrapple = 2.3f;
    Vector2 destination;
    bool grappling;
    float distance;
    Vector2 pos;

	// Use this for initialization
    // The start function runs only once at the beggining of the scene nothing here should need adjusting
	void Start () {
		controller = GetComponent<Controller2D>();

		gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        controller.collisionMask =  ~(1 << LayerMask.NameToLayer("Player"));
	
        grapplePoint = this.transform.Find("GrapplePoint");
    }
	
	// Update is called once per frame
	void Update () {

        if(!grappling)
        {
            Move();
        }
        else
        {
            grappleMove();
        }
        Grapple();
	}

	void Move()
	{
        //If the player touches the something above or below them set vertical move to 0
		if (controller.collisions.above)
        {
            velocity.y = 0;
        }

        if(controller.collisions.below)
        {
            velocity.y = 0;
            numberOfJumps = 1;
        }

        // Get input from a and d, convert it to a velocity, smooth and damp it, then add gravity to the y
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;

        //If the player can and wants to jump, jump
        if (Input.GetKeyDown(jump) && numberOfJumps >0)
        {
            velocity.y = jumpVelocity;
            numberOfJumps-=1;
        }
        //if the player holds space jump a bit higher
        if (Input.GetKey(jump) && !controller.collisions.below && velocity.y >0)
        {
        	velocity.y += jumpVelocity/extraHeightFactor;
        }

        if((controller.collisions.left || controller.collisions.right) && !controller.collisions.below)
        {
            if(Input.GetKeyDown(jump))
            {
                velocity.x = (controller.collisions.left)? jumpVelocity *.5f: jumpVelocity *-.5f;
                velocity.y = jumpVelocity *.5f;
            }
        }
        //actually move the player the correct amount
        controller.Move(velocity * Time.deltaTime);
	}

    void Grapple()
    {
        //if the player left clicks
        if(Input.GetMouseButtonDown(0))
        {
            //get the mouse position and the grapple position
            Vector2 mousePos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            Vector2 grappleStart = new Vector2(grapplePoint.position.x, grapplePoint.position.y);

            //send out a raycast to see if it hits something
            RaycastHit2D hit = Physics2D.Raycast(grappleStart, mousePos-grappleStart, grappleLength, mask);
        
            // if it hits
            if(hit)
            {
                //store the object in a variable
                Transform objectHit = hit.transform;
                //check to see what kind of object you hit
                if(objectHit.tag == "Grapplable")
                {
                    //velocity = new Vector2(0,0);
                    distance = hit.distance;
                    //grappleVelocity = (2*hit.distance)/grappleTime;
                    destination = objectHit.transform.position;
                    pos = new Vector2(this.transform.position.x, this.transform.position.y);
                    grappling =true;
                }
                else if(objectHit.tag == "Pullable")
                {
                    objectHit.transform.position = this.transform.position;
                }
            }
            //draw a line to visually debug
            Debug.DrawLine(grappleStart, mousePos);
        }

        
    }

    void grappleMove()
    {
        if(Input.GetKeyDown(jump))
        {
            grappling = false;
            return;
        }
        //while grappling get the position of the player currently
        Vector2 currentPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        
        //set its velocity in x and y to move towards the grapple location
        //Change the moveSpeed *1.5f to grappleVelocity to see constant time
        float targetVelocityX = ((destination.x - currentPosition.x)/distance) * moveSpeed * 1.5f;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);


        float targetVelocityY = ((destination.y - currentPosition.y)/distance) * moveSpeed * 1.5f;
        velocity.y = Mathf.SmoothDamp(velocity.y, targetVelocityY, ref velocityYSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        
        //Uncomment these two lines if you want to see without damping(instant acceleration)
        //velocity.x = ((destination.x - pos.x)/distance) * moveSpeed * 1.5f;
        //velocity.y = ((destination.y - pos.y)/distance) * moveSpeed * 1.5f;

        //prints the x and y velocity to the console
        Debug.Log(velocity.x + "  " + velocity.y);
        Debug.Log((destination - currentPosition).magnitude);
        //moves player
        controller.Move(velocity * Time.deltaTime);
        //if the player is within an acceptable distance stop grappling
        if((destination - currentPosition).magnitude < stopGrapple)
        {
            Debug.Log("Done Grappling");
            grappling = false;
        }
    }
}
