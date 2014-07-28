namespace EzBob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Experian;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class BackfillExperianConsumer : AStrategy {
		#region constructor

		public BackfillExperianConsumer(AConnection oDB, ASafeLog oLog) : base(oDB, oLog)
		{
			_experianHistoryRepository = ObjectFactory.GetInstance<ExperianHistoryRepository>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BackfillExperianConsumer"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogForConsumerBackfill", CommandSpecies.StoredProcedure);

			foreach (SafeReader sr in lst)
			{
				var serviceLogId = sr["Id"];
				var p = new ParseExperianConsumerData(serviceLogId, DB, Log);
				p.Execute();
				if (p.Result != null)
				{
					_experianHistoryRepository.SaveOrUpdateConsumerHistory(serviceLogId, p.Result.InsertDate, p.Result.CustomerId,
					                                                       p.Result.DirectorId, p.Result.BureauScore,
					                                                       GetConsumerCaisBalance(p.Result.Cais), p.Result.CII);
				}
			}
		} // Execute

		#endregion method Execute

		private int? GetConsumerCaisBalance(IEnumerable<ExperianConsumerDataCais> cais)
		{
			if (cais != null)
			{
				return cais.Where(c => c.AccountStatus != "S" && c.Balance.HasValue).Sum(c => c.Balance);
			}
			return null;
		}

		private readonly ExperianHistoryRepository _experianHistoryRepository;
	} // class BackfillExperianDirectors
} // namespace
