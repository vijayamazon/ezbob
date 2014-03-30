using System;
namespace ApplicationMng.Model
{
	public class HistoryItem
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual SecurityApplication SecApp
		{
			get;
			set;
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual Node Node
		{
			get;
			set;
		}
		public virtual System.DateTime? ChangeTime
		{
			get;
			set;
		}
		public virtual string ControlName
		{
			get;
			set;
		}
		public virtual string ControlValue
		{
			get;
			set;
		}
	}
}
