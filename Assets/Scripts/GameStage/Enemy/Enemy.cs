using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    public float deathDuration = 0.5f;
    
    // [추가] 체력바 프리팹 연결용
    public GameObject healthBarPrefab; 
    
    // [추가] 생성된 체력바 제어용
    EnemyHealthBar healthBar; 

    int currentHealth;
    bool isDead = false;
    SpriteRenderer spriteRenderer;
    Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // [추가] 체력바 생성 및 초기화
        if (healthBarPrefab != null)
        {
            // 적의 자식으로 생성하여 따라다니게 함
            GameObject hbObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity, transform);
            healthBar = hbObj.GetComponent<EnemyHealthBar>();
            
            // 위치 조정 (머리 위)
            hbObj.transform.localPosition = healthBar.offset;
            
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        
        // [추가] 데미지 팝업 생성 ---------------------------
        if (GameManager.instance != null && GameManager.instance.damagePopupPrefab != null)
        {
            // 데미지 텍스트 띄우기
            DamagePopup.Create(transform.position, damage, GameManager.instance.damagePopupPrefab);
        }
        // --------------------------------------------------

        // 체력바 갱신
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0) Die();
        else StartCoroutine(OnHitEffect());
    }

    // ... (Die, OnHitEffect 등 나머지 코드는 그대로 유지) ...
    void Die()
    {
        isDead = true;
        
        // 체력바 먼저 숨기기 (안 그러면 0인 상태로 둥둥 떠다님)
        if (healthBar != null) healthBar.gameObject.SetActive(false);

        // ... 기존 사망 로직 ...
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDie());
    }
    
    IEnumerator OnHitEffect()
    {
        spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator FadeOutAndDie()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        
        MonoBehaviour patrol = GetComponent("EnemyPatrol") as MonoBehaviour;
        if (patrol != null) patrol.enabled = false;

        // 체력바 숨기기
        if (healthBar != null) healthBar.gameObject.SetActive(false);

        // [핵심] 쉐이더를 변경해야만 실루엣이 하얗게 보입니다.
        // 이 줄이 없으면 그냥 투명해지기만 합니다.
        spriteRenderer.material.shader = Shader.Find("GUI/Text Shader");
        
        spriteRenderer.color = Color.white; // 순백색 설정

        float currentAlpha = 1f;
        while (currentAlpha > 0)
        {
            currentAlpha -= Time.deltaTime / deathDuration;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}