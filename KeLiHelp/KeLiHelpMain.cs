using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Diagnostics;

namespace KeLiHelp
{
	public partial class KeLiHelpMain : Form
	{

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		// 导入Windows API中的函数和结构
		[DllImport("user32.dll")]
		static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

		[DllImport("user32.dll", EntryPoint = "SetParent")]
		public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);


		[StructLayout(LayoutKind.Sequential)]
		struct INPUT
		{
			public uint type;
			public MOUSEINPUT mi;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct MOUSEINPUT
		{
			public int dx;
			public int dy;
			public uint mouseData;
			public uint dwFlags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		/// <summary>
		/// 窗口大小
		/// </summary>
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		public static bool Hotkey_K;
		public static bool GameInfo;
		public static IntPtr GameHandle;
		public static RECT GameRect;


		/// <summary>
		/// 自动按钮 内的白色部分
		/// </summary>
		public static Color Play_White_Color_Default = Color.FromArgb(236, 229, 216);
		/// <summary>
		/// 自动按钮 内的灰色部分
		/// </summary>
		public static Color Play_Black_Color_Default = Color.FromArgb(59, 67, 84);
		/// <summary>
		/// 对话框泡泡颜色
		/// </summary>
		public static Color Dialog_Color_Default = Color.FromArgb(255, 255, 255);
		/// <summary>
		/// 对话框拾取颜色
		/// </summary>
		public static Color Gather_Color_Default = Color.FromArgb(255, 255, 255);

		SoundPlayer start = new SoundPlayer(Properties.Resources.可莉来帮忙);
		SoundPlayer end = new SoundPlayer(Properties.Resources.玩累了);
		SoundPlayer start1 = new SoundPlayer(Properties.Resources.哒哒哒);
		SoundPlayer end1 = new SoundPlayer(Properties.Resources.啦啦啦);

		SizePoint UserSet = new SizePoint();
		SizePoint cb1 = new SizePoint();
		SizePoint cb2 = new SizePoint();

		public static Label InfoTXT = new Label { Text = "可莉来帮忙:帮忙跳过对话中！", BackColor = Color.Red, Size = new Size(230, 18), Visible = true, Font = new Font("宋体", 12), ForeColor = Color.White };

		/// <summary>
		/// 窗口实例化
		/// </summary>
		public KeLiHelpMain()
		{
			InitializeComponent();
			start.Play();
			comboBox1.Items.Add("1920*1080 (16:9)");
			comboBox1.Items.Add("2560*1080 (21:9)");
			comboBox1.SelectedIndex = Properties.Settings.Default.Set;

			//1920*1080 (16:9)
			cb1.Play_White_Point = new Point(60, 48);
			cb1.Play_Black_Point = new Point(72, 48);
			cb1.Dialog_Point1 = new Point(1300, 800);
			cb1.Dialog_Point2 = new Point(1300, 785);
			cb1.Gather_Point1 = new Point(0, 0);
			cb1.Gather_Point2 = new Point(0, 0);

			//2560*1080 (21:9)
			cb2.Play_White_Point = new Point(205, 48);
			cb2.Play_Black_Point = new Point(215, 48);
			cb2.Dialog_Point1 = new Point(1730, 795);
			cb2.Dialog_Point2 = new Point(1730, 780);
			cb2.Gather_Point1 = new Point(1525, 540);
			cb2.Gather_Point2 = new Point(1550, 540);

			switch (comboBox1.SelectedIndex)
			{
				case 0: { UserSet = cb1; }; break;
				case 1: { UserSet = cb2; }; break;
			}
		}

		/// <summary>
		/// 点位与颜色 结构体
		/// </summary>
		public struct SizePoint
		{
			/// <summary>
			/// 白色播放点位
			/// </summary>
			public Point Play_White_Point;
			/// <summary>
			/// 黑色播放点位
			/// </summary>
			public Point Play_Black_Point;
			/// <summary>
			/// 对话框泡泡位置1
			/// </summary>
			public Point Dialog_Point1;
			/// <summary>
			/// 对话框泡泡位置2
			/// </summary>
			public Point Dialog_Point2;
			/// <summary>
			/// 拾取点位1
			/// </summary>
			public Point Gather_Point1;
			/// <summary>
			/// 拾取点位2
			/// </summary>
			public Point Gather_Point2;
		}

		/// <summary>
		/// 检测线程并且进行点击
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void timer1_Tick(object sender, EventArgs e)
		{
			GameHandle = FindWindow("UnityWndClass", "原神");

			if (GameHandle != IntPtr.Zero)
			{
				label1.Text = "原神的句柄为" + GameHandle;
				GameInfo = true;
				SetParent(InfoTXT.Handle, GameHandle);
				GetWindowRect(GameHandle, out GameRect);
				InfoTXT.Location = new Point((GameRect.Right - GameRect.Left) / 2 - 115, (GameRect.Bottom - GameRect.Top) - 50);
			}
			else
			{
				label1.Text = "没有找到原神进程!";
			}
			GC.Collect();
		}

		/// <summary>
		/// 不断获取当前屏幕上的色点
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void timer2_Tick(object sender, EventArgs e)
		{
			//对话判断
			if (checkBox1.Checked == true)
			{
				//存在 自动播放按钮时
				if (GetColorAtPosition(UserSet.Play_White_Point, Play_White_Color_Default) && GetColorAtPosition(UserSet.Play_Black_Point, Play_Black_Color_Default))
				{
					MOUSEMain(UserSet.Dialog_Point1);
				}

				//存在 对话框选项时
				if (GetColorAtPosition(UserSet.Dialog_Point1, Dialog_Color_Default) && GetColorAtPosition(UserSet.Dialog_Point2, Dialog_Color_Default))
				{
					MOUSEMain(UserSet.Dialog_Point1);
				}
			}


			GC.Collect();
		}

		/// <summary>
		/// 判断当前是不是处于对话状态
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		static bool GetColorAtPosition(Point point, Color GamesC)
		{
			// 创建屏幕的截图
			Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
			using (Graphics graphics = Graphics.FromImage(screenshot))
			{
				graphics.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
			}

			// 获取指定位置的颜色值
			Color color = screenshot.GetPixel(point.X, point.Y);

			int cs = 10;
			int tmpr = Math.Abs(GamesC.R - color.R);
			int tmpg = Math.Abs(GamesC.G - color.G);
			int tmpb = Math.Abs(GamesC.B - color.B);

			//色差RGB只要有一个大于色差，为假
			if ((tmpr > cs) || (tmpg > cs) || (tmpb > cs))
			{
				return false;
			}
			return true;
		}


		// 鼠标点击事件的常量
		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;
		private const int MOUSEEVENTF_MOVE = 0x0001;
		private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

		/// <summary>
		/// 移动鼠标并且点击
		/// </summary>
		static async void MOUSEMain(Point point)
		{
			MoveMouseToPosition(point.X, point.Y);
			await Task.Delay(100);
			LeftClick();
		}

		/// <summary>
		/// 移动鼠标
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		static void MoveMouseToPosition(int x, int y)
		{
			// 计算归一化的坐标值
			int screenX = (int)(x * 65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);
			int screenY = (int)(y * 65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);

			// 创建INPUT结构并设置鼠标移动消息
			INPUT input = new INPUT();
			input.type = 0; // INPUT_MOUSE
			input.mi.dx = screenX;
			input.mi.dy = screenY;
			input.mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;

			// 发送鼠标移动消息
			SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
		}

		/// <summary>
		/// 发送鼠标点击消息
		/// </summary>
		static void LeftClick()
		{
			// 创建INPUT结构并设置鼠标左键按下和释放消息
			INPUT[] inputs = new INPUT[2];

			// 鼠标左键按下消息
			inputs[0].type = 0; // INPUT_MOUSE
			inputs[0].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;

			// 鼠标左键释放消息
			inputs[1].type = 0; // INPUT_MOUSE
			inputs[1].mi.dwFlags = MOUSEEVENTF_LEFTUP;
			// 发送鼠标点击消息
			SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (!timer1.Enabled && !timer2.Enabled)
			{
				timer1.Start();
				timer2.Start();
				start1.Play();
				button1.Text = "暂停【键盘快捷键 K 】";
				InfoTXT.Visible = true;
				InfoTXT.Show();
			}
			else
			{
				timer1.Stop();
				timer2.Stop();
				end1.Play();
				button1.Text = "开始【键盘快捷键 K 】";
				InfoTXT.Visible = false;
				InfoTXT.Hide();
			}
		}


		/// <summary>
		/// 关闭前
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void KeLiHelpMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			end.Play();
			DialogResult dr = MessageBox.Show("不需要帮忙了吗？", "可莉来帮忙", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
			if (dr != DialogResult.OK)
			{
				e.Cancel = true;
				stopListen();
			}
		}


		/// <summary>
		/// 窗体第一次加载
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void KeLiHelpMain_Load(object sender, EventArgs e)
		{
			startListen();
		}


		private static void hook_KeyDown(object sender, KeyEventArgs e)
		{
			//  这里写具体实现
			if (e.KeyCode == Keys.K)
			{
				Hotkey_K = true;
			}
		}

		KeyboardHook k_hook = new KeyboardHook();
		KeyEventHandler myKeyEventHandeler = new KeyEventHandler(hook_KeyDown);

		/// <summary>
		/// 开始监听
		/// </summary>
		public void startListen()
		{
			k_hook.KeyDownEvent += myKeyEventHandeler;//钩住键按下
			k_hook.Start();//安装键盘钩子
		}

		/// <summary>
		/// 结束监听
		/// </summary>
		public void stopListen()
		{
			if (myKeyEventHandeler != null)
			{
				k_hook.KeyDownEvent -= myKeyEventHandeler;//取消按键事件
				myKeyEventHandeler = null;
				k_hook.Stop();//关闭键盘钩子
			}
		}

		private void KeLiHelpMain_KeyDown(object sender, KeyEventArgs e)
		{

		}

		/// <summary>
		/// 检测当前按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void timer3_Tick(object sender, EventArgs e)
		{
			if (Hotkey_K == true)
			{
				button1_Click(null, e);
				Hotkey_K = false;
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.Set = comboBox1.SelectedIndex;
			Properties.Settings.Default.Save();
		}


		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{

		}

	}
}
