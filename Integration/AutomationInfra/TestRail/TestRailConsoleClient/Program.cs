namespace TestRailConsoleClient
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using TestRailConsoleClient.Tests;
    using TestRailCore;
    using TestRailData;
    using TestRailModels.TestRail;

    class Program
    {
        private static TestRailData client;
        private static string input;


        public static TestRailData GetTestRailClient()
        {
            var url = ConfigurationManager.AppSettings["testRailUrl"];
            var user = ConfigurationManager.AppSettings["testRailUser"];
            var apikey = ConfigurationManager.AppSettings["testRailApiKey"];
            return new TestRailData(url, user, apikey);
        }






        static void Main(string[] args)
        {

            //PrintCasesDependency();
            //return;
            var url = ConfigurationManager.AppSettings["testRailUrl"];
            var user = ConfigurationManager.AppSettings["testRailUser"];
            var apikey = ConfigurationManager.AppSettings["testRailApiKey"];
            client = new TestRailData(url, user, apikey);

            var ezbobProject = client.Projects.FirstOrDefault(x => x.Name == "EZbob");

            if (ezbobProject == null)
            {
                Console.WriteLine("Project EZbob was not found");
                Console.WriteLine("Project are:");
                foreach (var project in client.Projects)
                {
                    Console.WriteLine("Project {0}, id {1}", project.Name, project.ID);
                }

                return;
            }

            Console.WriteLine("Project found: {0}, id: {1}", ezbobProject.Name, ezbobProject.ID);
            Console.WriteLine();

            var configurationGroups = client.GetConfigurationGroups(ezbobProject.ID);
            List<TestRailModels.TestRail.Configuration> configs = new List<TestRailModels.TestRail.Configuration>();
            foreach (var confGroup in configurationGroups)
            {
                Console.WriteLine("Configuration group: {0}, id {1}", confGroup.Name, confGroup.ID);
                foreach (var conf in confGroup.Configurations)
                {
                    Console.WriteLine("\tConfiguration: {0} id {1}", conf.Name, conf.ID);
                }
                Console.WriteLine("select {0} configuration id:", confGroup.Name);
                input = Console.ReadLine();
                ulong confID = 0;
                if (!ulong.TryParse(input, out confID))
                {
                    Console.WriteLine("wrong input");
                    return;
                }
                var selectedConf = confGroup.Configurations.FirstOrDefault(x => x.ID == confID);
                if (selectedConf != null)
                {
                    configs.Add(selectedConf);
                }
            }

            var suites = client.GetSuites(ezbobProject.ID);

            foreach (var s in suites)
            {
                Console.WriteLine("suit: {0}, id: {1}", s.Name, s.ID);
            }

            Console.Write("Select suit id:");
            input = Console.ReadLine();

            ulong suitId = 0;
            if (!ulong.TryParse(input, out suitId))
            {
                Console.WriteLine("wrong input");
                return;
            }

            var suit = suites.FirstOrDefault(x => x.ID == suitId);
            if (suit == null || !suit.ID.HasValue)
            {
                Console.WriteLine("suit {0} not found", suitId);
                return;
            }

            var cases = client.GetCases(ezbobProject.ID, suit.ID.Value);

            foreach (var c in cases)
            {
                string labels = c.Labels.Any() ? c.Labels.Select(x => x.ToString()).Aggregate((a, b) => a + ", " + b) : "";
                Console.WriteLine("case: {0}, id: {1} \n {2}", c.Title, c.ID, labels);
            }

            var milestones = client.GetMilestones(ezbobProject.ID);

            foreach (var m in milestones)
            {
                Console.WriteLine("milestone {0}, id {1}", m.Name, m.ID);
            }

            Console.Write("select milestone id:");
            input = Console.ReadLine();
            ulong milestoneId = 0;
            if (!ulong.TryParse(input, out milestoneId))
            {
                Console.WriteLine("wrong input");
                return;
            }

            var milestone = milestones.FirstOrDefault(x => x.ID == milestoneId);
            if (milestone == null)
            {
                Console.WriteLine("milestone {0} not found", milestoneId);
                return;
            }

            Console.Write("run all cases (Y/N):");
            input = Console.ReadLine();
            if (input != "Y" && input != "N")
            {
                Console.WriteLine("wrong input");
                return;
            }

            if (input == "Y")
            {
                RunAllCases(ezbobProject, suit, milestone);
            }
            else
            {
                List<Label> labels = cases.SelectMany(x => x.Labels).Distinct().ToList();
                foreach (var label in labels)
                {
                    Console.WriteLine("label {0}, id {1}", label.ToString(), (int)label);
                }
                Console.WriteLine("select label id:");
                input = Console.ReadLine();
                int labelID = 0;
                if (!int.TryParse(input, out labelID))
                {
                    Console.WriteLine("wrong input");
                    return;
                }
                Label selectedLabel = (Label)labelID;

                HashSet<ulong> selectedCases = cases.Where(x => x.Labels.Contains(selectedLabel) && x.ID.HasValue).Select(x => x.ID.Value).ToHashSet();
                if (selectedCases.Any())
                {
                    RunSelectedCases(ezbobProject, suit, milestone, selectedCases, selectedLabel, configs, configurationGroups);
                }
                else
                {
                    Console.WriteLine("No cases match your filter");
                    return;
                }
            }
        }

        private static void RunSelectedCases(Project project, Suite suit, Milestone milestone, HashSet<ulong> selectedCases, Label label, List<TestRailModels.TestRail.Configuration> configs, List<ConfigurationGroup> configurationGroups)
        {
            //var run = client.AddRun(project.ID, suit.ID.Value, suit.Name + " " + label, suit.Description, milestone.ID, caseIDs: selectedCases);

            var plan = client.AddPlan(project.ID, suit.Name + " " + label, suit.Description, milestone.ID, new List<PlanEntry>() {
				new PlanEntry() {
					Name = suit.Name,
					CaseIDs = selectedCases.ToList(),
					ConfigIDs = configs.Select(x => x.ID).ToList(),
					SuiteID = suit.ID,
					RunList = new List<Run>() {
						new Run {
							Name = suit.Name + " " + label,
							ConfigIDs = configs.Select(x => x.ID)
								.ToList(),
							SuiteID = suit.ID,
							CaseIDs = selectedCases,
							Description = suit.Description,
							IncludeAll = false,
							MilestoneID = milestone.ID
						}
					}
				}
			});

            RunPlan(plan, configurationGroups);
        }

        private static void RunPlan(CommandResult<ulong> plan, List<ConfigurationGroup> configurationGroups)
        {
            if (!plan.WasSuccessful)
            {
                Console.WriteLine("failed to create plan \n{0}", plan.Exception);
                return;
            }

            var p = client.GetPlan(plan.Value);
            foreach (var entry in p.Entries)
            {

                foreach (var run in entry.RunList)
                {
                    var configurations = configurationGroups.SelectMany(x => x.Configurations)
                    .Where(x => run.ConfigIDs.Contains(x.ID)).ToList();
                    RunCases(new CommandResult<ulong>(true, run.ID.Value), configurations);
                }
            }

            client.ClosePlan(p.ID);

        }

        private static void RunAllCases(Project project, Suite suite, Milestone milestone)
        {
            var run = client.AddRun(project.ID, suite.ID.Value, suite.Name, suite.Description, milestone.ID);
            RunCases(run);
        }

        private static void RunCases(CommandResult<ulong> run, List<TestRailModels.TestRail.Configuration> configs = null)
        {
            if (!run.WasSuccessful)
            {
                Console.WriteLine("failed to create run \n{0}", run.Exception);
                return;
            }
            Console.WriteLine("run {0}", run.Value);
            var tests = client.GetTests(run.Value);
            foreach (var t in tests)
            {
                RunOneTest(t, configs);
            }
            client.CloseRun(run.Value);
        }

        private static void RunOneTest(Test test, List<TestRailModels.TestRail.Configuration> configs = null)
        {
            Console.WriteLine("test {0}, id {1}", test.Title, test.ID);
            if (!test.ID.HasValue || !test.CaseID.HasValue)
            {
                Console.WriteLine("test {0} has no Id skipping", test.Title);
                return;
            }

            var type = Helper.GetTypeWithAttributeValue<TestCaseIDAttribute>(Assembly.GetExecutingAssembly(),
                                                attribute => attribute.CaseID == test.CaseID.Value);

            if (type.Name == "String")
            {
                Console.WriteLine("Not implemented test for test {0} {1}", test.Title, test.ID);
                client.AddResult(test.ID.Value, ResultStatus.Untested, "Not implemented");
            }
            else
            {
                ITest instance = (ITest)Activator.CreateInstance(type);
                var testResult = instance.Run(configs);

                var result = client.AddResult(test.ID.Value, testResult.Status, testResult.Comment, null, new TimeSpan(testResult.TimeElapsed));
                if (!result.WasSuccessful)
                {
                    Console.WriteLine("failed to add result to test {0} {1}", test.Title, test.ID);
                }
            }
        }
    }
}
