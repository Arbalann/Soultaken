using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    [SerializeField]
    private float GRAVITY = -20;
    [SerializeField]
    private float SPEED = 7;
    [SerializeField]
    private float SKIN_WIDTH = .01f;
    [SerializeField]
    private LayerMask TERRAIN_LAYER;

    Vector2 velocity;

    BoxCollider2D body;

    void Start()
    {
        body = GetComponent<BoxCollider2D>();
    }

    public void Move(Vector2 direction)
    {       
        ApplyAccelerationForces();
        ApplyConstantMovement(direction);

        Vector2 currentVelocity = velocity * Time.deltaTime;
        CheckCollisions(ref currentVelocity);
        transform.Translate(currentVelocity);

        ApplyAccelerationForces();
    }

    void ApplyAccelerationForces()
    {
        velocity.y += GRAVITY / 2 * Time.deltaTime;
    }

    void ApplyConstantMovement(Vector2 direction)
    {
        velocity.x = direction.x * SPEED;
    }

    void CheckCollisions(ref Vector2 currentVelocity)
    {
        Bounds bounds = body.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        RaycastHit2D hit;

        if (currentVelocity.x != 0f)
        {
            Vector2 direction = currentVelocity.x > 0 ? Vector2.right : Vector2.left;
            hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, direction, Mathf.Abs(currentVelocity.x) + SKIN_WIDTH, TERRAIN_LAYER);
            if (hit.collider != null)
            {
                Vector2 distanceToHit = hit.centroid - (Vector2)bounds.center;

                velocity.x = 0;
                currentVelocity.x = distanceToHit.x - (SKIN_WIDTH * Mathf.Sign(distanceToHit.x));
            }
        }


        if (currentVelocity.y != 0f)
        {
            Vector2 direction = currentVelocity.y > 0 ? Vector2.up : Vector2.down;
            hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, direction, Mathf.Abs(currentVelocity.y) + SKIN_WIDTH, TERRAIN_LAYER);
            if (hit.collider != null)
            {
                Vector2 distanceToHit = hit.centroid - (Vector2)bounds.center;

                velocity.y = 0;
                currentVelocity.y = distanceToHit.y - (SKIN_WIDTH * Mathf.Sign(distanceToHit.y));
            }
        }
    }
}
