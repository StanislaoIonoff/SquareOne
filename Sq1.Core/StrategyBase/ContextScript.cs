﻿using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Sq1.Core.Backtesting;
using Sq1.Core.Execution;
using Sq1.Core.Indicators;
using Sq1.Core.Optimization;

namespace Sq1.Core.StrategyBase {
	public class ContextScript : ContextChart {
		[JsonIgnore]	public const string DEFAULT_NAME = "Default";
		
		[JsonProperty]	public PositionSize					PositionSize;
		[JsonProperty]	public Dictionary<int, ScriptParameter>					ScriptParametersById;
		[JsonProperty]	public Dictionary<string, List<IndicatorParameter>>		IndicatorParametersByName;
		
		[JsonProperty]	public bool							IsCurrent;
		[JsonProperty]	public bool							StrategyEmittingOrders;

		[JsonProperty]	public List<string>					ReporterShortNamesUserInvokedJSONcheck;
		[JsonProperty]	public bool							BacktestOnRestart;
		[JsonProperty]	public bool							BacktestOnSelectorsChange;
		[JsonProperty]	public bool							BacktestOnDataSourceSaved;
		[JsonProperty]	public Dictionary<string, object>	ReportersSnapshots;
		
		[JsonProperty]	public bool							ApplyCommission;
		[JsonProperty]	public bool							EnableSlippage;
		[JsonProperty]	public bool							LimitOrderSlippage;
		[JsonProperty]	public bool							RoundEquityLots;
		[JsonProperty]	public bool							RoundEquityLotsToUpperHundred;
		[JsonProperty]	public bool							NoDecimalRoundingForLimitStopPrice;
		[JsonProperty]	public double						SlippageUnits;
		[JsonProperty]	public int							SlippageTicks;
		[JsonProperty]	public int							PriceLevelSizeForBonds;
		
		[JsonProperty]	public bool							FillOutsideQuoteSpreadParanoidCheckThrow;
		[JsonProperty]	public string						SpreadModelerClassName;
		[JsonProperty]	public double						SpreadModelerPercent;
		[JsonProperty]	public BacktestStrokesPerBar					BacktestStrokesPerBar;


		[JsonIgnore]	public List<IndicatorParameter> ParametersMerged { get {
				// MAKE_SURE_YOU_DONT_KEEP_THE_REFERENCE; use ParametersMergedCloned otherwize
				List<IndicatorParameter> ret = new List<IndicatorParameter>();
				ret.AddRange(this.ScriptParametersById.Values);
				foreach (List<IndicatorParameter> iParams in this.IndicatorParametersByName.Values) {
					ret.AddRange(iParams);
				}
				return ret;
			} }
		[JsonIgnore]	public List<IndicatorParameter> ParametersMergedCloned { get {
				List<IndicatorParameter> ret = new List<IndicatorParameter>();
				foreach (IndicatorParameter iParam in this.ParametersMerged) ret.Add(iParam.Clone());
				return ret;
			} }
		[JsonIgnore]	public SortedDictionary<string, IndicatorParameter> ParametersMergedByName { get {
				SortedDictionary<string, IndicatorParameter> ret = new SortedDictionary<string, IndicatorParameter>();
				foreach (IndicatorParameter iParam in this.ParametersMerged) ret.Add(iParam.FullName, iParam);
				return ret;
			} }

		[JsonIgnore]	public bool WillBacktestOnAppRestart { get {
				return	this.BacktestOnRestart
					&&	this.IsStreaming
					&&	this.IsStreamingTriggeringScript;
		} }

		public ContextScript(ContextChart upgradingFromSimpleChart = null, string name = "UNDEFINED") : this(name) {
			base.AbsorbFrom(upgradingFromSimpleChart);
		}
		public ContextScript(string name = "UNDEFINED") : this() {
			this.Name = name;
		}
		protected ContextScript() : base() {
			PositionSize				= new PositionSize(PositionSizeMode.SharesConstantEachTrade, 1);
			ScriptParametersById		= new Dictionary<int, ScriptParameter>();
			IndicatorParametersByName	= new Dictionary<string, List<IndicatorParameter>>();
			
			IsCurrent					= false;
			StrategyEmittingOrders		= false;
			BacktestOnRestart			= false;
			BacktestOnSelectorsChange	= true;
			BacktestOnDataSourceSaved	= true;
			
			ReporterShortNamesUserInvokedJSONcheck	= new List<string>();
			ReportersSnapshots						= new Dictionary<string, object>();
			
			ApplyCommission					= false;
			EnableSlippage					= false;
			LimitOrderSlippage				= false;
			RoundEquityLots					= false;
			RoundEquityLotsToUpperHundred	= false;
			SlippageTicks					= 1;
			SlippageUnits					= 1.0;

			FillOutsideQuoteSpreadParanoidCheckThrow = false;
			SpreadModelerClassName		= typeof(BacktestSpreadModelerPercentage).Name;
			SpreadModelerPercent		= BacktestStreaming.PERCENTAGE_DEFAULT;
			BacktestStrokesPerBar				= BacktestStrokesPerBar.FourStrokeOHLC;
		}
		
