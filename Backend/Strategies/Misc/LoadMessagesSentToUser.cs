namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class LoadMessagesSentToUser : AStrategy {
		public LoadMessagesSentToUser(int userID) {
			this.userID = userID;
			Result = new List<MessagesModel>();
		} // constructor

		public override string Name {
			get { return "LoadMessagesSentToUser"; }
		} // Name

		public List<MessagesModel> Result { get; private set; }

		public override void Execute() {
			Result.AddRange(DB.Fill<MessagesModel>(
				"LoadMessagesSentToUser",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserID", this.userID)
			));
		} // Execute

		private readonly int userID;
	} // class LoadMessagesSentToUser
} // namespace

