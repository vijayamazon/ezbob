namespace ExperianLib.EBusiness {
	using System;
	using System.Globalization;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public class NonLimitedParser 
	{
		private readonly AConnection db;
		private readonly SafeILog log;

		public NonLimitedParser()
		{
			log = new SafeILog(LogManager.GetLogger(typeof(NonLimitedParser)));
			var env = new Ezbob.Context.Environment(log);
			db = new SqlConnection(env, log);
		}

		public void ParseAndStore(int customerId, string xml, string refNumber, long serviceLogId) 
		{
			string businessName = string.Empty;
			string address1 = string.Empty;
			string address2 = string.Empty;
			string address3 = string.Empty;
			string address4 = string.Empty;
			string address5 = string.Empty;
			string postcode = string.Empty;
			DateTime? incorporationDate = null;
			int score = 0;
			int creditLimit = 0;
			int riskScore = 0;
			int ageOfMostRecentCcj = 0;
			int numOfCcjsInLast12Months = 0;
			int numOfCcjsIn13To24Months = 0;
			int sumOfCcjsInLast12Months = 0;
			int sumOfCcjsIn13To24Months = 0;
			int numOfCcjsInLast24Months = 0;
			int numOfAssociatedCcjsInLast24Months = 0;
			int sumOfCcjsInLast24Months = 0;
			int sumOfAssociatedCcjsInLast24Months = 0;
			
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);

			XmlNodeList dn10Nodes = xmlDoc.SelectNodes("//DN10");
			if (dn10Nodes != null && dn10Nodes.Count == 1) 
			{
				XmlNode dn10Node = dn10Nodes[0];
				XmlNode node = dn10Node.SelectSingleNode("BUSINESSNAME");
				if (node != null)
				{
					businessName = node.InnerText;
				}
				node = dn10Node.SelectSingleNode("BUSADDR1");
				if (node != null)
				{
					address1 = node.InnerText;
				}
				node = dn10Node.SelectSingleNode("BUSADDR2");
				if (node != null)
				{
					address2 = node.InnerText;
				}
				node = dn10Node.SelectSingleNode("BUSADDR3");
				if (node != null)
				{
					address3 = node.InnerText;
				}
				node = dn10Node.SelectSingleNode("BUSADDR4");
				if (node != null)
				{
					address4 = node.InnerText;
				}
				node = dn10Node.SelectSingleNode("BUSADDR5");
				if (node != null)
				{
					address5 = node.InnerText;
				}
				node = dn10Node.SelectSingleNode("BUSPOSTCODE");
				if (node != null)
				{
					postcode = node.InnerText;
				}

				incorporationDate = GetDateFromNode(dn10Node, "DATEOWNSHPCOMMD") ?? GetDateFromNode(dn10Node, "EARLIESTKNOWNDATE");
			}

			XmlNodeList dn40Nodes = xmlDoc.SelectNodes("//DN40");
			if (dn40Nodes != null && dn40Nodes.Count == 1)
			{
				XmlNode dn40Node = dn40Nodes[0];
				riskScore = GetIntValueOrDefault(dn40Node, "RISKSCORE");
			}

			XmlNodeList dn73Nodes = xmlDoc.SelectNodes("//DN73");
			if (dn73Nodes != null && dn73Nodes.Count == 1)
			{
				XmlNode dn73Node = dn73Nodes[0];
				score = GetIntValueOrDefault(dn73Node, "NLCDSCORE");
				creditLimit = GetIntValueOrDefault(dn73Node, "CREDITLIMIT");
			}

			XmlNodeList dn14Nodes = xmlDoc.SelectNodes("//DN14");
			if (dn14Nodes != null && dn14Nodes.Count == 1)
			{
				XmlNode dn14Node = dn14Nodes[0];
				ageOfMostRecentCcj = GetIntValueOrDefault(dn14Node, "MAGEMOSTRECJUDGSINCEOWNSHP");
				numOfCcjsInLast12Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGCOUNTLST12MNTHS");
				numOfCcjsIn13To24Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGCOUNTLST13TO24MNTHS");
				sumOfCcjsInLast12Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGVALUELST12MNTHS");
				sumOfCcjsIn13To24Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGVALUELST13TO24MNTHS");
				numOfCcjsInLast24Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGCOUNTLST24MNTHS");
				numOfAssociatedCcjsInLast24Months = GetIntValueOrDefault(dn14Node, "ATOTJUDGCOUNTLST24MNTHS");
				sumOfCcjsInLast24Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGVALUELST24MNTHS");
				sumOfAssociatedCcjsInLast24Months = GetIntValueOrDefault(dn14Node, "ATOTJUDGVALUELST24MNTHS");
			}

			db.ExecuteNonQuery(
				"InsertNonLimitedResult",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber),
				new QueryParameter("ServiceLogId", serviceLogId),
				new QueryParameter("Created", DateTime.UtcNow),
				new QueryParameter("BusinessName", businessName),
				new QueryParameter("Address1", address1),
				new QueryParameter("Address2", address2),
				new QueryParameter("Address3", address3),
				new QueryParameter("Address4", address4),
				new QueryParameter("Address5", address5),
				new QueryParameter("Postcode", postcode),
				new QueryParameter("IncorporationDate", incorporationDate),
				new QueryParameter("RiskScore", riskScore),
				new QueryParameter("Score", score),
				new QueryParameter("CreditLimit", creditLimit),
				new QueryParameter("AgeOfMostRecentCcj", ageOfMostRecentCcj),
				new QueryParameter("NumOfCcjsInLast12Months", numOfCcjsInLast12Months),
				new QueryParameter("NumOfCcjsIn13To24Months", numOfCcjsIn13To24Months),
				new QueryParameter("SumOfCcjsInLast12Months", sumOfCcjsInLast12Months),
				new QueryParameter("SumOfCcjsIn13To24Months", sumOfCcjsIn13To24Months),
				new QueryParameter("NumOfCcjsInLast24Months", numOfCcjsInLast24Months),
				new QueryParameter("NumOfAssociatedCcjsInLast24Months", numOfAssociatedCcjsInLast24Months),
				new QueryParameter("SumOfCcjsInLast24Months", sumOfCcjsInLast24Months),
				new QueryParameter("SumOfAssociatedCcjsInLast24Months", sumOfAssociatedCcjsInLast24Months)
			);
		}

		private DateTime? GetDateFromNode(XmlNode node, string tag) 
		{
			XmlNode yearNode = node.SelectSingleNode(tag + "-YYYY");
			XmlNode monthNode = node.SelectSingleNode(tag + "-MM");
			XmlNode dayNode = node.SelectSingleNode(tag + "-DD");

			if (yearNode != null && monthNode != null && dayNode != null) 
			{
				string dateStr = string.Format(
					"{0}-{1}-{2}",
					yearNode.InnerText.Trim().PadLeft(4, '0'),
					monthNode.InnerText.Trim().PadLeft(2, '0'),
					dayNode.InnerText.Trim().PadLeft(2, '0')
				);

				DateTime result;

				if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					return result;
				}
			}

			return null;
		}

		private int GetIntValueOrDefault(XmlNode element, string nodeName) 
		{
			XmlNode node = element.SelectSingleNode(nodeName);

			if (node != null) 
			{
				int result;
				if (int.TryParse(node.InnerText, out result))
				{
					return result;
				}
			}

			return 0;
		}
	}
}
