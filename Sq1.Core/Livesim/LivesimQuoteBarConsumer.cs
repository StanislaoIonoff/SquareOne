﻿using System;

using Sq1.Core.DataTypes;
using Sq1.Core.Execution;
using Sq1.Core.Streaming;
using Sq1.Core.StrategyBase;

namespace Sq1.Core.Livesim {
	public class LivesimQuoteBarConsumer : IStreamingConsumer {
		protected		Livesimulator Livesimulator;

		public	LivesimQuoteBarConsumer(Livesimulator livesimulator) {
			this.Livesimulator = livesimulator;
		}
				Bars IStreamingConsumer.ConsumerBarsToAppendInto { get { return Livesimulator.BarsSimulating; } }
		void IStreamingConsumer.UpstreamSubscribedToSymbolNotification(Quote quoteFirstAfterStart) {
		}
		void IStreamingConsumer.UpstreamUnSubscribedFromSymbolNotification(Quote quoteLastBeforeStop) {
		}
		void IStreamingConsumer.ConsumeQuoteOfStreamingBar(Quote quote) {
			bool guiHasTime = this.Livesimulator.LivesimStreamingIsSleepingNow_ReportersAndExecutionHaveTimeToRebuild;
			ScriptExecutor executor = this.Livesimulator.Executor;
			ReporterPokeUnit pokeUnitNullUnsafe = this.Livesimulator.Executor.ExecuteOnNewBarOrNewQuote(quote);
			if (pokeUnitNullUnsafe != null && pokeUnitNullUnsafe.PositionsOpenNow.Count > 0) {
				executor.PerformanceAfterBacktest.BuildIncrementalOpenPositionsUpdatedDueToStreamingNewQuote_step2of3(executor.ExecutionDataSnapshot.PositionsOpenNow);
				if (guiHasTime) {
					executor.EventGenerator.RaiseOpenPositionsUpdatedDueToStreamingNewQuote_step2of3(pokeUnitNullUnsafe);
				}
			}
			if (guiHasTime) {
				// ALREADY_HANDLED_BY chartControl_BarStreamingUpdatedMerged_ShouldTriggerRepaint_WontUpdateBtnTriggeringScriptTimeline
				//executor.ChartShadow.Invalidate();
				//executor.ChartShadow.InvalidateAllPanels();
				//executor.ChartShadow.RefreshAllPanelsWaitFinishedSoLivesimCouldGenerateNewQuote(0);
			}
		}
		void IStreamingConsumer.ConsumeBarLastStaticJustFormedWhileStreamingBarWithOneQuoteAlreadyAppended(Bar barLastFormed, Quote quoteForAlertsCreated) {
			string msig = " //BacktestQuoteBarConsumer.ConsumeBarLastStaticJustFormedWhileStreamingBarWithOneQuoteAlreadyAppended";
			if (barLastFormed == null) {
				string msg = "THERE_IS_NO_STATIC_BAR_DURING_FIRST_4_QUOTES_GENERATED__ONLY_STREAMING"
					+ " Backtester starts generating quotes => first StreamingBar is added;"
					+ " for first four Quotes there's no static barsFormed yet!! Isi";
				Assembler.PopupException(msg + msig, null, false);
				return;
			}
			msig += "(" + barLastFormed.ToString() + ")";
			//v1 this.backtester.Executor.Strategy.Script.OnBarStaticLastFormedWhileStreamingBarWithOneQuoteAlreadyAppendedCallback(barLastFormed);
			ReporterPokeUnit pokeUnitNullUnsafe = this.Livesimulator.Executor.ExecuteOnNewBarOrNewQuote(quoteForAlertsCreated, false);
		}
		public override string ToString() {
			string ret = "CONSUMER_FOR_" + this.Livesimulator.ToString();
			return ret;
		}
	}
}
