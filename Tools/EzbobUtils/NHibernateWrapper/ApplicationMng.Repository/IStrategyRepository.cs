using ApplicationMng.Model;
using System;
namespace ApplicationMng.Repository
{
	public interface IStrategyRepository : IRepository<Strategy>, System.IDisposable
	{
		void Delete(int strategyId, int userId, string document, string signedDocument);
		void UpdateChampionChallendge();
		Strategy GetStrategyByName(string strategyName);
		bool HasNewerStrategyVersion(int strategyId);
		Strategy GetStrategyByDisplayName(string displayName);
		Strategy TryGetStrategyByDisplayName(string displayName);
	}
}
