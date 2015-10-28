namespace Ezbob.Integration.LogicalGlue.Interface {
	public interface IModelOutput {
		string Status { get; set; }
		IGrade Grade { get; set; }
		IError Error { get; set; }
	} // interface IModelOutput
} // namespace
