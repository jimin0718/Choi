using UnityEngine;
using TMPro;
using UnityEngine.UI; // [필수] 이미지 처리를 위해 추가

public class UIManager : MonoBehaviour
{
    [Header("Text UI")]
    public TextMeshProUGUI statusText; 

    [Header("Icon UI")]
    public Image weaponIconDisplay; // 아이콘을 띄울 UI 이미지
    public Sprite emptySlotSprite;  // 무기가 없을 때 보여줄 기본 이미지 (선택사항)

    PlayerStats playerStats;

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats != null)
        {
            playerStats.onStatsChangedCallback += UpdateUI;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (playerStats == null) return;

        // 1. 텍스트 갱신 (기존 코드)
        if (statusText != null)
        {
            string weaponName = (playerStats.currentWeapon != null) ? playerStats.currentWeapon.weaponName : "No Weapon";
            statusText.text = $"Weapon: {weaponName}\n" +
                              $"LP: {playerStats.currentLP}/{playerStats.maxLP}\n" +
                              $"MP: {playerStats.currentMP}/{playerStats.maxMP}";
        }

        // 2. [추가됨] 아이콘 갱신
        if (weaponIconDisplay != null)
        {
            if (playerStats.currentWeapon != null && playerStats.currentWeapon.icon != null)
            {
                // 무기가 있고 아이콘이 있다면 -> 아이콘 표시
                weaponIconDisplay.sprite = playerStats.currentWeapon.icon;
                weaponIconDisplay.color = Color.white; // 불투명하게
                weaponIconDisplay.enabled = true;      // 이미지 켜기
            }
            else
            {
                // 무기가 없거나 아이콘이 없다면 -> 숨기거나 투명하게
                if (emptySlotSprite != null)
                {
                    weaponIconDisplay.sprite = emptySlotSprite; // 빈 슬롯 이미지
                }
                else
                {
                    weaponIconDisplay.color = Color.clear; // 완전 투명하게
                    // 또는 weaponIconDisplay.enabled = false; // 아예 끄기
                }
            }
        }
    }
}