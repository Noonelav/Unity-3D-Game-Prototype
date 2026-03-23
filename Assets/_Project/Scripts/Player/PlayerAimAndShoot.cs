using UnityEngine;

public class PlayerAimAndShoot : MonoBehaviour
{
    [Header("Aim Settings")]
    public LayerMask groundMask;     // 鼠标指向的“地面”层
    public float rotateSpeed = 15f;  // 角色旋转速度

    [Header("Shoot Settings")]
    public GameObject bulletPrefab;  // 子弹预制体（带 Rigidbody）
    public Transform firePoint;      // 枪口位置（玩家子物体）
    public float bulletSpeed = 40f;  // 子弹速度

    void Update()
    {
        AimTowardsMouse();
        HandleShooting();
    }

    void AimTowardsMouse()
    {
        Camera cam = Camera.main;
        if (cam == null) return;   // 没有主摄像机就直接退出

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundMask))
        {
            // 鼠标在地面上的点
            Vector3 targetPoint = hit.point;

            // 只在水平面上旋转：Y 固定为玩家高度
            Vector3 lookPos = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);
            Vector3 dir = (lookPos - transform.position).normalized;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotateSpeed * Time.deltaTime
                );
            }
        }
    }

    void HandleShooting()
    {
        // 左键按下开火
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(transform.forward);   // 此时 forward 就是朝鼠标的方向
        }
    }

    void Shoot(Vector3 direction)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }
}
