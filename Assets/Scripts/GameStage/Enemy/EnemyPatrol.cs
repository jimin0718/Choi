using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float moveSpeed = 2f;
    public float rayDistance = 1f;
    public float wallCheckDistance = 0.5f;
    public Transform frontCheck;
    public LayerMask groundLayer;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float detectionRange = 6f; // 감지 범위는 넓게
    public float stopDistance = 1.0f; // 정지 거리는 좁게! (중요)
    public bool stopAtEdge = true;

    [Header("Jump Settings")]
    public float jumpPower = 5f;
    public float jumpCheckHeight = 1.5f;
    public float jumpCooldown = 1.0f;
    public Transform footCheck;

    Rigidbody2D rb;
    bool isFacingRight = true;
    Transform player;
    float jumpTimer = 0f;
    bool isGrounded = false;
    bool isChasing = false; // 디버그용

    void Start() 
    { 
        rb = GetComponent<Rigidbody2D>(); 
        PlayerController pc = FindFirstObjectByType<PlayerController>(); // 최신 문법 권장
        if (pc != null) player = pc.transform;

        if (footCheck == null) footCheck = transform;
    }

    void Update()
    {
        if (jumpTimer > 0) jumpTimer -= Time.deltaTime;
        CheckGround();

        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }

    void CheckGround()
    {
        // 반지름을 0.2 -> 0.3으로 살짝 키워서 관대하게 판정
        isGrounded = Physics2D.OverlapCircle(footCheck.position, 0.3f, groundLayer);
    }

    void Patrol()
    {
        isChasing = false;
        rb.linearVelocity = new Vector2(transform.right.x * moveSpeed, rb.linearVelocity.y);
        
        if (CheckCliff() || CheckWall())
        {
            Flip();
        }
    }

    void Chase()
    {
        isChasing = true;

        // 1. X축 거리만 계산 (가로 거리)
        float xDistance = Mathf.Abs(player.position.x - transform.position.x);

        // 2. 방향 전환
        if (xDistance > 0.2f) 
        {
            float direction = (player.position.x > transform.position.x) ? 1f : -1f;
            if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
            {
                Flip();
            }
        }

        // 3. [핵심] 정지 거리 체크
        // 플레이어와 너무 가까우면(공격 범위) 멈춥니다.
        if (xDistance < stopDistance)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return; // 여기서 함수 종료 (이동 안 함)
        }

        // --- 이동 로직 ---
        
        bool isCliff = CheckCliff();
        bool isWall = CheckWall();
        
        // 점프 로직
        if (isWall && isGrounded && jumpTimer <= 0)
        {
            bool isHighWall = CheckHighWall();
            if (!isHighWall)
            {
                Jump();
                rb.linearVelocity = new Vector2(transform.right.x * chaseSpeed, rb.linearVelocity.y);
                return; 
            }
        }

        // 낭떠러지 멈춤 로직
        if (stopAtEdge && (isCliff || isWall))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            // 전진
            rb.linearVelocity = new Vector2(transform.right.x * chaseSpeed, rb.linearVelocity.y);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        jumpTimer = jumpCooldown;
    }

    // [수정] 낭떠러지 체크
    bool CheckCliff()
    {
        // 경사로를 내려갈 때 땅이 조금 멀어질 수 있으므로, 
        // 레이저 길이를 기존보다 조금 더 여유 있게(1.5배) 줍니다.
        // 또한, 경사면을 따라가도록 frontCheck 위치에서 수직으로 쏘되 길이를 늘립니다.
        RaycastHit2D hit = Physics2D.Raycast(frontCheck.position, Vector2.down, rayDistance + 0.5f, groundLayer);
        
        // 땅이 없으면 true (낭떠러지)
        return hit.collider == null;
    }

    // [수정됨] 낮은 벽 체크 (BoxCast 사용)
    // [수정됨] 낮은 벽 체크 (경사로 구분 로직 추가)
    bool CheckWall()
    {
        Vector2 origin = (Vector2)frontCheck.position + new Vector2(0, 0.2f);
        Vector2 size = new Vector2(0.1f, 0.8f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, transform.right, wallCheckDistance, groundLayer);

        // 무언가에 부딪혔다면?
        if (hit.collider != null)
        {
            // [핵심 로직] 부딪힌 표면의 각도(Normal Vector)를 확인합니다.
            
            // 수직 벽(Wall)의 Normal은 (1, 0) 또는 (-1, 0)입니다. 즉, Y값이 0입니다.
            // 경사로(Slope)의 Normal은 대각선이므로 Y값이 0보다 큽니다. (예: 0.7)

            // "Y값이 0.1보다 크다" == "이것은 비탈길이다"
            if (Mathf.Abs(hit.normal.y) > 0.1f)
            {
                // 경사로니까 벽이 아님! (뒤로 돌지 말고 올라가라)
                return false; 
            }

            // "Y값이 0.1보다 작다" == "이것은 수직 벽이다"
            return true;
        }

        // 아무것도 없으면 벽 아님
        return false;
    }

    bool CheckHighWall()
    {
        // 기본 높이를 1.5 -> 1.8 정도로 넉넉하게 잡거나
        // 코드에서 강제로 높입니다.
        
        float checkHeight = jumpCheckHeight;
        
        // 경사로 오르는 중이면 더 높게!
        if (rb.linearVelocity.y > 0.1f) checkHeight += 0.5f;

        Vector2 origin = frontCheck.position + Vector3.up * checkHeight;
        RaycastHit2D hit = Physics2D.Raycast(origin, transform.right, wallCheckDistance, groundLayer);
        
        return hit.collider != null;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0, 180f, 0);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (frontCheck != null)
        {
            // [수정됨] 낮은 벽 체크 (박스 모양) - 빨간색
            Gizmos.color = Color.red;
            Vector2 origin = (Vector2)frontCheck.position + new Vector2(0, 0.2f); // 0.5 -> 0.2로 수정됨
            Vector2 size = new Vector2(0.1f, 0.8f); // 사이즈 수정됨
            Vector3 endPos = origin + (Vector2)transform.right * wallCheckDistance;
            Gizmos.DrawWireCube(endPos, size);

            // 높은 벽 체크 (파란 선)
            Gizmos.color = Color.blue;
            Vector3 highCheckPos = frontCheck.position + Vector3.up * jumpCheckHeight;
            
            // (아까 수정한 경사로 보정 로직 반영)
            if (GetComponent<Rigidbody2D>() != null && GetComponent<Rigidbody2D>().linearVelocity.y > 0.1f)
                highCheckPos += Vector3.up * 1.0f;

            Gizmos.DrawLine(highCheckPos, highCheckPos + transform.right * wallCheckDistance);
            
            // 낭떠러지 체크 (초록 선)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(frontCheck.position, frontCheck.position + Vector3.down * (rayDistance + 0.5f));
        }
        
        if (footCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(footCheck.position, 0.2f);
        }
    }
}