namespace Ezbob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;

	public class LoadExperianConsumerData : AStrategy {

		public LoadExperianConsumerData(int customerId, int? directorId, long? nServiceLogId) {
			Result = new ExperianConsumerData();

			if (nServiceLogId.HasValue)
				m_nWorkMode = WorkMode.ServiceLog;
			else if (directorId.HasValue && directorId.Value != 0)
				m_nWorkMode = WorkMode.CacheDirector;
			else
				m_nWorkMode = WorkMode.CacheCustomer;

			m_nCustomerId = customerId;
			m_nDirectorId = directorId;
			m_nServiceLogID = nServiceLogId;
		} // constructor

		public override string Name {
			get { return "LoadExperianConsumerData"; }
		} // Name

		public override void Execute() {
			IEnumerable<SafeReader> data;

			switch (m_nWorkMode) {
			case WorkMode.ServiceLog:
				data = DB.ExecuteEnumerable(
					"LoadFullExperianConsumer",
					CommandSpecies.StoredProcedure,
					new QueryParameter("ServiceLogId", m_nServiceLogID)
				);
				break;

			case WorkMode.CacheCustomer:
				data = DB.ExecuteEnumerable(
					"LoadExperianConsumerForCustomer",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", m_nCustomerId)
				);
				break;

			case WorkMode.CacheDirector:
				data = DB.ExecuteEnumerable(
					"LoadExperianConsumerForDirector",
					CommandSpecies.StoredProcedure,
					new QueryParameter("DirectorId", m_nDirectorId)
				);
				break;

			default:
				Log.Alert("Unsupported work mode: {0}", m_nWorkMode.ToString());
				return;
			} // switch

			var caisBalance = new List<ExperianConsumerDataCaisBalance>();
			var caisCards = new List<ExperianConsumerDataCaisCardHistory>();

			foreach (SafeReader sr in data) {
				string sType = sr["DatumType"];

				switch (sType) {
				case "ExperianConsumerData":
					sr.Fill(Result);
					Result.Id = sr["Id"];
					break;

				case "Metadata":
					Result.InsertDate = sr["InsertDate"];
					break;

				case "ExperianConsumerDataApplicant":
					var a = sr.Fill<ExperianConsumerDataApplicant>();
					a.Id = sr["Id"];
					Result.Applicants.Add(a);
					break;

				case "ExperianConsumerDataLocation":
					var l = sr.Fill<ExperianConsumerDataLocation>();
					l.Id = sr["Id"];
					Result.Locations.Add(l);
					break;

				case "ExperianConsumerDataResidency":
					var r = sr.Fill<ExperianConsumerDataResidency>();
					r.Id = sr["Id"];
					Result.Residencies.Add(r);
					break;

				case "ExperianConsumerDataCais":
					var c = sr.Fill<ExperianConsumerDataCais>();
					c.Id = sr["Id"];
					Result.Cais.Add(c);
					break;

				case "ExperianConsumerDataCaisBalance":
					var b = sr.Fill<ExperianConsumerDataCaisBalance>();
					b.Id = sr["Id"];
					caisBalance.Add(b);
					break;

				case "ExperianConsumerDataCaisCardHistory":
					var h = sr.Fill<ExperianConsumerDataCaisCardHistory>();
					h.Id = sr["Id"];
					caisCards.Add(h);
					break;

				case "ExperianConsumerNoc":
					var n = sr.Fill<ExperianConsumerDataNoc>();
					n.Id = sr["Id"];
					Result.Nocs.Add(n);
					break;
				} // switch
			} // for each row

			Log.Debug(
				"ServiceLogID: {3}, cais {2} caisBalance {0} caisCards {1}",
				caisBalance.Count,
				caisCards.Count,
				Result.Cais.Count,
				Result.ServiceLogId
			);

			if (!Result.Cais.Any())
				return;

			foreach (var c in Result.Cais)
				Log.Debug("caisid {0}", c.Id);

			foreach (var b in caisBalance) {
				if (b.ExperianConsumerDataCaisId.HasValue) {
					var cais = Result.Cais.FirstOrDefault(x => x.Id == b.ExperianConsumerDataCaisId.Value);

					Log.Debug("cais found {0} cais id {1}", cais != null, b.ExperianConsumerDataCaisId);

					if (cais != null) {
						Log.Debug("cais id {0} cais balance {1}", cais.Id, b.Id);
						cais.AccountBalances.Add(b);
					}
				}
			}

			foreach (var c in caisCards) {
				var cais = Result.Cais.FirstOrDefault(x => x.Id == c.ExperianConsumerDataCaisId);
				if (cais != null)
					cais.CardHistories.Add(c);
			}
		} // Execute

		public ExperianConsumerData Result { get; private set; }

		private enum WorkMode {
			ServiceLog,
			CacheCustomer,
			CacheDirector
		} // enum WorkMode

		private readonly WorkMode m_nWorkMode;

		private readonly long? m_nServiceLogID;
		private readonly int m_nCustomerId;
		private readonly int? m_nDirectorId;

	} // class LoadExperianConsumerData
} // namespace
