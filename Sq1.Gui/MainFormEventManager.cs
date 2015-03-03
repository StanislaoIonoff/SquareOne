﻿using System;
using System.Windows.Forms;

using Sq1.Core;
using Sq1.Core.DataFeed;
using Sq1.Core.Indicators;
using Sq1.Core.StrategyBase;
using Sq1.Core.Support;
using Sq1.Gui.Forms;
using Sq1.Gui.Singletons;
using Sq1.Widgets;

namespace Sq1.Gui {
	public class MainFormEventManager {
		private MainForm mainForm;

		public MainFormEventManager(MainForm mainForm) {
			this.mainForm = mainForm;
		}

		#region StrategiesTree
		public void StrategiesTree_OnStrategyDoubleClicked_NewChart(object sender, StrategyEventArgs e) {
			Strategy strategy = e.Strategy;
			if (strategy.Script == null) {
				string msg = "YES_OR_AFTER_RESTART YOU_OPENED_A_STRATEGY_YOU_JUST_ADDED_TO_REPOSITORY ?";
				//Assembler.PopupException(msg);
				//return;
			} else {
				if (strategy.Script.Executor == null) {
					string msg = "OPENED_FROM_STRATEGIES_TREE strategy.Script.Executor=null";
					Assembler.PopupException(msg, null, false);
				} else {
					strategy.ContextSwitchCurrentToNamedAndSerialize(e.scriptContextName);
				}
			}
			this.chartCreateShowPopulateSelectorsSlidersFromStrategy(strategy);
		}
		internal void StrategiesTree_OnStrategyOpenDefaultClicked_NewChart(object sender, StrategyEventArgs e) {
			//v1
			//Strategy strategy = e.Strategy;
			////strategy.ContextMarkNone();
			//strategy.ScriptContextCurrent.DataRange = SelectorsForm.Instance.BarDataRangeSelector.Popup.BarDataRange;
			//strategy.ScriptContextCurrent.ScaleInterval = SelectorsForm.Instance.BarScaleIntervalSelector.Popup.BarScaleInterval;
			//strategy.ScriptContextCurrent.PositionSize = SelectorsForm.Instance.PositionSizeSelector.Popup.PositionSize;
			//this.chartCreateShowPopulateSelectorsSliders(strategy);
			//v2
			e.scriptContextName = ContextScript.DEFAULT_NAME;
			this.StrategiesTree_OnStrategyDoubleClicked_NewChart(sender, e);
		}
		internal void StrategiesTree_OnStrategyLoadClicked(object sender, StrategyEventArgs e) {
			Strategy strategy = e.Strategy;
			ChartForm active = this.mainForm.ChartFormActiveNullUnsafe;
			if (active == null) {
				ChartFormManager msg = this.chartCreateShowPopulateSelectorsSlidersFromStrategy(strategy);
				active = msg.ChartForm;
			}
			active.ChartFormManager.InitializeWithStrategy(strategy, false);
			if (strategy.Script != null && strategy.Script.Executor != null) {
				strategy.ContextSwitchCurrentToNamedAndSerialize(e.scriptContextName);
			} else {
				string msg = "CANT_SWITCH_CONTEXT_SCRIPT";
				Assembler.PopupException(msg);
			}
		}
		internal void StrategiesTree_OnStrategyRenamed(object sender, StrategyEventArgs e) {
			foreach (ChartFormManager chartFormsManager in this.mainForm.GuiDataSnapshot.ChartFormManagers.Values) {
				if (chartFormsManager.Strategy != e.Strategy) continue;
				if (chartFormsManager.ScriptEditorFormConditionalInstance != null) {
					chartFormsManager.ScriptEditorFormConditionalInstance.Text = e.Strategy.Name;
				}
				if (chartFormsManager.ChartForm != null) {
					chartFormsManager.ChartForm.Text = e.Strategy.Name;
				}
			}
		}
		ChartFormManager chartCreateShowPopulateSelectorsSlidersFromStrategy(Strategy strategy) {
			ChartFormManager chartFormManager = new ChartFormManager(this.mainForm);
			chartFormManager.InitializeWithStrategy(strategy, false);
			this.mainForm.GuiDataSnapshot.ChartFormManagers.Add(chartFormManager.DataSnapshot.ChartSerno, chartFormManager);
			chartFormManager.ChartFormShow();
			chartFormManager.StrategyCompileActivatePopulateSlidersShow();
			return chartFormManager;
		}
		void chartCreateShowPopulateSelectorsSlidersNoStrategy(ContextChart contextChart) {
			ChartFormManager chartFormManager = new ChartFormManager(this.mainForm);
			chartFormManager.InitializeChartNoStrategy(contextChart);
			this.mainForm.GuiDataSnapshot.ChartFormManagers.Add(chartFormManager.DataSnapshot.ChartSerno, chartFormManager);
			chartFormManager.ChartFormShow();
		}
		#endregion
		
