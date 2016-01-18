using System;
using System.Collections.Generic;
using System.Linq;

namespace EzBobTest
{
	using Ezbob.Backend.ModelsWithDB.Experian;
	using NUnit.Framework;
	using System.Globalization;

	/// <summary>
	/// Tests calculation of late accounts for auto rejection In Rejection.cs strategy CountLateAccounts method
	/// </summary>
	[TestFixture]
	internal class TestExperianLateCount
	{
		[Test]
		public void TestCountLateAccounts()
		{

			int rejectLateLastMonth = 3;
			int rejectValidLate = 1;
			int numOfLateAccounts = 0;

			var caisList = new List<ExperianConsumerDataCais>
				{
					new ExperianConsumerDataCais {LastUpdatedDate = new DateTime(2014, 7, 15), AccountStatusCodes = "0000000233"},
					new ExperianConsumerDataCais {LastUpdatedDate = new DateTime(2014, 6, 2), AccountStatusCodes = "00000002U?"}
				};

			foreach (var cais in caisList)
			{
				DateTime lastUpdateDate = cais.LastUpdatedDate.HasValue ? cais.LastUpdatedDate.Value : new DateTime(1900, 0, 0);
				var days = (lastUpdateDate - DateTime.UtcNow.AddMonths(-rejectLateLastMonth)).TotalDays;
				int numOfRelevantStatuses = (int)Math.Ceiling(days / 30.0);
				if (numOfRelevantStatuses > 0) // If not then there is no relevant data
				{
					var relevantStatuses = cais.AccountStatusCodes.Substring(cais.AccountStatusCodes.Length - numOfRelevantStatuses,
																			 numOfRelevantStatuses).ToArray();
					foreach (var status in relevantStatuses)
					{
						int nStatus = 0;
						int.TryParse(status.ToString(CultureInfo.InvariantCulture), out nStatus);
						if (nStatus > rejectValidLate && nStatus < 8)
						{
							numOfLateAccounts++;
							break;
						}
					}
				}
			}

			Assert.AreEqual(1, numOfLateAccounts);
		}
	}
}