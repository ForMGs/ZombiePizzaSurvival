using UnityEngine;

// 지역 경계의 Trigger Collider에 부착하여 최초 방문을 기록합니다.
[RequireComponent(typeof(Collider))]
public sealed class RegionDiscoveryTrigger : MonoBehaviour
{
    [SerializeField] private string regionId;
    [SerializeField] private RegionDiscoveryManager regionManager;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (regionManager == null)
            regionManager = FindFirstObjectByType<RegionDiscoveryManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Inventory>() != null)
            regionManager?.Discover(regionId);
    }
}
