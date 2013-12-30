namespace Reports {
	using System;

	#region class UiControlData

	class UiControlData : IComparable<UiControlData>{
		#region public

		public int ID { get; set; }
		public string Name { get; set; }
		public int Position { get; set; }

		#region method ToString

		public override string ToString() {
			return string.Format("{0}: {1} at {2}", ID, Name, Position);
		} // ToString

		#endregion method ToString

		#region method CompareTo

		public int CompareTo(UiControlData y) {
			if (ReferenceEquals(y, null))
				return 1;

			if (ReferenceEquals(this, y))
				return 0;

			return Position.CompareTo(y.Position);
		} // Compare

		#endregion method CompareTo

		#endregion public
	} // class UiControlData

	#endregion class UiControlData
} // namespace Reports
