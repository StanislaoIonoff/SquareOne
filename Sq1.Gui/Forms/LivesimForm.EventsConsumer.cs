﻿using System;
using System.Windows.Forms;

using Sq1.Core;

namespace Sq1.Gui.Forms {
	public partial class LivesimForm {
		// ALREADY_HANDLED_BY_chartControl_BarAddedUpdated_ShouldTriggerRepaint
		//void livesimForm_StrategyExecutedOneQuoteOrBarOrdersEmitted(object sender, EventArgs e) {
		//	ChartControl chartControl = this.chartFormManager.ChartForm.ChartControl;
		//	//v1 SKIPS_REPAINTING_KOZ_NOW_BACKTEST=TRUE chartControl.InvalidateAllPanels();
		//	//chartControl.RefreshAllPanelsNonBlockingRefreshNotYetStarted();
		//}

		void btnStartStop_Click(object sender, EventArgs e) {
			//Button btnPauseResume = this.LivesimControl.BtnPauseResume;
			//Button btnStartStop = this.LivesimControl.BtnStartStop;
			ToolStripButton btnPauseResume = this.LivesimControl.TssBtnPauseResume;
			ToolStripButton btnStartStop = this.LivesimControl.TssBtnStartStop;
			bool clickedStart = btnStartStop.Text.Contains("Start");
			if (clickedStart) {
				btnStartStop.Text = "Starting";
				btnStartStop.Enabled = false;
				this.chartFormsManager.LivesimStartedOrUnpaused_AutoHiddeExecutionAndReporters();
				this.chartFormsManager.Executor.Livesimulator.Start_inGuiThread(btnStartStop, btnPauseResume, this.chartFormsManager.ChartForm.ChartControl);
				btnStartStop.Text = "Stop";
				btnStartStop.Enabled = true;
				btnPauseResume.Enabled = true;
				btnPauseResume.Checked = false;
			} else {
				btnStartStop.Text = "Stopping";
				btnStartStop.Enabled = false;
				this.chartFormsManager.Executor.Livesimulator.Stop_inGuiThread();
				this.chartFormsManager.LivesimEndedOrStoppedOrPaused_RestoreAutoHiddenExecutionAndReporters();
				btnStartStop.Text = "Start";
				btnStartStop.Enabled = true;
				btnPauseResume.Enabled = false;
				btnPauseResume.Checked = false;
			}
		}
		void btnPauseResume_Click(object sender, EventArgs e) {
			//Button btnPauseResume = this.LivesimControl.BtnPauseResume;
			ToolStripButton btnPauseResume = this.LivesimControl.TssBtnPauseResume;
			bool clickedPause = btnPauseResume.Text.Contains("Pause");
			if (clickedPause) {
				btnPauseResume.Text = "Pausing";
				btnPauseResume.Enabled = false;
				this.chartFormsManager.Executor.Livesimulator.Pause_inGuiThread();
				this.chartFormsManager.LivesimEndedOrStoppedOrPaused_RestoreAutoHiddenExecutionAndReporters();
				this.chartFormsManager.ReportersFormsManager.RebuildingFullReportForced_onLivesimPaused();
				btnPauseResume.Text = "Resume";
				btnPauseResume.Enabled = true;
				
				// when quote delay = 2..4, reporters are staying empty (probably GuiIsBusy) - clear&flush each like afterBacktestEnded
				this.chartFormsManager.ReportersFormsManager.BuildReportFullOnBacktestFinishedAllReporters();
				//?this.chartFormManager.ReportersFormsManager.RebuildingFullReportForced_onLivesimPausedStoppedEnded();
			} else {
				btnPauseResume.Text = "Resuming";
				btnPauseResume.Enabled = false;
				this.chartFormsManager.LivesimStartedOrUnpaused_AutoHiddeExecutionAndReporters();
				this.chartFormsManager.Executor.Livesimulator.Unpause_inGuiThread();
				btnPauseResume.Text = "Pause";
				btnPauseResume.Enabled = true;
			}
		}
		//void LivesimForm_Disposed(object sender, EventArgs e) {
		//	if (Assembler.InstanceInitialized.MainFormClosingIgnoreReLayoutDockedForms) return;
		//	// both at FormCloseByX and MainForm.onClose()
		//	this.chartFormManager.ChartForm.MniShowLivesim.Checked = false;
		//	this.chartFormManager.MainForm.MainFormSerialize();
		//}
		void livesimForm_FormClosing(object sender, FormClosingEventArgs e) {
			// only when user closed => allow scriptEditorForm_FormClosed() to serialize
			if (this.chartFormsManager.MainForm.MainFormClosingSkipChartFormsRemoval) {
				e.Cancel = true;
				return;
			}
			if (Assembler.InstanceInitialized.MainFormClosingIgnoreReLayoutDockedForms) {
				e.Cancel = true;
				return;
			}
		}
		void livesimForm_FormClosed(object sender, FormClosedEventArgs e) {
			// both at FormCloseByX and MainForm.onClose()
			this.chartFormsManager.ChartForm.MniShowLivesim.Checked = false;
			this.chartFormsManager.MainForm.MainFormSerialize();
		}
	}
}