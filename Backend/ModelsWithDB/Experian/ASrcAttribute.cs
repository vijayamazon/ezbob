namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public abstract class ASrcAttribute : Attribute {
		protected ASrcAttribute(string sNodeName) {
			GroupName = null;
			NodeName = sNodeName;
			IsTopLevel = true;
		} // constructor

		public string GroupName { get; protected set; }
		public string NodeName { get; private set; }

		public bool IsTopLevel { get; protected set; }

		public string GroupPath {
			get { return "." + (IsTopLevel ? "/REQUEST" : "") + "/" + GroupName; }
		} // GroupPath

		public string NodePath {
			get { return GroupPath + "/" + NodeName; }
		} // GroupPath

		public override string ToString() {
			return string.IsNullOrWhiteSpace(NodeName) ? GroupPath : NodePath;
		} // ToString
	} // class ASrcAttribute
} // namespace
