namespace GoogleAnalyticsLib
{
	#region class CountryData

	public class CountryData {
		#region public

		#region constructor

		public CountryData(int nUsers, int nNewUsers) {
			Users = nUsers;
			NewUsers = nNewUsers;
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(int nUsers, int nNewUsers)
		{
			Users += nUsers;
			NewUsers += nNewUsers;
		} // Add

		#endregion method Add

		#region property Users

		public int Users { get; private set; }

		#endregion property Users

		#region property NewUsers

		public int NewUsers { get; private set; }

		#endregion property NewUsers

		#region property Returning

		public int Returning { get { return Users - NewUsers; } }

		#endregion property Returning

		#region method ToString

		public override string ToString() {
			return string.Format(
				"total: {0}, new: {1}, returning: {2}",
				Users,
				NewUsers,
				Returning
			);
		} // ToString

		#endregion method ToString

		#endregion public
	} // class CountryData

	#endregion class CountryData
} // namespace EzAnalyticsConsoleClient
