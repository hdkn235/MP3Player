using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace 播放器
{
    public partial class confirmDel : Form
    {
        public confirmDel()
        {
            InitializeComponent();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            int t = ((Form1)this.Owner).selectTemp;
            if (cbDel.Checked==true)
            {
                string path=((Form1)this.Owner).playList[t];
                File.Delete(path);
                ((Form1)this.Owner).playList.RemoveAt(t);
                ((Form1)this.Owner).lbList.Items.RemoveAt(t);
            }
            else
            {
                ((Form1)this.Owner).playList.RemoveAt(t);
                ((Form1)this.Owner).lbList.Items.RemoveAt(t);
            }
            this.Dispose();
            this.Close();

        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }
    }
}
