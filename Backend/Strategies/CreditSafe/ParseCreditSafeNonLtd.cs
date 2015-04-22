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
    public class ParseCreditSafeNonLtd : AStrategy
    {
        public ParseCreditSafeNonLtd(long nServiceLogID)
        {
			Result = null;
			m_nServiceLogID = nServiceLogID;
		}
        public override string Name
        {
            get { return "ParseCreditSafeNonLtd"; }
        }

        public override void Execute()
        {
            Log.Info("Parsing CreditSafe Ltd for service log entry {0}...", m_nServiceLogID);
	        var loaded = Load();
	        var parsed = Parse(loaded);
            var oTbl = Save(parsed);

            if (oTbl != null)
                Result = oTbl;

            Log.Info("Parsing CreditSafe Ltd for service log entry {0} complete.", m_nServiceLogID);
        }
        public CreditSafeNonLtdBaseData Result { get; private set; }

        private readonly long m_nServiceLogID;

        private Tuple<CreditSafeNonLtdResponse, DateTime, string> Load()
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
                var outputRootSerializer = new XmlSerializer(typeof(CreditSafeNonLtdResponse), new XmlRootAttribute("xmlresponse"));
                var outputRoot = (CreditSafeNonLtdResponse)outputRootSerializer.Deserialize(new StringReader(sr["ResponseData"]));
                if (outputRoot == null)
                {
                    Log.Alert("Parsing CreditSafe Consumer for service log entry {0} failed root element is null.", m_nServiceLogID);
                    return null;
                }
                return new Tuple<CreditSafeNonLtdResponse, DateTime, string>(outputRoot, sr["InsertDate"], sr["CompanyRefNum"]);
            }
            catch (Exception e)
            {
                Log.Alert(e, "Parsing CreditSafe Consumer for service log entry {0} failed.", m_nServiceLogID);
                return null;
            } 
        }

     /*   private Tuple<CreditSafeNonLtdResponse, DateTime, string> TestLoad()
        {
            var outputRootSerializer = new XmlSerializer(typeof(CreditSafeNonLtdResponse), new XmlRootAttribute("xmlresponse"));
            var outputRoot = (CreditSafeNonLtdResponse)outputRootSerializer.Deserialize(new StringReader(response));
            return new Tuple<CreditSafeNonLtdResponse, DateTime, string>(outputRoot, DateTime.UtcNow, "X9999999");
        }*/

        private CreditSafeNonLtdBaseData Parse(Tuple<CreditSafeNonLtdResponse, DateTime, string> oDoc)
        {
            if (oDoc == null || oDoc.Item1 == null )
                return null;

            Log.Info("Parsing CreditSafe company data...");
            var builder = new CreditSafeNonLtdModelBuilder();
            return builder.Build(oDoc.Item1, oDoc.Item2, oDoc.Item3,m_nServiceLogID);

        }

        private CreditSafeNonLtdBaseData Save(CreditSafeNonLtdBaseData data)
        {
            if (data == null)
                return null;

            Log.Info("Saving NonLtdCreditSafe consumer data into DB...");

            var con = DB.GetPersistent();
            con.BeginTransaction();
            try
            {
                var id = DB.ExecuteScalar<long>(
                    con,
                    "SaveCreditSafeNonLtdBaseData",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<CreditSafeNonLtdBaseData>("Tbl", new List<CreditSafeNonLtdBaseData> { data })
                    );
                if (data.Tels.Any())
                {
                    foreach (var tel in data.Tels)
                    {
                        tel.CreditSafeNonLtdBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeNonLtdBaseDataTel",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeNonLtdBaseDataTel>("Tbl", data.Tels)
                        );
                }
                if (data.Faxs.Any())
                {
                    foreach (var fax in data.Faxs)
                    {
                        fax.CreditSafeNonLtdBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeNonLtdBaseDataFax",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeNonLtdBaseDataFax>("Tbl", data.Faxs)
                        );
                }
                if (data.Ratings.Any())
                {
                    foreach (var rating in data.Ratings)
                    {
                        rating.CreditSafeNonLtdBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeNonLtdRatings",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeNonLtdRatings>("Tbl", data.Ratings)
                        );
                }
                if (data.Limits.Any())
                {
                    foreach (var limit in data.Limits)
                    {
                        limit.CreditSafeNonLtdBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeNonLtdLimits",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeNonLtdLimits>("Tbl", data.Limits)
                        );
                }
                if (data.MatchedCcj.Any())
                {
                    foreach (var matched in data.MatchedCcj)
                    {
                        matched.CreditSafeNonLtdBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeNonLtdMatchedCCJ",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeNonLtdMatchedCCJ>("Tbl", data.MatchedCcj)
                        );
                }
                if (data.PossibleCcj.Any())
                {
                    foreach (var possible in data.PossibleCcj)
                    {
                        possible.CreditSafeNonLtdBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeNonLtdPossibleCCJ",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeNonLtdPossibleCCJ>("Tbl", data.PossibleCcj)
                        );
                }
                if (data.Events.Any())
                {
                    foreach (var ev in data.Events)
                    {
                        ev.CreditSafeNonLtdBaseDataID = id;
                    }
                    DB.ExecuteNonQuery(
                        con,
                        "SaveCreditSafeNonLtdEvents",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<CreditSafeNonLtdEvents>("Tbl", data.Events)
                        );
                }

            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to save NonLtdCreditSafe consumer");
                con.Rollback();
                return null;
            }

            con.Commit();
            Log.Info("Saving NonLtdCreditSafe consumer data into DB complete.");
            return data;
        }

    }
}
