using ApplicationMng.Model;
using NHibernate;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class StrategyRepository : NHibernateRepositoryBase<Strategy>, IStrategyRepository, IRepository<Strategy>, System.IDisposable
	{
		public StrategyRepository(ISession session) : base(session)
		{
		}
		public new void Delete(Strategy val)
		{
			throw new System.NotImplementedException();
		}
		public void Delete(int strategyId, int userId, string document, string signedDocument)
		{
			this.EnsureTransaction(() =>
			{
				Strategy strategy = this.Get(strategyId);
				User user = this._session.Load<User>(userId);
				strategy.IsDeleted = new int?(strategy.Id);
				strategy.User = user;
				strategy.TerminationDate = new System.DateTime?(System.DateTime.Now);
				if (strategy.Products != null)
				{
					strategy.Products.Clear();
				}
				strategy.SignedDocumentDelete = signedDocument;
				this.Update(strategy);
			});
		}
		public void UpdateChampionChallendge()
		{
			PublicNameRepository publicNameRepository = new PublicNameRepository(this._session);
			foreach (PublicName current in publicNameRepository.GetActivePublicNames())
			{
				int num = 0;
				foreach (PublicNameStrategy current2 in current.PublicNameStrategies)
				{
					current2.Strategy.State = (num == 0);
					this._session.Update(current2.Strategy);
					num++;
				}
			}
			this._session.Flush();
		}
		public bool HasNewerStrategyVersion(int strategyId)
		{
			Strategy currentItem = (
				from item in this.GetAll()
				where item.Id == strategyId
				select item).First<Strategy>();
			Strategy strategy = (
				from item in this.GetAll()
				where item.DisplayName == currentItem.DisplayName && item.TerminationDate == (System.DateTime?)null && (item.IsDeleted == null || item.IsDeleted == (int?)0)
				select item).FirstOrDefault<Strategy>();
			return strategy != null && strategy.Id != strategyId;
		}
		public Strategy GetStrategyByDisplayName(string displayName)
		{
			Strategy strategy = this.TryGetStrategyByDisplayName(displayName);
			if (strategy != null)
			{
				return strategy;
			}
			throw new StrategyNotFoundException(string.Format("Strategy with display name '{0}' was not found", displayName));
		}
		public Strategy TryGetStrategyByDisplayName(string displayName)
		{
			return this.GetAll().FirstOrDefault((Strategy item) => item.DisplayName == displayName && item.TerminationDate == (System.DateTime?)null && (item.IsDeleted == null || item.IsDeleted == (int?)0));
		}
		public Strategy GetStrategyByName(string strategyName)
		{
			return this.GetAll().Single((Strategy s) => s.Name == strategyName);
		}
	}
}
