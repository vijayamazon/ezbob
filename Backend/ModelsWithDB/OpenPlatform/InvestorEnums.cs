namespace Ezbob.Backend.ModelsWithDB.OpenPlatform
{
	public enum Operator
    {
        Or,
        And,
        GreaterThan,
        LessThan,
        Equal,
        NotEqual,
        Not,
        IsTrue
    };

    public enum Field
    {
        DailyInvestmentAllowed = 0,
        WeeklyInvestmentAllowed = 1,
        MonthlyInvestmentAllowed = 2,
        GradeMin = 3,
        GradeMax = 4,
    };

    public enum Grade
    {
        A,
        B, 
        C, 
        D, 
        E,
        F
    };

    public enum RuleType {
        System =1,
        UnderWriter = 2,
        Investor = 3 
    };

    public enum ParameterPeriod {
        Day=1, 
        Week=2,
        Month=3
    };
}
