using System.Diagnostics;
using FrameHelper;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
			frame.Connection.Connect();
		}

		private async void Connection_OnReceiveBytes(FrameBluetoothLEConnection sender, byte streamId, Stream stream)
		{
			Debug.WriteLine($"Received start of stream {streamId}");
			if (streamId < 20)
			{
				string soFar = "";
				while (true)
				{
					byte[] bytes = new byte[256];
					int bytesRead = await stream.ReadAsync(bytes, 0, bytes.Length);
					if (bytesRead == 0)
					{
						break;
					}
					string str = Encoding.UTF8.GetString(bytes, 0, bytesRead);
					Debug.WriteLine($"Received from stream {streamId}: {str}");
					soFar += str;
				}
				textBox2.Text = soFar;
			}
			if (streamId >= 20 && streamId < 30)
			{
				await Task.Run(() =>
				{
					Image img = Image.FromStream(stream);
					var tmp = Path.GetTempFileName() + ".jpg";
					img.Save(tmp);
					Process.Start(tmp);
				});
			}
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

		private async void button6_Click(object sender, EventArgs e)
		{
			string toSend = textBox1.Text.Trim();
			if (toSend.Length > 0)
			{
				textBox2.Text = "...";
				string reply = await frame.PrintLuaResponse(toSend);
				textBox2.Text = reply;
			}
		}

		private async void button7_Click(object sender, EventArgs e)
		{
			treeView1.Nodes.Clear();
			treeView1.Nodes.Add(await fillDirNode("/"));
			treeView1.Nodes[0].Expand();
		}

		private async Task<TreeNode> fillDirNode(string fullPath)
		{
			var thisDir = new FrameDirectory(frame, fullPath);
			TreeNode node = new TreeNode(thisDir.Name + "/");
			node.Tag = thisDir.FullName.TrimEnd('/') + "/";
			foreach (var childDir in await thisDir.GetAllDirectories())
			{
				var childDirNode = await fillDirNode(childDir.FullName);
				node.Nodes.Add(childDirNode);
			}
			foreach (var childFile in await thisDir.GetAllFiles())
			{
				var childFileNode = new TreeNode(childFile.Name);
				childFileNode.Tag = childFile.FullName;
				node.Nodes.Add(childFileNode);
			}
			return node;
		}

		private async void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			string fullPath = (string)(e.Node.Tag);
			if (e.Node.Text.EndsWith("/"))
			{
				fileContentsBox.Text = "";
			}
			else if (fullPath.EndsWith("!"))
			{
				FrameFile file = new FrameFile(frame, fullPath.Replace("!", ""));
				fileContentsBox.Text = "";
				fileContentsBox.Tag = "";
			}
			else
			{
				FrameFile file = new FrameFile(frame, fullPath);
				fileContentsBox.Text = (await file.GetContents() ?? "").ReplaceLineEndings().Trim();
				fileContentsBox.Tag = fileContentsBox.Text;
			}
		}

		private async void button10_Click(object sender, EventArgs e)
		{
			TreeNode node = treeView1.SelectedNode;
			if (node == null)
			{
				return;
			}
			string fullPath = (string)(node.Tag);
			if (node.Text.EndsWith("/"))
			{
				FrameDirectory dir = new FrameDirectory(frame, fullPath);
				Helpers.AutoRetry(() => dir.DeleteAsync(), maxTimeout: TimeSpan.FromSeconds(10));
			}
			else
			{
				FrameFile file = new FrameFile(frame, fullPath);
				Helpers.AutoRetry(() => file.DeleteAsync(), maxTimeout: TimeSpan.FromSeconds(10));
			}
			node.Remove();
		}

		private void treeView1_DoubleClick(object sender, EventArgs e)
		{

		}

		private async void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			TreeNode node = e.Node;
			if (node == null)
			{
				return;
			}
			if ((e.Node.Tag as string).EndsWith('!'))
			{
				string newName = e.Label ?? e.Node.Text;
				node.Tag = ((string)(node.Tag)).TrimEnd('!');
				if (((string)(node.Tag)).EndsWith("/"))
				{
					// make the new directory
					string newDirFullPath = ((string)(node.Tag)).TrimEnd('/').Substring(0, ((string)(node.Tag)).TrimEnd('/').LastIndexOf('/')) + "/" + newName.TrimEnd('/');
					var newDir = new FrameDirectory(frame, newDirFullPath.Replace("!", ""));
					node.Tag = newDir.FullName + "/";
					if (!newName.EndsWith("/"))
					{
						e.CancelEdit = true;
						node.Text = newName.TrimEnd('/') + "/";
					}
					bool? created = await newDir.Create();
					return;
				}
				else
				{
					// save the empty new file
					string newFileFullPath = ((string)(node.Tag)).Substring(0, ((string)(node.Tag)).LastIndexOf('/')) + "/" + newName.TrimEnd('/');
					var newFile = new FrameFile(frame, newFileFullPath.Replace("!", ""));
					node.Tag = newFile.FullName;
					await newFile.WriteContents(" ");
					return;
				}
			}
			if (string.IsNullOrWhiteSpace(e.Label))
			{
				e.CancelEdit = true;
				return;
			}

			string fullPath = (string)(node.Tag);
			FrameFileSystemInfo item = null;
			if (fullPath.EndsWith("/"))
			{
				item = new FrameDirectory(frame, fullPath);
				//node.Text = e.Label.TrimEnd('/') + "/";
			}
			else
			{
				item = new FrameFile(frame, fullPath);
			}
			if (!await item.Rename(e.Label.TrimEnd('/')))
			{
				e.CancelEdit = true;
			}
			else
			{
				if (fullPath.EndsWith("/"))
				{
					node.Tag = item.FullName + "/";
					if (!e.Label.EndsWith('/'))
					{
						e.CancelEdit = true;
						node.Text = e.Label + "/";
					}
				}
				else
				{
					node.Tag = item.FullName;
				}
			}
		}

		private async void button9_Click(object sender, EventArgs e)
		{
			if (treeView1.Nodes.Count == 0)
			{
				treeView1.Nodes.Clear();
				treeView1.Nodes.Add(await fillDirNode("/"));
				treeView1.Nodes[0].Expand();
			}
			TreeNode curNode = treeView1.SelectedNode;
			if (curNode == null)
			{
				curNode = treeView1.Nodes[0];
			}
			else
			{
				if (!curNode.Text.EndsWith('/'))
					curNode = curNode.Parent;
			}
			if (curNode == null)
			{
				curNode = treeView1.Nodes[0];
			}
			string path = (string)(curNode.Tag);
			string name = "untitled_" + Guid.NewGuid().ToString().Substring(0, 4) + ".lua";
			FrameFile file = new FrameFile(frame, path.TrimEnd('/') + "/" + name);
			TreeNode newNode = new TreeNode(name);
			newNode.Tag = file.FullName + "!";
			curNode.Nodes.Add(newNode);
			treeView1.SelectedNode = newNode;
			newNode.BeginEdit();
		}

		private void button11_Click(object sender, EventArgs e)
		{
			// confirm there is an active file
			TreeNode node = treeView1.SelectedNode;
			if (node == null || node.Text.EndsWith("/"))
			{
				return;
			}
			// confirm the file has been edited
			if (fileContentsBox.Text == (string)fileContentsBox.Tag)
			{
				return;
			}
			// see if it's only been appended
			if (fileContentsBox.Text.StartsWith((string)fileContentsBox.Tag) && !((string)(node.Tag)).EndsWith('!'))
			{
				// if so, only append the new text
				string toAppend = fileContentsBox.Text.Substring(((string)fileContentsBox.Tag).Length).TrimEnd();
				FrameFile file = new FrameFile(frame, ((string)(node.Tag)).TrimEnd('!'));
				file.AppendContents(toAppend);
			}
			else
			{
				// otherwise, overwrite the file
				FrameFile file = new FrameFile(frame, ((string)(node.Tag)).TrimEnd('!'));
				file.WriteContents(fileContentsBox.Text);
				node.Tag = file.FullName;
			}
		}

		private void treeView1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Node.Tag as string))
			{
				e.CancelEdit = true;
			}
		}

		private async void button8_Click(object sender, EventArgs e)
		{
			if (treeView1.Nodes.Count == 0)
			{
				treeView1.Nodes.Clear();
				treeView1.Nodes.Add(await fillDirNode("/"));
				treeView1.Nodes[0].Expand();
			}
			TreeNode curNode = treeView1.SelectedNode;
			if (curNode == null)
			{
				curNode = treeView1.Nodes[0];
			}
			else
			{
				if (!curNode.Text.EndsWith('/'))
					curNode = curNode.Parent;
			}
			if (curNode == null)
			{
				curNode = treeView1.Nodes[0];
			}
			string path = (string)(curNode.Tag);
			string name = "newfolder_" + Guid.NewGuid().ToString().Substring(0, 4);
			FrameDirectory dir = new FrameDirectory(frame, path.TrimEnd('/') + "/" + name);
			TreeNode newNode = new TreeNode(name);
			newNode.Tag = dir.FullName + "/!";
			curNode.Nodes.Add(newNode);
			treeView1.SelectedNode = newNode;
			newNode.BeginEdit();
		}

		private async void button12_Click(object sender, EventArgs e)
		{
			Image img = await frame.GetCameraPhoto();
			var tmp = Path.GetTempFileName() + ".jpg";
			img.Save(tmp);
			Debug.WriteLine("image saved to "+tmp);
			Process.Start(tmp);
		}
	}
}