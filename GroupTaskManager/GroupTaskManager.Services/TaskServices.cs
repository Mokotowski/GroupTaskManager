using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Security.Cryptography;

namespace GroupTaskManager.GroupTaskManager.Services
{
    public class TaskServices :  ITaskManageServices, ITaskActionsServices
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


        public async Task AddTaskUser(UserModel user, int Id_Task, List<string> Id_users)
        {
            try
            {
                TaskRecord task = await _databaseContext.TaskRecord.FindAsync(Id_Task);
                Group group = task?.Group;

                if (group != null && group.Id_User == user.Id)
                {
                    List<TaskAnswer> answers = new List<TaskAnswer>();

                    foreach (var userId in Id_users)
                    {
                        answers.Add(await TaskCreate(userId, task));
                    }

                    await _databaseContext.TaskAnswer.AddRangeAsync(answers);
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation("Users added to task {TaskId} successfully.", Id_Task);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding users to task {TaskId}.", Id_Task);
                throw;
            }
        }


        public async Task DeleteTaskUser(UserModel user, int Id_Task, List<string> Id_users)
        {
            try
            {
                TaskRecord task = await _databaseContext.TaskRecord.FindAsync(Id_Task);
                Group group = task?.Group;

                if (group != null && group.Id_User == user.Id)
                {
                    var answers = await _databaseContext.TaskAnswer
                        .Where(p => Id_users.Contains(p.Id_User) && p.Id_Task == task.Id)
                        .ToListAsync();

                    _databaseContext.TaskAnswer.RemoveRange(answers);
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation("Users removed from task {TaskId} successfully.", Id_Task);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing users from task {TaskId}.", Id_Task);
                throw;
            }
        }


        public async Task<List<UserTasksProgress>> GetUsersTask(UserModel user, int Id_Task)
        {
            try
            {
                TaskRecord task = await _databaseContext.TaskRecord.FindAsync(Id_Task);
                Group group = task?.Group;

                if (group != null && group.Id_User == user.Id)
                {
                    var userTasksProgress = await _databaseContext.TaskAnswer
                        .Where(taskRecord => taskRecord.Id_Task == Id_Task)
                        .Join(
                            _databaseContext.Users,
                            taskRecord => taskRecord.Id_User,
                            usr => usr.Id,
                            (taskRecord, usr) => new UserTasksProgress
                            {
                                Id = taskRecord.Id,
                                Firstname = usr.Firstname,
                                Lastname = usr.Lastname,
                                Id_User = taskRecord.Id_User,
                                State = taskRecord.State,
                                Completed = taskRecord.Completed,
                                CompletedTime = taskRecord.CompletedTime,
                                AddToTask = true
                            })
                        .ToListAsync();

                    _logger.LogInformation("Retrieved users for task {TaskId}.", Id_Task);
                    return userTasksProgress;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users for task {TaskId}.", Id_Task);
                throw;
            }

            return new List<UserTasksProgress>();
        }

        public async Task<List<UserTasksProgress>> GetUsersGroupForTask(UserModel user, int Id_Task)
        {
            var taskGroupData = await _databaseContext.TaskRecord
                .Where(p => p.Id == Id_Task)
                .Select(p => new
                {
                    Task = p,
                    Group = p.Group
                })
                .FirstOrDefaultAsync();

            if (taskGroupData == null || taskGroupData.Group?.Id_User != user.Id)
            {
                return new List<UserTasksProgress>();
            }

            var groupUserIds = await _databaseContext.Group_User
                .Where(g => g.Id_Group == taskGroupData.Group.Id)
                .Select(g => g.Id_User)
                .ToListAsync();

            var usersInGroup = await _databaseContext.Users
                .Where(u => groupUserIds.Contains(u.Id))
                .ToListAsync();

            var userTaskProgress = await GetUsersTask(user, Id_Task);

            var results = userTaskProgress.ToList();

            foreach (var userInGroup in usersInGroup)
            {
                if (!results.Any(p => p.Id_User == userInGroup.Id))
                {
                    results.Add(new UserTasksProgress
                    {
                        Firstname = userInGroup.Firstname,
                        Lastname = userInGroup.Lastname,
                        Id_User = userInGroup.Id,
                        AddToTask = false,
                    });
                }
            }

            return results;
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

        public async Task Complete(UserModel user, int Id, string State, string Result, byte[] result)
        {
            try
            {
                TaskAnswer answer = await _databaseContext.TaskAnswer.FindAsync(Id);

                if (answer != null && answer.Id_User == user.Id)
                {
                    answer.State = State;
                    answer.ResultString = Result;
                    answer.ResultFile = result;
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation("Task answer {AnswerId} completed with new state {State} by user {UserId}.", Id, State, user.Id);
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
                var tasks = await _databaseContext.TaskAnswer
                    .Where(p => p.Id_User == user.Id)
                    .Select(p => p.TaskRecord)
                    .ToListAsync();

                _logger.LogInformation("Retrieved tasks for user {UserId}.", user.Id);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tasks for user {UserId}.", user.Id);
                throw;
            }
        }
    }
}
