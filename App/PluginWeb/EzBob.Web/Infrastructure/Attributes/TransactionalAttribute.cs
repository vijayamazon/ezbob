namespace EzBob.Web.Infrastructure.Attributes {
	using System;
	using System.Web.Mvc;
	using NHibernate;
	using StructureMap;
	using log4net;

	public class TransactionalAttribute : ActionFilterAttribute, IAuthorizationFilter, IExceptionFilter {

		public TransactionalAttribute() {
			IsolationLevel = System.Data.IsolationLevel.ReadCommitted;
		} // constructor

		public System.Data.IsolationLevel IsolationLevel { get; set; }

		public override void OnResultExecuted(ResultExecutedContext filterContext) {
			CloseTransaction();
		} // OnResultExecuted

		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			BeginTransaction();
		} // OnActionExecuting

		public override void OnActionExecuted(ActionExecutedContext filterContext) {
			CloseTransaction();
		} // OnActionExecuted

		public void OnAuthorization(AuthorizationContext filterContext) {
			BeginTransaction();
		} // OnAuthorization

		public void OnException(ExceptionContext filterContext) {
			CloseTransaction(false);
		} // OnException

		private void BeginTransaction() {
			ISession instance = ObjectFactory.GetInstance<ISession>();

			if (instance.Transaction == null || !instance.Transaction.IsActive)
				instance.BeginTransaction(this.IsolationLevel);
		} // BeginTransaction

		private void CloseTransaction(bool commit = true) {
			using (ITransaction transaction = ObjectFactory.GetInstance<ISession>().Transaction) {
				try {
					if (transaction != null) {
						if (transaction.IsActive) {
							if (commit)
								transaction.Commit();
							else
								transaction.Rollback();
						} // if transaction is active
					} // if transaction exists
				}
				catch (Exception e) {
					LogManager.GetLogger(typeof (TransactionalAttribute)).Warn("Exception when closing transaction.", e);

					throw;
				} // try
			} // using
		} // CloseTransaction

	} // class TransactionalAttribute
} // namespace