		#region ChartForm
		internal void ChartForm_FormClosed(object sender, FormClosedEventArgs e) {
			if (this.mainForm.MainFormClosingSkipChartFormsRemoval) return;
			try {
				ChartForm chartFormClosed = sender as ChartForm;
				ChartFormManager chartFormManager = chartFormClosed.ChartFormManager;
				// chartFormsManager lifecycle ends here
				this.mainForm.GuiDataSnapshot.ChartFormManagers.Remove(chartFormManager.DataSnapshot.ChartSerno);

				if (DockContentImproved.IsNullOrDisposed(chartFormManager.ScriptEditorForm) == false) chartFormManager.ScriptEditorFormConditionalInstance.Close();
				//foreach (Reporter reporter in chartFormsManager.Reporters.Values) {
				//	Control reporterAsControl = reporter as Control;
				//	Control reporterParent = reporterAsControl.Parent;
				//	DockContent reporterParentForm = reporterParent as DockContent;
				//	if (chartFormsManager.FormIsNullOrDisposed(reporterParentForm)) reporterParentForm.Close();
				//}
				if (StrategiesForm.Instance.IsActivated == false) {
					StrategiesForm.Instance.Activate();
				}
			} catch (Exception ex) {
				string msg = "ChartFormsManagers.Remove() didn't go trought? duplicates";
				Assembler.PopupException(msg, ex);
			}
		}
		#endregion
		//v1
		internal void DockPanel_ActiveDocumentChanged(object sender, EventArgs e) {
			if (this.mainForm.MainFormClosingSkipChartFormsRemoval) {
				string msg = "onAppClose getting invoked for each [mosaically] visible document, right? nope just once per Close()";
				return;
			}

			ChartForm chartFormClicked = this.mainForm.DockPanel.ActiveDocument as ChartForm;
			if (chartFormClicked == null) {
				this.mainForm.GuiDataSnapshot.ChartSernoLastKnownHadFocus = -1;
				string msg = "focus might have moved away from a document to Docked Panel"
					+ "; I'm here after having focused on ExceptionsForm docked into Documents pane";
				return;
			}
			//if (chartFormClicked.IsActivated == false) return;	//NOUP ActiveDocumentChanged is invoked twice: 1) for a form loosing control, 2) for a form gaining control
			try {
				chartFormClicked.ChartFormManager.InterformEventsConsumer.MainForm_ActivateDocumentPane_WithChart(sender, e);
				this.mainForm.GuiDataSnapshot.ChartSernoLastKnownHadFocus = chartFormClicked.ChartFormManager.DataSnapshot.ChartSerno;
				this.mainForm.GuiDataSnapshotSerializer.Serialize();
				
				//v1: DOESNT_POPULATE_SYMBOL_AND_SCRIPT_PARAMETERS 
				//if (chartFormClicked.ChartFormManager.Strategy == null) {
				//	StrategiesForm.Instance.StrategiesTreeControl.UnSelectStrategy();
				//} else {
				//	StrategiesForm.Instance.StrategiesTreeControl.SelectStrategy(chartFormClicked.ChartFormManager.Strategy);
				//}
				chartFormClicked.ChartFormManager.PopulateMainFormSymbolStrategyTreesScriptParameters();
				//chartFormClicked.Activate();	// I_GUESS_ITS_ALREADY_ACTIVE
				chartFormClicked.Focus();		// FLOATING_FORM_CANT_BE_RESIZED_WITHOUT_FOCUS FOCUS_WAS_PROBABLY_STOLEN_BY_SOME_OTHER_FORM(MAIN?)_LAZY_TO_DEBUG
			} catch (Exception ex) {
				Assembler.PopupException("DockPanel_ActiveDocumentChanged()", ex);
			}
		}
		//v2
		internal void DockPanel_ActiveContentChanged(object sender, EventArgs e) {
			ChartForm chartFormClicked = this.mainForm.DockPanel.ActiveContent as ChartForm;
			if (chartFormClicked == null) {
				string msig = " DockPanel_ActiveContentChanged() is looking for mainForm.GuiDataSnapshot.ChartSernoLastKnownHadFocus["
					+ this.mainForm.GuiDataSnapshot.ChartSernoLastKnownHadFocus + "]";
				int lastKnownChartSerno = this.mainForm.GuiDataSnapshot.ChartSernoLastKnownHadFocus;
				ChartFormManager lastKnownChartFormManager = this.mainForm.GuiDataSnapshot.FindChartFormsManagerBySerno(lastKnownChartSerno, msig, false);
				if (lastKnownChartFormManager == null) {
					string msg = "DOCK_ACTIVE_CONTENT_CHANGED_BUT_CANT_FIND_LAST_CHART lastKnownChartSerno[" + lastKnownChartSerno + "]";
					// INFINITE_LOOP_HANGAR_NINE_DOOMED_TO_COLLAPSE Assembler.PopupException(msg + msig);
					return;
				}
				ChartForm lastKnownChartForm = lastKnownChartFormManager.ChartForm;
				chartFormClicked = lastKnownChartForm;
			}
			if (chartFormClicked == null) {
				//this.mainForm.GuiDataSnapshot.ChartSernoHasFocus = -1;
				string msg = "DockContent-derived activated by user isn't a ChartForm; I won't sync DataSourcesTree,StrategiesTree,Splitters to hightlight relevant";
				return;
			}
			try {
				chartFormClicked.ChartFormManager.InterformEventsConsumer.MainForm_ActivateDocumentPane_WithChart(sender, e);
				this.mainForm.GuiDataSnapshot.ChartSernoLastKnownHadFocus = chartFormClicked.ChartFormManager.DataSnapshot.ChartSerno;
				this.mainForm.GuiDataSnapshotSerializer.Serialize();
				chartFormClicked.ChartFormManager.PopulateMainFormSymbolStrategyTreesScriptParameters();
			} catch (Exception ex) {
				Assembler.PopupException("DockPanel_ActiveContentChanged()", ex);
			}
		}

