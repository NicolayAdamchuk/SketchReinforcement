using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Management;

namespace SketchReinforcement
{
    public static class KeyUtils
    {
        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(string pathName, 
            StringBuilder volumeNameBuffer, 
            UInt32 volumeNameSize, 
            ref UInt32 volumeSerialNumber, 
            ref UInt32 maximumComponentLength, 
            ref UInt32 fileSystemFlags, 
            StringBuilder fileSystemNameBuffer, 
            UInt32 fileSystemNameSize);

        public static string GetVolumeSerial(string strDriveLetter)
        {
            uint serNum = 0;
            uint maxCompLen = 0;
            StringBuilder _volLabel = new StringBuilder(256); // Label
            UInt32 VolFlags = new UInt32();
            StringBuilder FSName = new StringBuilder(256); // File System Name
            strDriveLetter += ":\\"; // fix up the passed-in drive letter for the API call
            long Ret = GetVolumeInformation(strDriveLetter, _volLabel, (UInt32)_volLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (UInt32)FSName.Capacity);
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(Ret.ToString());
            //bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();

            return password;
        }

        /// <summary>
        /// Получает ID процессора
        /// </summary>
        /// <returns></returns>
        private static string GetID()
        {
            string str = "";
            
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                str = queryObj["ProcessorId"].ToString();
            }
            return str;
        }

        private static string GetUniqueHardwaeId()
        {
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                sb.Append(queryObj["NumberOfCores"]);
                sb.Append(queryObj["ProcessorId"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["SocketDesignation"]);

                Console.WriteLine(queryObj["ProcessorId"]);
                Console.WriteLine(queryObj["Name"]);
                Console.WriteLine(queryObj["SocketDesignation"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                sb.Append(queryObj["Manufacturer"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["Version"]);

                Console.WriteLine(queryObj["Manufacturer"]);
                Console.WriteLine(queryObj["Name"]);
                Console.WriteLine(queryObj["Version"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                sb.Append(queryObj["Product"]);
                Console.WriteLine(queryObj["Product"]);
            }

            var bytes = Encoding.ASCII.GetBytes(sb.ToString());
            SHA256Managed sha = new SHA256Managed();

            byte[] hash = sha.ComputeHash(bytes);

            return BitConverter.ToString(hash);
        }

        public static string GetAllData()
        {
            Dictionary<string, string> ids = new Dictionary<string, string>();

            ManagementObjectSearcher searcher;
            string result = string.Empty;

            //процессор
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("ProcessorId", queryObj["ProcessorId"].ToString());

            //мать
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_Card");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("CardID", queryObj["SerialNumber"].ToString());

            //клавиатура
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_KeyBoard");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("KeyBoardID", queryObj["DeviceId"].ToString());

            //ОС
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_OperatingSystem");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("OSSerialNumber", queryObj["SerialNumber"].ToString());

            //мышь
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PointingDevice");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("MouseID", queryObj["DeviceID"].ToString());

            //звук
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SoundDevice");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("SoundCardID", queryObj["DeviceID"].ToString());

            //CD-ROM
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_CDROMDrive");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("CDROMID", queryObj["DeviceID"].ToString());

            //UUID
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("UUID", queryObj["UUID"].ToString());

            foreach (var x in ids)
                result += x.Key + ": " + x.Value + "\r\n";

            return result;

        }

        public static string GetIdCPU()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;

            //процессор
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["ProcessorId"].ToString();
            return result;
        }
        public static string GetIdHDD()
        {
            uint serNum = 0;
            uint maxCompLen = 0;
            StringBuilder _volLabel = new StringBuilder(256); // Label
            UInt32 VolFlags = new UInt32();
            StringBuilder FSName = new StringBuilder(256); // File System Name
            string strDriveLetter = "C:\\"; // fix up the passed-in drive letter for the API call
            long Ret = GetVolumeInformation(strDriveLetter, _volLabel, (UInt32)_volLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (UInt32)FSName.Capacity);
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(Ret.ToString());
            //bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }

            return serNum.ToString();
        }
        public static string GetIdMB()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;

            //мать
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_Card");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["SerialNumber"].ToString();

            return result;
        }
        public static string GetIdKeyboard()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;
            //клавиатура
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_KeyBoard");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["DeviceId"].ToString();

            return result;
        }
        public static string GetIdOS()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;

            //ОС
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_OperatingSystem");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["SerialNumber"].ToString();

            return result;
        }
        public static string GetIdMouse()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;

            //мышь
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PointingDevice");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["DeviceID"].ToString();
            return result;
        }
        public static string GetIdAudio()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;
            //звук
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SoundDevice");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["DeviceID"].ToString();

            return result;
        }
        public static string GetIdCDROM()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;
            //CD-ROM
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_CDROMDrive");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["DeviceID"].ToString();

            return result;
        }
        public static string GetIdUUID()
        {
            ManagementObjectSearcher searcher;
            string result = string.Empty;
            //UUID
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (ManagementObject queryObj in searcher.Get())
                result += queryObj["UUID"].ToString();

            return result;
        }

        public static string GetHesh(string input)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);

            StringBuilder s = new StringBuilder();
            foreach (byte b in bs)
            {
                //s.Append(b.ToString("x2").ToLower());
                s.Append(b.ToString());
            }
            
             return s.ToString();

        }

        public static string CreatePK()
        {
            string k = string.Empty;           

            string keyVal = "5C7547454C553646424D57334E485944";
            string[] licK = new string[14];            
            char[] keyArr = keyVal.ToCharArray();            
            List<string> keyVals = new List<string>();
            List<string> keyVues = new List<string>();
            
            for (int i = 0; i < keyArr.Length;)
            {                
                string st = new string(keyVal.ToCharArray(i,2));                
                keyVals.Add(st);                
                i += 2;
            }            

            foreach (string val in keyVals)
            {
                keyVues.Add("00" + val);
            }
            string su = "\"U" + keyVues[0] + keyVues[1];
            su = su.Remove(0, 1);            
            keyVues.RemoveRange(0, 2);
            licK[0] = "\u0047";
            licK[1] = "\u0045";
            licK[2] = "\u004C";
            licK[3] = "\u0055";
            licK[4] = "\u0036";
            licK[5] = "\u0046";
            licK[6] = "\u0042";
            licK[7] = "\u004D";
            licK[8] = "\u0057";
            licK[9] = "\u0033";
            licK[10] = "\u004E";
            licK[11] = "\u0048";
            licK[12] = "\u0059";
            licK[13] = "\u0044";

            for (int i = 0; i < licK.Length; i++ )
            {
                k += licK[i];
            }
                return k;
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
        
        public static string RandomString2(int size)
        {

            Random _rng = new Random();
            string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }
    }
}
