﻿namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Globalization;
	using System.Threading;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetAvailableFunds : AStrategy {

		static GetAvailableFunds() {
			ms_bStopBackgroundThread = false;
			ms_oThread = null;
			ms_oCultureInfo = new CultureInfo("en-GB", false);
			ms_oDB = null;
			ms_oLog = null;
			ms_nAvailableFunds = 0;
			ms_nReservedAmount = 0;
			ms_oDataLock = new object();
			ms_oProcLock = new object();
		} // GetAvailableFunds

		public static void LoadFromDB() {
			ms_oLog.Debug("Loading available funds from DB...");

			SafeReader sr;

			try {
				sr = ms_oDB.GetFirst("GetAvailableFunds", CommandSpecies.StoredProcedure);
			}
			catch (Exception e) {
				ms_oLog.Alert(
					e,
					"Failed to load available funds from DB, continuing with the previous loaded values."
				);

				return;
			} // try

			decimal nAvailableFunds = sr["AvailableFunds"];
			decimal nReservedAmount = sr["ReservedAmount"];

			lock (ms_oDataLock) {
				ms_nAvailableFunds = nAvailableFunds;
				ms_nReservedAmount = nReservedAmount;
			} // lock

			ms_oLog.Debug(
				"Available funds: {0}, reserved amount: {1}.", 
				nAvailableFunds.ToString("C2", ms_oCultureInfo),
				nReservedAmount.ToString("C2", ms_oCultureInfo)
			);
		} // LoadFromDB

		public GetAvailableFunds() {
			if (ms_oDB == null) {
				lock (ms_oProcLock) {
					if (ms_oDB == null) {
						ms_oDB = DB;
						ms_oLog = Log;
						LoadFromDB();

						ms_oThread = new Thread(LoadFromDBThread);
						ms_oThread.Start();

						Log.Debug("Available funds loader: background thread started with id {0}.", ms_oThread.ManagedThreadId);
					} // if
				} // lock
			} // if
		} // constructor

		public override string Name {
			get { return "Get Available Funds"; }
		} // Name

		public decimal AvailableFunds { get; private set; }

		public decimal ReservedAmount { get; private set; }

		public override void Execute() {
			lock (ms_oDataLock) {
				AvailableFunds = ms_nAvailableFunds;
				ReservedAmount = ms_nReservedAmount;
			} // lock
		} // Execute

		public static bool StopBackgroundThread {
			get {
				lock (ms_oProcLock) {
					return ms_bStopBackgroundThread;
				} // lock
			} // get
			set {
				lock (ms_oProcLock) {
					ms_bStopBackgroundThread = value;
				} // lock
			} // set
		} // StopBackgroundThread

		private static bool ms_bStopBackgroundThread;

		private static decimal ms_nAvailableFunds;
		private static decimal ms_nReservedAmount;
		private static volatile AConnection ms_oDB;
		private static ASafeLog ms_oLog;
		private static readonly CultureInfo ms_oCultureInfo;
		private static Thread ms_oThread;

		private static readonly object ms_oDataLock;
		private static readonly object ms_oProcLock;

		private static void LoadFromDBThread() {
			bool bStop = false;
			const int nStep = 100;
			int nRefreshInterval = CurrentValues.Instance.AvailableFundsRefreshInterval;

			for ( ; ; ) {
				ms_oLog.Debug("Available funds loader: sleeping...");

				for (int i = 0; i < nRefreshInterval; i += nStep) {
					if (StopBackgroundThread) {
						bStop = true;
						break;
					} // if

					Thread.Sleep(nStep);
				} // if

				if (bStop)
					break;

				LoadFromDB();
				nRefreshInterval = CurrentValues.Instance.AvailableFundsRefreshInterval;
			} // forever
		} // LoadFromDBThread

	} // class GetAvailableFunds
} // namespace
