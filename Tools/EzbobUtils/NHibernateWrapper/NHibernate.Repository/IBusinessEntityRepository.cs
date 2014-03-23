using ApplicationMng.Model;
using NHibernateWrapper.NHibernate.Model;
using System;
using System.Collections.Generic;
namespace NHibernateWrapper.NHibernate.Repository
{
	public interface IBusinessEntityRepository
	{
		bool CheckIfExistsInActiveState(string name, string data);
		System.Collections.Generic.IEnumerable<BusinessEntity> GetAllActiveBusinessEntities();
		System.Collections.Generic.IEnumerable<BusinessEntity> GetAll();
		BusinessEntity GetActive(string name);
		bool CheckIfBusinessEntityExists(string version);
		BusinessEntity Get(string version);
		bool Save(BusinessEntity newBusinessEntity, string comment, User user);
		void LinkToStrategy(int strategyId, string version);
		void Delete(string name, string signedDocument);
		string[] GetLinkedStrategiesNames(string version);
		string[] GetLinkedNodesNames(string version);
	}
}
