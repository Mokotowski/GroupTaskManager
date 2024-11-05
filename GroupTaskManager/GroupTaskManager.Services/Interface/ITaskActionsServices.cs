using GroupTaskManager.GroupTaskManager.Database;
using System.Collections.Generic;

namespace GroupTaskManager.GroupTaskManager.Services.Interface
{
    public interface ITaskActionsServices
    {
        public Task ChangeState(UserModel user, int Id, string State);
        public Task Complete(UserModel user, int Id, string State, string Result, byte[] result);
        public Task<List<TaskRecord>> MyTasks(UserModel user);

    }
}
