using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int health = 100;
    public int ammo = 0;

    public void AddHealth(int amount)
    {
        health += amount;
        Debug.Log("Health: " + health);
    }

    public void AddAmmo(int amount)
    {
        ammo += amount;
        Debug.Log("Ammo: " + ammo);
    }
}
