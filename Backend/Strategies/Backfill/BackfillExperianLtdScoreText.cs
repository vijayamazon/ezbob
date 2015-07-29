namespace Ezbob.Backend.Strategies.Experian
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Ezbob.Database;

    public class BackfillExperianLtdScoreText : AStrategy
    {

        public override string Name
        {
            get { return "BackfillExperianLtdScoreText"; }
        } // Name

        public override void Execute()
        {
            IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogScoreTextBackfill", CommandSpecies.StoredProcedure);

            foreach (SafeReader sr in lst)
            {
                long ServiceLogID = sr["Id"];
                string ResponseData = sr["ResponseData"];

                if (string.IsNullOrEmpty(ResponseData))
                    continue;
                try {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(ResponseData);
                    XmlNode dl79Node = doc.SelectSingleNode("/GEODS/REQUEST/DL79");

                    string text1 = TryRead(dl79Node, "CREDITTEXT1");
                    string text2 = TryRead(dl79Node, "CREDITTEXT2");
                    string text3 = TryRead(dl79Node, "CREDITTEXT3");
                    string text4 = TryRead(dl79Node, "CREDITTEXT4");
                    string text5 = TryRead(dl79Node, "CREDITTEXT5");
                    string context = TryRead(dl79Node, "CONCLUSIONTEXT");

                    DB.ExecuteNonQuery(
                        "UpdateExperianLtdScoreText",
                        CommandSpecies.StoredProcedure,
                        new QueryParameter("id", ServiceLogID),
                        new QueryParameter("text1", text1),
                        new QueryParameter("text2", text2),
                        new QueryParameter("text3", text3),
                        new QueryParameter("text4", text4),
                        new QueryParameter("text5", text5),
                        new QueryParameter("context", context));
                } catch (Exception e) {
                    //Do nothing
                }

            } // for each

        } // Execute

        private string TryRead(XmlNode node, string text)
        {
            try
            {
                return node[text].InnerText;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    } // class BackfillExperianLtdScoreText
} // namespace
