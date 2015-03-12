﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Sq1.Core.Indicators;
using Sq1.Core.StrategyBase;

namespace Sq1.Core.Execution {
	public partial class ExecutionDataSnapshot {
			   ScriptExecutor					executor;
			   object 							alertsMasterLock;
			   object							positionsMasterLock;
		
		public	AlertList						AlertsMaster				{ get; private set; }
		public	AlertList						AlertsNewAfterExec			{ get; private set; }
		public	AlertList						AlertsPending				{ get; private set; }
		//public Dictionary<int, List<Alert>>	AlertsPendingHistorySafeCopy { get { return this.AlertsPendingHistorySafeCopyForRenderer(0, -1); } }

		public	int								positionSernoAbs			{ get; private set; }
		public	PositionList					PositionsMaster				{ get; private set; }
		public	PositionList					PositionsOpenedAfterExec	{ get; private set; }
		public	PositionList					PositionsClosedAfterExec	{ get; private set; }
		public	PositionList					PositionsOpenNow			{ get; private set; }

		public	Dictionary<string, Indicator>	IndicatorsReflectedScriptInstances;

		public ExecutionDataSnapshot(ScriptExecutor strategyExecutor) {
			this.executor						= strategyExecutor;
			alertsMasterLock					= new object();
			positionsMasterLock					= new object();
			AlertsPending = new AlertList("AlertsPending", this);
			AlertsMaster = new AlertList("AlertsMaster", this);
			AlertsNewAfterExec = new AlertList("AlertsNewAfterExec", this);
			positionSernoAbs					= 0;
			PositionsMaster = new PositionList("PositionsMaster", this);
			PositionsOpenNow = new PositionList("PositionsOpenNow", this);
			PositionsOpenedAfterExec = new PositionList("PositionsOpenedAfterExec", this);
			PositionsClosedAfterExec = new PositionList("PositionsClosedAfterExec", this);
			IndicatorsReflectedScriptInstances	= new Dictionary<string, Indicator>();
			this.initializeScriptExecWatchdog();
		}

		public void Initialize() { lock (this.positionsMasterLock) {
			this.AlertsMaster				.Clear();
			this.AlertsNewAfterExec			.Clear();
			this.AlertsPending				.Clear();
			this.positionSernoAbs			= 0;
			this.PositionsMaster			.Clear();
			this.PositionsOpenedAfterExec	.Clear();
			this.PositionsClosedAfterExec	.Clear();
			this.PositionsOpenNow			.Clear();
		} }
		internal void PreExecutionOnNewBarOrNewQuoteClear() { lock (this.positionsMasterLock) {
			this.AlertsNewAfterExec.Clear();
			this.PositionsOpenedAfterExec.Clear();
			this.PositionsClosedAfterExec.Clear();
		} }
		internal void PositionsMasterOpenNewAdd(Position positionOpening) { lock (this.positionsMasterLock) {
			if (positionOpening.EntryFilledBarIndex == -1) {
				string msg = "ENTRY_BAR_NEGATIVE_CAN_NOT_STORE_POSITION_IN_PositionsMaster.ByEntryBarFilled"
					+ " Strategy[" + this.executor.Strategy.ToString() + "] EntryBar=-1 for position[" + positionOpening + "]";
				Assembler.PopupException(msg);
				return;
			}
				
			positionOpening.SernoAbs = ++this.positionSernoAbs;
			this.PositionsMaster.AddOpened_step1of2(positionOpening);
			this.PositionsOpenedAfterExec.AddOpened_step1of2(positionOpening);
			this.PositionsOpenNow.AddOpened_step1of2(positionOpening);
		} }
		public void AlertEnrichedRegister(Alert alert, bool registerInNewAfterExec = false) { lock (this.alertsMasterLock) {
			if (alert.Qty == 0.0) {
				string msg = "alert[" + alert + "].Qty==0; hopefully will be displayed but not executed...";
				throw new Exception(msg);
			}
			if (alert.Strategy.Script == null) {
				string msg = "TODO NYI alert submitted from mni / onChartTrading";
			}
			if (this.AlertsMaster.ContainsIdentical(alert)) {
				string msg = "AlertsMasterContainsIdentical=>won't add NewPending;"
					+ " 1) broker's order status dupe? 2) are you using CoverAtStop() in your strategy?"
					+ " //" + alert;
				Assembler.PopupException(msg);
				return;
			}
			this.AlertsMaster.AddNoDupe(alert);
			if (registerInNewAfterExec == true) this.AlertsNewAfterExec.AddNoDupe(alert);
			ByBarDumpStatus dumped = this.AlertsPending.AddNoDupe(alert);
			switch (dumped) {
				case ByBarDumpStatus.BarAlreadyContainedTheAlertToAdd:
					string msg1 = "DUPE while adding JUST CREATED??? alert[" + alert + "]";
					throw new Exception(msg1);
					break;
				case ByBarDumpStatus.SequentialAlertAddedForExistingBarInHistory:
					string msg2 = "Here is the case when PrototypeActivator changed alert[" + alert + "]";
					break;
			}
		} }
		public void MovePositionOpenToClosed(Position positionClosing, bool absenseInPositionsOpenNowIsAnError = true) { lock (this.positionsMasterLock) {
			bool added = this.PositionsMaster.AddToClosedDictionary_step2of2(positionClosing, absenseInPositionsOpenNowIsAnError);
			this.PositionsClosedAfterExec.AddClosed(positionClosing);
			this.PositionsOpenNow.Remove(positionClosing);
		} }
	}
}
