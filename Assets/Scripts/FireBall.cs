using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Enemy
{
    public const float speed = 2.5f;
    private Vector2 direction;
    // Start is called before the first frame update
    private void Start()
    {
        enemyType = EEnemyType.Fireball;
    }
    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTrigger(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
