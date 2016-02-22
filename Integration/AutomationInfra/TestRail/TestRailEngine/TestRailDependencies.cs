namespace TestRailEngine {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class TestRailDependencies {
        public static Stream GetDependenciesReport() {
            var ezbobProject = TestRailManager.Instance.Projects.FirstOrDefault(x => x.Name == "EZbob");
            var log = new StringBuilder();
            if (ezbobProject != null) {
                log.Append("Type,Suite Name,Case Title,Shared,Case	Depended,Case Reference\n");
                var suites = TestRailManager.Instance.GetSuites(ezbobProject.ID);
                foreach (var suiteItem in suites) {
                    if (suiteItem.ID != null) {
                        var cases = TestRailManager.Instance.GetCases(ezbobProject.ID, suiteItem.ID.Value);
                        if (cases != null) {
                            foreach (var caseItem in cases) {
                                if (caseItem.CustomPreConds != null) {
                                    var caseDependencies = GetDependencies(caseItem.CustomPreConds);
                                    if (caseDependencies != null) {
                                        foreach (var caseDependency in caseDependencies) {
                                            log.Append("Case" + ","
                                                + suiteItem.Name.ReplaceWordWrapChars() + ","
                                                + caseItem.Title.ReplaceWordWrapChars() + ","
                                                + caseDependency + "," +
                                                "(C" + caseItem.ID + "),"
                                                + caseItem.CustomPreConds.ReplaceWordWrapChars() + ("\n"));
                                        }

                                        foreach (var step in caseItem.Steps) {
                                            var stepDependencies = GetDependencies(step.Description);
                                            if (stepDependencies != null) {
                                                foreach (var stepDependency in stepDependencies)
                                                    log.Append("Step" + ","
                                                        + suiteItem.Name.ReplaceWordWrapChars() + ","
                                                        + caseItem.Title.ReplaceWordWrapChars() + ","
                                                        + stepDependency + "," +
                                                        "(C" + caseItem.ID + "),"
                                                        + step.Description.ReplaceWordWrapChars() + ("\n"));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(log.ToString()));
        }

        public static List<String> GetDependencies(string iText) {
            var matchesList = new List<String>();
            Regex regex = new Regex(@"\[c\d+\]", RegexOptions.IgnoreCase);
            var matches = regex.Matches(iText);
            foreach (Match ItemMatch in matches) {
                matchesList.Add(ItemMatch.Value);
            }
            return (matchesList);
        }




        public static List<ulong> GetDependencies(ulong? caseId) {
            var dependencies = new List<ulong>();

            foreach (var key in TestRailManager.CasesRepository.Keys) {
                foreach (var caseItem in TestRailManager.CasesRepository[key]) {
                    if (caseItem.CustomPreConds != null) {
                        var caseDependencies = GetDependencies(caseItem.CustomPreConds);
                        if (caseDependencies != null) {
                            foreach (var caseDependency in caseDependencies) {
                                if (("[c" + caseId.ToString() + "]").Equals(caseDependency, StringComparison.CurrentCultureIgnoreCase)) {
                                    if (caseItem.ID != null)
                                        dependencies.Add((ulong)caseItem.ID);
                                }
                            }
                            foreach (var step in caseItem.Steps) {
                                var stepDependencies = GetDependencies(step.Description);
                                if (stepDependencies != null) {
                                    foreach (var stepDependency in stepDependencies) {
                                        if (("[c" + caseId.ToString() + "]").Equals(stepDependency, StringComparison.CurrentCultureIgnoreCase)) {
                                            if (caseItem.ID != null)
                                                dependencies.Add((ulong)caseItem.ID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return dependencies;
        }


        //    public static List<ulong?> GetDependencies(ulong? caseId) {
        //        var dependencies = new List<ulong?>();

        //        var ezbobProject = TestRailManager.Instance.Projects.FirstOrDefault(x => x.Name == "EZbob");
        //        if (ezbobProject != null) {
        //            var suites = TestRailManager.Instance.GetSuites(ezbobProject.ID);
        //            foreach (var suiteItem in suites) {
        //                if (suiteItem.ID != null) {
        //                    var cases = TestRailManager.Instance.GetCases(ezbobProject.ID, suiteItem.ID.Value);
        //                    foreach (var caseItem in cases) {
        //                        if (caseItem.CustomPreConds != null) {
        //                            var caseDependencies = GetDependencies(caseItem.CustomPreConds);
        //                            if (caseDependencies != null) {
        //                                foreach (var caseDependency in caseDependencies) {
        //                                    if (("c" + caseId.ToString()).Equals(caseDependency, StringComparison.CurrentCultureIgnoreCase)) {
        //                                        dependencies.Add(caseItem.ID);
        //                                    }
        //                                }
        //                                foreach (var step in caseItem.Steps) {
        //                                    var stepDependencies = GetDependencies(step.Description);
        //                                    if (stepDependencies != null) {
        //                                        foreach (var stepDependency in stepDependencies) {
        //                                            if (("c" + caseId.ToString()).Equals(stepDependency, StringComparison.CurrentCultureIgnoreCase)) {
        //                                                dependencies.Add(caseItem.ID);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        return dependencies;
        //    }
        //}     
    }
}