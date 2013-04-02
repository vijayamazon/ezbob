namespace PostcodeAnywhere
{
    public interface IPostcodeAnywhereConfig
    {
        string Key { get; }
        bool Enabled { get; }
        int MaxBankAccountValidationAttempts { get; }
    }
}