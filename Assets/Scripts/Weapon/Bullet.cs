using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    // [추가됨] 주인의 스탯을 기억할 변수
    PlayerStats ownerStats; 
    int damage;
    float speed;
    float maxDistance;
    bool isPiercing;

    Vector2 startPos;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    bool isDying = false;

    // [수정됨] 초기화할 때 PlayerStats를 받아옵니다.
    public void Initialize(PlayerStats owner, int dmg, float spd, float range, bool piercing)
    {
        ownerStats = owner; // 주인 등록
        damage = dmg;
        speed = spd;
        maxDistance = range;
        isPiercing = piercing;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.position;
        rb.linearVelocity = transform.right * speed;
    }

    void Update()
    {
        if (isDying) return;
        if (Vector2.Distance(startPos, transform.position) > maxDistance) 
            StartCoroutine(FadeOutAndDestroy());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDying) return;

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null) 
            {
                enemy.TakeDamage(damage);

                // [추가됨] 적중했으니 주인의 마나 회복!
                if (ownerStats != null)
                {
                    ownerStats.GainMana();
                }
            }
            if (!isPiercing) StartCoroutine(FadeOutAndDestroy());
        }
        else if (collision.CompareTag("Ground")) 
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        isDying = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;
        
        // --- 오류 해결 부분: 최소 1프레임 대기 ---
        yield return null; 
        
        Destroy(gameObject); 
    }
}