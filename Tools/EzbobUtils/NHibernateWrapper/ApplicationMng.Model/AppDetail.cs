using Iesi.Collections.Generic;
using System;
namespace ApplicationMng.Model
{
	public class AppDetail
	{
		public virtual long Id
		{
			get;
			set;
		}
		public virtual ISet<AppDetail> ChildDetails
		{
			get;
			set;
		}
		public virtual string ValueStr
		{
			get;
			set;
		}
		public virtual AppDetail Parent
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual AppDetailName Name
		{
			get;
			set;
		}
		public AppDetail()
		{
		}
		public AppDetail(string name)
		{
			this.Name = new AppDetailName();
			this.Name.Name = name;
		}
		public AppDetail(params object[] values)
		{
			this.ChildDetails = new HashedSet<AppDetail>();
			for (int i = 0; i < values.Length; i++)
			{
				object obj = values[i];
				if (obj is string)
				{
					this.Name = new AppDetailName();
					this.Name.Name = obj.ToString();
				}
				else
				{
					if (obj is AppDetail)
					{
						AppDetail appDetail = obj as AppDetail;
						appDetail.Parent = this;
						this.ChildDetails.Add(appDetail);
					}
				}
			}
		}
		public virtual void SetApp(Application application)
		{
			this.App = application;
			if (this.ChildDetails != null)
			{
				foreach (AppDetail current in this.ChildDetails)
				{
					current.SetApp(application);
				}
			}
		}
	}
}
