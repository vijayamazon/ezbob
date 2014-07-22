namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public abstract class ASrcAttribute : Attribute {
		protected ASrcAttribute(string sGroupName, string sNodeName) {
			GroupName = sGroupName;
			NodeName = sNodeName;
		} // constructor

		public string GroupName { get; private set; }
		public string NodeName { get; private set; }

		public virtual bool IsTopLevel { get { return true; } }

		public virtual bool IsDisplayed { get { return true; } }

		public string GroupPath {
			get { return "." + (IsTopLevel ? "/REQUEST" : "") + "/" + GroupName; }
		} // GroupPath

		public string NodePath {
			get { return GroupPath + "/" + NodeName; }
		} // GroupPath

		public override string ToString() {
			return string.IsNullOrWhiteSpace(NodeName) ? GroupPath : NodePath;
		} // ToString

		public virtual DisplayMetaData MetaData { get { return null; } }
	} // class ASrcAttribute
} // namespace
