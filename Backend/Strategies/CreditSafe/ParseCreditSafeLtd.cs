using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ezbob.Backend.ModelsWithDB.CreditSafe;
using Ezbob.CreditSafeLib;
using Ezbob.Database;
using System.Xml.Serialization;

namespace Ezbob.Backend.Strategies.CreditSafe
{
    public class ParseCreditSafeLtd : AStrategy
    {
        public ParseCreditSafeLtd(long nServiceLogID)
        {
			Result = null;
			m_nServiceLogID = nServiceLogID;
		}
        public override string Name
        {
            get { return "ParseCreditSafeLtd"; }
        }

        public override void Execute()
        {
            Log.Info("Parsing CreditSafe Ltd for service log entry {0}...", m_nServiceLogID);

            var oTbl = Save(Parse(Load()));

            if (oTbl != null)
                Result = oTbl;

            Log.Info("Parsing CreditSafe Ltd for service log entry {0} complete.", m_nServiceLogID);
        }
        public CreditSafeBaseData Result { get; private set; }

        private readonly long m_nServiceLogID;

        private Tuple<CreditSafeLtdResponse, DateTime, string> Load()
        {
            SafeReader sr = DB.GetFirst(
                "LoadServiceLogEntry",
                CommandSpecies.StoredProcedure,
                new QueryParameter("@EntryID", m_nServiceLogID)
            );

            if (sr["Id"] != m_nServiceLogID)
            {
                Log.Info("Parsing CreditSafe Consumer for service log entry {0} failed: entry not found.", m_nServiceLogID);
                return null;
            } 

            try
            {
                var outputRootSerializer = new XmlSerializer(typeof(CreditSafeLtdResponse), new XmlRootAttribute("xmlresponse"));
                var outputRoot = (CreditSafeLtdResponse)outputRootSerializer.Deserialize(new StringReader(sr["ResponseData"]));
                if (outputRoot == null)
                {
                    Log.Alert("Parsing CreditSafe Consumer for service log entry {0} failed root element is null.", m_nServiceLogID);
                    return null;
                }
                return new Tuple<CreditSafeLtdResponse, DateTime, string>(outputRoot, sr["InsertDate"], sr["CompanyRefNum"]);
            }
            catch (Exception e)
            {
                Log.Alert(e, "Parsing CreditSafe Consumer for service log entry {0} failed.", m_nServiceLogID);
                return null;
            } 
        }

    /*    private Tuple<CreditSafeLtdResponse, DateTime, string> TestLoad()
        {
            var outputRootSerializer = new XmlSerializer(typeof(CreditSafeLtdResponse), new XmlRootAttribute("xmlresponse"));
            var outputRoot = (CreditSafeLtdResponse)outputRootSerializer.Deserialize(new StringReader(response));
            return new Tuple<CreditSafeLtdResponse, DateTime, string>(outputRoot, DateTime.UtcNow, "X9999999");
        }*/

        private CreditSafeBaseData Parse(Tuple<CreditSafeLtdResponse, DateTime, string> oDoc)
        {
            if (oDoc == null || oDoc.Item1 == null )
                return null;

            Log.Info("Parsing CreditSafe company data...");
            var builder = new CreditSafeLtdModelBuilder();
            return builder.Build(oDoc.Item1, oDoc.Item2, oDoc.Item3,m_nServiceLogID);

        } 

