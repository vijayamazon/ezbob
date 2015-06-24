namespace Reports.UiReportsExt {
	using System;

	class UiControlData : IComparable<UiControlData>{
		public int ID { get; set; }
		public string Name { get; set; }
		public int Position { get; set; }

		public override string ToString() {
			return string.Format("{0}: {1} at {2}", ID, Name, Position);
		} // ToString

		public int CompareTo(UiControlData y) {
			if (ReferenceEquals(y, null))
				return 1;

			if (ReferenceEquals(this, y))
				return 0;

			return Position.CompareTo(y.Position);
		} // Compare
	} // class UiControlData
} // namespace
