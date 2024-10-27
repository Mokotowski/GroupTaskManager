namespace GroupTaskManager.Services.Interface
{
    public interface IRegister
    {
        public Task<bool> Register(string Firstname, string Lastname, string PhoneNumber, string Email, string Password);

    }
}
