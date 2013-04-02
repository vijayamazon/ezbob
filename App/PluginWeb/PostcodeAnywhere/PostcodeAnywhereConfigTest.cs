namespace PostcodeAnywhere
{
    public class PostcodeAnywhereConfigTest : IPostcodeAnywhereConfig
    {
        public string Key { get { return "UW24-ZZ45-DF74-XP85"; } }
        public bool Enabled { get { return true; } }
        public int MaxBankAccountValidationAttempts { get { return 3; } }
    }
}