using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 1f; // 애니메이션 길이와 비슷하게 설정

    void Start()
    {
        // 생성된 후 delay 시간 뒤에 삭제
        Destroy(gameObject, delay); 
    }
}