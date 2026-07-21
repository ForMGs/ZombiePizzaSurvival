using System;
using System.Collections.Generic;
using UnityEngine;

// 플레이어가 한 번이라도 방문한 지역을 기록하고 로컬 저장합니다.
public sealed class RegionDiscoveryManager : MonoBehaviour
{
    private const string SaveKey = "ZombiePizzaSurvival.RegionProgress.v1";
    private readonly HashSet<string> visitedRegionIds = new();

    public event Action<string> RegionDiscovered;

    private void Awake() => Load();

    public bool HasVisited(string regionId)
    {
        return !string.IsNullOrWhiteSpace(regionId) && visitedRegionIds.Contains(regionId);
    }

    public bool Discover(string regionId)
    {
        if (string.IsNullOrWhiteSpace(regionId) || !visitedRegionIds.Add(regionId))
            return false;

        Save();
        RegionDiscovered?.Invoke(regionId);
        return true;
    }

    private void Save()
    {
        RegionSaveData data = new() { regionIds = new List<string>(visitedRegionIds) };
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        visitedRegionIds.Clear();
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        RegionSaveData data = JsonUtility.FromJson<RegionSaveData>(PlayerPrefs.GetString(SaveKey));
        if (data?.regionIds == null)
            return;

        foreach (string regionId in data.regionIds)
        {
            if (!string.IsNullOrWhiteSpace(regionId))
                visitedRegionIds.Add(regionId);
        }
    }

    [Serializable]
    private sealed class RegionSaveData
    {
        public List<string> regionIds = new();
    }
}
