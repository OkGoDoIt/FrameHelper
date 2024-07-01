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
			SuspendLayout();
			// 
			// textBox1
			// 
			textBox1.Location = new Point(207, 31);
			textBox1.Multiline = true;
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(435, 94);
			textBox1.TabIndex = 0;
			// 
			// textBox2
			// 
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
			button5.Location = new Point(665, 220);
			button5.Name = "button5";
			button5.Size = new Size(101, 23);
			button5.TabIndex = 3;
			button5.Text = "Stop";
			button5.UseVisualStyleBackColor = true;
			button5.Click += button5_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(saveBtn);
			Controls.Add(batteryBar);
			Controls.Add(pairedBox);
			Controls.Add(connectedBox);
			Controls.Add(stayAwakeBox);
			Controls.Add(foundBox);
			Controls.Add(button2);
			Controls.Add(button5);
			Controls.Add(button4);
			Controls.Add(button3);
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
	}
}
