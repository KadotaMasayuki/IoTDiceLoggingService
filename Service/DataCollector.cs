/*
 * 
 * 2023/07/12(Wed) 17:00 yt イイ感じに動作している。
 * 
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


namespace IoTDiceLoggingService
{
    public class DataCollector
    {
        // // エラーカウント。数回連続すると運転停止
        // int errorCount = 0;

        // イベントログを伝播するため
        Service1 service = null;

        // SerialPort経由でデータを取得する
        SerialPort serialPort1 = new SerialPort();

        // 取得したデータ列を一時的に格納する
        StringBuilder sb = new StringBuilder();

        public DataCollector(Service1 service)
        {
            this.service = service;
        }

        public DataCollector()
        {
            ;  // nop
        }

        public void InitCollector()
        {
            // 停止しておく
            StopCollect();

            // シリアルポートから読みだしたデータを格納するバッファをクリア
            sb.Clear();

            // シリアルポート準備
            serialPort1.DataReceived += SerialPort1_DataReceived; ;
            serialPort1.BaudRate = 115200;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = StopBits.One;
            serialPort1.Parity = Parity.None;
            serialPort1.Handshake = Handshake.None;
            serialPort1.ReadBufferSize = 9192;
            serialPort1.PortName = Settings.comString;  // ポート番号。"COM6"とか
        }

        public void StartCollect()
        {
            try
            {
                serialPort1.Open();
            }
            catch
            {
                throw new Exception("can not open COM Port <" + serialPort1.PortName + ">");
            }
        }

        public void StopCollect()
        {
            try
            {
                serialPort1.Close();
            }
            catch { }
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 全部読み出す
            sb.Append(serialPort1.ReadExisting());

            // 区切りごとに1フレームずつ処理する
            // ':'で始まり、'0x0d, 0x0a'で終わる
            int tailIdx = sb.ToString().IndexOf("\x0d\x0a");
            if (tailIdx >= 0)
            {
                string frame1 = sb.ToString(0, tailIdx);
                if (frame1[0].Equals(':') && frame1.Length >= 299)
                {
                    // データ完結したので解析開始
                    CollectData(frame1);
                }
                // 次のデータのために解析済み部分と終端記号'0x0d,0x0a'を削除
                // 先行データが壊れて完結しない場合もここで削除できる
                sb.Remove(0, tailIdx + 2);
            }
        }

		// データを解析して保存する
        private void CollectData(string dataFrame)
        {
            string relaySerialNumber;  // 中継器のID[1:1+8]
            Int16 lqi;  // 電波強度[9:9+2]
            string sn;  // データ送信のたびに＋１される続き番号[11:11+4]
            string deviceSerialNumber;  // 送信元のシリアル番号[15:15+8]
            string deviceId;  // 送信元の論理デバイスID[23:23+2]
            string sensorType;  // センサー種別[25:25+2]
            string versionId;  // PAL基板バージョンとPAL基板ID。TWELITE_CUEは'05'[27:27+2]
            Int32 batteryMv;  // 電池電圧[mv][69:69+4]
            string magnetStatus;  // 磁器センサ[93:93+2]
            Int16[] accXs = new Int16[10];  // 加速度センサ　XYZは[103:103+12]～20刻みで10個
            Int16[] accYs = new Int16[10];
            Int16[] accZs = new Int16[10];
            Int32 accX = 0;
            Int32 accY = 0;
            Int32 accZ = 0;
            Int32 faceNumber = 0;  // 上を向いている面 X+=ケース面4、X-=ケース面3、Y+=ケース面2、Y-=ケース面5、Z+=ケース面1、Z-=ケース面6

            relaySerialNumber = dataFrame.Substring(1, 8);
            lqi = Convert.ToInt16(dataFrame.Substring(9, 2), 16);
            sn = dataFrame.Substring(11, 4);
            deviceSerialNumber = dataFrame.Substring(15, 8);
            deviceId = dataFrame.Substring(23, 2);
            sensorType = dataFrame.Substring(25, 2);
            versionId = dataFrame.Substring(27, 2);
            batteryMv= Convert.ToInt32(dataFrame.Substring(69, 4), 16);
            magnetStatus = dataFrame.Substring(93, 2);

            if (relaySerialNumber[0] == '8' && deviceSerialNumber[0] == '8' && sensorType.Equals("80") && versionId.Equals("05"))
            {
                // データ取得対象のデバイスからのデータの場合

                // 加速度データ10個を各軸に分解して格納
                for (int i = 0; i < 10; i++)
                {
                    accXs[i] = Convert.ToInt16(dataFrame.Substring(103 + 20 * i, 4), 16);
                    accYs[i] = Convert.ToInt16(dataFrame.Substring(107 + 20 * i, 4), 16);
                    accZs[i] = Convert.ToInt16(dataFrame.Substring(111 + 20 * i, 4), 16);
                }
                // 各軸の平均値を取得
                accX = 0;
                accY = 0;
                accZ = 0;
                for (int i = 0; i < 10; i ++)
                {
                    accX += accXs[i];
                    accY += accYs[i];
                    accZ += accZs[i];
                }
                accX /= 10;
                accY /= 10;
                accZ /= 10;
                // どの軸が優勢か決定する
                // 上を向いている面 X+=ケース面4、X-=ケース面3、Y+=ケース面2、Y-=ケース面5、Z+=ケース面1、Z-=ケース面6
                Int32 absX = Math.Abs(accX);
                Int32 absY = Math.Abs(accY);
                Int32 absZ = Math.Abs(accZ);
                if (absX >= absY && absX >= absZ)
                {
                    if (accX > 0)
                    {
                        faceNumber = 4;
                    }
                    else
                    {
                        faceNumber = 3;
                    }
                }
                else if (absY >= absX && absY >= absZ)
                {
                    if (accY > 0)
                    {
                        faceNumber = 2;
                    }
                    else
                    {
                        faceNumber = 5;
                    }
                }
                else if (absZ > absX && absZ > absY)
                {
                    if (accZ > 0)
                    {
                        faceNumber = 1;
                    }
                    else
                    {
                        faceNumber = 6;
                    }
                }

                // ログファイル用に整形し格納する
                Log log = new Log(
						deviceSerialNumber,
						Settings.GetDiceNameBySerialNumber(deviceSerialNumber),
                        relaySerialNumber,
						Settings.GetRelayNameBySerialNumber(relaySerialNumber),
                        batteryMv,
						faceNumber,
						Settings.GetDiceFaceBySerialNumber(deviceSerialNumber, faceNumber),
                        accX,
						accY,
						accZ,
						lqi,
						DateTime.Now);

                // ログを保存する
                StoreLog(log);

                // 場合によってはネットワーク越しにログを投げる
                // SendLog(log) { SomeServicesProtocol.Send(log.ToBase64());  }
            }
            else
            {
                ;  // 既定のログフォーマットではないため何もしない
            }
        }

		// ログ保存
        private void StoreLog(Log log)
        {
            // 保存先ディレクトリを確保
            try
            {
                System.IO.Directory.CreateDirectory(Settings.logDir);
                // errorCount = 0;  // エラー回数クリア
            }
            catch
            {
                // // 連続してエラーしたときに停止する
                // errorCount++;
                // if (errorCount > 10)
                // {
                //     if (service != null)
                //     {
                //         service.WriteEventLog("can not get directory <" + Settings.logDir + ">(" + errorCount.ToString() + ") ... service stop", System.Diagnostics.EventLogEntryType.Error);
                //         service.Stop();
                //     }
                // }
                // else
                // {
                //     if (service != null)
                //     {
                //         service.WriteEventLog("can not get directory <" + Settings.logDir + ">(" + errorCount.ToString() + ")", System.Diagnostics.EventLogEntryType.Error);
                //     }
                // }
                service.WriteEventLog("can not get directory <" + Settings.logDir + ">", System.Diagnostics.EventLogEntryType.Error);
            }

            // ログを取得する
            string header = log.GetHeader();
            string data = log.GetString();
            DateTime dt = log.GetDateTime();

            // ファイル名を生成
            // 時刻を含まない日付だけのファイル名にする。1日1ファイル。
            string filename = dt.ToString("yyyy-MM-dd") + ".csv";
            string logPath = Settings.logDir + "\\" + filename;
            string dtStr = dt.ToString("yyyy/MM/dd HH:mm:ss");
            bool newFile = false;
            if (!System.IO.File.Exists(logPath))
            {
                newFile = true;
            }
            // ログファイルに追記
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logPath, true, Encoding.UTF8))
                {
                    if (newFile)
                    {
                        // 列ヘッダを付ける
                        writer.WriteLine(header);
                    }
                    writer.WriteLine(data);
                }
                // errorCount = 0;  // エラー回数クリア
            }
            catch
            {
                // // 連続してエラーしたときに停止する
                // errorCount++;
                // if (errorCount > 10)
                // {
                //     if (service != null)
                //     {
                //         service.WriteEventLog("can not write log <" + logPath + ">(" + errorCount.ToString() + ") ... service stop", System.Diagnostics.EventLogEntryType.Error);
                //         service.Stop();
                //     }
                // }
                // else
                // {
                //     if (service != null)
                //     {
                //         service.WriteEventLog("can not write log <" + logPath + ">(" + errorCount.ToString() + ")", System.Diagnostics.EventLogEntryType.Error);
                //     }
                // }
                service.WriteEventLog("can not write log <" + logPath + ">", System.Diagnostics.EventLogEntryType.Error);
            }
        }
    }
}
