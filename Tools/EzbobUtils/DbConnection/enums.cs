namespace Ezbob.Database {
	using System.ComponentModel;

	#region enum ActionResult

	public enum ActionResult {
		Continue,
		SkipCurrent,
		SkipAll
	} // ActionResult

	#endregion enum ActionResult

	#region enum CommandSpecies

	public enum CommandSpecies {
		/// <summary>
		/// With parameters: stored proc.
		/// Without parameters: text.
		/// </summary>
		[Description("raw text/stored procedure")]
		Auto,

		[Description("stored procedure")]
		StoredProcedure,

		[Description("raw text")]
		Text,

		[Description("table direct")]
		TableDirect
	} // enum CommandSpecies

	#endregion enum CommandSpecies
} // namespace Ezbob.Database
