namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;

	[Serializable]
	public class YodleeRuleModel
	{
		public List<YodleeGroupModel> Groups;
		public List<YodleeRulesModel> Rules;
		public Dictionary<string /*group*/, Dictionary<string,string> /*Rule,Literal*/> GroupRulesDict;

		public YodleeRuleModel(ISession session)
		{
			Groups = new YodleeGroupRepository(session).GetAll().Select(gr => new YodleeGroupModel{ Id = gr.Id, Group = gr.Group + (string.IsNullOrEmpty(gr.SubGroup) ? "" : " - " + gr.SubGroup) }).ToList();
			Rules = new YodleeRuleRepository(session).GetAll().Select(r => new YodleeRulesModel{Id =r.Id, Rule = r.Rule}).ToList();

			var groupRules = new YodleeGroupRuleMapRepository(session).GetAll().ToList();

			GroupRulesDict = new Dictionary<string, Dictionary<string, string>>();
			foreach (var gr in groupRules)
			{
				var group = gr.Group.Group + (string.IsNullOrEmpty(gr.Group.SubGroup) ? "" : " - " + gr.Group.SubGroup);
				if (!GroupRulesDict.ContainsKey(group))
				{
					GroupRulesDict[group] = new Dictionary<string, string>
						{
							{gr.Rule.Rule, groupRules.Where(r => r.Rule == gr.Rule && r.Group == gr.Group).Select(r => r.Literal).Aggregate((a, b) => a + ", " + b)}
						};
				}
				else
				{
					if (!GroupRulesDict[group].ContainsKey(gr.Rule.Rule))
					{
						GroupRulesDict[group].Add(gr.Rule.Rule, groupRules.Where(r => r.Rule == gr.Rule).Select(r => r.Literal).Aggregate((a, b) => a + ", " + b));
					}
					
				}
			}
		}
	}

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
}