		public ContextScript CloneAndAbsorbFromSystemPerformanceRestoreAble(SystemPerformanceRestoreAble sysPerfOptimized) {
			Assembler.PopupException("TESTME //CloneAndAbsorbFromSystemPerformanceRestoreAble()");
			ContextScript clone = this.MemberwiseCloneMadePublic();
			clone.AbsorbScriptAndIndicatorParamsOnlyFrom(
				sysPerfOptimized.ScriptParametersById_BuiltOnBacktestFinished,
				sysPerfOptimized.IndicatorParametersByName_BuiltOnBacktestFinished);
			return clone;
		}
		public void AbsorbScriptAndIndicatorParamsOnlyFrom(
					Dictionary<int, ScriptParameter>				scriptParametersById,
					Dictionary<string, List<IndicatorParameter>>	indicatorParametersByName) {
			this.ScriptParametersById			= scriptParametersById;
			this.IndicatorParametersByName		= indicatorParametersByName;
			this.CloneReferenceTypes(false);
		}
		public void AbsorbFrom(ContextScript found, bool absorbScriptAndIndicatorParams = true) {
			if (found == null) return;
			//KEEP_CLONE_UNDEFINED this.Name = found.Name;
			base.AbsorbFrom(found);
			
			this.PositionSize = found.PositionSize.Clone();
			if (absorbScriptAndIndicatorParams) {
				//v1
				//this.ScriptParametersById					= new Dictionary<int, ScriptParameter>(found.ScriptParametersById);
				//this.IndicatorParametersByName			= new Dictionary<string, List<IndicatorParameter>>(found.IndicatorParametersByName);
				//v2
				//this.ScriptParametersById					= found.ScriptParametersById;
				//this.IndicatorParametersByName			= found.IndicatorParametersByName;
				//this.CloneReferenceTypes(false);
				this.AbsorbScriptAndIndicatorParamsOnlyFrom(found.ScriptParametersById, found.IndicatorParametersByName);
			}
			
			//some of these guys can easily be absorbed by object.MemberwiseClone(), why do I prefer to maintain the growing list manually?... 
			//this.ChartBarSpacing							= found.ChartBarSpacing;
			this.StrategyEmittingOrders						= found.StrategyEmittingOrders;
			this.BacktestOnRestart							= found.BacktestOnRestart;
			this.BacktestOnSelectorsChange					= found.BacktestOnSelectorsChange;
			this.BacktestOnDataSourceSaved					= found.BacktestOnDataSourceSaved;
			this.ReporterShortNamesUserInvokedJSONcheck		= new List<string>(found.ReporterShortNamesUserInvokedJSONcheck);
			this.FillOutsideQuoteSpreadParanoidCheckThrow	= found.FillOutsideQuoteSpreadParanoidCheckThrow;
			this.BacktestStrokesPerBar								= found.BacktestStrokesPerBar;
			this.SpreadModelerClassName						= found.SpreadModelerClassName;
			this.SpreadModelerPercent						= found.SpreadModelerPercent;
		}
		public new ContextScript MemberwiseCloneMadePublic() {
			return (ContextScript)base.MemberwiseClone();
		}
		public ContextScript CloneResetAllToMinForOptimizer() {
			ContextScript ret = (ContextScript)base.MemberwiseClone();
			ret.CloneReferenceTypes();
			return ret;
		}
		public ContextScript CloneThatUserPushesFromOptimizerToStrategy(string scriptContextNewName) {
			ContextScript ret = new ContextScript(scriptContextNewName);
			ret.AbsorbFrom(this);
			return ret;
		}
		public void CloneReferenceTypes(bool resetAllToMin = true) {
			Dictionary<int, ScriptParameter> scriptParametersByIdClonedReset = new Dictionary<int, ScriptParameter>();
			foreach (int id in this.ScriptParametersById.Keys) {
				ScriptParameter sp = this.ScriptParametersById[id];
				ScriptParameter spClone = sp.Clone();
				if (resetAllToMin) spClone.ValueCurrent = spClone.ValueMin;
				scriptParametersByIdClonedReset.Add(id, spClone);
			}
			this.ScriptParametersById = scriptParametersByIdClonedReset;

			Dictionary<string, List<IndicatorParameter>> indicatorParametersByNameClonedReset = new Dictionary<string, List<IndicatorParameter>>();
			foreach (string indicatorName in this.IndicatorParametersByName.Keys) {
				List<IndicatorParameter> iParams = this.IndicatorParametersByName[indicatorName];
				List<IndicatorParameter> iParamsCloned = new List<IndicatorParameter>();
				indicatorParametersByNameClonedReset.Add(indicatorName, iParamsCloned);
				foreach (IndicatorParameter iParam in iParams) {
					IndicatorParameter ipClone = iParam.Clone();
					if (resetAllToMin) ipClone.ValueCurrent = ipClone.ValueMin;
					iParamsCloned.Add(ipClone);
				}
			}
			this.IndicatorParametersByName = indicatorParametersByNameClonedReset;
		}
		public object FindOrCreateReportersSnapshot(Reporter reporterActivated) {
			string reporterName = reporterActivated.TabText;
			if (this.ReportersSnapshots.ContainsKey(reporterName) == false) {
				this.ReportersSnapshots.Add(reporterName, reporterActivated.CreateSnapshotToStoreInScriptContext());
			}
			return this.ReportersSnapshots[reporterName];
		}
		public string ToStringSymbolScaleIntervalDataRangeForScriptContextNewName() {
			string ret = this.Symbol + " " + this.ScaleInterval + " " + this.DataRange;
			return ret;
		}

	}
}