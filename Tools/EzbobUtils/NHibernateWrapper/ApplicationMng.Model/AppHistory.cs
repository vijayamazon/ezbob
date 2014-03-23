using System;
namespace ApplicationMng.Model
{
	public class AppHistory
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual int UserId
		{
			get;
			set;
		}
		public virtual int SecurityApplicationId
		{
			get;
			set;
		}
		public virtual int ActionType
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual int CurrentNodeID
		{
			get;
			set;
		}
	}
}
