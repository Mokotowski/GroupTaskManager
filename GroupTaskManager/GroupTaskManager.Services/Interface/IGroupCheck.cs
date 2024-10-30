using GroupTaskManager.GroupTaskManager.Database;

namespace GroupTaskManager.GroupTaskManager.Services.Interface
{
    public interface IGroupCheck
    {
        public Task<bool> IsOwnerGroup(UserModel user, int Id_Group); 
        public Task<bool> IsMember(UserModel user, int Id_Group);

    }
}
