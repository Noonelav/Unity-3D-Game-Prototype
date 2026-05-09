using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public Slider healthSlider; // 血条 UI（用 Slider）

    void Awake()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
     if (Input.GetKeyDown(KeyCode.K))
     {
        TakeDamage(10);   // 按 K 掉 10 血
     }

     if (Input.GetKeyDown(KeyCode.H))
     {
        Heal(10);         // 按 H 回 10 血
     }
    }


    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Die()
    {
        Debug.Log("Player Dead");
        // 这里以后可以加死亡动画、重开场景等
    }
}
