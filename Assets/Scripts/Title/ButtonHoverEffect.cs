using UnityEngine;
using UnityEngine.EventSystems; // 마우스 이벤트를 감지하기 위해 필수
using TMPro; // 텍스트 색상을 바꾸기 위해 필수

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public float scaleMultiplier = 1.15f; // 크기 배율 (1.15 = 15% 증가)
    public Color hoverColor = new Color(1f, 0.6f, 0f); // 마우스 올렸을 때 색상 (기본값 주황색)

    Vector3 defaultScale;
    Color defaultColor;
    TextMeshProUGUI textMesh;

    void Start()
    {
        // 1. 원래 크기 저장
        defaultScale = transform.localScale;

        // 2. 자식(또는 본인)에 있는 텍스트 컴포넌트 찾기
        textMesh = GetComponentInChildren<TextMeshProUGUI>();

        // 3. 원래 색상 저장
        if (textMesh != null)
        {
            defaultColor = textMesh.color;
        }
    }

    // 마우스가 버튼 영역에 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 크기 키우기
        transform.localScale = defaultScale * scaleMultiplier;

        // 색상 바꾸기
        if (textMesh != null) textMesh.color = hoverColor;
    }

    // 마우스가 버튼 영역에서 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        // 크기 원상복구
        transform.localScale = defaultScale;

        // 색상 원상복구
        if (textMesh != null) textMesh.color = defaultColor;
    }
}