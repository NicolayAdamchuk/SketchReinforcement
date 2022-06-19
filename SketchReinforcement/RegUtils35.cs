using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;


namespace SketchReinforcement
{    
    public static class RegUtils35
    {
        private const long SHCNE_ASSOCCHANGED = 0x8000000L;
        private const uint SHCNF_IDLIST = 0x0U;

        [Obsolete]
        public static bool GetValue(string keyName, ref string keyVal)
        {
            bool result = false;           

            try
            {
                //RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); // 4.0
                RegistryKey hklm = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "NN");

                string keyname = "SOFTWARE\\ArCADia\\" + keyName;
                RegistryKey addinkey = hklm.CreateSubKey(keyname);
                object check = addinkey.GetValue("SN");
                keyVal = check.ToString();
                if (!string.IsNullOrEmpty(keyVal))
                    result = true;
            }
            catch (Exception)
            {
                return result;
            }
                                 
            return result;
        }
       
        public static bool CreateKey(string keyName, string licNo)
        {
            bool retval = false;
            
            RegistryKey mainKey = Registry.LocalMachine.OpenSubKey("SOFTWARE",true);                       

            if (mainKey!= null)
            {
                string keyNamep = "ArCADia\\" + keyName;                                
                    try
                    {                      
                        RegistryKey ArCADiaKeyProd = mainKey.CreateSubKey(keyNamep);
                        ArCADiaKeyProd.SetValue(null, 0);
                        ArCADiaKeyProd.SetValue("SN", licNo);
                        retval = true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }              
            }
            
            return retval;
        }

        public static bool CheckKey(string keyName, ref string licVal)
        {
            bool retval = false;

            RegistryKey mainKey = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);            

            if (mainKey != null)
            {
                string keyNamep = "ArCADia\\" + keyName;
                
                try
                {                 
                    RegistryKey ArCADiaKeyProd = mainKey.OpenSubKey(keyNamep);
                    object licValo = ArCADiaKeyProd.GetValue("SN");
                    licVal = licValo.ToString();
                    retval = true;
                }
                catch (Exception)
                {
                    return false;
                }                

            }

            return retval;
        }
        [Obsolete]
        public static void CreateSetValue(string keyName,string val)
        {
            //RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64); //4.0
            RegistryKey hklm = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine,"NN");
            //Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64);
            //string keyname = "SOFTWARE\\SolidWorks\\AddIns\\{" + @"123" + "}";
            string keyname = "SOFTWARE\\ArCADia\\" + keyName;
            Microsoft.Win32.RegistryKey addinkey = hklm.CreateSubKey(keyname);
            addinkey.SetValue(null, 0);
            addinkey.SetValue("SN", val);
            //addinkey.SetValue("Title", "MyFirstAddin");

            //keyname = "Software\\SolidWorks\\AddInsStartup\\{" + @"123" + "}";
            //addinkey = hkcu.CreateSubKey(keyname);
            //addinkey.SetValue(null, 0);
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHChangeNotify(long wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);


        static void registry()
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
