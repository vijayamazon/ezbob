namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class CustomerRelationState
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual bool? IsFollowUp { get; set; }
		public virtual CustomerRelationFollowUp FollowUp { get; set; }
		public virtual CRMStatuses Status { get; set; }
		public virtual CRMRanks Rank { get; set; }
	}

	public class CustomerRelationStateMap : ClassMap<CustomerRelationState>
	{
		public CustomerRelationStateMap()
		{
			Table("CustomerRelationState");
			Id(x => x.Id);
			Map(x => x.IsFollowUp);
			Map(x => x.CustomerId);
			References(x => x.FollowUp, "LastFollowUpId").Nullable();
			References(x => x.Status, "LastStatusId").Nullable();
			References(x => x.Rank, "LastRankId").Nullable();
		}
	}

	public interface ICustomerRelationStateRepository : IRepository<CustomerRelationState>
	{
		CustomerRelationState GetByCustomer(int customerId);
		void SaveUpdateState(int customerId, bool isFollowUp, CustomerRelationFollowUp followUp, CustomerRelations lastCrm);
	}
	
	public sealed class CustomerRelationStateRepository : NHibernateRepositoryBase<CustomerRelationState>, ICustomerRelationStateRepository
	{
		public CustomerRelationStateRepository(ISession session)
			: base(session)
		{
		}

		public CustomerRelationState GetByCustomer(int customerId)
		{
			return GetAll().FirstOrDefault(x => x.CustomerId == customerId);
		}

		public void SaveUpdateState(int customerId, bool isFollowUp, CustomerRelationFollowUp followUp, CustomerRelations lastCrm)
		{
			var state = GetAll().FirstOrDefault(x => x.CustomerId == customerId);
			if (state != null)
			{
				state.IsFollowUp = isFollowUp;
				state.FollowUp = followUp;
				state.Rank = lastCrm == null ? null : lastCrm.Rank;
				state.Status = lastCrm == null ? null : lastCrm.Status;
			}
			else
			{
				state = new CustomerRelationState
					{
						CustomerId = customerId,
						IsFollowUp = isFollowUp,
						FollowUp = followUp,
						Rank = lastCrm == null ? null : lastCrm.Rank,
						Status = lastCrm == null ? null : lastCrm.Status
					};
			}

			SaveOrUpdate(state);
		}

		public void SaveUpdateRank(int customerId, CustomerRelations lastCrm)
		{
			var state = GetAll().FirstOrDefault(x => x.CustomerId == customerId);
			if (state != null)
			{
				state.Status = lastCrm.Status;
				state.Rank = lastCrm.Rank;
			}
			else
			{
				state = new CustomerRelationState
				{
					CustomerId = customerId,
					Rank = lastCrm.Rank,
					Status = lastCrm.Status
				};
			}

			SaveOrUpdate(state);
		}
	}
}