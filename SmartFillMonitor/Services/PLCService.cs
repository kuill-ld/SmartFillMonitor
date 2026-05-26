using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using SmartFillMonitor.Models;
using Modbus.Device;
namespace SmartFillMonitor.Services
{
    public static class PLCService
    {
        private static SerialPort? _serialPort;//串口对象
        private static IModbusSerialMaster? _modbusMaster;//Modbus主站对象用于读写PLC数据
        private static CancellationTokenSource? _cts = new();//用于控制轮询读取PLC数据的后台任务
        private static readonly SemaphoreSlim _lock = new (1,1);//防止多线程同时连接或断开串口IO锁
        private const byte SlaveId = 1;//PLC Modbus从站地址，通常为1
        private static bool _isConnected => _serialPort != null&& _serialPort.IsOpen;//当前连接状态
        /// <summary>
        /// 获取串口列表
        /// </summary>
        /// <returns></returns>
        public static string[] GetPortNames() => SerialPort.GetPortNames();
        /// <summary>
        /// 当取到新数据触发，用于UI界面更新显示PLC数据
        /// </summary>
        public static event EventHandler<DeviceState>? DataReceived;
        /// <summary>
        /// 当连接状态发生变化触发
        /// </summary>
        public static event EventHandler<bool>? ConnectionChanged;
      
