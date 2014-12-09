namespace Reports {
	using System;
	using System.Data;

	public class CustomerData : Extractor, IComparable<CustomerData> {

		public CustomerData(IDataRecord oRow) : base(oRow) {
			ID = Retrieve<int>("CustomerID").Value;
			FirstName = Retrieve("FirstName");
			Surname = Retrieve("Surname");
			TypeOfBusiness = Retrieve("TypeOfBusiness");
			WizardStepName = Retrieve("WizardStepTypeName");
			WizardStepIsLast = Convert.ToBoolean(oRow["TheLastOne"]);

			bool? bIsOffline = Retrieve<bool>("IsOffline");

			IsOffline = bIsOffline.HasValue && bIsOffline.Value;
		} // constructor

		public int ID { get; private set; }
		public string FirstName { get; private set; }
		public string Surname { get; private set; }
		public string TypeOfBusiness { get; private set; }
		public bool IsOffline { get; private set; }
		public string WizardStepName { get; private set; }
		public bool WizardStepIsLast { get; private set; }

		public override string ToString() {
			return string.Format(
				"{0}: {1} {2} {3} {4}",
				ID, Value(FirstName), Value(Surname), Segment(), Value(TypeOfBusiness)
			);
		} // ToString

		public int CompareTo(CustomerData y) {
			if (ReferenceEquals(y, null))
				return 1;

			if (ReferenceEquals(this, y))
				return 0;

			if (this.WizardStepIsLast == y.WizardStepIsLast)
				return this.WizardStepName.CompareTo(y.WizardStepName);

			return this.WizardStepIsLast ? -1 : 1;
		} // Compare

		protected string Segment() {
			return IsOffline ? "Offline" : "Online";
		} // Segment

	} // class CustomerData

} // namespace Reports
