using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace GroupTaskManager.GroupTaskManager.Services.Interface
{
    public interface ITaskUserResult
    {
        public Task InCompleted(UserModel user, int Id, string State);
        public Task AddComment(UserModel user, int Id, string Comment);
        public Task DeleteComment(UserModel user, int Id);
        public Task ChangeStateAdm(UserModel user, int Id, string State);
        public Task<UserTaskResultData> GetUserTaskResult(UserModel user, int Id);

        public Task<FileResult?> DownloadFileResult(UserModel user, int Id, string Firstname, string Lastname);
        public Task<FileResult?> DownloadTextResult(UserModel user, int Id, string Firstname, string Lastname);

    }
}
