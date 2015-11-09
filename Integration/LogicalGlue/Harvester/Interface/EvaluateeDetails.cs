namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	/// <summary>
	/// This class should contain data about evaluatee (like name, birth date, address, company details, etc).
	/// We send this object to Logical Glue, they forward it to Equifax, collect data and return that data to us.
	/// This collected data is used as input data for Logical Glue models.
	/// Content of this object is to be agreed with Logical Glue (Oct 28 2015).
	/// </summary>
	public class EvaluateeDetails {
	} // class EvaluateeDetails
} // namespace
