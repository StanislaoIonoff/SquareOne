﻿using System;
using System.IO;

using Sq1.Core;
using Sq1.Core.StrategyBase;
using Sq1.Gui.Forms;
using Sq1.Widgets.ScriptEditor;

namespace Sq1.Gui.FormFactories {
	public class ScriptEditorFormFactory {	// REASON_TO_EXIST: allows to run backtest in ChartForm context, stores Strategy.SourceCode in JSON, bridges Sq1.Widgets.dll and Sq1.Core.dll
		const string prefixWhenNeedsToBeSaved = "* ";
		ChartFormManager chartFormManager;
		ScriptEditorForm scriptEditorForm {
			get { return this.chartFormManager.ScriptEditorFormConditionalInstance; }
			set { this.chartFormManager.ScriptEditorForm = value; }
		}
		Strategy strategy {
			get { return this.chartFormManager.Strategy; }
			set { this.chartFormManager.Strategy = value; }
		}
		public ScriptEditorFormFactory(ChartFormManager chartFormsManager) {
			this.chartFormManager = chartFormsManager;
		}
		public void CreateEditorFormSubscribePushToManager(ChartFormManager chartFormsManager) {
			this.scriptEditorForm = new ScriptEditorForm(chartFormsManager);
			this.scriptEditorForm.ScriptEditorControl.OnSave += ScriptEditorControl_OnSave;
			this.scriptEditorForm.ScriptEditorControl.OnCompile += ScriptEditorControl_OnCompile;
			this.scriptEditorForm.ScriptEditorControl.OnRun += ScriptEditorControl_OnRun;
			this.scriptEditorForm.ScriptEditorControl.OnDebug += ScriptEditorControl_OnDebug;
			this.scriptEditorForm.ScriptEditorControl.OnTextNotSaved += ScriptEditorControl_OnTextNotSaved;
			this.scriptEditorForm.Disposed += editorForm_Disposed;
		}
		void ScriptEditorControl_OnTextNotSaved(object sender, ScriptEditorEventArgs e) {
			try {
				if (this.chartFormManager.ScriptEditedNeedsSaving) return;
				this.chartFormManager.ScriptEditedNeedsSaving = true;
				this.chartFormManager.PopulateWindowTitlesFromChartContextOrStrategy();
			} catch (Exception ex) {
				Assembler.PopupException("ScriptEditorControl_OnTextNotSaved", ex);
			}
		}
		void ScriptEditorControl_OnSave(object sender, ScriptEditorEventArgs e) {
			try {
				this.strategy.ScriptSourceCode = e.ScriptText;
				this.strategy.Serialize();
				this.chartFormManager.MainForm.DisplayStatus("Strategy [" + Path.Combine(this.strategy.StoredInFolderRelName, this.strategy.StoredInJsonRelName) + "] saved");
				this.chartFormManager.ScriptEditedNeedsSaving = false;
				this.chartFormManager.PopulateWindowTitlesFromChartContextOrStrategy();
			} catch (Exception ex) {
				Assembler.PopupException("ScriptEditorControl_OnSave", ex);
			}
		}
		void ScriptEditorControl_OnCompile(object sender, ScriptEditorEventArgs e) {
			try {
				this.ScriptEditorControl_OnSave(sender, e);
			} catch (Exception ex) {
				Assembler.PopupException("SAVING_STRATEGY_SOURCE_CODE_FAILED //ScriptEditorControl_OnSave() << ScriptEditorControl_OnCompile()", ex);
			}
			try {
				this.chartFormManager.StrategyCompileActivatePopulateSlidersShow();
				this.chartFormManager.OptimizerFormIfOpenPropagateTextboxesOrMarkStaleResults();
				//v1 this.chartFormManager.Strategy.OptimizationResultsByContextIdent.Clear();
				this.chartFormManager.OptimizerForm.OptimizerControl.RepositoryDllJsonOptimizationResults.ItemsFoundDeleteAll();
			} catch (Exception ex) {
				Assembler.PopupException("COMPILING_STRATEGY_SOURCE_CODE_FAILED //ScriptEditorControl_OnCompile() << StrategyCompileActivatePopulateSlidersShow() has thrown", ex);
			}
		}
		void ScriptEditorControl_OnDebug(object sender, ScriptEditorEventArgs e) {
			throw new NotImplementedException();
		}
		void ScriptEditorControl_OnRun(object sender, ScriptEditorEventArgs e) {
			try {
				this.ScriptEditorControl_OnCompile(sender, e);
				this.chartFormManager.BacktesterRunSimulation();
			} catch (Exception ex) {
				Assembler.PopupException("ScriptEditorControl_OnRun", ex);
			}
		}
		void editorForm_Disposed(object sender, EventArgs e) {
			// both at FormCloseByX and MainForm.onClose()
			this.chartFormManager.ChartForm.MniShowSourceCodeEditor.Checked = false;
			this.scriptEditorForm = null;
		}
	}
}
