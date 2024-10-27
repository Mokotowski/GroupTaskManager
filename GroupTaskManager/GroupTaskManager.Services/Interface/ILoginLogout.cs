namespace GroupTaskManager.Services.Interface
{
    public interface ILoginLogout
    {
        public Task<bool> Login(string Email, string Password);
        public Task<bool> Logout();
    }
}
