using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Test Settings")]
    public WeaponData[] testWeapons; // 여기에 Sword_Basic, Whip_Basic 등을 넣을 겁니다.
    int currentWeaponIndex = 0;
    
    [Header("Physics Settings")]
    public float moveSpeed = 5f;       
    public float jumpPower = 10f;      
    public float fallMultiplier = 2f;
    public float lowJumpMultiplier = 3f;
    
    [Header("Collision Layers")]
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("Ground Check Settings")]
    public float rayDistance = 0.2f;
    public Vector2 boxSize = new Vector2(0.8f, 0.2f);
    public Vector2 castOffset = new Vector2(0, 0.4f);

    [Header("Components")]
    public Transform firePoint; // 투사체 발사 위치 (여기 딱 하나만 있어야 합니다!)

    [Header("Debug Settings")]
    public bool showHitboxAlways = true;

    // [변경] 입력을 저장해둘 변수들 (전역 변수로 승격)
    float xInput;
    bool jumpPressed;

    // 내부 변수
    float attackTimer = 0f;
    bool isInvincible = false;
    bool isGrounded;
    float defaultGravity;
    
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;
    PlayerStats stats;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<PlayerStats>();
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }

        CheckGround();
        CheckBodyCollision();

        UpdateAnimation();
        HandleAttack();

        // [추가] 테스트용 무기 교체 (R키)
        if (Input.GetKeyDown(KeyCode.R))
        {
            SwapWeapon();
        }
    }
    
    void SwapWeapon()
    {
        if (testWeapons.Length == 0) return;

        // 다음 인덱스로 이동 (목록 끝에 도달하면 0번으로 돌아감)
        currentWeaponIndex = (currentWeaponIndex + 1) % testWeapons.Length;

        // PlayerStats의 함수를 호출해서 무기 교체 + UI 갱신
        stats.EquipWeapon(testWeapons[currentWeaponIndex]);
    }

    // -----------------------------------------------------------
    // 공격 입력 및 마나 회복 처리
    // -----------------------------------------------------------

    void FixedUpdate()
    {
        Move();
        Jump();
        ModifyPhysics();
    }
    void HandleAttack()
    {
        if (stats.currentWeapon == null) return;

        attackTimer += Time.deltaTime;
        
        if (Input.GetButtonDown("Fire1") && attackTimer > stats.currentWeapon.cooldown)
        {
            PerformAttack();
            attackTimer = 0f;
        }
    }

    // -----------------------------------------------------------
    // 공격 분기점
    // -----------------------------------------------------------
    void PerformAttack()
    {
        switch (stats.currentWeapon.type)
        {
            case WeaponType.Sword:
                PerformSwordAttack();
                break;
            case WeaponType.Whip:
                PerformWhipAttack();
                break;
            case WeaponType.Bow:
                PerformBowAttack();
                break;
            case WeaponType.Staff:
                PerformStaffAttack();
                break;
        }
    }

    // -----------------------------------------------------------
    // 클래스별 개별 로직
    // -----------------------------------------------------------
    void PerformSwordAttack()
    {
        ExecuteMeleeLogic();
    }

    void PerformWhipAttack()
    {
        ExecuteMeleeLogic();
    }

    void PerformBowAttack()
    {
        ExecuteRangedLogic();
    }

    void PerformStaffAttack()
    {
        ExecuteRangedLogic();
    }

    // -----------------------------------------------------------
    // 공통 실행 함수 (중복 방지)
    // -----------------------------------------------------------
    void ExecuteMeleeLogic()
    {
        WeaponData weapon = stats.currentWeapon;
        float direction = transform.localScale.x; 

        Vector2 hitCenter = (Vector2)transform.position + new Vector2(weapon.hitboxXOffset * direction, weapon.hitboxYOffset);

        if (weapon.effectPrefab != null)
        {
            Vector2 effectPos = (Vector2)transform.position 
                              + new Vector2(weapon.visualXOffset * direction, weapon.visualYOffset);
            GameObject effect = Instantiate(weapon.effectPrefab, effectPos, Quaternion.identity);
            Vector3 scale = effect.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction; 
            effect.transform.localScale = scale;
        }

        // [핵심 변경] 적중 시 마나 회복
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitCenter, weapon.hitboxSize, 0, enemyLayer);
        foreach(Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if(enemy != null) 
            {
                enemy.TakeDamage(stats.GetTotalDamage());
                
                // [추가됨] 적을 때렸으므로 마나 회복!
                stats.GainMana();
            }
        }
    }

    // 3. 원거리 공격 로직 (수정됨)
    void ExecuteRangedLogic()
    {
        WeaponData weapon = stats.currentWeapon;
        if (weapon.effectPrefab == null) return;

        Quaternion rot = Quaternion.identity;
        if (transform.localScale.x < 0) rot = Quaternion.Euler(0, 0, 180);

        GameObject bulletObj = Instantiate(weapon.effectPrefab, firePoint.position, rot);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            // [수정됨] Bullet이 '누구의' 마나를 채워줄지 알아야 하므로 'stats' 본인을 넘겨줍니다.
            bullet.Initialize(stats, stats.GetTotalDamage(), weapon.projectileSpeed, weapon.projectileRange, weapon.isPiercing);
        }
    }

    void Move()
    {
        // Shift 키 처리 (여기서도 입력 확인 필요)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        // PlayerStats에서 이속 가져오는 게 있다면 그것 사용, 여기서는 간단히 구현
        // float currentSpeed = isSprinting ? stats.moveSpeed * 1.5f : stats.moveSpeed; 
        // (기존에 stats.moveSpeed를 쓰고 계셨다면 그대로 쓰세요)
        
        // 여기서는 간단히 moveSpeed 변수 사용 예시
        float currentSpeed = moveSpeed; 
        if(isSprinting) currentSpeed *= 1.5f;

        // [중요] xInput 변수를 사용해 이동
        rb.linearVelocity = new Vector2(xInput * currentSpeed, rb.linearVelocity.y);

        // 미끄러짐 방지
        if (xInput == 0 && isGrounded) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // 방향 전환
        if (xInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (xInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void Jump()
    {
        // 저장해둔 점프 신호가 있고 + 땅에 있다면
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            
            jumpPressed = false; // 점프 했으니 신호 끄기
        }
        else
        {
            // 땅에 없거나 점프 조건이 안 맞으면 신호 초기화 (예약 점프 방지)
            // (원한다면 코요테 타임 등을 위해 남겨둘 수도 있음)
            jumpPressed = false; 
        }
    }

    void CheckBodyCollision()
    {
        if (isInvincible) return;
        Vector2 checkPos = (Vector2)transform.position + new Vector2(0, 0.5f);
        Collider2D hit = Physics2D.OverlapBox(checkPos, new Vector2(0.6f, 0.8f), 0, enemyLayer);
        if (hit != null) TakeDamage(1);
    }

    public void TakeDamage(int damage)
    {
        stats.TakeDamage(damage);
        StartCoroutine(OnDamageEffect());
    }

    IEnumerator OnDamageEffect()
    {
        isInvincible = true;
        for (int i = 0; i < 4; i++)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.25f);
            yield return new WaitForSeconds(0.25f);
            spriteRenderer.color = new Color(1, 1, 1, 0.75f);
            yield return new WaitForSeconds(0.25f);
        }
        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    void CheckGround()
    {
        Vector2 origin = (Vector2)transform.position + castOffset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, rayDistance, groundLayer);
        isGrounded = (hit.collider != null);
    }

    void ModifyPhysics()
    {
        if (rb.linearVelocity.y < 0) rb.gravityScale = defaultGravity * fallMultiplier;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump")) rb.gravityScale = defaultGravity * lowJumpMultiplier;
        else rb.gravityScale = defaultGravity;
    }

    void UpdateAnimation()
    {
        // 움직이는 중인가?
        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0;
        
        // 달리는 중인가? (움직임 + Shift키)
        bool isSprinting = isMoving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // 애니메이터에 파라미터 전달
        anim.SetBool("isRunning", isMoving);    // 기존: 걷기/대기 판별용
        anim.SetBool("isSprinting", isSprinting); // 신규: 걷기/달리기 판별용
        
        anim.SetBool("isGrounded", isGrounded);
        float vY = rb.linearVelocity.y;
        if (vY < 0) vY = 0;
        anim.SetFloat("vSpeed", vY);
    }

    // -----------------------------------------------------------
    // 디버그용 기즈모 (히트박스 표시)
    // -----------------------------------------------------------
    void OnDrawGizmos()
    {
        if (showHitboxAlways) DrawWeaponHitbox();
    }

    void OnDrawGizmosSelected()
    {
        if (!showHitboxAlways) DrawWeaponHitbox();
    }

    void DrawWeaponHitbox()
    {
        if (stats == null || stats.currentWeapon == null) return;
        
        WeaponType t = stats.currentWeapon.type;
        // 근접 무기만 그림 (Sword, Whip)
        if (t != WeaponType.Sword && t != WeaponType.Whip) return;

        Gizmos.color = Color.red;
        
        float direction = transform.localScale.x;
        direction = Mathf.Sign(direction);

        Vector2 hitCenter = (Vector2)transform.position + new Vector2(stats.currentWeapon.hitboxXOffset * direction, stats.currentWeapon.hitboxYOffset);
        Gizmos.DrawWireCube(hitCenter, stats.currentWeapon.hitboxSize);
    }
}