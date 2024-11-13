using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
