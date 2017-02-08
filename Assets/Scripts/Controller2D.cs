using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    [SerializeField]
    private float MIN_JUMP_HEIGHT;
    [SerializeField]
    private float MAX_JUMP_HEIGHT;
    [SerializeField]
    private float JUMP_TIME_TO_APEX;
    [SerializeField]
    private float SPEED;
    [SerializeField]
    private float SKIN_WIDTH;
    [SerializeField]
    private LayerMask TERRAIN_LAYER;
    [SerializeField]
    private float X_SMOOTHING_GROUND;
    [SerializeField]
    private float X_SMOOTHING_AIRBORNE;
    [SerializeField]
    private float MAX_CLIMBABLE_SLOPE;

    private float gravity;
    private float maxJumpSpeed;
    private float minJumpSpeed;
    private float smoothVelocityX;

    Vector2 velocity;
    CollisionState state;
    BoxCollider2D body;

    void Start()
    {
        gravity = (2 * MAX_JUMP_HEIGHT) / Mathf.Pow(JUMP_TIME_TO_APEX, 2);
        maxJumpSpeed = gravity * JUMP_TIME_TO_APEX;
        minJumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MIN_JUMP_HEIGHT);

        state = new CollisionState();
        body = GetComponent<BoxCollider2D>();
    }

    public void Move(Vector2 input)
    {

        ApplyAccelerationForces();
        ApplyConstantMovement(input);

        state.Reset();

        Vector2 currentVelocity = velocity * Time.deltaTime;
        CheckCollisions(ref currentVelocity);
        transform.Translate(currentVelocity);
        
        ApplyAccelerationForces();
    }

    void ApplyAccelerationForces()
    {
        velocity.y -= gravity / 2 * Time.deltaTime;
    }

    void ApplyConstantMovement(Vector2 input)
    {
        float targetVelocityX = input.x * SPEED;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref smoothVelocityX, 
            state.grounded ? X_SMOOTHING_GROUND : X_SMOOTHING_AIRBORNE);

        if (state.grounded && input.y == 1)
        {
            velocity.y = maxJumpSpeed;
            state.jumping = true;
        }

        if (state.jumping && input.y != 1)
        {
            if (velocity.y > minJumpSpeed)
            {
                velocity.y = minJumpSpeed;
            }
            state.jumping = false;
        }
    }

    void CheckCollisions(ref Vector2 currentVelocity)
    {

        // Get body border and reduce it by skin size
        Bounds bounds = body.bounds;
        bounds.Expand(SKIN_WIDTH * -2f);

        // This will do a raycast if last frame we were grounded to glue us to the ground so we don't bounce on downslopes
        if (state.previouslyGrounded && velocity.y != maxJumpSpeed)
        {
            Vector2 rayOrigin;
            if (state.previousSlope == 0f)
            {
                rayOrigin = new Vector2(Mathf.Sign(currentVelocity.x) == -1 ? bounds.max.x : bounds.min.x, bounds.min.y);
            }
            else
            {
                rayOrigin = new Vector2(Mathf.Sign(state.previousSlope) == -1 ? bounds.max.x : bounds.min.x, bounds.min.y);
            }

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f + SKIN_WIDTH, TERRAIN_LAYER);

            if (hit.collider != null)
            {
                currentVelocity.y = -1f;
            }
        }
        
        // Run collision detection until we don't hit anything, limit to 10 times in case we get stuck in a wall
        for (int i = 0; i < 10; i++)
        {
            RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, currentVelocity, currentVelocity.magnitude, TERRAIN_LAYER);

            // Stop if we didn't hit anything
            if (hit.collider == null) return;

            // Calculate some numbers we will use later
            Vector2 distanceToHit = hit.centroid - (Vector2)bounds.center;
            Vector2 distanceAfterHit = currentVelocity - distanceToHit;
            float slopeOfCollider = hit.normal.x / hit.normal.y;

            // Hit floor or ceiling
            if (Mathf.Abs(slopeOfCollider) <= MAX_CLIMBABLE_SLOPE)
            {
                // If we hit both the ground and ceiling in the same frame
                if ((state.grounded && hit.normal.y < 0) || (state.ceilingCollision && hit.normal.y > 0))
                {
                    // Limit horizontal movement so we don't go through the ceiling or floor
                    currentVelocity.x = distanceToHit.x;
                    currentVelocity.x += SKIN_WIDTH * Mathf.Sign(hit.normal.x);
                    distanceAfterHit = currentVelocity - distanceToHit;
                }

                // Limit the movement to how far away the collider is
                currentVelocity.y = distanceToHit.y;
                // Calculate how far up and down we need to move to stay on a slope
                currentVelocity.y += distanceAfterHit.x * -slopeOfCollider;
                currentVelocity.y += SKIN_WIDTH * Mathf.Sign(hit.normal.y);
                
                // Hit ground
                if (hit.normal.y > 0)
                {
                    // Reset overall velocity to cancel out gravity
                    velocity.y = 0;

                    state.grounded = true;
                    state.groundSlope = slopeOfCollider;
                }
                // Hit ceiling
                else 
                {
                    state.ceilingCollision = true;
                    state.groundSlope = slopeOfCollider;

                    // Reset overall velocity so we don't cling to the ceiling
                    if (velocity.y > 0)
                    {
                        velocity.y = 0;
                    }
                }
            }

            // Hit wall
            else
            {
                // If grounded
                if (state.grounded)
                {
                    // Move back up/down slope the distance we had to stop early
                    currentVelocity.y -= distanceAfterHit.x * -state.groundSlope;
                    distanceAfterHit = currentVelocity - distanceToHit;
                }

                // Find out what we collided with
                if (hit.normal.x > 0)
                {
                    state.leftCollision = true;
                }
                else
                {
                    state.rightCollision = true;
                }

                // If we hit both a left and right wall in the same frame
                if (state.leftCollision && state.rightCollision)
                {
                    // Limit vertical movement so we don't go through walls
                    currentVelocity.y = distanceToHit.y;
                    currentVelocity.y += SKIN_WIDTH * Mathf.Sign(hit.normal.y);
                    distanceAfterHit = currentVelocity - distanceToHit;

                    // Landed in a pit "\/"
                    if (velocity.y < 0)
                    {
                        state.grounded = true;
                    }
                }

                currentVelocity.x = distanceToHit.x;
                currentVelocity.x += distanceAfterHit.y / -slopeOfCollider;
                currentVelocity.x += SKIN_WIDTH * Mathf.Sign(hit.normal.x);
                velocity.x = 0;

            }
        }
    }

    struct CollisionState
    {
        public bool grounded;
        public bool ceilingCollision;
        public bool leftCollision;
        public bool rightCollision;
        public float groundSlope;
        public bool jumping;
        public bool previouslyGrounded;
        public float previousSlope;

        public void Reset()
        {
            previouslyGrounded = grounded;
            previousSlope = groundSlope;

            grounded = false;
            ceilingCollision = false;
            leftCollision = false;
            rightCollision = false;

        }
    }
}
