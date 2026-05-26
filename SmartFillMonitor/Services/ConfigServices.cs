using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Navigation;
using System.Windows.Shell;

namespace SmartFillMonitor.Services
{
    public static class ConfigServices
    {
        private const string SettingFileName = "DeviceSettings.json";
        private static readonly SemaphoreSlim IoLock = new SemaphoreSlim(1, 1);//防止多线程读取同一个文件
        public static string GetConfigFilePath() => Path.Combine(AppContext.BaseDirectory, SettingFileName);
        /// <summary>
        /// 读取配置
        /// </summary>
        /// <returns></returns>
        public static async Task<DeviceSettings> LoadConfigAsync()
        {
            var path = GetConfigFilePath();
            DeviceSettings? settings = null;
            await IoLock.WaitAsync();//等待锁
            try
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(path);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };
                        settings = JsonSerializer.Deserialize<DeviceSettings>(json, options);
                        if (settings != null)
                        {
                            LogService.Info($"配置文件加载成功:{path}");
                            return settings;
                        }
                    }
                    catch (JsonException ex)
                    {
                        LogService.Error($"配置文件解析失败，可能文件已损坏:{ex.Message}");
                        BackCorrupFile(path);
                    }
                    catch (Exception e)
                    {
                        LogService.Error($"读取配置文件失败:{e.Message}");
                        BackCorrupFile(path);
                    }
                }
                else
                {
                    LogService.Warn($"配置文件不存在:{path},使用默认配置");
                }
            }
            finally
            {
                IoLock.Release();
            }
            if (settings == null)
            {
                settings = new();//new一个默认配置
                                 //保存默认配置
                await SaveConfigAsync(settings);
            }
            return settings;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static async Task<bool> SaveConfigAsync(DeviceSettings settings)
        {
            if (settings == null) return false;
            var path = GetConfigFilePath();
            var tempPath = path + ".tmp";//先写入临时文件，写入成功后再替换原文件，避免写入过程中程序崩溃导致配置文件损坏
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            await IoLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(settings, options);
                await File.WriteAllTextAsync(tempPath, json);
                File.Move(tempPath, path,true);//替换原文件

                LogService.Info($"配置文件保存成功:{path}");

                return true;
            }
            catch (Exception e)
            {
                LogService.Error($"保存配置文件失败:{e.Message}",e);
                return false;
            }
            finally
            {
                IoLock.Release();
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        LogService.Warn($"临时文件删除失败:{tempPath}");
                    }
                }
            }

        }
        /// <summary>
        /// 备份配置（报错的时候）
        /// </summary>
        /// <param name="originalPath"></param>
        public static void BackCorrupFile(string originalPath)
        {
            try
            {
                var backPath = originalPath + ".corrupt." + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");//备份路径
                File.Copy(originalPath, backPath, true);//备份原文件
                LogService.Info($"损坏的配置文件已备份到:{backPath}");
            }
            catch 
            {
               LogService.Warn($"损坏的配置文件备份失败:源路径{originalPath}");
            }
        }
    }
}

