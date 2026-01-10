using UnityEngine;
using Duet.Managers;

public class ObstacleModule : MonoBehaviour 
{
    public string poolKey; // 在 Inspector 里填入该模块对应的 Key
    public float fallSpeed = 5f;

    void Update() 
    {
        // 向下移动
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // 自动回收：当移出屏幕底部（假设 -10）
        if (transform.position.y < -10f) 
        {
            // 直接调用 PoolManager 回收自己
            PoolManager.Instance.ReturnToPool(poolKey, gameObject);
        }
    }

    // 重要：当物体被重新启用时（从池里出来时）调用
    void OnEnable() 
    {
        // 这里可以重置模块内部所有子物体的状态
        // 比如：foreach(Transform child in transform) child.gameObject.SetActive(true);
    }
}