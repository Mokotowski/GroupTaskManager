namespace GroupTaskManager.GroupTaskManager.Services.Interface
{
    public interface ISendEmail
    {
        public Task SendConfirmedEmail(string Email);
        public Task SendResetPasswordEmail(string EMail);
    }
}
