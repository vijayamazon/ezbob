namespace EzBob.Web.Code
{
	using NHibernate;
	using StructureMap;

	public class StrategyChecker
	{
		public static bool IsStrategyRunning(int customerId, bool isMainStrategy, string strategyName = null)
		{
			var session = ObjectFactory.GetInstance<ISession>();
			if (isMainStrategy)
			{
				strategyName = "Ezbob.Backend.Strategies.MainStrategy";
			}
			var status = (bool)session.CreateSQLQuery(string.Format("EXEC IsStrategyRunning @CustomerId={0}, @ActionName='{1}'", customerId, strategyName)).UniqueResult();
			return status;
		}
	}
}