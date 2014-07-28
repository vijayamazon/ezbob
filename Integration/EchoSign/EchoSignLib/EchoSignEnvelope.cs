namespace EchoSignLib {
	public class EchoSignEnvelope {
		public int CustomerID { get; set; }
		public int[] Directors { get; set; }
		public int[] ExperianDirectors { get; set; }
		public int TemplateID { get; set; }
		public bool SendToCustomer { get; set; }

		public bool IsValid {
			get {
				if (CustomerID < 1)
					return false;

				if (TemplateID < 1)
					return false;

				if (!SendToCustomer) {
					if ((Directors == null) || (Directors.Length < 1))
						if ((ExperianDirectors == null) || (ExperianDirectors.Length < 1))
							return false;
				} // if

				if (Directors != null)
					foreach (var nDirectorID in Directors)
						if (nDirectorID < 1)
							return false;

				if (ExperianDirectors != null)
					foreach (var nDirectorID in ExperianDirectors)
						if (nDirectorID < 1)
							return false;

				return true;
			} // get
		} // IsValid

		public override string ToString() {
			return string.Format(
				"customer ID: {0}, template ID: {1}, directors: {2}, experian directors: {4}, send to customer: {3}",
				CustomerID,
				TemplateID,
				Directors == null ? "-- none --" : "[ " +  string.Join(", ", Directors) + " ]",
				SendToCustomer ? "yes" : "no",
				ExperianDirectors == null ? "-- none --" : "[ " + string.Join(", ", ExperianDirectors) + " ]"
			);
		} // ToString
	} // class EchoSignEnvelope
} // namespace
