using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동용

public class Signpost : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName;   // 이동할 다음 스테이지(씬) 이름
    public bool isBossStage = false; // 보스 스테이지인가? (마지막 스테이지)
    public int currentLevelID = 1;   // 현재 레벨 ID (보스 클리어 시 저장용)

    [Header("Detection")]
    public Vector2 detectionSize = new Vector2(4f, 2f); // 감지 범위 (4x2 타일)
    public Vector2 detectionOffset = Vector2.zero;      // 중심점 보정
    public LayerMask playerLayer;

    [Header("UI Components")]
    public GameObject fKeyPrompt;  // 'F키' 스프라이트 오브젝트

    private bool isUnlocked = true; // 잠금 여부 (보스전용)
    private bool isPlayerInRange = false;

    void Start()
    {
        // 1. 보스 스테이지라면 처음엔 잠겨있어야 함
        if (isBossStage)
        {
            isUnlocked = false;
        }
        else
        {
            isUnlocked = true;
        }

        // F키 아이콘은 처음에 숨김
        if (fKeyPrompt != null) fKeyPrompt.SetActive(false);
    }

    void Update()
    {
        // 잠겨있다면 아무것도 안 함
        if (!isUnlocked) return;

        CheckPlayerInRange();

        // 범위 안에 있고 F를 눌렀다면
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            ProceedToNext();
        }
    }

    // 플레이어 감지 (Physics2D.OverlapBox 사용)
    void CheckPlayerInRange()
    {
        Vector2 center = (Vector2)transform.position + detectionOffset;
        
        // 사각형 범위 안에 플레이어가 있는지 확인
        Collider2D hit = Physics2D.OverlapBox(center, detectionSize, 0, playerLayer);
        bool detected = (hit != null);

        if (detected != isPlayerInRange)
        {
            isPlayerInRange = detected;
            
            // 범위 안에 들어오면 F키 아이콘 띄우기
            if (fKeyPrompt != null) fKeyPrompt.SetActive(isPlayerInRange);
        }
    }

    void ProceedToNext()
    {
        // 이동할 씬 이름 결정
        string targetScene = "";

        if (isBossStage)
        {
            // [보스 스테이지]
            Debug.Log($"레벨 {currentLevelID} 완전 클리어!");
            PlayerPrefs.SetInt($"Stage_{currentLevelID}_Cleared", 1);
            PlayerPrefs.Save();
            
            targetScene = "WorldMap"; // 월드맵으로 귀환
        }
        else
        {
            // [일반 스테이지]
            Debug.Log($"다음 스테이지({nextSceneName})로 이동");
            targetScene = nextSceneName;
        }

        // [수정됨] 직접 이동하지 않고, SceneEffect를 찾아서 페이드 아웃 요청
        SceneEffect effect = FindFirstObjectByType<SceneEffect>(); // (최신 버전 문법)
        // 구버전이라면: SceneEffect effect = FindObjectOfType<SceneEffect>();

        if (effect != null)
        {
            // 페이드 효과와 함께 이동
            effect.LoadSceneWithFade(targetScene);
        }
        else
        {
            // 만약 SceneEffect가 없다면 그냥 즉시 이동 (오류 방지)
            SceneManager.LoadScene(targetScene);
        }
    }

    // 보스가 죽었을 때 호출할 함수 (외부에서 호출)
    public void UnlockSignpost()
    {
        isUnlocked = true;
        Debug.Log("표지판이 해금되었습니다!");
        
        // 해금 효과음이나 파티클을 여기서 재생하면 좋습니다.
    }

    // 에디터에서 범위 확인용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + detectionOffset;
        Gizmos.DrawWireCube(center, detectionSize);
    }
}