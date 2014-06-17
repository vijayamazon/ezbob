namespace Ezbob.Utils {
	public class NameList : ObjList<string> {
		public NameList(params string[] args) : base(args) {
			Separator = "/";
		} // constructor
	} // class NameList
} // namespace Ezbob.Utils
