namespace Ezbob.Utils {
	#region class NameList

	public class NameList : ObjList<string> {
		#region constructor

		public NameList(params string[] args) : base(args) {
			Separator = "/";
		} // constructor

		#endregion constructor
	} // class NameList

	#endregion class NameList
} // namespace Ezbob.Utils
