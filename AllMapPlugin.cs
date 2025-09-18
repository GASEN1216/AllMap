using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

// AllMap 模组 - 自动开启全地图功能
// 功能：在玩家进入游戏时自动解锁所有地图区域

namespace AllMap
{
    [BepInPlugin("allmap", "All Map", "1.0.0")]
    public class AllMapPlugin : BaseUnityPlugin
    {
        // 配置选项
        public ConfigEntry<bool> _enableAllMap;
        private ConfigEntry<bool> _debugLog;
        
        // 运行状态
        private bool _hasAppliedOnce = false;
        private float _lastUpdateTime = 0f;
        
        private static AllMapPlugin _instance;
        internal static AllMapPlugin Instance => _instance;

        private void Awake()
        {
            // 防止多个实例
            if (_instance != null && _instance != this)
            {
                Logger.LogWarning("检测到多个AllMapPlugin实例，销毁重复实例");
                Destroy(this.gameObject);
                return;
            }
            
            _instance = this;
            
            // 配置选项
            _enableAllMap = Config.Bind("General", "EnableAllMap", true,
                "是否启用全地图功能 | Enable all map functionality");
            _debugLog = Config.Bind("Debug", "EnableDebugLog", false,
                "是否启用调试日志 | Enable debug logging");
            
            Logger.LogInfo("AllMap模组已加载");
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                Logger.LogInfo("AllMap模组已卸载");
            }
        }

        private void Update()
        {
            try
            {
                // 每5秒检查一次
                if (Time.time - _lastUpdateTime > 5f)
                {
                    _lastUpdateTime = Time.time;

                    // 检查是否在游戏中且还未应用全地图
                    if (!_hasAppliedOnce && GameManager.instance != null && PlayerData.instance != null)
                    {
                        var gm = GameManager.instance;
                        var pd = PlayerData.instance;

                        // 检查是否在游戏世界中（不是菜单）
                        if (gm.IsGameplayScene() && HeroController.instance != null)
                        {
                            ForceLog("检测到游戏状态，应用全地图");
                            ApplyAllMap();
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                // 捕获异常，避免无限循环错误
                Logger.LogError($"Update方法执行时出错: {e}");
                _hasAppliedOnce = true; // 设置为已应用，避免继续尝试
            }
        }
        
        // 调试日志输出（根据配置决定是否输出）
        private void D(string message)
        {
            if (_debugLog?.Value == true)
            {
                Logger.LogInfo($"[AllMap-Debug] {message}");
            }
        }

        // 强制日志输出（总是输出）
        public void ForceLog(string message)
        {
            Logger.LogInfo($"[AllMap] {message}");
            UnityEngine.Debug.Log($"[AllMap] {message}");
        }
        
        // 应用全地图功能（简化版本）
        private void ApplyAllMap()
        {
            try
            {
                if (!_enableAllMap.Value)
                {
                    D("全地图功能已禁用，跳过应用");
                    return;
                }

                var playerData = PlayerData.instance;
                if (playerData == null)
                {
                    D("PlayerData不可用，跳过应用全地图");
                    return;
                }

                ForceLog("开始应用全地图功能");

                // 设置核心地图标志
                playerData.hasQuill = true;        // 给玩家羽毛笔
                playerData.QuillState = 1;         // 设置羽毛笔状态
                playerData.mapAllRooms = true;     // 显示所有房间
                
                ForceLog("设置核心地图标志完成");

                // 强制添加当前场景到已映射列表
                var gameManager = GameManager.instance;
                if (gameManager != null && !string.IsNullOrEmpty(gameManager.sceneName))
                {
                    string currentScene = gameManager.sceneName;
                    
                    // 确保集合已初始化
                    if (playerData.scenesMapped == null)
                        playerData.scenesMapped = new HashSet<string>();
                    if (playerData.scenesVisited == null)
                        playerData.scenesVisited = new HashSet<string>();
                    
                    // 添加当前场景
                    playerData.scenesMapped.Add(currentScene);
                    playerData.scenesVisited.Add(currentScene);
                    
                    ForceLog($"添加当前场景到已映射列表: {currentScene}");
                }

                Logger.LogInfo("全地图功能已应用");
                _hasAppliedOnce = true;
            }
            catch (Exception e)
            {
                Logger.LogError($"应用全地图功能时出错: {e}");
                _hasAppliedOnce = true; // 避免无限重试
            }
        }
    }
}
