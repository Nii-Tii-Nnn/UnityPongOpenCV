using UnityEngine;

public class Player2 : MonoBehaviour
{
    public float racketSpeed = 8f;
    private Rigidbody2D rb;
    private Vector2 racketDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float dirY = 0f;

        if (HandCommand.p2Command == "P2_UP")
            dirY = 1f;
        else if (HandCommand.p2Command == "P2_DOWN")
            dirY = -1f;

        racketDirection = new Vector2(0, dirY);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = racketDirection * racketSpeed;
    }
}
