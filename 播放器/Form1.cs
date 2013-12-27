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
    public partial class Form1 : Form
    {
        public List<string> playList = new List<string>();
        List<string> lrcList = new List<string>();
        public int selectTemp
        {
            get;
            set;
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)//打开按钮功能，添加歌曲文件到播放列表
        {
            lbList.Items.Clear();
            playList.Clear();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "MP3文件（*.mp3）|*.mp3";
            ofd.Title = "打开音乐文件";
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                myPlayer.URL = ofd.FileNames[0];
                for (int i = 0; i < ofd.FileNames.Length; i++)
                {
                    string str = ofd.FileNames[i].Substring(ofd.FileNames[i].LastIndexOf("\\") + 1);
                    lbList.Items.Add(str);
                }
                playList.AddRange(ofd.FileNames);
                lbList.SelectedIndex = 0;
            }
            btnPlayPause.Enabled = true;
            btnStop.Enabled = true;
            btnNext.Enabled = true;
            btnBack.Enabled = true;
        }

        private void btnPlayPause_Click(object sender, EventArgs e)//播放和暂停按钮
        {
            if (btnPlayPause.Tag.ToString() == "pause")
            {
                lrcList = createLrc();
                myPlayer.Ctlcontrols.play();
                btnPlayPause.Tag = "play";
            }
            else if (btnPlayPause.Tag.ToString() == "play")
            {
                myPlayer.Ctlcontrols.pause();
                btnPlayPause.Tag = "pause";
            }
        }

        private void Form1_Load(object sender, EventArgs e)//窗体载入，取消自动播放
        {
            myPlayer.settings.autoStart = false;
            tbVolume.Value = myPlayer.settings.volume;
        }

        private void btnStop_Click(object sender, EventArgs e)//停止播放按钮
        {
            myPlayer.Ctlcontrols.stop();
            btnPlayPause.Tag = "pause";
        }

        private void lbList_MouseDoubleClick(object sender, MouseEventArgs e)//双击列表播放音乐
        {
            myPlayer.URL = playList[lbList.SelectedIndex];
            lrcList = createLrc();
            myPlayer.Ctlcontrols.play();
            btnPlayPause.Tag = "play";
            txtLrc.Text = myPlayer.currentMedia.name;
        }

        private void btnBack_Click(object sender, EventArgs e)//上一曲按钮
        {
            nextBack(btnBack.Text);
            lrcList = createLrc();
            timer1.Enabled = true;
            myPlayer.Ctlcontrols.play();
            txtLrc.Text = myPlayer.currentMedia.name;
        }

        private void nextBack(string select)//下一曲和上一曲的功能函数
        {
            txtMessage.Text = "";
            int t = getIndex();
            if (select == "上一曲")
            {
                if (t == 0)
                {
                    t = lbList.Items.Count - 1;
                    myPlayer.URL = playList[t];
                }
                else
                {
                    myPlayer.URL = playList[--t];
                }
            }
            else if (select == "下一曲")
            {
                if (t == lbList.Items.Count - 1)
                {
                    t = 0;
                    myPlayer.URL = playList[t];
                }
                else
                {
                    myPlayer.URL = playList[++t];
                }
            }

            lbList.SelectedIndex = t;
        }

        private void btnNext_Click(object sender, EventArgs e)//下一曲按钮
        {
            nextBack(btnNext.Text);
            lrcList = createLrc();
            timer1.Enabled = true;
            myPlayer.Ctlcontrols.play();
            txtLrc.Text = myPlayer.currentMedia.name;
        }

        private void myPlayer_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)//自动下一曲
        {
            if (myPlayer.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                int t = getIndex();
                if (playList.Count == 1 || playList.Count == t + 1)
                {
                    return;
                }
                myPlayer.URL = playList[++t];
                lbList.SelectedIndex = t;
                myPlayer.Tag = 1;
                txtMessage.Text = "";
                txtLrc.Text = "";
            }
            try
            {
                if (myPlayer.playState == WMPLib.WMPPlayState.wmppsReady && myPlayer.Tag.ToString() == "1")
                {
                    lrcList = createLrc();
                    myPlayer.Ctlcontrols.play();
                    timer1.Start();
                }
            }
            catch (Exception)
            {

            }

        }

        private int getIndex()//获得当前播放音乐的索引
        {
            int t = -1;
            for (int i = 0; i < lbList.Items.Count; i++)
            {
                if (lbList.Items[i].ToString() == myPlayer.URL.Substring(myPlayer.URL.LastIndexOf("\\") + 1))
                {
                    t = i;
                    break;
                }
            }
            return t;
        }

        private void timer1_Tick(object sender, EventArgs e)//计时器
        {
            if (myPlayer.currentMedia != null)
            {
                //txtMessage.Text = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}",
                //    myPlayer.currentMedia.name,
                //    //myPlayer.currentMedia.sourceURL,
                //    myPlayer.currentMedia.duration,
                //    myPlayer.currentMedia.durationString,
                //    myPlayer.settings.mute,
                //    myPlayer.settings.volume,
                //    myPlayer.Ctlcontrols.currentPosition,
                //    myPlayer.Ctlcontrols.currentPositionString
                //    );
                txtMessage.Text = string.Format("歌名：{0}\r\n总长：{1}\r\n进度：{2}",
                    myPlayer.currentMedia.name,
                    myPlayer.currentMedia.durationString,
                    myPlayer.Ctlcontrols.currentPositionString
                    );
            }
            showLrc();
        }

        private List<string> createLrc()//生成歌词到一个集合中方法，并返回一个集合
        {
            string path = myPlayer.currentMedia.sourceURL;
            string lrcPath = path.Substring(0, path.LastIndexOf('.')) + ".lrc";
            List<string> tempList = new List<string>();
            if (File.Exists(lrcPath))
            {
                lrcList.AddRange(File.ReadAllLines(lrcPath, Encoding.Default));
                for (int i = 0; i < lrcList.Count; i++)
                {
                    string[] str = lrcList[i].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    if (str.Length > 1)
                    {
                        for (int j = 0; j < str.Length - 1; j++)
                        {
                            tempList.Add(str[j] + '|' + str[str.Length - 1]);
                        }
                    }
                }
                for (int i = 0; i < tempList.Count; i++)
                {
                    for (int j = 0; j < tempList.Count - i - 1; j++)
                    {
                        if (string.Compare(tempList[j].Split('|')[0], tempList[j + 1].Split('|')[0]) > 0)
                        {
                            string t;
                            t = tempList[j];
                            tempList[j] = tempList[j + 1];
                            tempList[j + 1] = t;
                        }
                    }
                }
            }
            return tempList;
        }

        private void showLrc()//显示歌词方法
        {
            if (lrcList.Count > 0 && myPlayer.currentMedia != null)
            {
                string nowTime = myPlayer.Ctlcontrols.currentPositionString;

                for (int i = 0; i < lrcList.Count - 1; i++)
                {
                    if (string.Compare(lrcList[i], nowTime) < 0 && string.Compare(nowTime, lrcList[i + 1]) < 0)
                    {
                        txtLrc.Text = lrcList[i].Split('|')[1];
                    }
                }

            }
            else
            {
                txtLrc.Text = "未找到歌词";
            }
        }

        private void btnDel_Click(object sender, EventArgs e)//调用删除对话框
        {

            selectTemp = lbList.SelectedIndex;
            confirmDel f2 = new confirmDel();
            f2.Owner = this;  //设置附属，好搭关系~
            f2.ShowDialog();
        }

        private void btnFind_Click(object sender, EventArgs e)//显示搜索框
        {
            txtSongName.Visible = true;
            lbHide.Visible = true;
            txtSongName.Focus();
        }

        private void lbHide_MouseClick(object sender, MouseEventArgs e)//关闭搜索框
        {
            txtSongName.Visible = false;
            lbHide.Visible = false;
            txtSongName.Text = "";
        }

        private void tbSongName_TextChanged(object sender, EventArgs e)//模糊查找音乐事件
        {
           var tlist = lbList.Items;
            for (int i = 0; i < tlist.Count; i++)
            {
                if (tlist[i].ToString().ToUpper().Contains(txtSongName.Text.ToUpper()))
                {
                    lbList.SelectedIndex = i;
                    return;
                }
                
            }
        }

        private void tbVolume_Scroll(object sender, EventArgs e)//控制音量
        {
            myPlayer.settings.volume = tbVolume.Value;
        }

        private void btnQuiet_Click(object sender, EventArgs e)//设置静音
        {
            myPlayer.settings.mute = myPlayer.settings.mute == false ? true : false;
        }

        private void btnAdd_Click(object sender, EventArgs e)//添加音乐
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "MP3文件（*.mp3）|*.mp3";
            ofd.Title = "打开音乐文件";
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for (int i = 0; i < ofd.FileNames.Length; i++)
                {
                    string str = ofd.FileNames[i].Substring(ofd.FileNames[i].LastIndexOf("\\") + 1);
                    if (lbList.Items.Contains(str))
                    {
                        break;
                    }
                    lbList.Items.Add(str);
                    playList.Add(ofd.FileNames[i]);
                }
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState==FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (this.WindowState==FormWindowState.Minimized)
            {
                this.Visible = true;
                //this.notifyIcon1.Visible = false;
                this.WindowState = FormWindowState.Normal;
            }
        }

    }
}
