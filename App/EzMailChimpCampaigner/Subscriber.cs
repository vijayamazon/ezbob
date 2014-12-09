namespace EzMailChimpCampaigner {
	using System;

	public class Subscriber {
		public string Email { get; set; }
		public string BrokerEmail { get; set; }
		public string Group { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public decimal LoanOffer { get; set; }
		public DateTime? DayAfter { get; set; }
		public DateTime? Week { get; set; }
		public DateTime? TwoWeeks { get; set; }
		public DateTime? Month { get; set; }

		public Subscriber Clone() {
			return new Subscriber {
				Email = Email,
				BrokerEmail = BrokerEmail,
				Group = Group,
				FirstName = FirstName,
				LastName = LastName,
				LoanOffer = LoanOffer,
				DayAfter = DayAfter,
				Week = Week,
				TwoWeeks = TwoWeeks,
				Month = Month,
			};
		} // Clone

		public override string ToString() {
			return string.Format(
				"Email: {0}\n\tLoan: {1} Name: {2} Broker email: {7}\n\tDayAfter: {3} Week: {4} TwoWeeks: {5} Month: {6}",
				Email,
				LoanOffer,
				FirstName,
				DayAfter,
				Week,
				TwoWeeks,
				Month,
				BrokerEmail
			);
		} // ToString

	} // class Subscriber
} // namespace
