using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float StartSpeed = 8f;       // Starting speed
    public float ExtraSpeed = 0.5f;     // Speed added per hit
    public float MaxExtraSpeed = 8f;    // Max speed boost from hits (ball can double)
    public float MinVerticalRatio = 0.2f; // Minimum Y velocity ratio to prevent horizontal deadlock
    public bool player1Start=true;
    private int HitCounter = 0;
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(Launch());
    }

    void Update()
    {
        // Prevent horizontal deadlock by ensuring minimum vertical velocity
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float speed = rb.linearVelocity.magnitude;
            float minYVelocity = speed * MinVerticalRatio;
            
            if (Mathf.Abs(rb.linearVelocity.y) < minYVelocity)
            {
                // Add vertical velocity while preserving horizontal direction and speed
                float signY = rb.linearVelocity.y >= 0 ? 1f : -1f;
                // If Y is exactly 0, pick a random direction
                if (Mathf.Abs(rb.linearVelocity.y) < 0.01f)
                    signY = Random.value > 0.5f ? 1f : -1f;
                
                Vector2 newVelocity = new Vector2(rb.linearVelocity.x, signY * minYVelocity);
                rb.linearVelocity = newVelocity.normalized * speed;
            }
        }
    }

    private void RestartBall()
    {
        rb.linearVelocity = new Vector2(0, 0);
        transform.position = new Vector2(0, 0);
    }
    public IEnumerator Launch()
    {
        RestartBall();  
        HitCounter = 0;
        yield return new WaitForSeconds(1);

        // Ball goes left if player1 starts, right if player2 starts
        float x = player1Start ? Random.Range(-1f, -0.5f) : Random.Range(0.5f, 1f);
        // Ensure Y has a minimum magnitude to avoid flat trajectories
        float y = Random.Range(0.3f, 0.7f) * (Random.value > 0.5f ? 1f : -1f);
    
        MoveBall(new Vector2(x, y));
    }
    public void MoveBall(Vector2 direction)
    {
        direction = Vector2.Normalize(direction);
        float BallSpeed= StartSpeed + HitCounter * ExtraSpeed;
        
        // Clamp ball speed to prevent runaway velocities
        BallSpeed = Mathf.Min(BallSpeed, StartSpeed + MaxExtraSpeed);
        
        Debug.Log($"Ball Speed: {BallSpeed} (Hit #{HitCounter})");
        rb.linearVelocity = direction * BallSpeed;
    }

    public void IncreaseHitCounter()
    {
        HitCounter++;
        Debug.Log($"Hit Counter increased to: {HitCounter}");
    }
}