        /// <summary>
        /// 初始化串口
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static async Task Initialize(DeviceSettings settings)
        {
            await DisConnectAsync();//先断开之前的连接，释放资源
            //如果设置了自动连接，则创建串口对象并尝试连接
            if (settings is { IsAutoLogin: true })
            {
                _serialPort = new()
                {
                    PortName = settings.PortName,
                    BaudRate = settings.BaudRate,
                    DataBits = settings.DataBits,
                    Parity = Enum.TryParse<Parity>(settings.Parity, out var parity) ? parity : Parity.None,
                    StopBits = Enum.TryParse<StopBits>(settings.StopBits, out var stopBits) ? stopBits : StopBits.One,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                await ConnectAsync();//尝试连接
            }
            
        }
        /// <summary>
        /// 打开串口创建Modbus对象，并开始轮询读取PLC数据
        /// </summary>
        /// <returns></returns>
        public static async Task ConnectAsync()
        {
            if (_serialPort == null||_isConnected) return;
            try
            {
                _serialPort.Open();
                _modbusMaster= ModbusSerialMaster.CreateRtu(_serialPort);
                _modbusMaster.Transport.ReadTimeout = 1000;//设置Modbus读超时1s
                _modbusMaster.Transport.WriteTimeout = 1000;//设置Modbus写超时1s
                ConnectionChanged?.Invoke(null, true);//通知界面连接成功
                LogService.Info($"成功连接串口:{_serialPort.PortName}");
                _cts = new CancellationTokenSource();
                _=Task.Run(()=>PollDataLoop(_cts.Token));//启动后台任务轮询读取PLC数据
            }
            catch (Exception ex)
            {
                ConnectionChanged?.Invoke(null, false);//通知界面连接失败
                LogService.Error($"连接串口失败:{_serialPort.PortName}:{ex.Message}",ex);
            }
            await Task.CompletedTask;//占位，保持异步方法签名
        }
        /// <summary>
        /// 安全断开释放所有资源，关闭串口，停止轮询读取PLC数据
        /// </summary>
        /// <returns></returns>
        public static async Task DisConnectAsync()
        {
            _cts.Cancel();//停止轮询任务
            await _lock.WaitAsync();//等待锁，确保没有正在连接或断开的操作
            try
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                        
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                }
                if (_modbusMaster != null)
                {
                    _modbusMaster.Dispose();
                    _modbusMaster = null;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                _lock.Release();//释放锁
                ConnectionChanged?.Invoke(null, false);//通知界面连接状态变为断开
            }
        }
        /// <summary>
        /// 后台持续轮询读取PLC数据DataReceived事件公布数据
        /// </summary>
        private static async Task PollDataLoop(CancellationToken token)
        {
            int errCount = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!_isConnected)
                    {
                        await Task.Delay(1000);//如果没有连接，等待1s后重试
                        continue;
                    }
                    var state = await ReadStateAsync();//从PLC读取当前设备状态数据并转换为DeviceState对象
                    errCount = 0;//重置错误计数
                    DataReceived?.Invoke(null, state);//公布数据
                    await Task.Delay(200, token);//每200ms读取一次数据
                }
                catch (OperationCanceledException ex)
                {
                    break;//任务取消，退出循环
                }
                catch (Exception ex)
                {
                    errCount++;
                    if (errCount >= 3)
                    {
                        LogService.Error($"连续3次读取PLC数据失败，可能连接已断开,{ex.Message}");
                        ConnectionChanged?.Invoke(null, false);//通知界面连接状态变为断开
                        errCount = 0;//重置错误计数
                    }
                    await Task.Delay(1000, token);//读取数据失败，等待1s后重试
                }
               
            }
        }
        /// <summary>
        /// 从PLC读取当前设备状态数据并转换为DeviceState对象
        /// </summary>
        /// <returns></returns>
        public static async Task<DeviceState> ReadStateAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if(_modbusMaster == null) throw new InvalidOperationException("未连接");
                ushort[] registers= await _modbusMaster.ReadHoldingRegistersAsync(SlaveId, 0, 10);//从PLC读取10个寄存器数据，地址根据实际PLC程序定义
                const ushort barcodeStart = 10;
                const ushort barcodeLength = 10;
                string barcode = string.Empty;
                try
                {
                    ushort[] barcodeRes = await _modbusMaster.ReadHoldingRegistersAsync(SlaveId, barcodeStart, barcodeLength);
                    barcode = ConvertRegisterToString(barcodeRes);
                }
                catch (Exception ex)
                {
                    LogService.Warn($"读取条码失败:{ex.Message}");
                }
                return new DeviceState
                {
                    ActualCount = registers[ModbusConfigHelper.ActualCount],
                    TargetCount = registers[ModbusConfigHelper.TargetCount],
                    CurrentTemp = registers[ModbusConfigHelper.CurrentTemp]/100.0,
                    SettingTemp = registers[ModbusConfigHelper.SettingTemp]/100.0,
                    RunningTime = registers[ModbusConfigHelper.RunningTime]/100.0,
                    CurrentCycleTime = registers[ModbusConfigHelper.CurrentCycleTime] /100.0,
                    StandardCycleTime = registers[ModbusConfigHelper.StandardCycleTime]/100.0,
                    LiquidLevel = registers[ModbusConfigHelper.LiquidLevel]/100.0,
                    ValueOpen = registers[ModbusConfigHelper.ValueOpen] == 1?true:false,
                    BarCode = barcode
                };

            }
       
            finally 
            {    
                _lock.Release();
               
            }


        }

        private static string ConvertRegisterToString(ushort[] barcodeRes)
        {
           if(barcodeRes.Length == 0||barcodeRes==null) return string.Empty;//空检查
           List<byte> bytes = new();
            foreach (var item in barcodeRes)
            {
                //如果寄存器值为0，表示字符串结束
                if (item == 0) break;
                byte high = (byte)(item >> 8); //高字节
                byte low = (byte)(item &0xff);//低字节  
                if(high != 0) bytes.Add(high);
                if(low != 0) bytes.Add(low);
            }
            return Encoding.ASCII.GetString(bytes.ToArray()).Trim();

        }

        /// <summary>
        /// 向PLC写入命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task WriteCommandAsync(string command,bool value)
        {
            ushort address = command == "Start" ? (ushort)1 : (ushort)2;
            await _lock.WaitAsync();
            try
            {
                if (_modbusMaster == null) return;
                await _modbusMaster.WriteSingleCoilAsync(SlaveId, address, value);
                LogService.Info($"写入:{command}={value}");
            }
            catch (Exception ex)
            {
                LogService.Error($"写入命令失败:{command}={value}:{ex.Message}",ex);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
