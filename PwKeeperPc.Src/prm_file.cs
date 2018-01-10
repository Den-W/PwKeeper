using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CPrm
{
    /// <summary>
    /// Create a New PARAM file to store or load data
    /// Section identified 
    /// </summary>
    public class CPrmFile
    {
        public bool     mbWriteAfterRead;
        public string   mPath;
        public string   mSecDivName;
        public int      mNameWidth;
        
        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public CPrmFile(string INIPath, string SectionDividerName, int NameWidth = 12 )
        {
            mNameWidth = NameWidth;
            mbWriteAfterRead = true;
            mSecDivName = SectionDividerName;

            if (INIPath.IndexOf('\\') > 0) mPath = INIPath;
            else
            {
                if (INIPath.Length == 0) INIPath = "Params";
                mPath =  System.Reflection.Assembly.GetEntryAssembly().Location;
                int i = mPath.LastIndexOf('\\');
                if (i>0) i++;
                mPath = mPath.Substring(0, i) + INIPath;
            }
        }

        public string GetSection( int n, string SecName )
        {
            try
            {

                string[] sa = File.ReadAllLines(mPath, Encoding.GetEncoding("Windows-1251"));

                if (SecName.Length == 0) SecName = mSecDivName;

                foreach (string s in sa)
                {
                    string ts = s.TrimStart(' ', '\t');
                    if (!ts.StartsWith(SecName)) continue;
                    if (--n < 0)
                        return PrmVal(ts);
                }
            }
            catch ( FileNotFoundException )
            {

                MessageBox.Show(mPath, "File not found",                    
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                Application.Exit();
            }

            return "";
        }

        /// <summary>
        /// Write Data to the Param File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void PutVal(string Section,string Key,string Value)
        {
            Io(Section, Key, Value, true);            
        }
        
        /// <summary>
        /// Read Data Value From the Param File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string GetVal(string Section,string Key, string Default = "" )
        {
            return Io(Section, Key, Default, false );
        }

        public int GetVal(string Section, string Key, int Default = 0)
        {
            string s = GetVal(Section, Key, Default.ToString());
            s = Regex.Match( s, "\\d+" ).Value;
            if (s.Length <= 0) return Default;
            return Convert.ToInt32(s);
        }

        public string PrmVal(string Src)
        {   int i = Src.IndexOf('=');
            if (i++ <= 0) return "";
            Src = Src.Substring(i).TrimStart('\t', ' ');
            i = Src.IndexOf(';');
            if (i < 0) i = Src.IndexOf('#');
            if (i >= 0) Src = Src.Substring(0, i);
            return Src.TrimEnd('\t', ' ');
        }

        private string Io( string Section, string Key, string Val, bool bSet)
        {
            try
            {
                bool bWasPrm = false;
                string[] sa = File.ReadAllLines(mPath, Encoding.GetEncoding("Windows-1251"));
                List<string> sl = new List<string>(sa);

                int bFnd = 0, n = 0;
                if (Section.Length == 0) bFnd = 1;

                foreach (string li in sl)
                {
                    n++;
                    string ts = li.TrimStart(' ', '\t');
                    if (ts.StartsWith(mSecDivName))
                    {
                        if (bFnd == 0)
                        {
                            if (!PrmVal(ts).Equals(Section)) continue;
                            bFnd++;
                            continue;
                        }
                        // One more section begins. So stop now;       
                        if (n > 0) n--;
                        break;
                    }

                    if (bFnd < 1) continue;

                    if (ts.StartsWith(Key))
                    {
                        if (!bSet)
                            return PrmVal(ts);

                        bSet = mbWriteAfterRead;
                        int i = ts.IndexOf(';');
                        if (i < 0) i = ts.IndexOf('#');
                        if (i > 0) Val = Val + ts.Substring(i);
                        sl.RemoveAt(--n);
                        bWasPrm = true;
                        break;
                    }
                }

                if (bSet)
                {
                    if (!bWasPrm)
                    {
                        while (n > 1)
                        {
                            string sp = sl[n - 1].TrimStart(' ', '\t');
                            if (sp.Length == 0 || sp.StartsWith(";"))
                            {
                                n--;
                                continue;
                            }
                            break;
                        }
                    }

                    if (bFnd == 0)
                        sl.Insert(n++, mSecDivName.PadRight(mNameWidth) + " = " + Section);

                    sl.Insert(n, Key.PadRight(mNameWidth) + " = " + Val);

                    File.WriteAllLines(mPath, sl, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch (FileNotFoundException )
            {
            }
            return Val;
        }
    }
}
