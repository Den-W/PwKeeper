using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Reflection;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace PwKeeperPC
{
    public delegate void dProcAsw( int What, string Asw ); // What:toStatus

    public partial class Form1 : Form
    {
        enum ePhase
        {
            eRST,       // Restart            
            eIDLE,      // Wait for command
            eREAD,      // Read data
            eWRITE,     // Write data
            ePWDWAIT,   // Password mismatch. Wait for confirm
            ePWDOWR,    // Overwrite Password
        };

        public int          mClickTick = 0;
        int                 mStatusTicks = 0;
        int                 mLstIdx = -1;
        bool                mbStop = false;
        bool                mbIgnoreChange = false;        
        volatile ePhase     mPhase = ePhase.eRST;
        string              mFilename = "PwKeeper.pwk";
        string              mPasswUD = "";
        byte   []           mDataBin = new byte[2000];
        static SerialPort   mCom;
        Thread              mThread;
        dProcAsw            mProcAsw;
        CPrm.CPrmFile       mPrm = new CPrm.CPrmFile( "PwKeeper", "PwKeeper");

        public Form1()
        {
            InitializeComponent();

            mProcAsw = new dProcAsw(ProcAsw);

            //Extensions.SetStl(btUp, ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            //btUp.DoubleClick += new EventHandler(btUp_DoubleClick);
            mClickTick = Environment.TickCount;
            GenPwdImg();
            mCom = new SerialPort();
            mThread = new Thread(new ThreadStart(thrRun));
            mThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mbStop = true;
            Thread.Sleep(500);
            mThread.Abort();
        }

        private void btAbout_Click(object sender, EventArgs e)
        {
            Form3 F3 = new Form3();
            F3.Show();
            F3.Top = Top + (Height - F3.Height) / 2;
            F3.Left = Left + (Width - F3.Width) / 2;
            F3.Activate();
        }

        private void tbPwd_TextChanged(object sender, EventArgs e)
        {
            GenPwdImg();
        }
       
        private void Rec_TextChanged(object sender, EventArgs e)
        {
            if (mLstIdx < 0) return;
            if (mbIgnoreChange == true) return;
            mbIgnoreChange = true;
            mLst.BeginUpdate();
            mLst.Items[mLstIdx].Text = tbDispName.Text;
            mLst.Items[mLstIdx].SubItems[1].Text = tbTxName.Text;
            mLst.Items[mLstIdx].SubItems[2].Text = tbTxPwd.Text;

            string s = "";
            if (tbV1.Text.Length > 0) s = s + "V1:" + tbV1.Text + ", ";
            if (tbV2.Text.Length > 0) s = s + "V2:" + tbV2.Text + ", ";
            if (tbV3.Text.Length > 0) s = s + "V3:" + tbV3.Text + ", ";
            if (tbV4.Text.Length > 0) s = s + "V4:" + tbV4.Text + ", ";
            if (tbV5.Text.Length > 0) s = s + "V5:" + tbV5.Text + ", ";
            if (tbV6.Text.Length > 0) s = s + "V6:" + tbV6.Text;
            mLst.Items[mLstIdx].SubItems[3].Text = s;
            mbIgnoreChange = false;
            mLst.EndUpdate();
            SyncData();
        }

        private void mLst_SelectedIndexChanged(object sender, EventArgs e)
        {   mLstIdx = -1;
            if (mLst.SelectedIndices.Count <= 0) return; 
            int i = mLst.SelectedIndices[0]; 
            if ( i < 0) return;

            mbIgnoreChange = true;
            mLstIdx = i;
            tbDispName.Text = mLst.Items[i].Text;
            tbTxName.Text = mLst.Items[i].SubItems[1].Text;
            tbTxPwd.Text = mLst.Items[i].SubItems[2].Text;
            tbV1.Text = "";
            tbV2.Text = "";
            tbV3.Text = "";
            tbV4.Text = "";
            tbV5.Text = "";
            tbV6.Text = "";
            
            foreach (string z in mLst.Items[i].SubItems[3].Text.Split(','))
                if (z.Trim().Length > 0)
                {   string v = z.Trim();
                    switch (v.Substring(0, 2))
                    {
                        case "V1":
                            tbV1.Text = v.Substring(3);
                            break;
                        case "V2":
                            tbV2.Text = v.Substring(3);
                            break;
                        case "V3":
                            tbV3.Text = v.Substring(3);
                            break;
                        case "V4":
                            tbV4.Text = v.Substring(3);
                            break;
                        case "V5":
                            tbV5.Text = v.Substring(3);
                            break;
                        case "V6":
                            tbV6.Text = v.Substring(3);
                            break;
                    }
                }
            mbIgnoreChange = false;
        }

        private void MoveSelectedItem( System.Windows.Forms.ListView lv, int idx, bool moveUp)
        {
            // Gotta have >1 item in order to move
            if(lv.Items.Count < 1)  return;
            
            int offset = 0;
            if (idx >= 0)
            {
                    if (moveUp) offset = -1;                    
                    else
                    {
                        // ignore movedown of last item
                        if (idx < (lv.Items.Count - 1)) 
                            offset = 1;
                    }
            }

            if (offset == 0) return;

            mbIgnoreChange = true;
            lv.BeginUpdate();

            int selitem = idx + offset;
            for (int i = 0; i < lv.Items[idx].SubItems.Count; i++)
            {
                string cache = lv.Items[selitem].SubItems[i].Text;
                lv.Items[selitem].SubItems[i].Text = lv.Items[idx].SubItems[i].Text;
                lv.Items[idx].SubItems[i].Text = cache;
            }

            lv.Focus();
            lv.Items[idx].Selected = false;
            lv.Items[selitem].Selected = true;
            lv.EnsureVisible(selitem);
            mbIgnoreChange = false;
            lv.EndUpdate();
            SyncData();
        }

        private void btRecUp_Click(object sender, EventArgs e)
        {   if (mLstIdx <= 0) return;
            MoveSelectedItem( mLst, mLstIdx, true );            
        }

        private void btRecDn_Click(object sender, EventArgs e)
        {
            if (mLstIdx <= 0) return;
            MoveSelectedItem( mLst, mLstIdx, false );            
        }

        private void AddLine( string NmDisp, string Nm, string Pw, string Val )
        {
            if( NmDisp.Length > 14 ) NmDisp = NmDisp.Substring(0, 14);
            mLst.Items.Add(NmDisp, NmDisp, 0);                // Add row
            mLst.Items[NmDisp].UseItemStyleForSubItems = true;

            ListViewItem.ListViewSubItem sb = new ListViewItem.ListViewSubItem();
            sb.Name = "Name";
            sb.Text = Nm;
            mLst.Items[NmDisp].SubItems.Add(sb);

            sb = new ListViewItem.ListViewSubItem();
            sb.Name = "Pass";
            sb.Text = Pw;
            mLst.Items[NmDisp].SubItems.Add(sb);

            sb = new ListViewItem.ListViewSubItem();
            sb.Name = "Val";
            sb.Text = Val;
            mLst.Items[NmDisp].SubItems.Add(sb);
        }

        private void btRecNew_Click(object sender, EventArgs e)
        {
            string Nm = ResPw.sNew; // NewCite
            for (int i = 1; i < 100; i++)
            {
                Nm = ResPw.sNew + i.ToString("d2");
                if (!mLst.Items.ContainsKey(Nm))
                    break;
            }

            AddLine(Nm, Nm + "\\t", "NewPass\\r", "V1:NewVal\\r");
            SyncData();
        }

        private void btRecDel_Click(object sender, EventArgs e)
        {
            if (mLstIdx < 0) return;
            mbIgnoreChange = true;
            mLst.Items[mLstIdx].Remove();
            mLstIdx = -1;                        
            tbDispName.Text = "";
            tbTxName.Text = "";
            tbTxPwd.Text = "";
            tbV1.Text = "";
            tbV2.Text = "";
            tbV3.Text = "";
            tbV4.Text = "";
            tbV5.Text = "";
            tbV6.Text = "";
            mbIgnoreChange = false;
            SyncData();
        }

        private void btLoad_Click(object sender, EventArgs e)
        {   
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result != DialogResult.OK) return;// Test result.

            mFilename = openFileDialog1.FileName;
            mPrm = new CPrm.CPrmFile(openFileDialog1.FileName, "Site");
            tbPwd.Text = mPrm.GetSection(0, "PwKeeperPwd");
            mLst.Items.Clear();
            for(int i = 0; i < 64; i++)
            {
                string sv, sp = "", s = mPrm.GetSection(i, "Site" );
                if (s.Length == 0) continue;
                
                for (int v = 1; v < 7; v++)
                {
                    string vn = "V" + v.ToString();
                    sv = mPrm.GetVal( s, vn, "" );
                    if (sv.Length > 0) sp = sp + vn + ":" + sv + ", ";
                }

                AddLine(s, mPrm.GetSection(i, "Name"), mPrm.GetSection(i, "Pass"), sp);
            }
            SyncData();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = mFilename;
            DialogResult result = saveFileDialog1.ShowDialog(); // Show the dialog.
            if (result != DialogResult.OK) return;// Test result.

            mPrm.mPath = saveFileDialog1.FileName;
            int n = mLst.Items.Count;

            string s = "PwKeeperPwd: " + tbPwd.Text + "\r\n";

            for (int i = 0; i < mLst.Items.Count; i++)
            {
                s = s + "\r\nSite = " + mLst.Items[i].Text + "\r\n";
                s = s + "Name = " + mLst.Items[i].SubItems[1].Text + "\r\n";
                s = s + "Passw = " + mLst.Items[i].SubItems[2].Text + "\r\n";

                foreach (string z in mLst.Items[i].SubItems[3].Text.Split(','))
                    if (z.Trim().Length > 0)
                    {   string v = z.Trim();
                        s = s + v.Substring(0, 2) + " = " + v.Substring(3) + "\r\n";
                    }
            }
            System.IO.File.WriteAllText(saveFileDialog1.FileName, s, Encoding.GetEncoding("Windows-1251"));            
        }

        private void btRead_Click(object sender, EventArgs e)
        {
            if (mPhase != ePhase.eIDLE) return;
            mPhase = ePhase.eREAD;
            btStop.Enabled = true;
        }

        private void btWrite_Click(object sender, EventArgs e)
        {
            if (mPhase != ePhase.eIDLE) return;
            SyncData();
            mPhase = ePhase.eWRITE;
            btStop.Enabled = true;
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            if (mPhase != ePhase.eIDLE) mPhase = ePhase.eRST;
            mProcAsw(0, "");
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            if (mPhase == ePhase.ePWDWAIT) mPhase = ePhase.ePWDOWR;
        }

        private void btUp_Click(object sender, EventArgs e)
        {
            int n = Environment.TickCount - mClickTick;
            if (n > 300) tbPwd.Text = tbPwd.Text + "u";
            else
            {
                if (tbPwd.Text.Length != 0) tbPwd.Text = tbPwd.Text.Remove(tbPwd.Text.Length-1) + "U";
                else tbPwd.Text = tbPwd.Text + "U";
            }

            mClickTick = Environment.TickCount;        
        }
      
        private void btDn_Click(object sender, EventArgs e)
        {
            int n = Environment.TickCount - mClickTick;
            if (n > 300) tbPwd.Text = tbPwd.Text + "d";
            else
            {
                if (tbPwd.Text.Length != 0) tbPwd.Text = tbPwd.Text.Remove(tbPwd.Text.Length - 1) + "D";
                else tbPwd.Text = tbPwd.Text + "D";
            }
            mClickTick = Environment.TickCount;
        }

        void GenPwdImg()
        {
            int         ch, n, i = 0, pos = 0;                        
            Bitmap      bmp = new Bitmap( pbPwd.Width, pbPwd.Height );
            Graphics    bmpGraphics = Graphics.FromImage(bmp);
            Font        drawFont = new Font("Arial", 14);
            SolidBrush  drawBrush = new SolidBrush(Color.Black);

            mPasswUD = "";
            while (tbPwd.TextLength > 0)
            {
                n = 1;
                ch = tbPwd.Text[i];
                while (++i < tbPwd.TextLength && ch == tbPwd.Text[i]) 
                    if( ++n >= 15 ) break;

                int x = pos * (int)drawFont.SizeInPoints+1;
                bmpGraphics.DrawString(n.ToString("X"), drawFont, drawBrush, x-1, pbPwd.Height / 8);
                switch (ch)
                { case 'u':
                        n |= 0x20;
                        bmpGraphics.FillRectangle(drawBrush, new Rectangle(x, 2, (int)drawFont.SizeInPoints-2, 2) );
                        break;
                  case 'U':
                        n |= 0x30;
                        bmpGraphics.FillRectangle(drawBrush, new Rectangle(x, 0, (int)drawFont.SizeInPoints-2, 4));
                        break;
                  case 'd':
                        bmpGraphics.FillRectangle(drawBrush, new Rectangle(x, pbPwd.Height-2-pbPwd.Height / 8, (int)drawFont.SizeInPoints - 2, 2));
                        break;
                  case 'D':
                        n |= 0x10;
                        bmpGraphics.FillRectangle(drawBrush, new Rectangle(x, pbPwd.Height-2-pbPwd.Height / 8, (int)drawFont.SizeInPoints - 2, 4));
                        break;
                }
                mPasswUD = mPasswUD + Convert.ToChar(n | 0x40);
                pos++;
                if (i >= tbPwd.TextLength) break;
            }

            for (; pos < 20; pos++)
            {
                mPasswUD = mPasswUD + Convert.ToChar(0x40);
                bmpGraphics.DrawString("0", drawFont, drawBrush, pos * (int)drawFont.SizeInPoints, pbPwd.Height / 8);
            }
            
            pbPwd.Image = bmp;            
        }

        public void ProcAsw(int What, string Asw)
        {   if (InvokeRequired)
            {
                Invoke(new dProcAsw(ProcAsw), new object[] { What, Asw });
                return;
            }

            int i = 0;
            string s = "";

            switch (What)
            {
                case 0:
                  { Color c = Color.Black;
                    rtStatus.Text = "";
                    rtStatus.SelectionColor = c;
                    for( i=0; true; i++ )
                    {   if( i < Asw.Length )
                        {   if( Asw[i] != '#' )
                            {   s += Asw.Substring( i, 1 );
                                continue;
                            }
                            if( ++i < Asw.Length )
                                switch( Asw[i] )
                                { default: 
                                    c = Color.Black;
                                    break;
                                  case 'R':
                                    c = Color.Red;
                                    break;
                                  case 'B':
                                    c = Color.Blue;
                                    break;
                                  case 'G':
                                    c = Color.Green;
                                    break;
                                }
                        }

                        rtStatus.SelectionStart = rtStatus.TextLength;
                        rtStatus.SelectionLength = 0;
                        rtStatus.AppendText(s);
                        rtStatus.SelectionColor = c;
                        s = "";
                        if( i >= Asw.Length ) break;
                    }
                    break;
                  }

                case 1: // Disable buttons
                    btStop.Enabled = false;
                    btClear.Hide();
                    break;

                case 2: // Enable CLEAR button
                    btClear.Show();
                    break;

                case 3: // New Data from PwKeeper set in mDataBin
                    {   int     ph = 0;
                        byte[] b1 = new byte[1];
                        string  sd = "", sn = "", sp = "", sv = "";
                        mLst.Items.Clear();
                        for( i = 0; true; i++)
                        {   byte b = mDataBin[i];
                            
                            b1[0] = b;
                            String sc = Encoding.UTF8.GetString(Encoding.Convert(Encoding.GetEncoding("windows-1251"),Encoding.GetEncoding("utf-8"), b1,0,1));

                            if( b != 0 ) 
                            {
                              switch( b )
                              {
                                case 0x01: // Disp
                                    ph = 0;
                                    goto gNxt;                                    
                                case 0x02: // Name
                                    ph = 1;
                                    continue;
                                case 0x03: // Pass
                                    ph = 2;
                                    continue;
                                case 0x04:
                                    ph = 3;
                                    sv = "V1:";
                                    continue;
                                case 0x05:
                                    ph = 3;
                                    if (sv.Length > 0) sv = sv + ", ";
                                    sv = sv + "V2:";
                                    continue;
                                case 0x06:
                                    ph = 3;
                                    if (sv.Length > 0) sv = sv + ", ";
                                    sv = sv + "V3:";
                                    continue;
                                case 0x07:
                                    ph = 3;
                                    if (sv.Length > 0) sv = sv + ", ";
                                    sv = sv + "V4:";
                                    continue;
                                case 0x08:
                                    ph = 3;
                                    if (sv.Length > 0) sv = sv + ", ";
                                    sv = sv + "V5:";
                                    continue;
                                case 0x09:
                                    ph = 3;
                                    if (sv.Length > 0) sv = sv + ", ";
                                    sv = sv + "V6:";
                                    continue;                   
                              }

                              switch (ph)
                              {
                                  case 0:
                                      sd = sd + sc;
                                      continue;
                                  case 1:
                                      sn = sn + sc;
                                      continue;
                                  case 2:
                                      sp = sp + sc;
                                      continue;
                                  case 3:
                                      sv = sv + sc;
                                      continue;
                              }
                            }
                        gNxt:
                            if( sd.Length > 0 )
                                AddLine( sd, sn, sp, sv );
                            if (b == 0 || i >= 1000) break;
                            sd = sn = sp = sv = "";
                        }                                
                        SyncData();
                        break;                
                    }
            }
        }

        void SyncData()
        {   int i;
            for ( i = 0; i < 1000; i++) mDataBin[i] = 0;
            for ( i = 0; i < mLst.Items.Count; i++)
            {
                if (mLst.Items[i].Text.Length <= 0) continue;

                if (mLst.Items[i].Text.Length > 14) mLst.Items[i].Text = mLst.Items[i].Text.Substring(0, 14);
                AddStr( 0x01, mLst.Items[i].Text );
                if (mLst.Items[i].SubItems[1].Text.Length > 0) AddStr(0x02, mLst.Items[i].SubItems[1].Text );
                if (mLst.Items[i].SubItems[2].Text.Length > 0) AddStr(0x03, mLst.Items[i].SubItems[2].Text);                

                foreach (string z in mLst.Items[i].SubItems[3].Text.Split(','))
                    if (z.Trim().Length > 0)
                    {
                        string v = z.Trim();
                        switch (v.Substring(0, 2))
                        {
                            case "V1":
                                AddStr(0x04, v.Substring(3));
                                break;
                            case "V2":
                                AddStr(0x05, v.Substring(3));
                                break;
                            case "V3":
                                AddStr(0x06, v.Substring(3));
                                break;
                            case "V4":
                                AddStr(0x07, v.Substring(3));
                                break;
                            case "V5":
                                AddStr(0x08, v.Substring(3));
                                break;
                            case "V6":
                                AddStr(0x09, v.Substring(3));
                                break;
                        }
                    }
            }            
        }

        void AddStr( int Tag, string StrAdd )
        {
            int z;
            for (z = 0; z < 1000; z++) if (mDataBin[z] == 0) break;
            byte[] b = Encoding.Convert(Encoding.GetEncoding("utf-8"), Encoding.GetEncoding("windows-1251"), Encoding.UTF8.GetBytes(StrAdd));
            
            mDataBin[z++] = Convert.ToByte(Tag);
            for (int i = 0; i < b.Length; i++)
            {                   
                byte    c = b[i];
                /*
                if( c == '\\' )
                {
                    if (++i < b.Length)
                    {
                        c = b[i];                        
                        switch (c)
                        { case 0x74: // \t == Tab
                                c = 0x89;
                                break;
                          case 0x72: // \r
                                c = 0x8D;
                                break;
                          case 0x6E: // \n
                                c = 0x8A;
                                break;
                        }
                    }
                }*/
                mDataBin[z++] = c;                
            }            
        }
        
        // Packet: <Type(AN1)><Seq0x40|0x3F(AN1)><Data(AN?)><Crc8_0x40|0x3F(AN1)><\n>
        // Type:    P - Enter password
        //          S - Set new password & clear data
        //          W - write packet. Each appends to mData
        //          R - Read data
        //          F - Save data to EEPROM.
        // Asw:     X - Answer on unrecognized PKT
        //          C - Bad CRC        
        //          L - OutOfSequence
        //          p,s,w,r - All ok. Cmd repeated or data added
        string UsbPktRx( int To )
        {
            byte    crc = 0;
            byte[]  Pkt = new byte[4096];
            int     i;
            int     PktSz = 0;

            mCom.ReadTimeout = To * 1000;
            mCom.WriteTimeout = To * 1000;
            
            try
            {
                while (true)
                {   int ch = mCom.ReadByte();
                    if( ch == '\n' ) break;
                    Pkt[PktSz++] = Convert.ToByte(ch);
                }

                if(PktSz < 3) return null;

                for (i = 0; i < PktSz - 1; i++)
                    crc = crc8(crc, Pkt[i]);
                
                i = (Pkt[PktSz - 1] & 0x3F);
                if (i != (Convert.ToInt32(crc) & 0x3F) || PktSz < 2 )
                    return null;

                PktSz--;
                if (Pkt[0] == 'r')
                    for (i = 0; i < PktSz-1; i++) mDataBin[i] = Pkt[i+2];

                return Encoding.UTF8.GetString( Pkt, 0, PktSz );
            }
            catch (TimeoutException )
            {
            }
            return null;
        }

        string UsbPktTx(char Cmd, int No, string Data, int To)
        {
            byte[] bf = Encoding.UTF8.GetBytes(Data);
            return UsbPktTxBin(Cmd, No, bf, To);
        }
        
        string UsbPktTxBin( char Cmd, int No, byte[] Txd, int To )
        {
            int     i, n;
            byte    crc = 0;
            byte[]  bf = new byte[2000];

            bf[0] = Convert.ToByte(Cmd);
            bf[1] = Convert.ToByte(No|0x40);
            for (n = 0; n < Txd.Length; n++) bf[2 + n] = Txd[n];

            No = 0;
            for ( i = 0; i < Txd.Length+2; i++)
                crc = crc8( crc, bf[i] );

            bf[i++] = Convert.ToByte(0x40 | (crc & 0x3F));
            bf[i++] = Convert.ToByte(0x0A);
            
            mCom.Write( bf, 0, i );
            Thread.Sleep(1000);
            return UsbPktRx(To);	
        }
        
        byte crc8(byte crc, int ch)
        {
            ch &= 0xFF;
            for (int i = 8; i!=0; i--)
            {
                int mix = crc ^ ch;
                crc >>= 1;
                if ((mix & 0x01)!=0) crc ^= 0x8C;
                ch >>= 1;
            }
            return crc;
        }

        //---------------------------------------------------------------
        public void thrRun()
        {   string      s;
            int         i, n;

            while (!mbStop)
            {
                try
                {
                    switch (mPhase)
                    {
                        case ePhase.eRST:
                            mProcAsw(1, "");
                            mCom.Close();
                            mPhase = ePhase.eIDLE;
                            mStatusTicks = Environment.TickCount;
                            continue;

                        case ePhase.eIDLE:
                            Thread.Sleep(100);
                            i = Environment.TickCount - mStatusTicks;
                            if (i > 5000)
                            {
                                mProcAsw(0, "");
                                mStatusTicks = Environment.TickCount;
                            }
                            continue;

                        case ePhase.eREAD:
                            if (thrFindCom() <= 0) break;

                            mProcAsw(0, "#G" + mCom.PortName + ": #B" + ResPw.sPwLogin);
                            s = UsbPktTx('P', 1, mPasswUD, 15);
                            if (s.Substring(0, 2) != "pA")
                            {
                                mProcAsw(0, "#G" + mCom.PortName + ": #R" + ResPw.sPwBad);
                                break;
                            }
                            mCom.Write("\n");
                            s = UsbPktRx(1);
                            s = UsbPktTx('R', 2, "0",10);
                            if (s.Substring(0, 2) != "rB")
                            {
                                mProcAsw(0, "#G" + mCom.PortName + ": #R" + ResPw.sPwReadFail);
                                break;
                            }
                            mProcAsw(0, "#G" + mCom.PortName + ": #B" + ResPw.sPwReadOk);                            
                            mProcAsw(3, s);
                            break;

                        case ePhase.eWRITE:
                            if (thrFindCom() <= 0) break;

                            mProcAsw(0, "#G" + mCom.PortName + ": #B" + ResPw.sPwLogin);
                            mCom.Write("\n");
                            s = UsbPktRx(1);
                            s = UsbPktTx('P', 3, mPasswUD, 15);
                            if (s.Substring(0, 2) != "pC")
                            {
                                mPhase = ePhase.ePWDWAIT;
                                mProcAsw(2, ""); // Unblock btCLEAR
                                continue;
                            }
                            thrWrData();
                            break;

                        case ePhase.ePWDWAIT:
                            mProcAsw(0, "");
                            Thread.Sleep(100);
                            mProcAsw(0, "#G" + mCom.PortName + ": #R" + ResPw.sPwNew);
                            Thread.Sleep(200);
                            continue;

                        case ePhase.ePWDOWR:
                            mProcAsw(0, "#G" + mCom.PortName + ": #B" + ResPw.sPwNewDo);                            
                            mCom.Write("\n");
                            s = UsbPktRx(1);
                            s = UsbPktTx('S', 4, mPasswUD, 15);
                            if (s.Substring(0, 2) != "sD") break;

                            thrWrData();
                            break;
                    }                    
                }
                catch (Exception e)
                {
                    mProcAsw(0, "#ROperation failed! " + e.Message );
                }
                mPhase = ePhase.eRST;
            }
        }

        int  thrFindCom()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                mProcAsw(0, "#R" + ResPw.sNoCom);
                return 0;
            }
             
            for (int i = ports.Length - 1; i >= 0; i--)
            {
                try
                {
                    if (mPhase == ePhase.eRST)
                    {
                        mProcAsw(0, "#RStopped!");
                        return 0;
                    }

                    mProcAsw(0, "#B" + ports[i] + "#G: Scan..." );
                    mCom.Close();
                    mCom.PortName = ports[i];
                    mCom.BaudRate = 115200;
                    mCom.DataBits = 8;
                    mCom.Parity = Parity.None;
                    mCom.StopBits = StopBits.One;
                    mCom.Handshake = Handshake.None;
                    mCom.DtrEnable = true;
                    mCom.RtsEnable = true;
                    mCom.ReadTimeout = 1000;
                    mCom.WriteTimeout = 1000;
                    mCom.Open();

                    for ( int n = 0; n < 2; n++)
                    {
                        mCom.Write("\n");
                        string s = UsbPktRx(1);
                        if (s == null) continue;
                        if (s.IndexOf("PwKp") > 0) 
                            return 1;     // Found alive PwKeeper
                    }
                }
                catch (UnauthorizedAccessException ex)
                {   mProcAsw(0, ports[i] + ": #R " + ex.Message );                    
                }
            }
            mProcAsw(0, "#R" + ResPw.sNoPw);
            return 0;
        }

        int  thrWrData()
        {   byte[] buf = new byte[120];
            int i = 0;
            int n = 0;

            mProcAsw(0, "#G" + mCom.PortName + ": #B" + ResPw.sPwWr);            

            while (true)
            {
                byte c = mDataBin[i++];
                buf[n++] = c;
                if (c != 0 && n < 120 && i < 1000) continue;

                if (n < 120)
                    Array.Resize(ref buf, n);

                n = (i - 1) / 120;
                string s = UsbPktTxBin('W', n, buf, 15);
                if ( s == null || s.Substring(0, 1) != "w")
                {
                    mProcAsw(0, "#G" + mCom.PortName + ": #R" + ResPw.sPwWrBad);

                    return 0;
                }
                n = 0;
                if (c == 0 || i >= 1000) break;
            }
            mProcAsw(0, "#G" + mCom.PortName + ": #B" + ResPw.sPwWrOk);
            return 1;
        }
        
    }

    public static class Extensions
    {
        public static void SetStl(this Control control, ControlStyles flags, bool value)
        {
            Type type = control.GetType();
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo method = type.GetMethod("SetStyle", bindingFlags);
            if (method != null)
            {
                object[] param = { flags, value };
                method.Invoke(control, param);
            }
        }
    }       

}
