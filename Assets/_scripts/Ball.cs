using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Ball : MonoBehaviour 
{
    public BoxCollider2D collider;

    public void TurnOn()
    {
        collider.enabled = true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name.Contains("goal"))
        {
            if (col.gameObject.name.Contains("left"))
                Hockey.Instance.Score(1);
            else
                Hockey.Instance.Score(0);

            return;
        }
        else if (col.gameObject.name.Contains("paddle"))
        {
            // Calculate hit Factor
            float y = hitFactor(transform.localPosition, col.transform.localPosition, col.gameObject.GetComponent<Image>().rectTransform.rect.height);

            Vector2 dir = Vector2.zero;

            // Hit the left Racket?
            if (col.gameObject.name.Contains("left"))
            {
                // Calculate direction, make length=1 via .normalized
                dir = new Vector2(1, y).normalized;

                // this is the player hitting it... randomise the CPU chance to hit back
                Hockey.Instance.RandomiseCpuHit();
            }
            // Hit the right Racket?
            else if (col.gameObject.name.Contains("right"))
            {
                // Calculate direction, make length=1 via .normalized
                dir = new Vector2(-1, y).normalized;
            }

            // Set Velocity with dir * speed
            GetComponent<Rigidbody2D>().velocity = dir * Hockey.Instance.ballSpeed;

            Hockey.Instance.AS_hit.Play();
        }
    }

    float hitFactor(Vector2 ballPos, Vector2 racketPos, float racketHeight)
    {
        // ascii art:
        // ||  1 <- at the top of the racket
        // ||
        // ||  0 <- at the middle of the racket
        // ||
        // || -1 <- at the bottom of the racket
        return (ballPos.y - racketPos.y) / racketHeight;
    }
}
