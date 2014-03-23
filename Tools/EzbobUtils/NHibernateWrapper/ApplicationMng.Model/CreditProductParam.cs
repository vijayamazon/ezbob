using System;
namespace ApplicationMng.Model
{
	public class CreditProductParam
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual string Name
		{
			get;
			set;
		}
		public virtual string Type
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public virtual CreditProduct Product
		{
			get;
			set;
		}
		public virtual string Value
		{
			get;
			set;
		}
	}
}
