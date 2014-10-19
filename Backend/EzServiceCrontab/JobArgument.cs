namespace EzServiceCrontab {
	using ArgumentTypes;
	using Ezbob.Database;

	internal class JobArgument {
		#region constructor

		public JobArgument(SafeReader sr, TypeRepository oTypeRepo) {
			sr.Fill(this);

			UnderlyingType = oTypeRepo.Find(TypeName, CanBeNull, TypeHint);
		} // constructor

		#endregion constructor

		[FieldName("ArgumentID")]
		public long ID { get; set; }

		public int SerialNo { get; set; }
		public string Value { get; set; }
		public string TypeHint { get; set; }
		public string TypeName { get; set; }

		[FieldName("IsNullable")]
		public bool CanBeNull { get; set; }

		public int TypeID { get; set; }

		public IType UnderlyingType { get; private set; }

		public override string ToString() {
			return string.Format(
				"{0}: ({1}) {2}: '{3}'",
				ID,
				SerialNo,
				UnderlyingType,
				Value
			);
		} // ToString

		#region method Differs

		public bool Differs(JobArgument oPrevious) {
			if (this.ID != oPrevious.ID)
				return true;

			if (this.Value != oPrevious.Value)
				return true;

			if (this.UnderlyingType.Differs(oPrevious.UnderlyingType))
				return true;

			return false;
		} // Differs

		#endregion method Differs
	} // class JobArgument
} // namespace
