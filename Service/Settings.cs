using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDiceLoggingService
{

    public static class Settings
    {
        // ログ保存フォルダのパス
        public static string logDir = "";

        // COMポート番号
        public static string comString = "";
        
        // サイコロ設定を格納するディクショナリ
        // キー：サイコロID
        // 値：次のデータの配列　「サイコロID、サイコロ名称、面1の名称、面2の名称、、、面6の名称」
        static System.Collections.Specialized.OrderedDictionary dice = new System.Collections.Specialized.OrderedDictionary();

        // 中継器設定を格納するディクショナリ
        // キー：中継器ID
        // 値：中継器の名称
        static System.Collections.Specialized.OrderedDictionary relay = new System.Collections.Specialized.OrderedDictionary();

        public static void ReadSettings(string settingsPath)
        {
            // 設定をクリア
            logDir = "";
            comString = "";
            dice.Clear();
            relay.Clear();

            // 設定ファイルを読む
            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(settingsPath, Encoding.UTF8))
                {
                    string keyword = "";
                    while (!reader.EndOfStream)
                    {
                        string aLine = reader.ReadLine();
                        if (aLine.Equals(""))
                        {
                            // 空行は無視
                            continue;
                        }
                        if (aLine.StartsWith("[") && aLine.EndsWith("]"))
                        {
                            // キーワード行
                            keyword = aLine;
                            continue;
                        }
                        if (keyword.Equals("[log]", StringComparison.OrdinalIgnoreCase))
                        {
                            // ログフォルダのパス。"C:\bin\log\xxx"など
                            logDir = aLine;
                            // パスの末尾から区切り文字を消す
                            while (logDir.EndsWith("\\"))
                            {
                                logDir = logDir.Substring(0, logDir.Length - 1);
                            }
                        }
                        else if (keyword.Equals("[com]", StringComparison.OrdinalIgnoreCase))
                        {
                            // COMポート。"COM6"など
                            comString = aLine;
                        }
                        else if (keyword.Equals("[relay]", StringComparison.OrdinalIgnoreCase))
                        {
                            // 中継器のIDと名称をタブ区切りで
                            string[] flds = aLine.Split(new char[] { '\x09' });
                            if (flds.Length > 1 && !flds[0].Equals("") && !flds[1].Equals(""))
                            {
                                relay.Add(flds[0], flds[1]);
                            }
                        }
                        else if (keyword.Equals("[face]", StringComparison.OrdinalIgnoreCase))
                        {
                            // サイコロ面。タブ区切りで、"サイコロID<TAB>サイコロ名称<TAB>面1名称<TAB>面2名称<TAB>"
                            string[] flds = aLine.Split(new char[] { '\x09' });
                            string[] dummy = new string[] { "", "", "1", "2", "3", "4", "5", "6" };
                            for (int i = 0; i < dummy.Length; i++)
                            {
                                if (i < flds.Length)
                                {
                                    if (!flds[i].Equals(""))
                                    {
                                        dummy[i] = flds[i];  // dummyを元に、fldsで上書きして行く
                                    }
                                }
                            }
                            if (!dummy[0].Equals(""))
                            {
                                if (dummy[1].Equals(""))
                                {
                                    dummy[1] = dummy[0];  // dummy[1]=名前 が空なら、dummy[0]=ID を入れておく
                                }
                                dice.Add(dummy[0], dummy);   // dummy[0]=ID をキーに、全フィールドを突っ込む
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("can not read settings <" + settingsPath + ">");
            }
        }

        // ダイスID、使用者名、面１の名称、面２の名称、、、、面６の名称
        public static string[] GetDiceInfoBySerialNumber(string sn)
        {
            if (dice.Contains(sn))
            {
                return (string[])dice[sn];
            }
            else
            {
                return new string[] { sn, sn, "1", "2", "3", "4", "5", "6" };
            }
        }

        // ダイスの名前
        public static string GetDiceNameBySerialNumber(string sn)
        {
            if (dice.Contains(sn))
            {
                return ((string[])dice[sn])[1];
            }
            else
            {
                return sn;
            }
        }

        // ダイス面の名称
        // face : １～６
        public static string GetDiceFaceBySerialNumber(string sn, int face)
        {
            return GetDiceInfoBySerialNumber(sn)[face + 1];
        }

        // ダイス面設定すべて
        public static System.Collections.Specialized.OrderedDictionary GetDiceFaceSetting()
        {
            return dice;
        }

        // 中継器の名称
        public static string GetRelayNameBySerialNumber(string sn)
        {
            if (relay.Contains(sn))
            {
                return (string)relay[sn];
            }
            else
            {
                return sn;
            }
        }

        // 中継器設定すべて
        public static System.Collections.Specialized.OrderedDictionary GetRelaySetting()
        {
            return relay;
        }

    }
}
