namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;

	[Serializable]
	public class YodleeRulesModel
	{
		public int Id { get; set; }
		public string Rule { get; set; }
	}

	[Serializable]
	public class YodleeGroupModel
	{
		public int Id { get; set; }
		public string Group { get; set; }
	}

	[Serializable]
	public class YodleeRuleModel
	{
		public List<YodleeGroupModel> Groups;
		public List<YodleeRulesModel> Rules;
		public SortedDictionary<string /*group*/, SortedDictionary<string, string> /*Rule,Literal*/> GroupRulesDict;
	}

	public class YodleeRuleModelBuilder
	{

		private readonly ISession _session;
		public YodleeRuleModelBuilder(ISession session)
		{
			_session = session;
		}

		public YodleeRuleModel Build()
		{
			var model = new YodleeRuleModel();
			model.Groups =
				new YodleeGroupRepository(_session)
					.GetAll()
					.Select(
						gr =>
						new YodleeGroupModel
							{
								Id = gr.Id,
								Group =
									gr.Group + (string.IsNullOrEmpty(gr.SubGroup) ? "" : " - " + gr.SubGroup)
							})
					.ToList();

			model.Rules =
				new YodleeRuleRepository(_session).GetAll().Select(r => new YodleeRulesModel { Id = r.Id, Rule = r.Rule }).ToList();

			var groupRules = new YodleeGroupRuleMapRepository(_session).GetAll().ToList();

			model.GroupRulesDict = new SortedDictionary<string, SortedDictionary<string, string>>();
			foreach (var gr in groupRules)
			{
				var group = gr.Group.Group + (string.IsNullOrEmpty(gr.Group.SubGroup) ? "" : " - " + gr.Group.SubGroup);
				if (!model.GroupRulesDict.ContainsKey(group))
				{
					model.GroupRulesDict[group] = new SortedDictionary<string, string>
						{
							{
								gr.Rule.Rule,
								groupRules
									.Where(r => r.Rule == gr.Rule && r.Group == gr.Group)
									.Select(r => r.Literal)
									.Aggregate((a, b) => a + ", " + b)
							}
						};
				}
				else
				{
					if (!model.GroupRulesDict[group].ContainsKey(gr.Rule.Rule))
					{
						model.GroupRulesDict[group].Add(gr.Rule.Rule,
														groupRules
															.Where(r => r.Rule == gr.Rule && r.Group == gr.Group)
															.Select(r => r.Literal)
															.Aggregate((a, b) => a + ", " + b));
					}

				}
			}

			return model;
		}
	}
}
