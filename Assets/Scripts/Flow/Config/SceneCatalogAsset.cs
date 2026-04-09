using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flow/Scene Catalog", fileName = "SceneCatalog")]
public class SceneCatalogAsset : ScriptableObject
{
    [Serializable]
    public struct SceneEntry
    {
        public string sceneId;
        public string sceneName;
        public RuntimeDomain domain;
    }

    [SerializeField] private List<SceneEntry> scenes = new List<SceneEntry>();

    public bool TryGetSceneName(string sceneId, out string sceneName)
    {
        sceneName = null;
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            return false;
        }

        for (int i = 0; i < scenes.Count; i++)
        {
            SceneEntry entry = scenes[i];
            if (!string.Equals(entry.sceneId, sceneId, StringComparison.Ordinal))
            {
                continue;
            }

            sceneName = entry.sceneName;
            return !string.IsNullOrWhiteSpace(sceneName);
        }

        return false;
    }

    public bool TryGetScene(string sceneId, out SceneEntry sceneEntry)
    {
        sceneEntry = default;
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            return false;
        }

        for (int i = 0; i < scenes.Count; i++)
        {
            SceneEntry entry = scenes[i];
            if (!string.Equals(entry.sceneId, sceneId, StringComparison.Ordinal))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.sceneName))
            {
                return false;
            }

            sceneEntry = entry;
            return true;
        }

        return false;
    }
}
