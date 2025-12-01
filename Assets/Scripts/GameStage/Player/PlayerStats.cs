using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // [변경] 직업(Enum) 삭제 -> 무기 데이터(ScriptableObject) 추가
    [Header("Equipment")]
    public WeaponData currentWeapon; 

    [Header("Base Stats")]
    public int maxLP = 30;
    public int currentLP;
    public int maxMP = 30; 
    public int gainMP = 1; // 기본 회복량
    public int currentMP = 0;

    [Header("Attributes")]
    public int str = 0; 
    public int dex = 0; 
    public int mag = 0; // 마나 회복량 증가

    // 이벤트
    public delegate void OnStatsChanged();
    public event OnStatsChanged onStatsChangedCallback;

    void Start()
    {
        currentLP = maxLP;
        currentMP = 0;
        onStatsChangedCallback?.Invoke();
    }

    // [수정됨] 마나 회복 로직 (기본 + MAG 비례)
    public void GainMana()
    {
        // 회복량 계산: 기본 1 + MAG 수치
        int amount = gainMP + mag;
        
        currentMP += amount;
        if (currentMP > maxMP) currentMP = maxMP; // 최대치 넘지 않게
        
        // Debug.Log($"마나 회복: {amount} (기본{gainMP} + MAG{mag})");
        onStatsChangedCallback?.Invoke();
    }
    
    // ... (UseMana, TakeDamage, GetTotalDamage 등 기존 함수 유지) ...
    // UseMana는 나중에 스킬 시스템에서 쓰이게 됩니다.
    public bool UseMana(int amount)
    {
        if (currentMP >= amount)
        {
            currentMP -= amount;
            onStatsChangedCallback?.Invoke();
            return true;
        }
        return false;
    }

    public void TakeDamage(int damage)
    {
        currentLP -= damage;
        onStatsChangedCallback?.Invoke();
        if (currentLP <= 0) Debug.Log("Game Over");
    }

    public int GetTotalDamage()
    {
        if (currentWeapon == null) return 1;
        int statBonus = 0;
        switch (currentWeapon.type)
        {
            case WeaponType.Sword: statBonus = str; break;
            case WeaponType.Whip: statBonus = str; break; // 위퍼도 힘 비례라면 str
            case WeaponType.Bow: statBonus = dex; break;
            case WeaponType.Staff: statBonus = mag; break;
        }
        return currentWeapon.damage + statBonus;
    }

    // [추가] 무기 장착 및 UI 갱신 함수
    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
        
        // 중요: 무기가 바뀌었으니 UI에게 알림!
        onStatsChangedCallback?.Invoke();
        
        Debug.Log($"무기 교체됨: {newWeapon.weaponName}");
    }
}