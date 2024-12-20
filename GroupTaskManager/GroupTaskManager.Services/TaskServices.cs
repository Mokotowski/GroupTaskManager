﻿using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Security.Cryptography;

namespace GroupTaskManager.GroupTaskManager.Services
{
    public class TaskServices : ITaskManageServices, ITaskActionsServices, ITaskUserResult
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<TaskServices> _logger;

        public TaskServices(DatabaseContext databaseContex, ILogger<TaskServices> logger)
        {
            _databaseContext = databaseContex;
            _logger = logger;
        }

        public async Task CreateTask(UserModel user, int Id_Group, string Name, string Description, string AnswerType, DateTime end)
        {
            try
            {
                Group group = await _databaseContext.Group.FindAsync(Id_Group);

                if (group != null && group.Id_User == user.Id)
                {
                    var task = new TaskRecord
                    {
                        Id_Group = Id_Group,
                        Name = Name,
                        Description = Description,
                        AnswerType = AnswerType,
                        AddTime = DateTime.Now,
                        EndTime = end,
                        Group = group
                    };

                    await _databaseContext.TaskRecord.AddAsync(task);
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation("Task created successfully for group {GroupId}.", Id_Group);
                }
                else
                {
                    _logger.LogWarning("User {UserId} does not have permission to create a task for group {GroupId}.", user.Id, Id_Group);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating task for group {GroupId}.", Id_Group);
                throw;
            }
        }
        public async Task UpdateTask(UserModel user, int Id, int Id_Group, string Name, string Description, string AnswerType, DateTime end)
        {
            try
            {
                Group group = await _databaseContext.Group.FindAsync(Id_Group);

                if (group != null && group.Id_User == user.Id)
                {
                    TaskRecord task = await _databaseContext.TaskRecord.FindAsync(Id);

                    if (task != null && group.Id == task.Id_Group)
                    {
                        if (DateTime.Now < end)
                        {
                            task.Name = Name;
                            task.Description = Description;
                            task.AnswerType = AnswerType;
                            task.EndTime = end;
                            _databaseContext.TaskRecord.Update(task);
                            await _databaseContext.SaveChangesAsync();
                            _logger.LogInformation("Task {TaskId} updated successfully.", Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating task {TaskId}.", Id);
                throw;
            }
        }

        public async Task DeleteTask(UserModel user, int Id, int Id_Group)
        {
            try
            {
                Group group = await _databaseContext.Group.FindAsync(Id_Group);

                if (group != null && group.Id_User == user.Id)
                {
                    TaskRecord task = await _databaseContext.TaskRecord.FindAsync(Id);

                    if (task != null && group.Id == task.Id_Group)
                    {
                        _databaseContext.TaskRecord.Remove(task);
                        await _databaseContext.SaveChangesAsync();
                        _logger.LogInformation("Task {TaskId} deleted successfully.", Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting task {TaskId}.", Id);
                throw;
            }
        }

        private async Task<TaskAnswer> TaskCreate(string Id_User, TaskRecord task)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id_User))
                {
                    _logger.LogWarning("Attempted to create a TaskAnswer with an empty User ID.");
                    throw new ArgumentException("User ID cannot be null or empty.", nameof(Id_User));
                }

                if (task == null)
                {
                    _logger.LogWarning("Attempted to create a TaskAnswer with a null TaskRecord.");
                    throw new ArgumentNullException(nameof(task), "TaskRecord cannot be null.");
                }

                TaskAnswer answer = new TaskAnswer()
                {
                    Id_Task = task.Id,
                    Id_User = Id_User,
                    State = "Added",
                    Completed = false,
                    TaskRecord = task
                };

                _logger.LogInformation("Created TaskAnswer for User ID {UserId} for Task ID {TaskId}.", Id_User, task.Id);
                return answer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating TaskAnswer for User ID {UserId} for Task ID {TaskId}.", Id_User, task?.Id);
                throw;
            }
        }


        public async Task AddTaskUser(UserModel user, int Id_Task, string Id_user)
        {
            try
            {
                _logger.LogInformation("Attempting to add user {UserId} to task {Id_Task}.", Id_user, Id_Task);

                TaskRecord task = await _databaseContext.TaskRecord.FirstOrDefaultAsync(p => p.Id == Id_Task);


                if (task == null)
                {
                    _logger.LogWarning("Task with ID {Id_Task} not found.", Id_Task);
                    return;
                }

                Group group = await _databaseContext.Group.FirstOrDefaultAsync(p => p.Id == task.Id_Group);

                if (group == null || group.Id_User != user.Id)
                {
                    _logger.LogWarning("User {Id_user} is not authorized to add users to task {Id_Task}.", user.Id, Id_Task);
                    return;
                }

                var taskAnswer = await TaskCreate(Id_user, task);
                await _databaseContext.TaskAnswer.AddAsync(taskAnswer);
                await _databaseContext.SaveChangesAsync();

                _logger.LogInformation("User {UserId} successfully added to task {Id_Task}.", Id_user, Id_Task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding user {Id_user} to task {Id_Task}.", Id_user, Id_Task);
                throw;
            }
        }



        public async Task DeleteTaskUser(UserModel user, int Id_Task, string Id_user)
        {
            try
            {
                _logger.LogInformation("Attempting to remove user {Id_user} from task {Id_Task}.", Id_user, Id_Task);

                TaskRecord task = await _databaseContext.TaskRecord.FirstOrDefaultAsync(p => p.Id == Id_Task);


                if (task == null)
                {
                    _logger.LogWarning("Task with ID {Id_Task} not found.", Id_Task);
                    return;
                }

                Group group = await _databaseContext.Group.FirstOrDefaultAsync(p => p.Id == task.Id_Group);

                if (group == null || group.Id_User != user.Id)
                {
                    _logger.LogWarning("User {Id_user} is not authorized to remove users from task {Id_Task}.", user.Id, Id_Task);
                    return;
                }

                TaskAnswer answer = await _databaseContext.TaskAnswer.FirstOrDefaultAsync(p => p.Id_User == Id_user && p.Id_Task == Id_Task);

                if (answer == null)
                {
                    _logger.LogWarning("No TaskAnswer found for user {Id_user} in task {Id_Task}.", Id_user, Id_Task);
                    return;
                }

                _databaseContext.TaskAnswer.Remove(answer);
                await _databaseContext.SaveChangesAsync();

                _logger.LogInformation("User {Id_user} successfully removed from task {Id_Task}.", Id_user, Id_Task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing user {Id_user} from task {Id_Task}.", Id_user, Id_Task);
                throw;
            }
        }



        public async Task<List<UserTasksProgress>> GetUsersTask(UserModel user, int Id_Task)
        {
            try
            {
                _logger.LogInformation("Retrieving users for task {TaskId}.", Id_Task);

                TaskRecord task = await _databaseContext.TaskRecord.FirstOrDefaultAsync(t => t.Id == Id_Task);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", Id_Task);
                    return new List<UserTasksProgress>();
                }

                Group group = await _databaseContext.Group.FirstOrDefaultAsync(g => g.Id == task.Id_Group);

                if (group == null || group.Id_User != user.Id)
                {
                    _logger.LogWarning("User {UserId} is not authorized to view users for task {TaskId}.", user.Id, Id_Task);
                    return new List<UserTasksProgress>();
                }

                List<TaskAnswer> taskAnswers = await _databaseContext.TaskAnswer
                    .Where(p => p.Id_Task == Id_Task)
                    .ToListAsync();

                var userIds = taskAnswers.Select(p => p.Id_User).ToList();
                List<UserModel> users = await _databaseContext.Users
                    .Where(p => userIds.Contains(p.Id))
                    .ToListAsync();

                List<UserTasksProgress> usersTask = users.Select(useran =>
                {
                    var answer = taskAnswers.Single(p => p.Id_User == useran.Id);
                    return new UserTasksProgress
                    {
                        Id = answer.Id,
                        Firstname = useran.Firstname,
                        Lastname = useran.Lastname,
                        Id_User = useran.Id,
                        State = answer.State,
                        Completed = answer.Completed,
                        CompletedTime = answer.CompletedTime,
                        AddToTask = true
                    };
                }).ToList();

                _logger.LogInformation("Successfully retrieved users for task {TaskId}.", Id_Task);
                return usersTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users for task {TaskId}.", Id_Task);
                throw;
            }
        }


        public async Task<List<UserTasksProgress>> GetUsersGroupForTask(UserModel user, int Id_Task)
        {
            try
            {
                _logger.LogInformation("Retrieving group users for task {TaskId}.", Id_Task);

                List<UserTasksProgress> users = await GetUsersTask(user, Id_Task);
                TaskRecord task = await _databaseContext.TaskRecord.FirstOrDefaultAsync(t => t.Id == Id_Task);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", Id_Task);
                    return users;
                }

                List<UserTasksProgress> groupUsers = await _databaseContext.Group_User
                    .Where(p => p.Id_Group == task.Id_Group)
                    .Join(
                        _databaseContext.Users,
                        groupUser => groupUser.Id_User,
                        user => user.Id,
                        (groupUser, user) => new UserTasksProgress
                        {
                            Id_User = user.Id,
                            Firstname = user.Firstname,
                            Lastname = user.Lastname,
                            AddToTask = false
                        })
                    .ToListAsync();

                foreach (var usergroup in groupUsers)
                {
                    if (!users.Any(p => p.Id_User == usergroup.Id_User))
                    {
                        users.Add(usergroup);
                    }
                }

                _logger.LogInformation("Successfully retrieved group users for task {TaskId}.", Id_Task);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving group users for task {TaskId}.", Id_Task);
                throw;
            }
        }



        public async Task<List<TaskRecord>> MyManageTasks(UserModel user, int Id_Group)
        {
            try
            {
                Group group = await _databaseContext.Group.FindAsync(Id_Group);

                if (group != null && group.Id_User == user.Id)
                {
                    List<TaskRecord> tasks = await _databaseContext.TaskRecord
                        .Where(p => p.Id_Group == Id_Group)
                        .ToListAsync();

                    _logger.LogInformation("Retrieved managed tasks for group {GroupId} by user {UserId}.", Id_Group, user.Id);
                    return tasks;
                }
                else
                {
                    _logger.LogWarning("User {UserId} does not have permission to manage tasks for group {GroupId}.", user.Id, Id_Group);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving managed tasks for group {GroupId}.", Id_Group);
                throw;
            }

            return new List<TaskRecord>();
        }

        public async Task<TaskRecord> MyManageTask(UserModel user, int Id_Task)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve task {Id_Task} for user {UserId}.", Id_Task, user.Id);

                TaskRecord task = await _databaseContext.TaskRecord.FirstOrDefaultAsync(t => t.Id == Id_Task);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {Id_Task} not found.", Id_Task);
                    return null;
                }

                _logger.LogInformation("Task {Id_Task} retrieved successfully for user {UserId}.", Id_Task, user.Id);

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving task {Id_Task} for user {UserId}.", Id_Task, user.Id);
                throw;
            }
        }







        public async Task AddAnswer(UserModel user, int Id, string? answer, byte[]? fileanswer, string? extensionfile)
        {
            try
            {
                _logger.LogInformation("Starting AddAnswer for User {UserId} and TaskAnswer {AnswerId}.", user.Id, Id);

                TaskAnswer taskAnswer = await _databaseContext.TaskAnswer.FindAsync(Id);

                if (taskAnswer == null)
                {
                    _logger.LogWarning("TaskAnswer with Id {AnswerId} not found.", Id);
                    return;
                }

                if (taskAnswer.Id_User == user.Id)
                {
                    if (answer != null)
                    {
                        taskAnswer.ResultString = answer;
                        _logger.LogInformation("Text answer updated for TaskAnswer {AnswerId} by User {UserId}.", Id, user.Id);
                    }

                    if (fileanswer != null && extensionfile != null)
                    {
                        taskAnswer.ResultFile = fileanswer;
                        taskAnswer.ResultFileExtension = extensionfile;
                        _logger.LogInformation("File answer updated for TaskAnswer {AnswerId} by User {UserId}.", Id, user.Id);
                    }

                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation("Changes successfully saved for TaskAnswer {AnswerId} by User {UserId}.", Id, user.Id);
                }
                else
                {
                    _logger.LogWarning("User {UserId} does not have permission to add answer for TaskAnswer {AnswerId}.", user.Id, Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding answer for TaskAnswer {AnswerId}.", Id);
                throw;
            }
        }


        public async Task<FileResult?> DownloadAnswerFile(UserModel user, int Id)
        {
            try
            {
                _logger.LogInformation("Starting file download process for User {UserId} and TaskAnswer {AnswerId}.", user.Id, Id);

                var taskAnswer = await _databaseContext.TaskAnswer.FindAsync(Id);

                if (taskAnswer == null)
                {
                    _logger.LogWarning("Task answer with ID {Id} was not found.", Id);
                    return null;
                }

                if (taskAnswer.Id_User != user.Id)
                {
                    _logger.LogWarning("User {UserId} does not have permission to download the file for TaskAnswer {AnswerId}.", user.Id, Id);
                    return null;
                }

                if (taskAnswer.ResultFile == null || string.IsNullOrEmpty(taskAnswer.ResultFileExtension))
                {
                    _logger.LogWarning("No file available to download for TaskAnswer {AnswerId} by User {UserId}.", Id, user.Id);
                    return null;
                }

                byte[] fileData = taskAnswer.ResultFile;
                string fileExtension = taskAnswer.ResultFileExtension;
                string fileName = $"AnswerFile{fileExtension}";

                _logger.LogInformation("File for TaskAnswer {AnswerId} prepared for download by User {UserId}.", Id, user.Id);

                return new FileContentResult(fileData, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading the file for TaskAnswer {AnswerId}.", Id);
                throw;
            }
        }


        public async Task DeleteAnswerFile(UserModel user, int Id)
        {
            try
            {
                _logger.LogInformation("Starting DeleteAnswerFile for User {UserId} and TaskAnswer {AnswerId}.", user.Id, Id);

                TaskAnswer taskAnswer = await _databaseContext.TaskAnswer.FindAsync(Id);

                if (taskAnswer == null)
                {
                    _logger.LogWarning("TaskAnswer with Id {AnswerId} not found.", Id);
                    return;
                }

                if (taskAnswer.Id_User == user.Id)
                {
                    taskAnswer.ResultFile = null;
                    taskAnswer.ResultFileExtension = null;

                    await _databaseContext.SaveChangesAsync();

                    _logger.LogInformation("File answer successfully deleted for TaskAnswer {AnswerId} by User {UserId}.", Id, user.Id);
                }
                else
                {
                    _logger.LogWarning("User {UserId} does not have permission to delete file answer for TaskAnswer {AnswerId}.", user.Id, Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting file answer for TaskAnswer {AnswerId}.", Id);
                throw;
            }
        }





        public async Task ChangeState(UserModel user, int Id, string State)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);

                if (answer != null && answer.Id_User == user.Id)
                {
                    answer.State = State;
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation("State changed to {State} for task answer {AnswerId} by user {UserId}.", State, Id, user.Id);
                }
                else
                {
                    _logger.LogWarning("User {UserId} does not have permission to change the state of task answer {AnswerId}.", user.Id, Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing state for task answer {AnswerId}.", Id);
                throw;
            }
        }

        public async Task Complete(UserModel user, int Id)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);

                if (answer != null && answer.Id_User == user.Id)
                {
                    answer.Completed = true;
                    answer.CompletedTime = DateTime.Now;
                    answer.State = "Completed";
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation("Task answer {AnswerId} completed by user {UserId}.", Id, user.Id);
                }
                else
                {
                    _logger.LogWarning("User {UserId} does not have permission to complete task answer {AnswerId}.", user.Id, Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while completing task answer {AnswerId}.", Id);
                throw;
            }
        }


        public async Task<List<TaskRecord>> MyTasks(UserModel user)
        {
            try
            {
                _logger.LogInformation("Retrieving tasks for user {UserId}.", user.Id);

                var tasks = await _databaseContext.TaskAnswer
                    .Where(p => p.Id_User == user.Id)
                    .Select(p => p.TaskRecord)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {TaskCount} tasks for user {UserId}.", tasks.Count, user.Id);

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tasks for user {UserId}.", user.Id);
                throw;
            }
        }

        public async Task<List<TaskRecord>> MyTasksGroup(UserModel user, int Id_Group)
        {
            try
            {
                _logger.LogInformation("Retrieving tasks for user {UserId} in group {GroupId}.", user.Id, Id_Group);

                List<TaskRecord> alltasks = await MyTasks(user);
                List<TaskRecord> tasks = alltasks.Where(p => p.Id_Group == Id_Group).ToList();

                _logger.LogInformation("Successfully retrieved {TaskCount} tasks for user {UserId} in group {GroupId}.", tasks.Count, user.Id, Id_Group);

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tasks for user {UserId} in group {GroupId}.", user.Id, Id_Group);
                throw;
            }
        }

        public async Task<TaskWorkModel> WorkTask(UserModel user, int Id)
        {
            try
            {
                _logger.LogInformation("Retrieving work task for user {UserId} and task {TaskId}.", user.Id, Id);

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(Id);

                if (record == null)
                {
                    _logger.LogWarning("TaskRecord with ID {TaskId} not found for user {UserId}.", Id, user.Id);
                    throw new InvalidOperationException($"TaskRecord with ID {Id} not found.");
                }

                TaskAnswer answer = await _databaseContext.TaskAnswer
                    .SingleOrDefaultAsync(p => p.Id_Task == record.Id && p.Id_User == user.Id);

                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer not found for TaskRecord {TaskId} and user {UserId}.", Id, user.Id);
                    throw new InvalidOperationException($"TaskAnswer not found for TaskRecord {Id} and user {user.Id}.");
                }

                TaskWorkModel task = new TaskWorkModel(record, answer);

                _logger.LogInformation("Successfully retrieved work task for user {UserId} and task {TaskId}.", user.Id, Id);

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving work task for user {UserId} and task {TaskId}.", user.Id, Id);
                throw;
            }
        }










        private async Task<bool> CheckPermission(UserModel user, TaskRecord record)
        {
            try
            {
                Group group = await _databaseContext.Group.FindAsync(record.Id_Group);
                return user.Id == group.Id_User;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking permissions for user {UserId}.", user.Id);
                return false;
            }

        }




        public async Task InCompleted(UserModel user, int Id, string State)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);
                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer with ID {AnswerId} not found.", Id);
                    return;
                }

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(answer.Id_Task);
                if (record == null || !await CheckPermission(user, record))
                {
                    _logger.LogWarning("Permission denied for user {UserId} on TaskRecord {RecordId}.", user.Id, record?.Id);
                    return;
                }

                answer.Completed = false;
                answer.CompletedTime = null;
                answer.State = State;

                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation("Marked TaskAnswer {AnswerId} as incomplete by user {UserId}.", Id, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while marking TaskAnswer {AnswerId} as incomplete.", Id);
            }
        }

        public async Task AddComment(UserModel user, int Id, string Comment)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);
                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer with ID {AnswerId} not found.", Id);
                    return;
                }

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(answer.Id_Task);
                if (record == null || !await CheckPermission(user, record))
                {
                    _logger.LogWarning("Permission denied for user {UserId} on TaskRecord {RecordId}.", user.Id, record?.Id);
                    return;
                }

                answer.ResultComment = Comment;
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation("Added comment to TaskAnswer {AnswerId} by user {UserId}.", Id, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a comment to TaskAnswer {AnswerId}.", Id);
            }
        }
        public async Task DeleteComment(UserModel user, int Id)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);
                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer with ID {AnswerId} not found.", Id);
                    return;
                }

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(answer.Id_Task);
                if (record == null || !await CheckPermission(user, record))
                {
                    _logger.LogWarning("Permission denied for user {UserId} on TaskRecord {RecordId}.", user.Id, record?.Id);
                    return;
                }

                answer.ResultComment = null;
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation("Deleted comment from TaskAnswer {AnswerId} by user {UserId}.", Id, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting a comment from TaskAnswer {AnswerId}.", Id);
            }
        }
        public async Task ChangeStateAdm(UserModel user, int Id, string State)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);
                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer with ID {AnswerId} not found.", Id);
                    return;
                }

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(answer.Id_Task);
                if (record == null || !await CheckPermission(user, record))
                {
                    _logger.LogWarning("Permission denied for user {UserId} on TaskRecord {RecordId}.", user.Id, record?.Id);
                    return;
                }

                answer.State = State;
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation("Changed state of TaskAnswer {AnswerId} by user {UserId}.", Id, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing the state of TaskAnswer {AnswerId}.", Id);
            }
        }
        public async Task<UserTaskResultData> GetUserTaskResult(UserModel user, int Id)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);
                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer with ID {AnswerId} not found.", Id);
                    return new UserTaskResultData();
                }

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(answer.Id_Task);
                UserModel taskuser = await _databaseContext.Users.FindAsync(answer.Id_User);

                if (record == null || !await CheckPermission(user, record))
                {
                    _logger.LogWarning("Permission denied for user {UserId} on TaskRecord {RecordId}.", user.Id, record?.Id);
                    return new UserTaskResultData();
                }

                _logger.LogInformation("Retrieved TaskResultData for TaskAnswer {AnswerId} by user {UserId}.", Id, user.Id);
                return new UserTaskResultData(answer, record, taskuser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving TaskResultData for TaskAnswer {AnswerId}.", Id);
                return new UserTaskResultData();
            }
        }

        public async Task<FileResult?> DownloadFileResult(UserModel user, int Id, string Firstname, string Lastname)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);
                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer with ID {AnswerId} not found.", Id);
                    return null;
                }

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(answer.Id_Task);
                if (record == null)
                {
                    _logger.LogWarning("TaskRecord for TaskAnswer {AnswerId} not found.", Id);
                    return null;
                }

                Group group = await _databaseContext.Group.FindAsync(record.Id_Group);
                if (group == null || group.Id_User != user.Id)
                {
                    _logger.LogWarning("Group not found or user {UserId} does not have permission for TaskAnswer {AnswerId}.", user.Id, Id);
                    return null;
                }

                if (answer.ResultFile == null || string.IsNullOrEmpty(answer.ResultFileExtension))
                {
                    _logger.LogInformation("No file attached to TaskAnswer {AnswerId}.", Id);
                    return null;
                }

                byte[] fileData = answer.ResultFile;
                string fileExtension = answer.ResultFileExtension;
                string fileName = $"{Firstname} {Lastname} - AnswerFile{fileExtension}";

                _logger.LogInformation("File for TaskAnswer {AnswerId} prepared for download by user {UserId}.", Id, user.Id);
                return new FileContentResult(fileData, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading the file for TaskAnswer {AnswerId}.", Id);
                throw;
            }
        }

        public async Task<FileResult?> DownloadTextResult(UserModel user, int Id, string Firstname, string Lastname)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);
                if (answer == null)
                {
                    _logger.LogWarning("TaskAnswer with ID {AnswerId} not found.", Id);
                    return null;
                }

                TaskRecord record = await _databaseContext.TaskRecord.FindAsync(answer.Id_Task);
                if (record == null)
                {
                    _logger.LogWarning("TaskRecord for TaskAnswer {AnswerId} not found.", Id);
                    return null;
                }

                Group group = await _databaseContext.Group.FindAsync(record.Id_Group);
                if (group == null || group.Id_User != user.Id)
                {
                    _logger.LogWarning("Group not found or user {UserId} does not have permission for TaskAnswer {AnswerId}.", user.Id, Id);
                    return null;
                }

                if (string.IsNullOrEmpty(answer.ResultString))
                {
                    _logger.LogInformation("No text result attached to TaskAnswer {AnswerId}.", Id);
                    return null;
                }

                byte[] fileData = System.Text.Encoding.UTF8.GetBytes(answer.ResultString);
                string fileName = $"{Firstname} {Lastname} - AnswerFile.txt";

                _logger.LogInformation("Text result for TaskAnswer {AnswerId} prepared for download by user {UserId}.", Id, user.Id);
                return new FileContentResult(fileData, "text/plain")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading the text result for TaskAnswer {AnswerId}.", Id);
                throw;
            }
        }


    }
}
