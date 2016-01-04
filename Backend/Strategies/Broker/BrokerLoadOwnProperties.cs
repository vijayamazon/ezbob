﻿namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLoadOwnProperties : AStrategy {
		public BrokerLoadOwnProperties(string sContactEmail, CustomerOriginEnum origin) {
			m_oSp = new SpBrokerLoadOwnProperties(DB, Log) {
				ContactEmail = sContactEmail,
				Origin = (int)origin,
			};

			Properties = new BrokerProperties();
		} // constructor

		public override string Name {
			get { return "Broker load own properties"; } // get
		} // Name

		public override void Execute() {
			m_oSp.FillFirst(Properties);

			SafeReader sr = DB.GetFirst(
				"LoadActiveLotteries",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", Properties.BrokerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			Properties.LotteryPlayerID = sr.IsEmpty ? string.Empty : ((Guid)sr["UniqueID"]).ToString("N");
			Properties.LotteryCode = sr["LotteryCode"];

			Log.Debug("BrokerLoadOwnProperties loaded \n{0}", Properties.ToString());
		} // Execute

		public BrokerProperties Properties { get; private set; }

		private readonly SpBrokerLoadOwnProperties m_oSp;
	} // class BrokerLoadOwnProperties
} // namespace Ezbob.Backend.Strategies.Broker
