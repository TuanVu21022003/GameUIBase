using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreData;
using Sirenix.Utilities;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif
using UnityEngine;

namespace CoreGame.GameGlobalConfig
{
    [CreateAssetMenu(
        fileName = "LevelGlobalConfig",
        menuName = "GlobalConfigs/LevelGlobalConfig")]
    [GlobalConfig("Assets/GameBase/Resources/GlobalConfig/")]
    public class LevelGlobalConfig : GlobalConfig<LevelGlobalConfig>
    {
        [SerializeField] private int levelMaxInConfig;
#if UNITY_EDITOR
        [SerializeField, Sirenix.OdinInspector.OnValueChanged(nameof(OnEncryptModeChanged))]
#endif
        private bool isEncrypt = true;
        [SerializeField] private LevelCategory levelCategory;

        [SerializeField] private List<LevelConfig> levelConfigs;
        public bool IsEncrypt => isEncrypt;
        public LevelCategory LevelCategory
        {
            get => levelCategory;
            set => levelCategory = value;
        }
        public List<LevelConfig> LevelConfigs => levelConfigs;

#if UNITY_EDITOR
        private void OnEncryptModeChanged()
        {
            Debug.Log($"Encrypt mode changed: {isEncrypt}");
            FetchLevelData();
        }
#endif

        public LevelConfig GetLevelConfig(int level)
        {
            if (level < 1 || level > levelMaxInConfig)
                return GetExtraLevelConfig(level);

            for (int i = 0; i < levelConfigs.Count; i++)
            {
                if (levelConfigs[i].Level == level)
                    return levelConfigs[i];
            }

            return GetExtraLevelConfig(level);
        }

        public LevelConfig GetLevelConfigAvailable(int level)
        {
            for (int i = 0; i < levelConfigs.Count; i++)
            {
                if (levelConfigs[i].Level == level)
                    return levelConfigs[i];
            }

            return null;
        }

        public LevelConfig GetExtraLevelConfig(int level)
        {
            int max = Mathf.Max(levelMaxInConfig, 1);
            int oldLevel = XorShift32(level) % max + 1;

            LevelConfig baseConfig =
                GetLevelConfigAvailable(oldLevel) ??
                GetLevelConfig(1);

            return baseConfig.CreateExtra(level);
        }

        public int XorShift32(int seed)
        {
            seed ^= (seed << 13);
            seed ^= (seed >> 17);
            seed ^= (seed << 5);
            return seed & 0x7FFFFFFF;
        }

#if UNITY_EDITOR
        [SerializeField] private string LEVEL_FOLDER =
            "Assets/BaseGame/TextAssets/NewLevel/";

        public string CurrentLevelFolder =>
            LEVEL_FOLDER +
            (IsEncrypt ? "Encrypt" : "Decrypt") + "/" + levelCategory + "/";

        [PropertyOrder(1)]
        [BoxGroup("Base")]
        [BoxGroup("Base/Actions")]
        [Button(ButtonSizes.Large)]
        [GUIColor(0.6f, 1f, 0.6f)]
        [PropertySpace(SpaceBefore = 6)]
        private void FetchLevelData()
        {
            EditorUtility.SetDirty(this);
            levelConfigs.Clear();

            string[] guids = AssetDatabase.FindAssets(
                "t:TextAsset Level_",
                new[] { CurrentLevelFolder });
            foreach (string guid in guids)
            {
                TextAsset textAsset =
                    AssetDatabase.LoadAssetAtPath<TextAsset>(
                        AssetDatabase.GUIDToAssetPath(guid));

                int level =
                    int.Parse(textAsset.name.Split('_').Last());

                LevelModel levelData = null;
                if (IsEncrypt)
                {
                    levelData =
                        DataSerializer.Deserialize<LevelModel>(
                            textAsset.text);
                }
                else
                {
                    levelData = JsonUtility.FromJson<LevelModel>(textAsset.text);
                }
                if (levelData == null) continue;

                levelConfigs.Add(new LevelConfig
                {
                    Level = level,
                    LevelType = levelData.LevelDifficulty,
                    LevelTextAsset = textAsset
                });
            }

            if (levelConfigs.Count > 0)
                levelMaxInConfig = levelConfigs.Max(l => l.Level);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Fetch Level Data",
                $"Fetch completed successfully.\nLoaded {levelConfigs.Count} levels.",
                "OK"
            );
        }
        // ======================================================
        // ============== LEVEL ENCRYPTION TOOL =================
        // ======================================================