		#region DataSourcesTree
		internal void DataSourcesTree_OnBarsAnalyzerClicked(object sender, DataSourceSymbolEventArgs e) {
		}
		internal void DataSourcesTree_OnDataSourceDeletedClicked(object sender, DataSourceEventArgs e) {
		}
		internal void RepositoryJsonDataSource_OnDataSourceCanBeRemoved(object sender, NamedObjectJsonEventArgs<DataSource> e) {
			int a = 1;
			// ask them before deleting using another event and check if DataSourceEventArgs.DoNotDeleteThisDataSourceBecauseItsUsedElsewhere
		}
		internal void RepositoryJsonDataSource_OnDataSourceRemoved(object sender, NamedObjectJsonEventArgs<DataSource> e) {
			int a = 1;
			//if a running optimizer / backtester / streaming chart had DataSource, possibly shut them down?
		}
		internal void DataSourcesTree_OnDataSourceEditClicked(object sender, DataSourceEventArgs e) {
			//DataSourceEditorForm.Instance.DataSourceEditorControl.Initialize(e.DataSource);
			DataSourceEditorForm.Instance.Initialize(e.DataSource.Name);
			try {
				DataSourceEditorForm.Instance.ShowAsDocumentTabNotPane(this.mainForm.DockPanel);
			} catch (Exception exc) {
				string msg = "DataSourceEditorForm(DataSource[" + e.DataSource + "]): internal Exception";
				Assembler.PopupException(msg, exc);
				return;
			}
		}
		internal void DataSourcesTree_OnDataSourceRenameClicked(object sender, DataSourceEventArgs e) {
		}
		internal void DataSourcesTree_OnDataSourceSelected(object sender, DataSourceEventArgs e) {
		}
		internal void DataSourcesTree_OnNewChartForSymbolClicked(object sender, DataSourceSymbolEventArgs e) {
			ContextChart contextChart = new ContextChart("CHART_" + e.Symbol);
			contextChart.DataSourceName = e.DataSource.Name;
			contextChart.Symbol = e.Symbol;
			contextChart.ScaleInterval = e.DataSource.ScaleInterval;
			//this.chartCreateShowPopulateSelectorsSliders(contextChart);
			this.chartCreateShowPopulateSelectorsSlidersNoStrategy(contextChart);
		}
		internal void DataSourcesTree_OnOpenStrategyForSymbolClicked(object sender, DataSourceSymbolEventArgs e) {
		}
		internal void DataSourcesTree_OnSymbolSelected(object sender, DataSourceSymbolEventArgs e) {
			try {
				if ((this.mainForm.DockPanel.ActiveDocument is ChartForm) == false) {
					return;
				}
				// mainForm.ChartFormActive will already throw if Documents have no Charts selected; no need to check
				this.mainForm.ChartFormActiveNullUnsafe.ChartFormManager.InterformEventsConsumer.DataSourcesTree_OnSymbolSelected(sender, e);
			} catch (Exception ex) {
				Assembler.PopupException(null, ex);
			}
		}
		#endregion DataSourcesTree


