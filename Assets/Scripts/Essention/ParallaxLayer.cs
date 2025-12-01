using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Settings")]
    [Range(0f, 1f)]
    public float parallaxFactorX; // X축 따라오는 정도 (1=완전고정, 0=안따라옴)
    [Range(0f, 1f)]
    public float parallaxFactorY; // Y축 따라오는 정도
    
    public float autoScrollSpeedX; // 구름용 자동 이동 속도

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        // 무한 스크롤을 위해 이미지의 가로 길이를 구함
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            textureUnitSizeX = sr.bounds.size.x;
        }
    }

    void LateUpdate() // 카메라 이동 후(LateUpdate)에 처리해야 덜덜거림이 없습니다.
    {
        // 1. 카메라가 이동한 양(Delta) 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 2. 패럴랙스 적용 (팩터만큼 따라가기)
        transform.position += new Vector3(deltaMovement.x * parallaxFactorX, deltaMovement.y * parallaxFactorY, 0);

        // 3. 자동 스크롤 (구름 등)
        if (autoScrollSpeedX != 0)
        {
            transform.position += Vector3.right * autoScrollSpeedX * Time.deltaTime;
        }

        // 4. 무한 루프 처리 (카메라와 너무 멀어지면 위치 재조정)
        // 배경 중심과 카메라 중심의 X축 거리 차이
        if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
        }

        // 마지막 카메라 위치 갱신
        lastCameraPosition = cameraTransform.position;
    }
}