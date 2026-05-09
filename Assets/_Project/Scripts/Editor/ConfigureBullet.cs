using UnityEditor;
using UnityEngine;

public class ConfigureBullet
{
    [MenuItem("Tools/Configure Bullet Prefab")]
    public static void ConfigureBulletPrefab()
    {
        string bulletPrefabPath = "Assets/_Project/Prefabs/BulletTemp.prefab";

        // Load the prefab
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);

        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab not found at: " + bulletPrefabPath);
            return;
        }

        Debug.Log("Configuring bullet prefab...");

        // Add BulletController script if not present
        if (bulletPrefab.GetComponent<BulletController>() == null)
        {
            bulletPrefab.AddComponent<BulletController>();
            Debug.Log("BulletController script added");
        }

        // Scale down the bullet
        bulletPrefab.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        Debug.Log("Bullet scaled to 0.15x");

        // Make sure Rigidbody exists
        Rigidbody rb = bulletPrefab.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Debug.Log("Rigidbody collision detection set to Continuous");
        }

        // Save the prefab
        EditorUtility.SetDirty(bulletPrefab);
        AssetDatabase.SaveAssets();
        Debug.Log("Bullet prefab configured and saved successfully!");
    }
}
