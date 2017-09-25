using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    [RequireComponent(typeof(BoxCollider2D))]
    class Collisions2D : MonoBehaviour
    {
        private BoxCollider2D body;

        private const float SKIN_WIDTH = .02f;

        void Start()
        {
            body = GetComponent<BoxCollider2D>();
        }
        public void Move(Vector2 velocity)
        {
            transform.Translate(velocity);
        }

        public void CheckCollisions(Vector2 velocity, LayerMask mask)
        {
            CollisionInfo collisionInfo = new CollisionInfo();
            CheckHorizontalCollisions(velocity, mask, ref collisionInfo);
        }

        private void CheckHorizontalCollisions(Vector2 velocity, LayerMask layerMask, ref CollisionInfo collisionInfo)
        {
            Bounds bounds = body.bounds;
            bounds.Expand(SKIN_WIDTH * -2f);

            RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, 
                                                 Vector2.right * Mathf.Sign(velocity.x), velocity.x, 
                                                 layerMask);

            if (hit.collider == null) return;


        }

        public struct CollisionInfo
        {
            bool leftCollision, rightCollision;
        }
    }
}
