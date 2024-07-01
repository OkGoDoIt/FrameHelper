using System.Diagnostics;
using FrameHelper;

namespace FrameTester
{
	public partial class Form1 : Form
	{

		Frame frame = new Frame();
		public Form1()
		{
			InitializeComponent();
		}

		private async void StatusCheckTmr_Tick(object sender, EventArgs e)
		{
			connectedBox.Checked = frame.Connection.IsConnected;
			foundBox.Checked = !frame.Connection.IsSearching;
			pairedBox.Checked = frame.Connection.IsPaired;

			if (frame.Connection.IsConnected && frame.BatteryLevel.HasValue)
			{
				batteryBar.Value = (int)frame.BatteryLevel;
			}
			else if (frame.Connection.IsConnected)
			{
				Debug.WriteLine("Getting battery level...");
				float? level = await frame.GetBatteryLevelAsync();
				Debug.WriteLine($"Battery level: {level}");
				if (level.HasValue)
				{
					batteryBar.Value = (int)level;
				}
			}
		}

		private async void button1_Click(object sender, EventArgs e)
		{
			string toSend = textBox1.Text.Trim();
			if (toSend.Length > 0)
			{
				textBox2.Text = "...";
				string reply = await frame.SendLuaWithResponse(toSend);
				textBox2.Text = reply;
			}
		}

		private async void Form1_Load(object sender, EventArgs e)
		{
			frame.Connection.OnStatusChanged += Connection_OnStatusChanged;
		}

		private void Connection_OnStatusChanged(FrameBluetoothLEConnection sender, bool isConnected, bool isPaired, bool isFound)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => Connection_OnStatusChanged(sender, isConnected, isPaired, isFound)));
				return;
			}
			connectedBox.Checked = frame.Connection.IsConnected;
			foundBox.Checked = !frame.Connection.IsSearching;
			pairedBox.Checked = frame.Connection.IsPaired;

			if (frame.Connection.IsConnected && frame.BatteryLevel.HasValue)
			{
				batteryBar.Value = (int)frame.BatteryLevel;
			}
		}

		private void stayAwakeBox_CheckedChanged(object sender, EventArgs e)
		{
			frame.StayAwake = stayAwakeBox.Checked;
		}

		private async void saveBtn_Click(object sender, EventArgs e)
		{
			await frame.RunScriptViaFile(textBox1.Text.Trim());
		}

		private void button2_Click(object sender, EventArgs e)
		{
			frame.Sleep();
		}

		private async void button3_Click(object sender, EventArgs e)
		{
			string toSend = textBox1.Text.Trim();
			if (toSend.Length > 0)
			{
				textBox2.Text = "...";
				string reply = await frame.PrintShortLuaResponse(toSend);
				textBox2.Text = reply;
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			frame.Stop();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			frame.Reset();
		}
	}
}