using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDialogueEditor
{
    public class Config
    {

        public string LastOpenedScript { get; set; }

        public void LoadConfig()
        {
            // Open Subkey
            var key = Registry.CurrentUser.CreateSubKey("Software\\DALTools\\ScriptDialogueEditor");
            LastOpenedScript = GetRegistryString(key, "LastOpenedScript");
            // Close Subkey
            key.Close();
        }

        public void SaveConfig()
        {
            // Open Subkey
            var key = Registry.CurrentUser.CreateSubKey("Software\\DALTools\\ScriptDialogueEditor");
            SetRegistryString(key, "LastOpenedScript", LastOpenedScript);
            // Close Subkey
            key.Close();
        }

        public static int GetRegistryInt(RegistryKey subkey, string name, int defaultValue = -1)
        {
            if (subkey == null)
                return defaultValue;
            if (subkey.GetValue(name) is int value)
                return value;
            return defaultValue;
        }

        public static void SetRegistryInt(RegistryKey subkey, string name, int value)
        {
            if (subkey == null)
                return;
            subkey.SetValue(name, value, RegistryValueKind.DWord);
        }

        public static bool GetRegistryBool(RegistryKey subkey, string name, bool defaultValue = false)
        {
            if (subkey == null)
                return defaultValue;
            if (subkey.GetValue(name) is int i)
            {
                if (i == 1)
                    return true;
                else
                    return false;
            }
            return defaultValue;
        }

        public static void SetRegistryBool(RegistryKey subkey, string name, bool value)
        {
            if (subkey == null)
                return;
            subkey.SetValue(name, value ? 1 : 0, RegistryValueKind.DWord);
        }

        public static string GetRegistryString(RegistryKey subkey, string name, string defaultValue = "")
        {
            if (subkey == null)
                return defaultValue;
            if (subkey.GetValue(name) is string s)
                return s;
            return defaultValue;
        }

        public static void SetRegistryString(RegistryKey subkey, string name, string value)
        {
            if (subkey == null)
                return;
            subkey.SetValue(name, value, RegistryValueKind.String);
        }
    }
}
