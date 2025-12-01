using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    // 어디서든 DamagePopup.Create(...)로 호출하기 위한 정적 함수
    public static DamagePopup Create(Vector3 position, int damageAmount, GameObject prefab)
    {
        // 1. 생성
        Transform damagePopupTransform = Instantiate(prefab, position, Quaternion.identity).transform;
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        
        // 2. 설정
        damagePopup.Setup(damageAmount);

        return damagePopup;
    }

    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount)
    {
        textMesh.text = damageAmount.ToString();
        textColor = textMesh.color;
        disappearTimer = 0.5f; // 0.5초 뒤부터 흐려지기 시작

        // [핵심] 스틱 레인저 스타일 궤적 설정
        // 오른쪽(X: 1~3) + 위쪽(Y: 5~8)으로 튀어 오르는 초기 속도 설정
        // 랜덤성을 주어 여러 개가 겹치지 않게 함
        float x = Random.Range(1f, 2f); 
        float y = Random.Range(5f, 7f);
        moveVector = new Vector3(x, y) * 1.5f; // 속도 배율 조절
        
        // 크기 튀기기 (선택사항: 등장 시 약간 커짐)
        transform.localScale = Vector3.one * 1f; 
    }

    void Update()
    {
        // 1. 이동 로직 (포물선 운동)
        transform.position += moveVector * Time.deltaTime;
        
        // 중력 적용: Y축 속도를 빠르게 감소시켜 아래로 떨어지게 만듦
        moveVector.y -= 30f * Time.deltaTime; 

        // 공기 저항: X축 속도도 서서히 줄어듦
        if (moveVector.y < 0) // 떨어지기 시작하면 수평 이동도 줄임
        {
            moveVector.x -= 3f * Time.deltaTime;
        }


        // 2. 사라짐 로직 (페이드 아웃)
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // 투명도 감소 속도
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            // 완전히 투명해지면 삭제
            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}