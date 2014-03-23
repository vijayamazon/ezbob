using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;
using NHibernateWrapper.NHibernate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class BusinessEntityRepository : NHibernateRepositoryBase<BusinessEntity>, IBusinessEntityRepository
	{
		public BusinessEntityRepository(ISession session) : base(session)
		{
		}
		public System.Collections.Generic.IEnumerable<BusinessEntity> GetAllActiveBusinessEntities()
		{
			return 
				from entity in this.GetAll()
				where !entity.IsDeleted.HasValue || entity.IsDeleted == 0
				select entity;
		}
		public new System.Collections.Generic.IEnumerable<BusinessEntity> GetAll()
		{
			return base.GetAll();
		}
		public bool CheckIfBusinessEntityExists(string version)
		{
			return this.GetAllActiveBusinessEntities().Any((BusinessEntity entity) => entity.Version == version);
		}
		public BusinessEntity GetActive(string name)
		{
			return this.GetAllActiveBusinessEntities().FirstOrDefault((BusinessEntity entity) => entity.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
		}
		public BusinessEntity Get(string version)
		{
			return this.GetAll().FirstOrDefault((BusinessEntity entity) => entity.Version == version);
		}
		public bool CheckIfExistsInActiveState(string name, string data)
		{
			BusinessEntity active = this.GetActive(name);
			bool result;
			if (active != null)
			{
				if (active.Document == data)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}
		public bool Save(BusinessEntity newBusinessEntity, string comment, User user)
		{
			newBusinessEntity.Comment = comment;
			newBusinessEntity.User = user;
			newBusinessEntity.CreationDate = new System.DateTime?(System.DateTime.Now);
			newBusinessEntity.Version = BusinessEntityRepository.CalculateMd5Hash(newBusinessEntity.Document);
			this.Save(newBusinessEntity);
			return true;
		}
		private static string CalculateMd5Hash(string input)
		{
			return BusinessEntityRepository.CalculateMd5Hash(System.Text.Encoding.UTF8.GetBytes(input));
		}
		private static string CalculateMd5Hash(byte[] inputArray)
		{
			System.Security.Cryptography.MD5 mD = System.Security.Cryptography.MD5.Create();
			byte[] inArray = mD.ComputeHash(inputArray);
			return System.Convert.ToBase64String(inArray);
		}
		public void Delete(string version, string signedDocument)
		{
			BusinessEntity businessEntity = this.Get(version);
			if (businessEntity != null)
			{
				businessEntity.SignedDocumentDelete = signedDocument;
				businessEntity.IsDeleted = new int?(businessEntity.Id);
				businessEntity.TerminationDate = new System.DateTime?(System.DateTime.Now);
				this.Save(businessEntity);
			}
		}
		public string[] GetLinkedStrategiesNames(string version)
		{
			return (
				from relation in this._session.Query<BusinessEntitiyToStrategyRelation>()
				where (relation.Strategy.IsDeleted == null || relation.Strategy.IsDeleted == (int?)0) && relation.BusinessEntity.Version == version
				select relation into strategyRelation
				select strategyRelation.Strategy.DisplayNameWithTermDate()).ToArray<string>();
		}
		public void LinkToStrategy(int strategyId, string version)
		{
			BusinessEntitiyToStrategyRelation businessEntitiyToStrategyRelation = this._session.Query<BusinessEntitiyToStrategyRelation>().FirstOrDefault((BusinessEntitiyToStrategyRelation relation) => relation.Strategy.Id == strategyId && relation.BusinessEntity.Version == version);
			if (businessEntitiyToStrategyRelation == null)
			{
				BusinessEntity businessEntity = this._session.Query<BusinessEntity>().First((BusinessEntity entity) => entity.Version == version);
				Strategy strategy1 = this._session.Query<Strategy>().First((Strategy strategy) => strategy.Id == strategyId);
				this._session.Save(new BusinessEntitiyToStrategyRelation
				{
					BusinessEntity = businessEntity,
					Strategy = strategy1
				});
			}
		}
		public string[] GetLinkedNodesNames(string version)
		{
			return (
				from relation in this._session.Query<BusinessEntitiyToNodeRelation>()
				where (relation.Node.IsDeleted == null || relation.Node.IsDeleted == (int?)0) && relation.BusinessEntity.Version != null
				select relation into strategyRelation
				select strategyRelation.Node.DisplayNameWithTermDate()).ToArray<string>();
		}
	}
}
