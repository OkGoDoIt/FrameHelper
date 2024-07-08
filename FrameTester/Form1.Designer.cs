namespace FrameTester
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			textBox1 = new TextBox();
			textBox2 = new TextBox();
			label1 = new Label();
			label2 = new Label();
			button1 = new Button();
			foundBox = new CheckBox();
			StatusCheckTmr = new System.Windows.Forms.Timer(components);
			pairedBox = new CheckBox();
			connectedBox = new CheckBox();
			label3 = new Label();
			batteryBar = new ProgressBar();
			stayAwakeBox = new CheckBox();
			saveBtn = new Button();
			button2 = new Button();
			button3 = new Button();
			button4 = new Button();
			button5 = new Button();
			button6 = new Button();
			treeView1 = new TreeView();
			fileContentsBox = new TextBox();
			button7 = new Button();
			button8 = new Button();
			button9 = new Button();
			button10 = new Button();
			button11 = new Button();
			button12 = new Button();
			SuspendLayout();
			// 
			// textBox1
			// 
			textBox1.AcceptsReturn = true;
			textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			textBox1.Location = new Point(207, 31);
			textBox1.Multiline = true;
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(435, 94);
			textBox1.TabIndex = 0;
			// 
			// textBox2
			// 
			textBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			textBox2.Location = new Point(207, 131);
			textBox2.Name = "textBox2";
			textBox2.Size = new Size(435, 23);
			textBox2.TabIndex = 1;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(122, 34);
			label1.Name = "label1";
			label1.Size = new Size(56, 15);
			label1.TabIndex = 2;
			label1.Text = "Message:";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(122, 134);
			label2.Name = "label2";
			label2.Size = new Size(60, 15);
			label2.TabIndex = 2;
			label2.Text = "Response:";
			// 
			// button1
			// 
			button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button1.Location = new Point(665, 30);
			button1.Name = "button1";
			button1.Size = new Size(101, 23);
			button1.TabIndex = 3;
			button1.Text = "Send";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// foundBox
			// 
			foundBox.AutoSize = true;
			foundBox.Enabled = false;
			foundBox.Location = new Point(207, 195);
			foundBox.Name = "foundBox";
			foundBox.Size = new Size(96, 19);
			foundBox.TabIndex = 4;
			foundBox.Text = "Frame Found";
			foundBox.UseVisualStyleBackColor = true;
			// 
			// StatusCheckTmr
			// 
			StatusCheckTmr.Enabled = true;
			StatusCheckTmr.Interval = 5000;
			StatusCheckTmr.Tick += StatusCheckTmr_Tick;
			// 
			// pairedBox
			// 
			pairedBox.AutoSize = true;
			pairedBox.Enabled = false;
			pairedBox.Location = new Point(207, 220);
			pairedBox.Name = "pairedBox";
			pairedBox.Size = new Size(95, 19);
			pairedBox.TabIndex = 4;
			pairedBox.Text = "Frame Paired";
			pairedBox.UseVisualStyleBackColor = true;
			// 
			// connectedBox
			// 
			connectedBox.AutoSize = true;
			connectedBox.Enabled = false;
			connectedBox.Location = new Point(207, 245);
			connectedBox.Name = "connectedBox";
			connectedBox.Size = new Size(120, 19);
			connectedBox.TabIndex = 4;
			connectedBox.Text = "Frame Connected";
			connectedBox.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(122, 286);
			label3.Name = "label3";
			label3.Size = new Size(47, 15);
			label3.TabIndex = 2;
			label3.Text = "Battery:";
			// 
			// batteryBar
			// 
			batteryBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			batteryBar.Location = new Point(207, 278);
			batteryBar.Name = "batteryBar";
			batteryBar.Size = new Size(435, 23);
			batteryBar.Step = 1;
			batteryBar.Style = ProgressBarStyle.Continuous;
			batteryBar.TabIndex = 5;
			// 
			// stayAwakeBox
			// 
			stayAwakeBox.AutoSize = true;
			stayAwakeBox.Checked = true;
			stayAwakeBox.CheckState = CheckState.Indeterminate;
			stayAwakeBox.Location = new Point(206, 318);
			stayAwakeBox.Name = "stayAwakeBox";
			stayAwakeBox.Size = new Size(171, 19);
			stayAwakeBox.TabIndex = 4;
			stayAwakeBox.Text = "Stay Awake While Charging";
			stayAwakeBox.UseVisualStyleBackColor = true;
			stayAwakeBox.CheckedChanged += stayAwakeBox_CheckedChanged;
			// 
			// saveBtn
			// 
			saveBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			saveBtn.Location = new Point(665, 59);
			saveBtn.Name = "saveBtn";
			saveBtn.Size = new Size(101, 23);
			saveBtn.TabIndex = 6;
			saveBtn.Text = "Run as Script";
			saveBtn.UseVisualStyleBackColor = true;
			saveBtn.Click += saveBtn_Click;
			// 
			// button2
			// 
			button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button2.Location = new Point(665, 278);
			button2.Name = "button2";
			button2.Size = new Size(101, 23);
			button2.TabIndex = 3;
			button2.Text = "Sleep";
			button2.UseVisualStyleBackColor = true;
			button2.Click += button2_Click;
			// 
			// button3
			// 
			button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button3.Location = new Point(665, 88);
			button3.Name = "button3";
			button3.Size = new Size(101, 23);
			button3.TabIndex = 3;
			button3.Text = "Evaluate";
			button3.UseVisualStyleBackColor = true;
			button3.Click += button3_Click;
			// 
			// button4
			// 
			button4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button4.Location = new Point(665, 249);
			button4.Name = "button4";
			button4.Size = new Size(101, 23);
			button4.TabIndex = 3;
			button4.Text = "Reset";
			button4.UseVisualStyleBackColor = true;
			button4.Click += button4_Click;
			// 
			// button5
			// 
			button5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button5.Location = new Point(665, 220);
			button5.Name = "button5";
			button5.Size = new Size(101, 23);
			button5.TabIndex = 3;
			button5.Text = "Stop";
			button5.UseVisualStyleBackColor = true;
			button5.Click += button5_Click;
			// 
			// button6
			// 
			button6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button6.Location = new Point(665, 117);
			button6.Name = "button6";
			button6.Size = new Size(101, 23);
			button6.TabIndex = 3;
			button6.Text = "Evaluate (Long)";
			button6.UseVisualStyleBackColor = true;
			button6.Click += button6_Click;
			// 
			// treeView1
			// 
			treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			treeView1.LabelEdit = true;
			treeView1.Location = new Point(12, 392);
			treeView1.Name = "treeView1";
			treeView1.Size = new Size(170, 356);
			treeView1.TabIndex = 7;
			treeView1.BeforeLabelEdit += treeView1_BeforeLabelEdit;
			treeView1.AfterLabelEdit += treeView1_AfterLabelEdit;
			treeView1.AfterSelect += treeView1_AfterSelect;
			treeView1.DoubleClick += treeView1_DoubleClick;
			// 
			// fileContentsBox
			// 
			fileContentsBox.AcceptsReturn = true;
			fileContentsBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			fileContentsBox.Location = new Point(207, 360);
			fileContentsBox.Multiline = true;
			fileContentsBox.Name = "fileContentsBox";
			fileContentsBox.Size = new Size(435, 388);
			fileContentsBox.TabIndex = 8;
			// 
			// button7
			// 
			button7.Location = new Point(12, 360);
			button7.Name = "button7";
			button7.Size = new Size(31, 23);
			button7.TabIndex = 9;
			button7.Text = "🔁";
			button7.UseVisualStyleBackColor = true;
			button7.Click += button7_Click;
			// 
			// button8
			// 
			button8.Location = new Point(49, 360);
			button8.Name = "button8";
			button8.Size = new Size(42, 23);
			button8.TabIndex = 9;
			button8.Text = "+ Dir";
			button8.UseVisualStyleBackColor = true;
			button8.Click += button8_Click;
			// 
			// button9
			// 
			button9.Location = new Point(97, 360);
			button9.Name = "button9";
			button9.Size = new Size(46, 23);
			button9.TabIndex = 9;
			button9.Text = "+ File";
			button9.UseVisualStyleBackColor = true;
			button9.Click += button9_Click;
			// 
			// button10
			// 
			button10.Location = new Point(149, 360);
			button10.Name = "button10";
			button10.Size = new Size(31, 23);
			button10.TabIndex = 9;
			button10.Text = "❌";
			button10.UseVisualStyleBackColor = true;
			button10.Click += button10_Click;
			// 
			// button11
			// 
			button11.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button11.Location = new Point(665, 360);
			button11.Name = "button11";
			button11.Size = new Size(101, 23);
			button11.TabIndex = 3;
			button11.Text = "Save";
			button11.UseVisualStyleBackColor = true;
			button11.Click += button11_Click;
			// 
			// button12
			// 
			button12.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			button12.Location = new Point(665, 713);
			button12.Name = "button12";
			button12.Size = new Size(101, 23);
			button12.TabIndex = 3;
			button12.Text = "Take Photo";
			button12.UseVisualStyleBackColor = true;
			button12.Click += button12_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(811, 760);
			Controls.Add(button9);
			Controls.Add(button8);
			Controls.Add(button10);
			Controls.Add(button7);
			Controls.Add(fileContentsBox);
			Controls.Add(treeView1);
			Controls.Add(saveBtn);
			Controls.Add(batteryBar);
			Controls.Add(pairedBox);
			Controls.Add(connectedBox);
			Controls.Add(stayAwakeBox);
			Controls.Add(foundBox);
			Controls.Add(button2);
			Controls.Add(button5);
			Controls.Add(button4);
			Controls.Add(button6);
			Controls.Add(button3);
			Controls.Add(button12);
			Controls.Add(button11);
			Controls.Add(button1);
			Controls.Add(label2);
			Controls.Add(label3);
			Controls.Add(label1);
			Controls.Add(textBox2);
			Controls.Add(textBox1);
			Name = "Form1";
			Text = "Form1";
			Load += Form1_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TextBox textBox1;
		private TextBox textBox2;
		private Label label1;
		private Label label2;
		private Button button1;
		private CheckBox foundBox;
		private System.Windows.Forms.Timer StatusCheckTmr;
		private CheckBox pairedBox;
		private CheckBox connectedBox;
		private Label label3;
		private ProgressBar batteryBar;
		private CheckBox stayAwakeBox;
		private Button saveBtn;
		private Button button2;
		private Button button3;
		private Button button4;
		private Button button5;
		private Button button6;
		private TreeView treeView1;
		private TextBox fileContentsBox;
		private Button button7;
		private Button button8;
		private Button button9;
		private Button button10;
		private Button button11;
		private Button button12;
	}
}
