namespace MailApi
{
    public interface IMail
    {
        void SendMessageFinishWizard(string emailTo, string fullName);
    }
}
