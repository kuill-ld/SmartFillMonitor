using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFillMonitor.Services
{
    public class AlarmServcies
    {
        /// <summary>
        /// 报警触发事件
        /// </summary>
        public static event EventHandler<AlarmRecord> AlarmTriggered;
        /// <summary>
        /// 报警消除事件
        /// </summary>
        public static event EventHandler<AlarmRecord> AlarmRecover;

        /// <summary>
        /// 触发报警
        /// </summary>
        /// <param name="alarmRecord"></param>
        /// <returns></returns>
        public static async Task TriggerAlarmAsync(AlarmRecord alarmRecord)
        {
            try
            {
                bool isAlreadyActive = await DbProvider.Fsql.Select<AlarmRecord>()
                .Where(a => a.AlarmCord == alarmRecord.AlarmCord && a.IsActive)
                .AnyAsync();
                if (isAlreadyActive)
                {
                    LogService.Warn($"报警已处于活跃状态，不再重复触发 → {alarmRecord.AlarmCord}");
                    return;
                }
                await DbProvider.Fsql.Insert(alarmRecord).ExecuteAffrowsAsync();//没有该报警就保存该报警数据到数据库
                var lastestRecord = await DbProvider.Fsql.Select<AlarmRecord>()
                    .Where(a => a.AlarmCord == alarmRecord.AlarmCord)
                    .OrderByDescending(a => a.Id)
                    .FirstAsync();//查询刚插入的报警记录，获取数据库生成的Id
                if (lastestRecord!=null)
                {
                    alarmRecord.Id = lastestRecord.Id;//把数据库生成的Id赋值回报警记录对象
                }
                LogService.Error($"报警触发了:{alarmRecord.AlarmCord}:{alarmRecord.Message}");
                AlarmTriggered.Invoke(null, alarmRecord);//通知界面有新的报警触发了
            }
            catch (Exception e)
            {

                LogService.Error($"触发报警异常:{alarmRecord.AlarmCord}", e);
            }

        }
        /// <summary>
        /// 报警恢复
        /// </summary>
        /// <param name="alarmCord"></param>
        /// <returns></returns>
        public static async Task RecoverAlarmAsync(AlarmCord alarmCord)
        {
            try
            {
                var activeAlarm = await DbProvider.Fsql.Select<AlarmRecord>()
                        .Where(a => a.AlarmCord == alarmCord&& a.IsActive)
                        .FirstAsync();
                if (activeAlarm == null) return;//没找到对应的活跃报警
                activeAlarm.IsActive = false;//恢复为不活跃
                activeAlarm.EndTime = DateTime.Now;//记录恢复时间
                activeAlarm.Duration = (activeAlarm.EndTime - activeAlarm.StartTime).TotalSeconds;//计算持续时间
                await DbProvider.Fsql.Update<AlarmRecord>()
                    .SetSource(activeAlarm)
                    .UpdateColumns(a => new { a.IsActive, a.EndTime, a.Duration })
                    .ExecuteAffrowsAsync();//更新数据库记录
                LogService.Info("报警恢复了:" + alarmCord);
                AlarmRecover?.Invoke(null, activeAlarm);//通知界面有报警恢复了

            }
            catch (Exception ex)
            {
                LogService.Error($"恢复报警异常{alarmCord}",ex);
            }
        }
        /// <summary>
        /// 人工确认报警
        /// </summary>
        /// <param name="alarmId"></param>
        /// <returns></returns>
        public static async Task<bool> AckAlarmAsync(long alarmId,string operateUser)
        {
            try
            {
                var result = await DbProvider.Fsql.Update<AlarmRecord>()
                    .Set(a => a.IsRead, true)
                    .Set(a => a.IsActive, false)//确认报警的同时把报警设置为非活跃状态，表示设备已经解除报警了
                    .Set(a => a.ReadTime, DateTime.Now)
                    .Set(a=>a.RemarkUser,operateUser)
                    .Where(a => a.Id == alarmId && a.IsRead == false)//只能确认非活跃的报警
                    .ExecuteAffrowsAsync();
                if(result>0)//受影响的行数大于0表示成功确认了报警
                {
                    LogService.Info($"报警已确认了:Id={alarmId},操作员{operateUser}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogService.Error($"确认报警异常 Id={alarmId}", ex);
                return false;
            }
        }
        /// <summary>
        /// 获取当前实时的报警
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AlarmRecord>> GetActiveAlarmsAsync()
        {
            return await DbProvider.Fsql.Select<AlarmRecord>()
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }
        /// <summary>
        /// 获取历史所有的报警
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static async Task<(List<AlarmRecord> Items, long Total)> GetHistoryAlarmsAsync(int pageIndex,int pageSize,DateTime?startTime = null ,DateTime ?endTime=null,AlarmSeverity alarmSeverity = AlarmSeverity.All)
        {
            try
            {
                var query = DbProvider.Fsql.Select<AlarmRecord>();
                if (startTime.HasValue)
                {
                    query = query.Where(a => a.StartTime >= startTime.Value);
                }
                if (endTime.HasValue)
                {
                    query = query.Where(a => a.EndTime <= endTime.Value);
                }
                if (alarmSeverity!=AlarmSeverity.All)
                {
                    query = query.Where(w=>w.AlarmSeverity == alarmSeverity);
                }
                var total = await query.CountAsync();
                var list = await query.OrderByDescending(a => a.StartTime)
                    .Page(pageIndex, pageSize)
                    .ToListAsync();
                return (list,total);
            }
            catch (Exception ex) 
            {
                LogService.Error($"查询历史报警异常", ex);
                return (new(),0);
            }
        }
    }

}
