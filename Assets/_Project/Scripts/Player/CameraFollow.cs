using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 拖 Player1 进来
    public Vector3 offset = new Vector3(0f, 15f, -10f);

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
