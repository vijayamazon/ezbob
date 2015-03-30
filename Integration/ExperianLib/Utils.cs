namespace ExperianLib
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using EZBob.DatabaseLib.Repository;
	using EzServiceAccessor;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using StructureMap;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class Utils
	{

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
			where TY : class {

			string serializedInput;
			string serializedOutput;
			if (typeof (TX) == typeof (string) && typeof (TY) == typeof (string)) {
				serializedInput = input as string;
				serializedOutput = output as string;
			}
			else {
				serializedInput = XSerializer.Serialize(input);
				serializedOutput = XSerializer.Serialize(output);
			}

			var pkg = new WriteToLogPackage(serializedInput, serializedOutput, type, customerId, directorId, firstname, surname, dob, postCode, companyRefNum);
			WriteLog(pkg);
			return pkg.Out;

		} // WriteLog

		public static void WriteLog(WriteToLogPackage oPackage)
		{
			if (oPackage == null)
				throw new ArgumentNullException("oPackage", "Cannot save to MP_ServiceLog: no package specified.");

			var oRetryer = new SqlRetryer(oLog: ms_oLog);

			oPackage.Out.ServiceLog = new MP_ServiceLog
			{
				InsertDate = DateTime.UtcNow,
				RequestData = oPackage.In.Request,
				ResponseData = oPackage.In.Response,
				ServiceType = oPackage.In.ServiceType.DescriptionAttr(),
				Firstname = oPackage.In.Firstname,
				Surname = oPackage.In.Surname,
				DateOfBirth = oPackage.In.DateOfBirth,
				Postcode = oPackage.In.PostCode,
				CompanyRefNum = oPackage.In.CompanyRefNum
			};

			try
			{
				if (oPackage.In.CustomerID != 0) {
					oRetryer.Retry(() => {
						var customerRepo = ObjectFactory.GetInstance<CustomerRepository>();
						oPackage.Out.ServiceLog.Customer = customerRepo.Get(oPackage.In.CustomerID);
					});
				}
				if (oPackage.In.DirectorID != null)
				{
					oRetryer.Retry(() =>
					{
						var directorRepo = ObjectFactory.GetInstance<DirectorRepository>();
						oPackage.Out.ServiceLog.Director = directorRepo.Get(oPackage.In.DirectorID);
					});
				} // if

				ms_oLog.Debug("Input data was: {0}", oPackage.Out.ServiceLog.RequestData);
				ms_oLog.Debug("Output data was: {0}", oPackage.Out.ServiceLog.ResponseData);
				var repoLog = ObjectFactory.GetInstance<ServiceLogRepository>();
				oRetryer.Retry(() => repoLog.SaveOrUpdate(oPackage.Out.ServiceLog));

				switch (oPackage.In.ServiceType)
				{
					case ExperianServiceType.LimitedData:
						oPackage.Out.ExperianLtd = ObjectFactory.GetInstance<IEzServiceAccessor>().ParseExperianLtd(oPackage.Out.ServiceLog.Id);
						break;

					case ExperianServiceType.Consumer:
						oPackage.Out.ExperianConsumer = ObjectFactory.GetInstance<IEzServiceAccessor>().ParseExperianConsumer(oPackage.Out.ServiceLog.Id);
						if (oPackage.Out.ExperianConsumer != null)
						{
							if (oPackage.Out.ServiceLog.Director != null)
							{
								oPackage.Out.ServiceLog.Director.ExperianConsumerScore = oPackage.Out.ExperianConsumer.BureauScore;
							}
							else
							{
								oPackage.Out.ServiceLog.Customer.ExperianConsumerScore = oPackage.Out.ExperianConsumer.BureauScore;
							}
						}
						repoLog.SaveOrUpdate(oPackage.Out.ServiceLog);
						break;
				} // switch

				try
				{
					var historyRepo = ObjectFactory.GetInstance<ExperianHistoryRepository>();

					var history = new MP_ExperianHistory
					{
						CustomerId = oPackage.Out.ServiceLog.Customer != null ? oPackage.Out.ServiceLog.Customer.Id : (int?)null,
						DirectorId = oPackage.Out.ServiceLog.Director != null ? oPackage.Out.ServiceLog.Director.Id : (int?)null,
						CompanyRefNum = oPackage.Out.ServiceLog.CompanyRefNum,
						ServiceLogId = oPackage.Out.ServiceLog.Id,
						Date = oPackage.Out.ServiceLog.InsertDate,
						Type = oPackage.Out.ServiceLog.ServiceType,
					};

					switch (oPackage.In.ServiceType)
					{
						case ExperianServiceType.Consumer:
							if (oPackage.Out.ExperianConsumer != null) {
								history.Score = oPackage.Out.ExperianConsumer.BureauScore;
								history.CII = oPackage.Out.ExperianConsumer.CII;
								history.CaisBalance = GetConsumerCaisBalance(oPackage.Out.ExperianConsumer.Cais);
							}
							historyRepo.SaveOrUpdate(history);
							break;

						case ExperianServiceType.LimitedData:
							history.Score = (oPackage.Out.ExperianLtd == null) ? -1 : (oPackage.Out.ExperianLtd.CommercialDelphiScore ?? -1);
							history.CaisBalance = GetLimitedCaisBalance(oPackage.Out.ExperianLtd);
							historyRepo.SaveOrUpdate(history);
							break;

						case ExperianServiceType.NonLimitedData:
							CompanyDataForCreditBureau notLimitedBusinessData = ObjectFactory.GetInstance<IEzServiceAccessor>().GetCompanyDataForCreditBureau(0/*should be refactored*/, oPackage.Out.ServiceLog.CompanyRefNum);
							history.Score = notLimitedBusinessData != null ? notLimitedBusinessData.Score : 0;
							historyRepo.SaveOrUpdate(history);
							break;
					} // switch
				}
				catch (Exception ex)
				{
					ms_oLog.Warn(ex, "Failed to save Experian history.");
				} // try
			}
			catch (Exception e)
			{
				ms_oLog.Error(e,
					"Failed to save a '{0}' entry for customer id {1} of type {2} into MP_ServiceLog.",
					oPackage.In.ServiceType, oPackage.In.CustomerID, oPackage.In.ServiceType.DescriptionAttr()
				);
			} // try
		} // WriteToLog

		public static void TryRead(Action a, string key, StringBuilder errors)
		{
			try
			{
				a();
			}
			catch (Exception e)
			{
				errors.AppendFormat("Can't read value for {0} because of exception: {1}", key, e.Message);
			} // try
		} // TryRead

		public static decimal? GetConsumerCaisBalance(List<ExperianConsumerDataCais> cais)
		{
			if (cais != null)
			{
				return cais.Where(c => c.AccountStatus != "S" && c.Balance.HasValue && c.MatchTo == 1).Sum(c => c.Balance);
			}
			return null;
		}

		public static decimal? GetLimitedCaisBalance(ExperianLtd oExperianLtd)
		{
			if (oExperianLtd == null)
				return null;

			int nFoundCount = 0;
			decimal balance = 0;

			foreach (var oRow in oExperianLtd.Children)
			{
				if (oRow.GetType() != typeof(ExperianLtdDL97))
					continue;

				nFoundCount++;

				var dl97 = (ExperianLtdDL97)oRow;

				if ((dl97.AccountState != null) && (dl97.AccountState != "S"))
					balance += dl97.CurrentBalance ?? 0;
			} // for each

			return nFoundCount == 0 ? (decimal?)null : balance;
		} // GetLimitedCaisBalance

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(Utils));

	} // class Utils
} // namespace