        private CreditSafeBaseData Save(CreditSafeBaseData data)
        {
            if (data == null)
                return null;

            Log.Info("Saving CreditSafe consumer data into DB...");

            var con = DB.GetPersistent();
            con.BeginTransaction();
            try
            {
                var id = DB.ExecuteScalar<long>(
                    con,
                    "SaveCreditSafeBaseData",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<CreditSafeBaseData>("Tbl", new List<CreditSafeBaseData> { data })
                    );
                if (data.SecondarySicCodes.Any())
                {
                    foreach (var sicCode in data.SecondarySicCodes)
                    {
                        sicCode.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeBaseData_SecondarySicCodes",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeBaseData_SecondarySicCodes>("Tbl", data.SecondarySicCodes)
                        );
                }
                if (data.Industries.Any())
                {
                    foreach (var industry in data.Industries)
                    {
                        industry.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeIndustries",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeIndustries>("Tbl", data.Industries)
                        );
                }
                if (data.TradingAddresseses.Any())
                {
                    foreach (var address in data.TradingAddresseses)
                    {
                        address.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeTradingAddresses",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeTradingAddresses>("Tbl", data.TradingAddresseses)
                        );
                }
                if (data.CreditRatings.Any())
                {
                    foreach (var rating in data.CreditRatings)
                    {
                        rating.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeCreditRatings",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeCreditRatings>("Tbl", data.CreditRatings)
                        );
                }
                if (data.CreditLimits.Any())
                {
                    foreach (var limit in data.CreditLimits)
                    {
                        limit.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeCreditLimits",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeCreditLimits>("Tbl", data.CreditLimits)
                        );
                }
                if (data.PreviousNames.Any())
                {
                    foreach (var name in data.PreviousNames)
                    {
                        name.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafePreviousNames",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafePreviousNames>("Tbl", data.PreviousNames)
                        );
                }
                if (data.CcjDetails.Any())
                {
                    foreach (var ccj in data.CcjDetails)
                    {
                        ccj.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeCCJDetails",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeCCJDetails>("Tbl", data.CcjDetails)
                        );
                }
                if (data.StatusHistory.Any())
                {
                    foreach (var ccj in data.StatusHistory)
                    {
                        ccj.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeStatusHistory",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeStatusHistory>("Tbl", data.StatusHistory)
                        );
                }
                if (data.Mortgages.Any())
                {
                    foreach (var mortgage in data.Mortgages)
                    {
                        mortgage.CreditSafeBaseDataID = id;
                        var mortgageid = DB.ExecuteScalar<long>(
                            con,
                            "SaveCreditSafeMortgages",
                            CommandSpecies.StoredProcedure,
                            DB.CreateTableParameter<CreditSafeMortgages>("Tbl", new List<CreditSafeMortgages> { mortgage })
                            );
                        if (mortgage.PersonEntitled.Any())
                        {
                            foreach (var person in mortgage.PersonEntitled)
                            {
                                person.CreditSafeMortgagesID = mortgageid;
                            }
                            DB.ExecuteNonQuery(
                                con,
                                "SaveCreditSafeMortgages_PersonEntitled",
                                CommandSpecies.StoredProcedure,
                                DB.CreateTableParameter<CreditSafeMortgages_PersonEntitled>("Tbl", mortgage.PersonEntitled)
                                );
                        }
                    }
                }
                if (data.ShareHolders.Any())
                {
                    foreach (var shareholder in data.ShareHolders)
                    {
                        shareholder.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeShareHolders",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeShareHolders>("Tbl", data.ShareHolders)
                        );
                }
                if (data.Financial.Any())
                {
                    foreach (var fin in data.Financial)
                    {
                        fin.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeFinancial",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeFinancial>("Tbl", data.Financial)
                        );
                }
                if (data.EventHistory.Any())
                {
                    foreach (var hist in data.EventHistory)
                    {
                        hist.CreditSafeBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeEventHistory",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeEventHistory>("Tbl", data.EventHistory)
                        );
                }
                if (data.Directors.Any())
                {
                    foreach (var director in data.Directors)
                    {
                        director.CreditSafeBaseDataID = id;
                        var directorid = DB.ExecuteScalar<long>(
                            con,
                            "SaveCreditSafeDirectors",
                            CommandSpecies.StoredProcedure,
                            DB.CreateTableParameter<CreditSafeDirectors>("Tbl", new List<CreditSafeDirectors> { director })
                            );
                        if (director.Directorships.Any())
                        {
                            foreach (var ship in director.Directorships)
                            {
                                ship.CreditSafeDirectorsID = directorid;
                            }
                            DB.ExecuteNonQuery(
                                con,
                                "SaveCreditSafeDirectors_Directorships",
                                CommandSpecies.StoredProcedure,
                                DB.CreateTableParameter<CreditSafeDirectors_Directorships>("Tbl", director.Directorships)
                                );
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to save CreditSafe consumer");
                con.Rollback();
                return null;
            }

            con.Commit();
            Log.Info("Saving CreditSafe consumer data into DB complete.");
            return data;
        }

    }
}
