﻿namespace EzBob.Web.Infrastructure {
	using System;
	using System.Data;
	using Ezbob.Logger;
	using NHibernate;
	using StructureMap;

	public class Transactional {
		#region public

		#region static method Execute

		public static void Execute(Action oAction, IsolationLevel nLevel = IsolationLevel.ReadCommitted) {
			new Transactional(oAction, nLevel).Execute();
		} // constructor

		#endregion static method Execute

		#region constructor

		public Transactional(Action oAction, IsolationLevel nLevel = IsolationLevel.ReadCommitted) {
			m_oAction = oAction;
			IsolationLevel = nLevel;
			m_oLog = new SafeILog(this);
		} // constructor

		#endregion constructor

		#region property IsolationLevel

		public IsolationLevel IsolationLevel { get; private set; }

		#endregion property IsolationLevel

		#region method Execute

		public void Execute() {
			if (m_oAction == null)
				return;

			ISession instance = ObjectFactory.GetInstance<ISession>();

			ITransaction tran = null;

			try {
				tran = instance.Transaction;

				if (tran == null || !tran.IsActive)
					tran = instance.BeginTransaction(IsolationLevel);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Exception caught while opening a transaction.");
			} // try

			if (tran == null)
				return;

			m_oLog.Debug("Transaction started.");

			bool bCommit = false;
			Exception oExceptionToThrow = null;

			try {
				m_oAction();
				bCommit = true;
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Exception caught while transactional execution.");
				oExceptionToThrow = e;
			} // try

			m_oLog.Debug("Transactional execution done.");

			try {
				if (tran.IsActive) {
					if (bCommit)
						tran.Commit();
					else
						tran.Rollback();
				} // if transaction is active
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Exception caught while closing transaction.");
			} // try

			m_oLog.Debug("Transaction complete.");

			if (oExceptionToThrow != null)
				throw oExceptionToThrow;
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly Action m_oAction;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // Transactional
} // namespace