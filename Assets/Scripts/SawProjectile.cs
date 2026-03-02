using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SawProjectile : MonoBehaviour
{
    [SerializeField]
    private string monsterLayerName = "monster";

    private SawController owner;
    private Vector2 moveDirection = Vector2.right;
    private float speed;
    private float expireTime;

    public void Initialize(SawController sawController, Vector2 direction, float moveSpeed, float lifeTime)
    {
        owner = sawController;
        speed = Mathf.Max(0f, moveSpeed);
        moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        expireTime = Time.time + Mathf.Max(0.05f, lifeTime);
        AlignToDirection();
    }

    private void Update()
    {
        if (GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
            return;
        }

        MoveAndBounce();
    }

    private void MoveAndBounce()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
            return;
        }

        Vector3 position3 = transform.position;
        Vector2 nextPosition = (Vector2)position3 + moveDirection * speed * Time.deltaTime;

        float distanceToPlane = Mathf.Abs(position3.z - camera.transform.position.z);
        if (distanceToPlane <= 0.01f)
        {
            distanceToPlane = Mathf.Abs(camera.transform.position.z);
        }

        Vector3 min = camera.ViewportToWorldPoint(new Vector3(0f, 0f, distanceToPlane));
        Vector3 max = camera.ViewportToWorldPoint(new Vector3(1f, 1f, distanceToPlane));

        bool bouncedX = false;
        bool bouncedY = false;

        if (nextPosition.x <= min.x || nextPosition.x >= max.x)
        {
            nextPosition.x = Mathf.Clamp(nextPosition.x, min.x, max.x);
            moveDirection.x *= -1f;
            bouncedX = true;
        }

        if (nextPosition.y <= min.y || nextPosition.y >= max.y)
        {
            nextPosition.y = Mathf.Clamp(nextPosition.y, min.y, max.y);
            moveDirection.y *= -1f;
            bouncedY = true;
        }

        if (bouncedX || bouncedY)
        {
            ApplyCornerBounceSpeedCorrection(bouncedX, bouncedY);
            moveDirection.Normalize();
            AlignToDirection();
        }

        transform.position = new Vector3(nextPosition.x, nextPosition.y, position3.z);
    }

    private void ApplyCornerBounceSpeedCorrection(bool bouncedX, bool bouncedY)
    {
        if (owner == null)
        {
            return;
        }

        Vector2 velocity = moveDirection * speed;
        Vector2 playerVelocity = owner.GetPlayerVelocity();

        if (bouncedX)
        {
            float requiredX = Mathf.Abs(playerVelocity.x);
            if (Mathf.Abs(velocity.x) < requiredX)
            {
                velocity.x = Mathf.Sign(moveDirection.x) * requiredX;
            }
        }

        if (bouncedY)
        {
            float requiredY = Mathf.Abs(playerVelocity.y);
            if (Mathf.Abs(velocity.y) < requiredY)
            {
                velocity.y = Mathf.Sign(moveDirection.y) * requiredY;
            }
        }

        if (velocity.sqrMagnitude > 0.0001f)
        {
            speed = velocity.magnitude;
            moveDirection = velocity.normalized;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.gameObject);
    }

    private void TryDamage(GameObject target)
    {
        if (target == null || owner == null || GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        if (!IsLayerName(target.layer, monsterLayerName))
        {
            return;
        }

        DamageSystem.ApplyPlayerDamage(target, owner.CalculateDamage());
    }

    private void AlignToDirection()
    {
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private static bool IsLayerName(int layer, string expectedLayerName)
    {
        if (string.IsNullOrWhiteSpace(expectedLayerName))
        {
            return false;
        }

        return string.Equals(LayerMask.LayerToName(layer), expectedLayerName, StringComparison.OrdinalIgnoreCase);
    }
}
