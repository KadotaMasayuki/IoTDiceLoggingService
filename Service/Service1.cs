/*
 * チュートリアル: Windows サービス アプリを作成する
 * https://learn.microsoft.com/ja-jp/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer
 * イベントログ：いちいちデザイナから作らず、ソースコードを書くほうが分かりやすい。
 * インストーラーは不要。管理者モードのコマンドプロンプトでSCコマンドでインストール等する。
 */



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO.Ports;


namespace IoTDiceLoggingService
{
    // サービス用の各種定数
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };


    public partial class Service1 : ServiceBase
    {
        // イベントログ
        private System.Diagnostics.EventLog eventLog1;
        private int eventId = 1;

        // サービスステータス
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        // データ収集用クラス
        DataCollector dc;

        // 設定ファイルのパス
        string settingsPath;

        public Service1()
        {
            InitializeComponent();

            // イベントログ
            this.eventLog1 = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            if (!System.Diagnostics.EventLog.SourceExists("IoTDiceLoggingServiceSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("IoTDiceLoggingServiceSource", "IoTDiceLoggingServiceLog");
            }
            eventLog1.Source = "IoTDiceLoggingServiceSource";
            eventLog1.Log = "IoTDiceLoggingServiceLog";

            // 起動ディレクトリ
            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            // 設定ファイルは起動ディレクトリにある
            settingsPath = appDir + "\\Settings.txt";

            // データ収集用クラス
            dc = new DataCollector(this);
        }

        // イベントログ書込み
        public void WriteEventLog(string msg, EventLogEntryType type)
        {
            eventLog1.WriteEntry(msg, type, eventId++);
        }

        // サービススタート
        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // start
            WriteEventLog("Start", EventLogEntryType.Information);
            try
            {
                // 設定ファイルを読む
                Settings.ReadSettings(settingsPath);
                // UARTを再設定
                dc.InitCollector();
                // UARTを開始
                dc.StartCollect();
            }
            catch (Exception exp)
            {
                WriteEventLog(exp.Message, EventLogEntryType.Error);
                Stop();
                return;
            }
            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        // サービス停止
        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // end
            dc.StopCollect();
            WriteEventLog("Stop", EventLogEntryType.Information);
            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

    }
}
