using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 2f;

    float width;

    void Start()
    {
        // 스프라이트 렌더러 컴포넌트를 가져옴
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // bounds.size.x는 스케일(Scale)이 적용된 실제 월드 크기를 가져옵니다.
        width = sr.bounds.size.x;
    }

    void Update()
    {
        // 1. 왼쪽으로 이동
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 2. 위치 초기화 (무한 스크롤 로직)
        if (transform.position.x <= -width)
        {
            transform.position += new Vector3(width * 2f, 0, 0);
        }
    }
}