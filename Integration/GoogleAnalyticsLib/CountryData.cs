namespace GoogleAnalyticsLib
{

	public class CountryData {

		public CountryData(int nUsers, int nNewUsers) {
			Users = nUsers;
			NewUsers = nNewUsers;
		} // constructor

		public void Add(int nUsers, int nNewUsers)
		{
			Users += nUsers;
			NewUsers += nNewUsers;
		} // Add

		public int Users { get; private set; }

		public int NewUsers { get; private set; }

		public int Returning { get { return Users - NewUsers; } }

		public override string ToString() {
			return string.Format(
				"total: {0}, new: {1}, returning: {2}",
				Users,
				NewUsers,
				Returning
			);
		} // ToString

	} // class CountryData

} // namespace EzAnalyticsConsoleClient
