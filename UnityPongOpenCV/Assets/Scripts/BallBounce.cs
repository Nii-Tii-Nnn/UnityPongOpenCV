using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class BallBounce : MonoBehaviour
{
    public GameObject hitSFX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public BallMovement ballMovement;
    [Range(15f, 75f)]
    public float maxBounceAngle = 60f;
    private float collisionCooldown = 0f;
    private float collisionCooldownTime = 0.1f; // Prevent multiple collisions within 100ms
    public ScoreManager scoreManager;

    void Start()
    {
        // Auto-find BallMovement if not assigned
        if (ballMovement == null)
        {
            ballMovement = GetComponent<BallMovement>();
        }
    }

    private void Bounce(Collision2D collision)
    {
        Vector3 ballPosition = transform.position;
        Vector3 racketPosition = collision.transform.position;
        float racketHeight = collision.collider.bounds.size.y;

        float directionX = collision.gameObject.CompareTag("Player1") ? 1f : -1f;
        float hitPoint = (ballPosition.y - racketPosition.y) / racketHeight;
        hitPoint = Mathf.Clamp(hitPoint, -1f, 1f);

        float bounceAngleRad = hitPoint * maxBounceAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(
            directionX * Mathf.Cos(bounceAngleRad),
            Mathf.Sin(bounceAngleRad)
        );

        ballMovement.IncreaseHitCounter();
        ballMovement.MoveBall(direction);
    }

    private void Update()
    {
        if (collisionCooldown > 0)
            collisionCooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionCooldown > 0)
            return; // Ignore collision if cooldown is still active

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            
            Bounce(collision);
            collisionCooldown = collisionCooldownTime;
        }
        else if(collision.gameObject.name=="RightBorder")
        {
            scoreManager.player1Goal();
            ballMovement.player1Start=true;  // Ball goes to player 1 (who scored)
            StartCoroutine(ballMovement.Launch());
        }
        else if(collision.gameObject.name=="LeftBorder")
        {
            scoreManager.player2Goal();
            ballMovement.player1Start=false;  // Ball goes to player 2 (who scored)
            StartCoroutine(ballMovement.Launch());
        }
        Instantiate(hitSFX, transform.position, transform.rotation);
    }
}
