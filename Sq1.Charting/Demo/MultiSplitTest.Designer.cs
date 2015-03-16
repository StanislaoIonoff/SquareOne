﻿/*
 * Created by SharpDevelop.
 * User: worldexplorer
 * Date: 29-Sep-14
 * Time: 8:39 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Sq1.Charting.Demo
{
	partial class MultiSplitTest
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.multiSplitContainer1 = new Sq1.Charting.MultiSplit.MultiSplitContainer();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.multiSplitContainerOfPanelBase1 = new Sq1.Charting.MultiSplit.MultiSplitContainer();
			this.panel5 = new System.Windows.Forms.Panel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// multiSplitContainer1
			// 
			this.multiSplitContainer1.BackColor = System.Drawing.Color.LemonChiffon;
			this.multiSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.multiSplitContainer1.Location = new System.Drawing.Point(0, 0);
			this.multiSplitContainer1.Name = "multiSplitContainer1";
			this.multiSplitContainer1.Size = new System.Drawing.Size(461, 505);
			this.multiSplitContainer1.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Green;
			this.panel1.Location = new System.Drawing.Point(186, 11);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(220, 67);
			this.panel1.TabIndex = 1;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.Yellow;
			this.panel2.Location = new System.Drawing.Point(148, 57);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(200, 100);
			this.panel2.TabIndex = 2;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.Red;
			this.panel3.Location = new System.Drawing.Point(23, 30);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(200, 100);
			this.panel3.TabIndex = 3;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.DodgerBlue;
			this.panel4.Location = new System.Drawing.Point(228, 250);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(200, 100);
			this.panel4.TabIndex = 4;
			// 
			// multiSplitContainerOfPanelBase1
			// 
			this.multiSplitContainerOfPanelBase1.Location = new System.Drawing.Point(25, 317);
			this.multiSplitContainerOfPanelBase1.Name = "multiSplitContainerOfPanelBase1";
			this.multiSplitContainerOfPanelBase1.Size = new System.Drawing.Size(150, 150);
			this.multiSplitContainerOfPanelBase1.TabIndex = 5;
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.Color.MediumBlue;
			this.panel5.Location = new System.Drawing.Point(192, 356);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(200, 100);
			this.panel5.TabIndex = 5;
			// 
			// panel6
			// 
			this.panel6.BackColor = System.Drawing.Color.MediumSlateBlue;
			this.panel6.Location = new System.Drawing.Point(206, 285);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(200, 100);
			this.panel6.TabIndex = 6;
			// 
			// MultiSplitTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(461, 505);
			this.Controls.Add(this.panel6);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel5);
			this.Controls.Add(this.multiSplitContainerOfPanelBase1);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.multiSplitContainer1);
			this.Name = "MultiSplitTest";
			this.Text = "MultiSplitTest";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Panel panel5;
		private Sq1.Charting.MultiSplit.MultiSplitContainer multiSplitContainerOfPanelBase1;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private Sq1.Charting.MultiSplit.MultiSplitContainer multiSplitContainer1;
	}
}