        [PropertyOrder(2)]
        [BoxGroup("Encryption Tool")]
        [BoxGroup("Encryption Tool/Settings")]
        [LabelText("Target Level")]
        [PropertyRange(1, 999)]
        [SerializeField]
        private int levelTarget = 1;

        [PropertyOrder(2)]
        [BoxGroup("Encryption Tool")]
        [BoxGroup("Encryption Tool/Actions")]
        [HorizontalGroup("Encryption Tool/Actions/Buttons")]
        [Button(ButtonSizes.Large)]
        [GUIColor(1f, 0.4f, 0.4f)]
        [DisableIf(nameof(IsEncrypt))]
        private void Encrypt()
        {
            string sourceFolder = CurrentLevelFolder;
            string targetFolder = LEVEL_FOLDER +
                                  "Encrypt" + "/" + levelCategory + "/";
            if (!EditorUtility.DisplayDialog(
                    "Confirm Encrypt",
                    $"Encrypt level {levelTarget}?",
                    "Encrypt", "Cancel"))
                return;

            string levelName = $"Level_{levelTarget}";

            if (!EncryptLevel(levelName, sourceFolder, targetFolder, out string error))
            {
                EditorUtility.DisplayDialog(
                    "Encrypt Failed",
                    error,
                    "OK");
                return;
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Encrypt Completed",
                $"Level {levelName} encrypted successfully.",
                "OK");
        }

        [PropertyOrder(2)]
        [PropertySpace(SpaceBefore = 8)]
        [HorizontalGroup("Encryption Tool/Actions/ButtonsAll1")]
        [Button(ButtonSizes.Large)]
        [GUIColor(0.6f, 0.6f, 1f)]
        private void EncryptAll()
        {
            string sourceFolder = CurrentLevelFolder;
            string targetFolder = LEVEL_FOLDER +
                                  "Encrypt" + "/" + levelCategory + "/";
            if (!EditorUtility.DisplayDialog(
                    "Confirm Encrypt All",
                    $"Encrypt ALL levels?\n\nFrom:\n{sourceFolder}\n\nTo:\n{targetFolder}",
                    "Encrypt All", "Cancel"))
                return;

            string[] sourceGuids = AssetDatabase.FindAssets(
                "t:TextAsset Level_",
                new[] { sourceFolder });

            if (sourceGuids == null || sourceGuids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Encrypt Failed",
                    $"No level files found in source folder:\n{sourceFolder}",
                    "OK");
                return;
            }

            int success = 0;
            int failed = 0;

