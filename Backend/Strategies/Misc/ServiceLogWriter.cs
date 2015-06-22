namespace Ezbob.Backend.Strategies.Misc
{
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.Experian;
    using Ezbob.Database;
    using Ezbob.Utils.Extensions;
    using EZBob.DatabaseLib.Model.Database;
    using EZBob.DatabaseLib.Model.Database.Mapping;
    using EZBob.DatabaseLib.Model.Database.Repository;
    using EZBob.DatabaseLib.Model.Experian;
    using EZBob.DatabaseLib.Repository;
    using StructureMap;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.ModelsWithDB.Experian;
    using Ezbob.Backend.Strategies.CallCreditStrategy;
    using Ezbob.Backend.Strategies.CreditSafe;

    public class ServiceLogWriter:AStrategy
    {
        public ServiceLogWriter(WriteToLogPackage oPackage) {
            Package = oPackage;
        }
        
        public override string Name {
            get { return "ServiceLogWriter"; }
        }

        public override void Execute() {
            WriteLog(Package);
        }

        public void WriteLog(WriteToLogPackage oPackage)
        {
            if (oPackage == null)
                throw new ArgumentNullException("oPackage", "Cannot save to MP_ServiceLog: no package specified.");

            var oRetryer = new SqlRetryer(oLog: Log);

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
                if (oPackage.In.CustomerID != 0)
                {
                    oRetryer.Retry(() =>
                    {
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

                Log.Debug("Input data was: {0}", oPackage.Out.ServiceLog.RequestData);
                Log.Debug("Output data was: {0}", oPackage.Out.ServiceLog.ResponseData);
                var repoLog = ObjectFactory.GetInstance<ServiceLogRepository>();
                oRetryer.Retry(() => repoLog.SaveOrUpdate(oPackage.Out.ServiceLog));

                switch (oPackage.In.ServiceType)
                {
                    case ExperianServiceType.LimitedData:
                        var parseExperianLtd = new ParseExperianLtd(oPackage.Out.ServiceLog.Id);
                        parseExperianLtd.Execute();
                        oPackage.Out.ExperianLtd = parseExperianLtd.Result;
                        break;

                    case ExperianServiceType.Consumer:
                        var stra = new ParseExperianConsumerData(oPackage.Out.ServiceLog.Id);
			            stra.Execute();
                        oPackage.Out.ExperianConsumer = stra.Result;

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
                    case ExperianServiceType.CreditSafeNonLtd:
                        var parseCreditSafeNonLtdData = new ParseCreditSafeNonLtd(oPackage.Out.ServiceLog.Id);
                        parseCreditSafeNonLtdData.Execute();
                        break;
					case ExperianServiceType.CallCredit:
						var parseCallCredit = new ParseCallCredit(oPackage.Out.ServiceLog.Id);
						parseCallCredit.Execute();
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
                            if (oPackage.Out.ExperianConsumer != null)
                            {
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
                            GetCompanyDataForCreditBureau strategyInstance = new GetCompanyDataForCreditBureau(oPackage.Out.ServiceLog.CompanyRefNum);

                			strategyInstance.Execute();

                            var notLimitedBusinessData = new CompanyDataForCreditBureau
                            {
				                LastUpdate = strategyInstance.LastUpdate,
				                Score = strategyInstance.Score,
				                Errors = strategyInstance.Errors
			                };

                            history.Score = notLimitedBusinessData != null ? notLimitedBusinessData.Score : 0;
                            historyRepo.SaveOrUpdate(history);
                            break;
                    } // switch
                }
                catch (Exception ex)
                {
                    Log.Warn(ex, "Failed to save Experian history.");
                } // try
            }
            catch (Exception e)
            {
                Log.Error(e,
                    "Failed to save a '{0}' entry for customer id {1} of type {2} into MP_ServiceLog.",
                    oPackage.In.ServiceType, oPackage.In.CustomerID, oPackage.In.ServiceType.DescriptionAttr()
                );
            } // try
        } // WriteToLog

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

        public WriteToLogPackage Package { get; set; }
    }
}
