namespace Loderunner
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.left = new System.Windows.Forms.Button();
            this.rtbBoard = new System.Windows.Forms.RichTextBox();
            this.right = new System.Windows.Forms.Button();
            this.up = new System.Windows.Forms.Button();
            this.down = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbEvalTime = new System.Windows.Forms.TextBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbHeroPos = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // left
            // 
            this.left.Location = new System.Drawing.Point(22, 54);
            this.left.Name = "left";
            this.left.Size = new System.Drawing.Size(75, 23);
            this.left.TabIndex = 0;
            this.left.Text = "<-";
            this.left.UseVisualStyleBackColor = true;
            this.left.Click += new System.EventHandler(this.left_Click);
            // 
            // rtbBoard
            // 
            this.rtbBoard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbBoard.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbBoard.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rtbBoard.Location = new System.Drawing.Point(323, 6);
            this.rtbBoard.Name = "rtbBoard";
            this.rtbBoard.ReadOnly = true;
            this.rtbBoard.Size = new System.Drawing.Size(447, 824);
            this.rtbBoard.TabIndex = 1;
            this.rtbBoard.Text = "";
            // 
            // right
            // 
            this.right.Location = new System.Drawing.Point(131, 54);
            this.right.Name = "right";
            this.right.Size = new System.Drawing.Size(75, 23);
            this.right.TabIndex = 2;
            this.right.Text = "->";
            this.right.UseVisualStyleBackColor = true;
            this.right.Click += new System.EventHandler(this.right_Click);
            // 
            // up
            // 
            this.up.Location = new System.Drawing.Point(79, 25);
            this.up.Name = "up";
            this.up.Size = new System.Drawing.Size(75, 23);
            this.up.TabIndex = 3;
            this.up.Text = "UP";
            this.up.UseVisualStyleBackColor = true;
            this.up.Click += new System.EventHandler(this.up_Click);
            // 
            // down
            // 
            this.down.Location = new System.Drawing.Point(79, 83);
            this.down.Name = "down";
            this.down.Size = new System.Drawing.Size(75, 23);
            this.down.TabIndex = 4;
            this.down.Text = "DOWN";
            this.down.UseVisualStyleBackColor = true;
            this.down.Click += new System.EventHandler(this.down_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Eval time (ms):";
            // 
            // tbEvalTime
            // 
            this.tbEvalTime.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tbEvalTime.Location = new System.Drawing.Point(93, 117);
            this.tbEvalTime.Name = "tbEvalTime";
            this.tbEvalTime.ReadOnly = true;
            this.tbEvalTime.Size = new System.Drawing.Size(55, 20);
            this.tbEvalTime.TabIndex = 7;
            // 
            // rtbLog
            // 
            this.rtbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbLog.Location = new System.Drawing.Point(3, 158);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(314, 672);
            this.rtbLog.TabIndex = 8;
            this.rtbLog.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 142);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Eval log:";
            // 
            // textBox1
            // 
            this.tbHeroPos.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tbHeroPos.Location = new System.Drawing.Point(235, 117);
            this.tbHeroPos.Name = "textBox1";
            this.tbHeroPos.ReadOnly = true;
            this.tbHeroPos.Size = new System.Drawing.Size(66, 20);
            this.tbHeroPos.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(184, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Hero at:";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 834);
            this.Controls.Add(this.tbHeroPos);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.tbEvalTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.down);
            this.Controls.Add(this.up);
            this.Controls.Add(this.right);
            this.Controls.Add(this.rtbBoard);
            this.Controls.Add(this.left);
            this.Name = "MainWindow";
            this.Text = "Loderunner Bot";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button left;
        private System.Windows.Forms.RichTextBox rtbBoard;
        private System.Windows.Forms.Button right;
        private System.Windows.Forms.Button up;
        private System.Windows.Forms.Button down;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbEvalTime;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbHeroPos;
        private System.Windows.Forms.Label label3;
    }
}