            foreach (string guid in sourceGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextAsset asset =
                    AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                if (asset == null) continue;

                if (EncryptLevel(asset.name, sourceFolder, targetFolder, out _))
                    success++;
                else
                    failed++;
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Encrypt All Completed",
                $"Success: {success}\nFailed: {failed}",
                "OK");
        }

        [PropertyOrder(2)]
        [HorizontalGroup("Encryption Tool/Actions/Buttons")]
        [Button(ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [EnableIf(nameof(IsEncrypt))]
        private void Decrypt()
        {
            string sourceFolder = CurrentLevelFolder;
            string targetFolder = LEVEL_FOLDER +
                                  "Decrypt" + "/" + levelCategory + "/";
            if (!EditorUtility.DisplayDialog(
                    "Confirm Decrypt",
                    $"Decrypt level {levelTarget}?",
                    "Decrypt", "Cancel"))
                return;

            string levelName = $"Level_{levelTarget}";

            if (!DecryptLevel(levelName, sourceFolder, targetFolder, out string error))
            {
                EditorUtility.DisplayDialog(
                    "Decrypt Failed",
                    error,
                    "OK");
                return;
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Decrypt Completed",
                $"Level {levelName} decrypted successfully.",
                "OK");
        }
        [PropertyOrder(2)]
        [PropertySpace(SpaceBefore = 8)]
        [HorizontalGroup("Encryption Tool/Actions/ButtonsAll2")]
        [Button(ButtonSizes.Large)]
        [GUIColor(1f, 0.95f, 0.6f)]
        private void DecryptAll()
        {
            string sourceFolder = CurrentLevelFolder;
            string targetFolder = LEVEL_FOLDER +
                                  "Decrypt" + "/" + levelCategory + "/";
            if (!EditorUtility.DisplayDialog(
                    "Confirm Decrypt All",
                    $"Decrypt ALL levels?\n\nFrom:\n{sourceFolder}\n\nTo:\n{targetFolder}",
                    "Decrypt All", "Cancel"))
                return;

            string[] sourceGuids = AssetDatabase.FindAssets(
                "t:TextAsset Level_",
                new[] { sourceFolder });

            if (sourceGuids == null || sourceGuids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Decrypt Failed",
                    $"No level files found in source folder:\n{sourceFolder}",
                    "OK");
                return;
            }

            int success = 0;
            int failed = 0;

            foreach (string guid in sourceGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextAsset asset =
                    AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                if (asset == null) continue;

                if (DecryptLevel(asset.name, sourceFolder, targetFolder, out _))
                    success++;
                else
                    failed++;
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Decrypt All Completed",
                $"Success: {success}\nFailed: {failed}",
                "OK");
        }

        #region File Utils

        private bool EncryptLevel(
            string levelName,
            string sourceFolder,
            string targetFolder,
            out string error)
        {
            error = null;

            // ---- Find source (plain JSON) ----
            TextAsset sourceAsset = FindLevelAsset(sourceFolder, levelName);
            if (sourceAsset == null)
            {
                error = $"Source level not found: {levelName}";
                return false;
            }

            string plainJson = sourceAsset.text;

            // ---- Encrypt ----
            string encrypted;
            try
            {
                encrypted = DataSerializer.Encrypt(plainJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"Encrypt failed [{levelName}]: {e}");
                error = "Encrypt failed due to invalid data.";
                return false;
            }

            // ---- Target ----
            string targetPath = GetOrCreateTargetPath(targetFolder, levelName);

            // ---- Write ----
            File.WriteAllText(targetPath, encrypted, Encoding.UTF8);
            AssetDatabase.ImportAsset(targetPath);

            return true;
        }

        private bool DecryptLevel(
            string levelName,
            string sourceFolder,
            string targetFolder,
            out string error)
        {
            error = null;

            // ---- Find source ----
            TextAsset sourceAsset = FindLevelAsset(sourceFolder, levelName);
            if (sourceAsset == null)
            {
                error = $"Source level not found: {levelName}";
                return false;
            }

            // ---- Decrypt ----
            string decryptedJson;
            try
            {
                decryptedJson = DataSerializer.Decrypt(sourceAsset.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"Decrypt failed [{levelName}]: {e}");
                error = "Source file is not encrypted or data is invalid.";
                return false;
            }

            // ---- Target ----
            string targetPath = GetOrCreateTargetPath(targetFolder, levelName);

            // ---- Write ----
            File.WriteAllText(targetPath, decryptedJson, Encoding.UTF8);
            AssetDatabase.ImportAsset(targetPath);

            return true;
        }

        private TextAsset FindLevelAsset(string folder, string levelName)
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:TextAsset Level_",
                new[] { folder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                if (asset != null && asset.name == levelName)
                    return asset;
            }

            return null;
        }

        private string GetOrCreateTargetPath(string folder, string levelName)
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:TextAsset Level_",
                new[] { folder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                if (asset != null && asset.name == levelName)
                    return path;
            }

            // Không có → tạo mới
            string newPath = Path.Combine(folder, $"{levelName}.json")
                .Replace("\\", "/");

            File.WriteAllText(newPath, "{}", Encoding.UTF8);
            AssetDatabase.ImportAsset(newPath);

            return newPath;
        }
        #endregion


#endif
    }

    [Serializable]
    public class LevelConfig
    {
        public int Level;
        public LevelDifficulty LevelType;
        public TextAsset LevelTextAsset;
#if UNITY_EDITOR
        [ShowInInspector, ReadOnly] 
#endif
        private bool isRemoteSet;

        public LevelModel LevelData => LevelGlobalConfig.Instance.IsEncrypt ?
            DataSerializer.Deserialize<LevelModel>(
                LevelTextAsset.text) : JsonUtility.FromJson<LevelModel>(LevelTextAsset.text);

        public void SetRemote(int value)
        {
            isRemoteSet = true;
        }

        public int GetTimeRemote()
        {
            return isRemoteSet
                ? 0
                : 1;
        }

        public LevelConfig CreateExtra(int extraLevel)
        {
            return new LevelConfig
            {
                Level = extraLevel,
                LevelType = LevelType,
                LevelTextAsset = LevelTextAsset,
                isRemoteSet = isRemoteSet,
                // Other properties can be copied as needed
            };
        }
    }

    public enum LevelCategory
    {
        None = 10000,
        ANDROID = 0,
        IOS = 1,
    }
}