namespace EzBob.Models.Marketplaces.Yodlee
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using NHibernate;

	public class YodleeRuleModel
	{
		public List<MP_YodleeGroup> Groups;
		public List<MP_YodleeRule> Rules;
		public Dictionary<string /*group*/, Dictionary<string,string> /*Rule,Literal*/> GroupRulesDict;

		public YodleeRuleModel(ISession session)
		{
			Groups = new YodleeGroupRepository(session).GetAll().ToList();
			Rules = new YodleeRuleRepository(session).GetAll().ToList();

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
}