		#region SlidersForm.Instance.SlidersAutoGrow
		internal void SlidersAutoGrow_OnScriptContextLoadClicked(object sender, StrategyEventArgs e) {
			Strategy strategy = e.Strategy;
			strategy.ContextSwitchCurrentToNamedAndSerialize(e.scriptContextName);
			//v1 SlidersForm.Instance.PopulateFormTitle(strategy);
			//v2 WILLBEDONE_BY_PopulateSelectorsFromCurrentChartOrScriptContextLoadBarsSaveBacktestIfStrategy() SlidersForm.Instance.Initialize(strategy);
			try {
				this.mainForm.ChartFormActiveNullUnsafe.ChartFormManager
					.PopulateSelectorsFromCurrentChartOrScriptContextLoadBarsSaveBacktestIfStrategy("StrategiesTree_OnScriptContextLoadClicked()");
			} catch (Exception ex) {
				Assembler.PopupException("StrategiesTree_OnScriptContextLoadClicked()", ex);
			}
		}
		internal void SlidersAutoGrow_OnScriptContextRenamed(object sender, StrategyEventArgs e) {
			Strategy strategy = e.Strategy;
			if (strategy.ScriptContextCurrentName != e.scriptContextName) return;	//refresh FormTitle only when renaming current context
			SlidersForm.Instance.PopulateFormTitle(strategy);
		}
		// TYPE_MANGLING_INSIDE_WARNING NOTICE_THAT_BOTH_PARAMETER_SCRIPT_AND_INDICATOR_VALUE_CHANGED_EVENTS_ARE_HANDLED_BY_SINGLE_HANDLER
		internal void SlidersAutoGrow_SliderValueChanged(object sender, IndicatorParameterEventArgs e) {
			ChartForm chartFormActive = this.mainForm.ChartFormActiveNullUnsafe;
			if (chartFormActive == null) {
				string msg = "DRAG_CHART_INTO_DOCUMENT_AREA";
				Assembler.PopupException(msg);
				return;
			}
			Strategy strategyToSaveAndRun = chartFormActive.ChartFormManager.Strategy;
			if (strategyToSaveAndRun.Script.Executor == null) {
				string msg = "slider_ValueCurrentChanged(): did you forget to assign Script.Executor after compilation?...";
				Assembler.PopupException(msg);
				return;
			}

			ScriptParameterEventArgs scripParamChanged = e as ScriptParameterEventArgs;
			if (scripParamChanged != null) {
				strategyToSaveAndRun.PushChangedScriptParameterValueToScriptAndSerialize(scripParamChanged.ScriptParameter);
			} else {
				strategyToSaveAndRun.PushChangedIndicatorParameterValueToScriptAndSerialize(e.IndicatorParameter);
			}
			chartFormActive.ChartFormManager.PopulateSelectorsFromCurrentChartOrScriptContextLoadBarsSaveBacktestIfStrategy("SlidersAutoGrow_SliderValueChanged", false);
			
			ScriptParameterEventArgs demuxScriptParameterEventArgs = e as ScriptParameterEventArgs;   
			if (demuxScriptParameterEventArgs == null) {
				string msg = "MultiSplitterPropertiesByPanelName[ATR (Period:5[1..11/2]) ] key should be synchronized when user clicks Period 5=>7";
				chartFormActive.ChartControl.SerializeSplitterDistanceOrPanelName();
			} else {
				string msg = "DO_NOTHING_ELSE_INDICATOR_PANEL_SPLITTER_POSITIONS_SHOULDNT_BE_SAVED_HERE";
			}
		}
		#endregion SlidersForm.Instance.SlidersAutoGrow

	}
}
