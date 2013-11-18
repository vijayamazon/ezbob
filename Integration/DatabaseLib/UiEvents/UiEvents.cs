using System;
using System.Linq;
using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib {
	#region class UiEvent

	public class UiEvent {
		#region public

		public virtual long ID { get; set; }
		public virtual UiControl UiControl { get; set; }
		public virtual UiAction UiAction { get; set; }
		public virtual DateTime Time { get; set; }
		public virtual string HtmlID { get; set; }
		public virtual BrowserVersion BrowserVersion { get; set; }
		public virtual SecurityUser SecurityUser { get; set; }
		public virtual long RefNum { get; set; }
		public virtual string Arguments { get; set; }
		public virtual string RemoteIP { get; set; }
		public virtual string SessionCookie { get; set; }

		#endregion public
	} // class UiEvent

	#endregion class UiEvent

	#region class UiEventMap

	public class UiEventMap : ClassMap<UiEvent> {
		#region public

		public UiEventMap() {
			Table("UiEvents");
			Id(x => x.ID, "UiEventID").GeneratedBy.Native();
			References(x => x.UiControl, "UiControlID");
			References(x => x.UiAction, "UiActionID");
			Map(x => x.Time, "EventTime");
			Map(x => x.HtmlID, "ControlHtmlID").Length(255);
			References(x => x.BrowserVersion, "BrowserVersionID");
			References(x => x.SecurityUser, "UserID");
			Map(x => x.RefNum, "EventRefNum");
			Map(x => x.Arguments, "EventArguments");
			Map(x => x.RemoteIP).Length(64);
			Map(x => x.SessionCookie).Length(255);
		} // constructor

		#endregion public
	} // class UiEventMap

	#endregion class UiEventMap

	#region class UiEventRepository

	public class UiEventRepository : NHibernateRepositoryBase<UiEvent> {
		public UiEventRepository(ISession session) : base(session) {}

		public UiEvent FindByRefNum(long nRefNum) {
			return GetAll().FirstOrDefault(x => x.RefNum == nRefNum);
		} // FindByRefNum
	} // class UiEventRepository

	#endregion class UiEventRepository
} // namespace EZBob.DatabaseLib
