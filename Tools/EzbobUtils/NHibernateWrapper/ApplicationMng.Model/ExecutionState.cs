using System;
namespace ApplicationMng.Model
{
	public class ExecutionState
	{
		public virtual long Id
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual Node CurrentNode
		{
			get;
			set;
		}
		public virtual string CurrentNodePostfix
		{
			get;
			set;
		}
		public virtual string Data
		{
			get;
			set;
		}
	}
}
