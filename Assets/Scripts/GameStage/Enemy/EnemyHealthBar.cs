using UnityEngine;
using UnityEngine.UI; // UI 기능을 위해 필수

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider; // 유니티 UI 슬라이더
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 3f, 0); // 머리 위로 띄울 높이

    public void SetMaxHealth(int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    public void SetHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;
    }

    // 체력바가 항상 똑바로 보이게 (적이 뒤집혀도 체력바는 안 뒤집히게)
    void LateUpdate()
    {
        // 체력바의 회전값을 (0,0,0)으로 고정 -> 적이 회전해도 영향 안 받음
        transform.rotation = Quaternion.identity;
        Vector3 parentScale = transform.parent.localScale;
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * (parentScale.x > 0 ? 1 : 1); // 항상 정방향
        transform.localScale = newScale;
        
    }
}