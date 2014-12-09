namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Serialization;
	using EZBob.DatabaseLib.Model.Database;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using System.IO;

	public class BackfillExperianConsumer : AStrategy {

		public BackfillExperianConsumer(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog) {
		} // constructor

		public override string Name {
			get { return "BackfillExperianConsumer"; }
		} // Name

		public override void Execute() {
			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogForConsumerBackfill", CommandSpecies.StoredProcedure);
			long serviceLogId = 0;
			foreach (SafeReader sr in lst) {
				try {
					serviceLogId = sr["Id"];
					var p = new ParseExperianConsumerData(serviceLogId, DB, Log);
					p.Execute();
					if (p.Result != null) {
						DB.ExecuteNonQuery("SaveExperianHistory", CommandSpecies.StoredProcedure,
										   new QueryParameter("ServiceLogId", serviceLogId),
										   new QueryParameter("InsertDate", p.Result.InsertDate),
										   new QueryParameter("Type", ExperianServiceType.Consumer.DescriptionAttr()),
										   new QueryParameter("CustomerId", p.Result.CustomerId),
										   new QueryParameter("DirectorId", p.Result.DirectorId),
										   new QueryParameter("Score", p.Result.BureauScore),
										   new QueryParameter("Balance", GetConsumerCaisBalance(p.Result.Cais)),
										   new QueryParameter("CII", p.Result.CII));
					}

					var request = DB.ExecuteScalar<string>("GetServiceLogRequestData", CommandSpecies.StoredProcedure,
														   new QueryParameter("@ServiceLogId", serviceLogId));

					if (!string.IsNullOrEmpty(request)) {
						var inputSerializer = new XmlSerializer(typeof(Input));
						var input = (Input)inputSerializer.Deserialize(new StringReader(request));

						var firstname = input.Applicant[0].Name != null ? input.Applicant[0].Name.Forename : string.Empty;
						var surname = input.Applicant[0].Name != null ? input.Applicant[0].Name.Surname : input.Applicant[0].FormattedName;
						var dateOfBirth =
							(input.Applicant[0].DateOfBirth != null &&
							 input.Applicant[0].DateOfBirth.CCYYSpecified &&
							 input.Applicant[0].DateOfBirth.MMSpecified &&
							 input.Applicant[0].DateOfBirth.DDSpecified)
													 ? new DateTime(input.Applicant[0].DateOfBirth.CCYY, input.Applicant[0].DateOfBirth.MM,
																	input.Applicant[0].DateOfBirth.DD)
													 : (DateTime?)null;
						string postCode = string.Empty;
						if (input.LocationDetails != null && input.LocationDetails[0].UKLocation != null) {
							postCode = input.LocationDetails[0].UKLocation.Postcode;
						} else if (input.LocationDetails != null && input.LocationDetails[0].MultiLineLocation != null) {
							if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine6)) {
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine6;
							} else if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine5)) {
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine5;
							} else if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine4)) {
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine4;
							} else if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine3)) {
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine3;
							}
						}

						var updateParams = new List<QueryParameter> {
							new QueryParameter("@ServiceLogId", serviceLogId),
							new QueryParameter("@Firstname", firstname), 
							new QueryParameter("@Surname", surname),
							new QueryParameter("@Postcode", postCode),
							new QueryParameter("@DateOfBirth", dateOfBirth), };

						if (p != null && p.Result != null) {
							updateParams.Add(new QueryParameter("@Score", p.Result.BureauScore));
							updateParams.Add(new QueryParameter("@CustomerId", p.Result.CustomerId));
							updateParams.Add(new QueryParameter("@DirectorId", p.Result.DirectorId));
						}

						DB.ExecuteNonQuery("UpdateServiceLogData", CommandSpecies.StoredProcedure, updateParams.ToArray());

					}
				} catch (Exception ex) {
					Log.Warn(ex, "Failed to update service log details for id {0}", serviceLogId);
				}
			}
		} // Execute

		private int? GetConsumerCaisBalance(IEnumerable<ExperianConsumerDataCais> cais) {
			if (cais != null) {
				return cais.Where(c => c.AccountStatus != "S" && c.Balance.HasValue && c.MatchTo == 1).Sum(c => c.Balance);
			}
			return null;
		}
	} // class BackfillExperianDirectors
} // namespace
