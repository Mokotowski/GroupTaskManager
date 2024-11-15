using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GroupTaskManager.GroupTaskManager.Services.Interface
{
    public interface ITaskActionsServices
    {
        public Task ChangeState(UserModel user, int Id, string State);
        public Task AddAnswer(UserModel user, int Id, string? answer, byte[]? fileanswer, string? extensionfile);
        public Task<FileResult?> DownloadAnswerFile(UserModel user, int Id);
        public Task DeleteAnswerFile(UserModel user, int Id);
        public Task Complete(UserModel user, int Id);


        public Task<List<TaskRecord>> MyTasks(UserModel user);
        public Task<List<TaskRecord>> MyTasksGroup(UserModel user, int Id_Group);
        public Task<TaskWorkModel> WorkTask(UserModel user, int Id); 

    }
}
