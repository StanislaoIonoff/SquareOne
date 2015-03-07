﻿using System;
using System.Collections.Generic;

using Sq1.Core.Accounting;
using Sq1.Core.Broker;
using Sq1.Core.Support;
using Sq1.Core.Execution;
using Sq1.Core.StrategyBase;
using Sq1.Core.Backtesting;
using Sq1.Core.DataTypes;

namespace Sq1.Core.Livesim {
	[SkipInstantiationAt(Startup = true)]
	public class LivesimBroker : BrokerAdapter {
		public	List<Order>			OrdersSubmittedForOneLivesimBacktest	{ get; private set; }
				LivesimDataSource	livesimDataSource;

		public LivesimBroker(LivesimDataSource livesimDataSource) : base() {
			base.Name = "LivesimBroker";
			base.AccountAutoPropagate = new Account("LIVESIM_ACCOUNT", -1000);
			base.AccountAutoPropagate.Initialize(this);
			this.OrdersSubmittedForOneLivesimBacktest = new List<Order>();
			this.livesimDataSource = livesimDataSource;
		}

		public override void OrderSubmit(Order order) {
			this.OrdersSubmittedForOneLivesimBacktest.Add(order);
		}

		internal void ConsumeQuoteOfStreamingBarToFillPending(QuoteGenerated quoteUnattached, Bar bar2simulate) {
			ScriptExecutor executor = this.livesimDataSource.Executor;
			ExecutionDataSnapshot snap = executor.ExecutionDataSnapshot;
			if (snap.AlertsPending.Count == 0) {
				string msg = "CHECK_IT_UPSTACK_AND_DONT_INVOKE_ME!!!";
				Assembler.PopupException(msg);
				return;
			}
			//var dumped = snap.DumpPendingAlertsIntoPendingHistoryByBar();
			int dumped = snap.AlertsPending.ByBarPlaced.Count;
			if (dumped > 0) {
				//string msg = "here is at least one reason why dumping on fresh quoteToReach makes sense"
				//	+ " if we never reach this breakpoint the remove dump() from here"
				//	+ " but I don't see a need to invoke it since we dumped pendings already after OnNewBarCallback";
				string msg = "DUMPED_BEFORE_SCRIPT_EXECUTION_ON_NEW_BAR_OR_QUOTE";
			}
			int pendingCountPre = executor.ExecutionDataSnapshot.AlertsPending.Count;
			QuoteGenerated quoteAttachedToStreamingToConsumerBars = quoteUnattached.DeriveIdenticalButFresh();
			quoteAttachedToStreamingToConsumerBars.SetParentBarStreaming(this.livesimDataSource.Executor.Bars.BarStreaming);
			quoteUnattached = quoteAttachedToStreamingToConsumerBars;

			if (quoteAttachedToStreamingToConsumerBars.ParentBarStreaming.ParentBars == null) {
				string msg = "STREAMING_BAR_UNATTACHED_REPLACED_TO_SIMULATED_BARS_STREAMING_BAR QUICK_AND_DIRTY_EARLY_BINDER_HERE";
				Assembler.PopupException(msg);
				string err = "NOT_FILLED_YET";
				bool same = quoteAttachedToStreamingToConsumerBars.ParentBarStreaming.HasSameDOHLCVas(this.livesimDataSource.Executor.Bars.BarStreaming, "Executor.Bars.BarStreaming", "quote.ParentBarStreaming", ref err);
				if (same == false) {
					Assembler.PopupException("CANT_SUBSTITUTE__EXCEPTIONS_COMING" + err);
				} else {
					quoteAttachedToStreamingToConsumerBars.SetParentBarStreaming(this.livesimDataSource.Executor.Bars.BarStreaming);
				}
			}

			int pendingFilled = executor.MarketsimBacktest.SimulateFillAllPendingAlerts(
					quoteAttachedToStreamingToConsumerBars, new Action<Alert, double, double>(this.onAlertFilled));
			int pendingCountNow = executor.ExecutionDataSnapshot.AlertsPending.Count;
			if (pendingCountNow != pendingCountPre - pendingFilled) {
				string msg = "NOT_ONLY it looks like AnnihilateCounterparty worked out!";
			}
			if (pendingCountNow > 0) {
				string msg = "pending=[" + pendingCountNow + "], it must be prototype-induced 2 closing TP & SL";
			}
			//executor.Script.OnNewQuoteCallback(quoteToReach);

			ReporterPokeUnit pokeUnitNullUnsafe = executor.ExecuteOnNewBarOrNewQuote(quoteAttachedToStreamingToConsumerBars);
			//base.GeneratedQuoteEnrichSymmetricallyAndPush(quote, bar2simulate);
		}
		void onAlertFilled(Alert alertFilled, double priceFilled, double qtyFilled) {
			Order order = alertFilled.OrderFollowed;
			OrderStateMessage osm = new OrderStateMessage(order, OrderState.Filled, "LIVESIM_FILLED_THROUGH_MARKETSIM_BACKTEST");
			OrderProcessor orderProcessor = Assembler.InstanceInitialized.OrderProcessor;
			orderProcessor.UpdateOrderStateAndPostProcess(order, osm, priceFilled, qtyFilled);
			if (alertFilled.PriceFilledThroughPosition != priceFilled) {
				string msg = "WHO_FILLS_POSITION_PRICE_FILLED_THEN?";
			}
			if (alertFilled.QtyFilledThroughPosition != qtyFilled) {
				string msg = "WHO_FILLS_POSITION_QTY_FILLED_THEN?";
			}
		}
	}
}