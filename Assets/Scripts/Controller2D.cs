using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    [SerializeField]
    private float JUMP_HEIGHT;
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
    private float jumpSpeed;
    private float smoothVelocityX;

    Vector2 velocity;
    CollisionState state;
    BoxCollider2D body;

    void Start()
    {
        gravity = (2 * JUMP_HEIGHT) / Mathf.Pow(JUMP_TIME_TO_APEX, 2);
        jumpSpeed = gravity * JUMP_TIME_TO_APEX;

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
            velocity.y = jumpSpeed;
            state.jumping = true;
        }
    }

    void CheckCollisions(ref Vector2 currentVelocity)
    {
        Bounds bounds = body.bounds;
        bounds.Expand(SKIN_WIDTH * -2f);

        RaycastHit2D hit;

        for (int i = 0; i < 10; i++)
        { 
            hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, currentVelocity, currentVelocity.magnitude + SKIN_WIDTH, TERRAIN_LAYER);
            if (hit.collider == null) return;

            Vector2 distanceToHit = hit.centroid - (Vector2)bounds.center;
            Vector2 distanceAfterHit = currentVelocity - distanceToHit;
            float slopeOfCollider = hit.normal.x / hit.normal.y;

            // Hit floor or ceiling
            if (Mathf.Abs(slopeOfCollider) <= MAX_CLIMBABLE_SLOPE)
            {
                currentVelocity.y = distanceToHit.y;
                currentVelocity.y += distanceAfterHit.x * -slopeOfCollider;
                currentVelocity.y += SKIN_WIDTH * Mathf.Sign(hit.normal.y);

                if (hit.normal.y > 0)
                {
                    velocity.y = 0;
                    state.grounded = true;
                    state.groundSlope = slopeOfCollider;
                }
                else if (velocity.y > 0)
                {
                    velocity.y = 0;
                }
            }

            // Hit wall
            else
            {
                if (state.grounded)
                {
                    currentVelocity.y -= distanceAfterHit.x * -state.groundSlope;
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
        public float groundSlope;
        public bool jumping;

        public void Reset()
        {
            grounded = false;
            groundSlope = 0;
        }
    }
}
