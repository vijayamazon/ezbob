namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class ModelOutput {
		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public Grade Grade {
			get {
				if (this.grade == null)
					this.grade = new Grade();

				return this.grade;
			} // get
			set { this.grade = this.grade.CloneFrom(value); }
		} // Grade

		[DataMember]
		public ModelError Error {
			get {
				if (this.error == null)
					this.error = new ModelError();

				return this.error;
			} // get
			set { this.error = this.error.CloneFrom(value); }
		} // Set

		private Grade grade;
		private ModelError error;
	} // class ModelOutput
} // namespace
