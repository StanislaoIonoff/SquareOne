﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;

using Sq1.Core.Broker;
using Sq1.Core.DataFeed;
using Sq1.Core.Accounting;

namespace Sq1.Adapters.Quik {
	public partial class BrokerQuikEditor {
		public string QuikFolder {
			get { return this.txtQuikFolder.Text; }
			set { this.txtQuikFolder.Text = value; }
		}
		public int ReconnectTimeoutMillis {
			get {
				int ret = 0;
				try {
					ret = Convert.ToInt32(this.txtReconnectTimeoutMillis.Text);
					this.txtReconnectTimeoutMillis.BackColor = Color.White;
				} catch (Exception e) {
					this.txtReconnectTimeoutMillis.BackColor = Color.LightCoral;
					this.txtReconnectTimeoutMillis.Text = "1000";	// induce one more event?...
				}
				return ret;
			}
			set { this.txtReconnectTimeoutMillis.Text = value.ToString(); }
		}
		public Account Account {
			get {
				Account ret;
				try {
					ret = new Account(this.txtQuikAccount.Text,
						Convert.ToDouble(this.txtCashAvailable.Text));
				} catch (Exception e) {
					ret = new Account();
					ret.AccountNumber = this.txtQuikAccount.Text;
				}
				return ret;
			}
			set {
				if (value == null) return;
				this.txtQuikAccount.Text = value.AccountNumber;
				this.txtCashAvailable.Text = value.CashAvailable.ToString();
			}
		}
		public Account AccountMicex {
			get {
				Account ret;
				try {
					ret = new Account(this.txtQuikAccountMicex.Text, Convert.ToDouble(this.txtCashAvailableMicex.Text));
				} catch (Exception e) {
					ret = new Account();
					ret.AccountNumber = this.txtQuikAccountMicex.Text;
				}
				return ret;
			}
			set {
				if (value == null) return;
				this.txtQuikAccountMicex.Text = value.AccountNumber;
				this.txtCashAvailableMicex.Text = value.CashAvailable.ToString();
			}
		}
		BrokerQuik BrokerQuik {
			get { return base.brokerProvider as BrokerQuik; }
		}

		public BrokerQuikEditor(BrokerQuik BrokerQuik, IDataSourceEditor dataSourceEditor)
			: base(BrokerQuik, dataSourceEditor) {
			InitializeComponent();
			base.InitializeEditorFields();
		}
		public override void PushBrokerProviderSettingsToEditor() {
			this.Account = this.BrokerQuik.AccountAutoPropagate;
			// quik-specific
			this.AccountMicex = this.BrokerQuik.AccountMicexAutoPopulated;
			this.QuikFolder = this.BrokerQuik.QuikFolder;
			this.ReconnectTimeoutMillis = Convert.ToInt32(this.BrokerQuik.ReconnectTimeoutMillis);
			//QuikClientCode = SettingsEditor.QuikClientCode;
		}
		public override void PushEditedSettingsToBrokerProvider() {
			if (base.ignoreEditorFieldChangesWhileInitializingEditor) return;
			this.BrokerQuik.AccountAutoPropagate = this.Account;
			// quik-specific
			this.BrokerQuik.AccountMicexAutoPopulated = this.AccountMicex;
			this.BrokerQuik.QuikFolder = QuikFolder;
			this.BrokerQuik.ReconnectTimeoutMillis = ReconnectTimeoutMillis;
			//this.editor.QuikClientCode = QuikClientCode;
		}
    }
}