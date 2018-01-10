using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace Ini
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class CIniFile
    {
        public bool     mbWriteAfterRead;
        public string   mPath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key,string val,string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key,string def, StringBuilder retVal,
            int size,string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public CIniFile(string INIPath)
        {
            mbWriteAfterRead = true;    

            if (INIPath.Length != 0) mPath = INIPath;
            else
            {
                mPath =  System.Reflection.Assembly.GetEntryAssembly().Location;
                int i = mPath.LastIndexOf('.');
                mPath = mPath.Substring( 0, i ) + ".INI";
            }
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void IniWriteValue(string Section,string Key,string Value)
        {
            WritePrivateProfileString(Section,Key,Value,this.mPath);
        }
        
        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string IniReadValue(string Section,string Key, string Default = "" )
        {
            StringBuilder temp = new StringBuilder(2000);
            int i = GetPrivateProfileString(Section,Key,Default,temp, 
                                            2000, this.mPath);
            if (mbWriteAfterRead )
                WritePrivateProfileString(Section, Key, temp.ToString(), this.mPath);

            return temp.ToString();
        }

        public int IniReadValue(string Section, string Key, int Default = 0)
        {
            string s = IniReadValue(Section, Key, Default.ToString());
            return Convert.ToInt32(s);
        }
    }
}
