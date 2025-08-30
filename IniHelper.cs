using System.Runtime.InteropServices;
using System.Text;

namespace mongo
{
    public class IniHelper
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void WriteValue(string section, string key, string value, string path)
        {
            WritePrivateProfileString(section, key, value, path);
        }
        public static string ReadValue(string section, string key, string path)
        {
            var temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp.ToString();
        }
    }
} 