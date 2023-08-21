using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDiceLoggingService
{
    public class Log
    {
        string deviceSerialNumber;
        string deviceName;
        string relaySerialNumber;
        string relayName;
        Int32 batteryMv;
        Int32 faceNumber;
        string faceName;
        Int32 accX;
        Int32 accY;
        Int32 accZ;
        Int16 LQI;
        DateTime dt;

        public Log(string deviceSerialNumber, string deviceName, string relaySerialNumber, string relayName,
        Int32 batteryMv, Int32 faceNumber, string faceName, Int32 accX, Int32 accY, Int32 accZ, Int16 LQI, DateTime dt)
        {
            this.deviceSerialNumber = deviceSerialNumber;
            this.deviceName = deviceName;
            this.relaySerialNumber = relaySerialNumber;
            this.relayName = relayName;
            this.batteryMv = batteryMv;
            this.faceNumber = faceNumber;
            this.faceName = faceName;
            this.accX = accX;
            this.accY = accY;
            this.accZ = accZ;
            this.LQI = LQI;
            this.dt = dt;
        }

        public DateTime GetDateTime()
        {
            return dt;
        }

        // データ文字列（タブ区切り）
        public string GetString()
        {
            string line = 
                deviceSerialNumber + "\x09" +
                deviceName + "\x09" +
                relaySerialNumber +"\x09" +
                relayName +"\x09" +
                batteryMv.ToString() +"\x09" +
                faceNumber +"\x09" +
                faceName + "\x09" +
                accX.ToString() +"\x09" +
                accY.ToString() +"\x09" +
                accZ.ToString() +"\x09" +
                LQI.ToString() +"\x09" +
                dt.ToString("yyyy/MM/dd HH:mm:ss");
            return line;
        }

        // ヘッダ文字列（タブ区切り）
        public string GetHeader()
        {
            string header =
                "ID\x09" +
                "NAME\x09" +
                "RELAY-ID\x09" +
                "RELAY-NAME\x09" +
                "BATTERY-MV\x09" +
                "FACE\x09" +
                "FACE-NAME\x09" +
                "ACC-X\x09" +
                "ACC-Y\x09" +
                "ACC-Z\x09" +
                "LQI\x09" +
                "DATETIME";
            return header;
        }

        // 文字列を受けとり、BASE64にした文字列を返す
        public string ToBase64(string str)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(str));
        }

        // BASE64文字列を受けとり、フィールドに格納する
        public void FromBase64(string str64)
        {
            string str = Encoding.Default.GetString(Convert.FromBase64String(str64));
            string[] flds = str.Split(new char[] { '\x09' });
            deviceSerialNumber = flds[0];
            deviceName = flds[1];
            relaySerialNumber = flds[2];
            relayName = flds[3];
            batteryMv = Convert.ToInt32(flds[4]);
            faceNumber = Convert.ToInt32(flds[5]);
            faceName = flds[6];
            accX = Convert.ToInt32(flds[7]);
            accY = Convert.ToInt32(flds[8]);
            accZ = Convert.ToInt32(flds[9]);
            LQI = Convert.ToInt16(flds[10]);
            dt = Convert.ToDateTime(flds[11]);
        }
    }
}
