namespace EzBob.Backend.Strategies.Experian
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Serialization;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Experian;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using StructureMap;
	using EZBob.DatabaseLib.Repository;
	using System.IO;

	public class BackfillExperianConsumer : AStrategy
	{
		#region constructor

		public BackfillExperianConsumer(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			_experianHistoryRepository = ObjectFactory.GetInstance<ExperianHistoryRepository>();
			_serviceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name
		{
			get { return "BackfillExperianConsumer"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute()
		{
			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogForConsumerBackfill", CommandSpecies.StoredProcedure);
			long serviceLogId = 0;
			foreach (SafeReader sr in lst)
			{
				try
				{
					serviceLogId = sr["Id"];
					var p = new ParseExperianConsumerData(serviceLogId, DB, Log);
					p.Execute();
					if (p.Result != null)
					{
						_experianHistoryRepository.SaveOrUpdateConsumerHistory(serviceLogId, p.Result.InsertDate, p.Result.CustomerId,
																			   p.Result.DirectorId, p.Result.BureauScore,
																			   GetConsumerCaisBalance(p.Result.Cais), p.Result.CII);
					}

					var serviceLog = _serviceLogRepository.Get(serviceLogId);
					if (serviceLog != null && serviceLog.ServiceType == ExperianServiceType.Consumer.DescriptionAttr())
					{
						var inputSerializer = new XmlSerializer(typeof(Input));
						var input = (Input)inputSerializer.Deserialize(new StringReader(serviceLog.RequestData));

						serviceLog.Firstname = input.Applicant[0].Name.Forename;
						serviceLog.Surname = input.Applicant[0].Name.Surname;
						serviceLog.DateOfBirth =
							(input.Applicant[0].DateOfBirth != null &&
							 input.Applicant[0].DateOfBirth.CCYYSpecified &&
							 input.Applicant[0].DateOfBirth.MMSpecified &&
							 input.Applicant[0].DateOfBirth.DDSpecified)
													 ? new DateTime(input.Applicant[0].DateOfBirth.CCYY, input.Applicant[0].DateOfBirth.MM,
																	input.Applicant[0].DateOfBirth.DD)
													 : (DateTime?)null;
						string postCode = null;
						if (input.LocationDetails[0].UKLocation != null)
						{
							postCode = input.LocationDetails[0].UKLocation.Postcode;
						}
						else if (input.LocationDetails[0].MultiLineLocation != null)
						{
							if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine6))
							{
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine6;
							}
							else if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine5))
							{
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine5;
							}
							else if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine4))
							{
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine4;
							}
							else if (!string.IsNullOrEmpty(input.LocationDetails[0].MultiLineLocation.LocationLine3))
							{
								postCode = input.LocationDetails[0].MultiLineLocation.LocationLine3;
							}
						}
						serviceLog.Postcode = postCode;
						if (serviceLog.Director != null)
						{
							serviceLog.Director.ExperianConsumerScore = p.Result.BureauScore;
						}
						else
						{
							serviceLog.Customer.ExperianConsumerScore = p.Result.BureauScore;
						}
						_serviceLogRepository.Update(serviceLog);

					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, "Failed to update service log details for id {0}", serviceLogId);
				}
			}
		} // Execute

		#endregion method Execute

		private int? GetConsumerCaisBalance(IEnumerable<ExperianConsumerDataCais> cais)
		{
			if (cais != null)
			{
				return cais.Where(c => c.AccountStatus != "S" && c.Balance.HasValue && c.MatchTo == 1).Sum(c => c.Balance);
			}
			return null;
		}

		private readonly ExperianHistoryRepository _experianHistoryRepository;
		private readonly ServiceLogRepository _serviceLogRepository;
	} // class BackfillExperianDirectors
} // namespace
