using GroupTaskManager.GroupTaskManager.Database;

namespace GroupTaskManager.GroupTaskManager.Services.Interface
{
    public interface IGroupManage
    {
        public Task CreateGroup(UserModel user, string Name, string Description);
        public Task UpdateGroup(UserModel user, int Id_Group, string Name, string Description);
        public Task DeleteGroup(UserModel user, int Id_Group);

        public Task AddUser(UserModel user, int Id_Group, string Id_newuser);
        public Task DeleteUser(UserModel user, int Id_Group, int Id);


        public Task<List<Group_User>> GetGroupUsers(UserModel user, int Id_Group);
        public Task<List<Group>> MyManageGroups(UserModel user);
        public Task<List<Group>> MyGroups(UserModel user);
    }
}
