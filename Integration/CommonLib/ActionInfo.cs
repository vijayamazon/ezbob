using System;

namespace EzBob.CommonLib
{
	public enum ActionAccessType
	{
		Full,
		Limit
	}

	public class ActionInfo
	{
		public ActionInfo(string name)
		{
			Name = name;
		}

		public Action Action { get; set; }
		public ActionAccessType Access { get; set; }

		public string Name { get; private set; }
	}
}