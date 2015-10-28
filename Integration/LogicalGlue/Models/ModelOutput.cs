namespace Ezbob.Integration.LogicalGlue.Models {
	using Ezbob.Integration.LogicalGlue.Interface;

	public class ModelOutput : IModelOutput {
		public string Status { get; set; }

		IGrade IModelOutput.Grade {
			get { return Grade; }
			set { SetGrade(value); }
		} // IModelOutput.Grade

		public Grade Grade {
			get {
				if (this.grade == null)
					this.grade = new Grade();

				return this.grade;
			} // get
			set { SetGrade(value); }
		} // Grade

		IError IModelOutput.Error {
			get { return Error; }
			set { SetError(value); }
		} // IModelOutput.Error

		public Error Error {
			get {
				if (this.error == null)
					this.error = new Error();

				return this.error;
			} // get
			set { SetError(value); }
		} // Set

		private void SetGrade(IGrade grd) {
			this.grade = this.grade.CloneFrom(grd);
		} // SetGrade

		private void SetError(IError err) {
			this.error = this.error.CloneFrom(err);
		} // SetError

		private Grade grade;
		private Error error;
	} // class ModelOutput
} // namespace
