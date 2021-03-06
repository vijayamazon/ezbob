﻿namespace ExperianLib {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using EzServiceAccessor;
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap;

	public class Utils {
		public static WriteToLogPackage.OutputData WriteLog<TX, TY>(
			TX input,
			TY output,
			ExperianServiceType type,
			int customerId,
			int? directorId = null,
			string firstname = null,
			string surname = null,
			DateTime? dob = null,
			string postCode = null,
			string companyRefNum = null
		)
			where TX : class
			where TY : class
		{
			string serializedInput;
			string serializedOutput;

			if (typeof(TX) == typeof(string) && typeof(TY) == typeof(string)) {
				serializedInput = input as string;
				serializedOutput = output as string;
			} else {
				serializedInput = XSerializer.Serialize(input);
				serializedOutput = XSerializer.Serialize(output);
			} // if

			var pkg = new WriteToLogPackage(
				serializedInput,
				serializedOutput,
				type,
				customerId,
				directorId,
				firstname,
				surname,
				dob,
				postCode,
				companyRefNum
			);

			return WriteLog(pkg);
		} // WriteLog

		public static WriteToLogPackage.OutputData WriteLog(WriteToLogPackage package) {
			return ObjectFactory.GetInstance<IEzServiceAccessor>().ServiceLogWriter(package);
		} // WriteToLog

		public static void TryRead(Action a, string key, StringBuilder errors) {
			try {
				a();
			} catch (Exception e) {
				errors.AppendFormat("Can't read value for {0} because of exception: {1}", key, e.Message);
			} // try
		} // TryRead

		public static decimal? GetConsumerCaisBalance(List<ExperianConsumerDataCais> cais) {
			return cais == null
				? null
				: cais.Where(c => c.AccountStatus != "S" && c.Balance.HasValue && c.MatchTo == 1).Sum(c => c.Balance);
		} // GetConsumerCaisBalance

		public static decimal? GetLimitedCaisBalance(ExperianLtd oExperianLtd) {
			if (oExperianLtd == null)
				return null;

			int nFoundCount = 0;
			decimal balance = 0;

			foreach (var oRow in oExperianLtd.Children) {
				if (oRow.GetType() != typeof(ExperianLtdDL97))
					continue;

				nFoundCount++;

				var dl97 = (ExperianLtdDL97)oRow;

				if ((dl97.AccountState != null) && (dl97.AccountState != "S"))
					balance += dl97.CurrentBalance ?? 0;
			} // for each

			return nFoundCount == 0 ? (decimal?)null : balance;
		} // GetLimitedCaisBalance
	} // class Utils
} // namespace
