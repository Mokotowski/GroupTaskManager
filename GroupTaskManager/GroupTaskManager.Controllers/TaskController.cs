using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
using GroupTaskManager.GroupTaskManager.Services;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GroupTaskManager.GroupTaskManager.Controllers
{
    public class TaskController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly ITaskManageServices _taskManage;
        private readonly ITaskActionsServices _taskActions;
        private readonly ITaskUserResult _taskResult;

        private readonly DatabaseContext _databaseContext;
        public TaskController(UserManager<UserModel> userManager, ITaskManageServices taskManage, ITaskActionsServices taskActions, ITaskUserResult taskResult)
        {
            _userManager = userManager;
            _taskManage = taskManage;
            _taskActions = taskActions;
            _taskResult = taskResult;
        }


        /*
            ManageTasks
                CreateTask
                UpdateTask - rozwijane się to menu po klknięciu
                DeleteTask


            ManageUsers
                AddTaskUser - Listy z wszystkimi userami info czy są przypisań
                DeleteTaskUser - listy z dodanymi do taska
        */


        [HttpGet]
        public async Task<IActionResult> ManageUsers(int Id_Task)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            ViewBag.Id_Task = Id_Task;

            SingleTaskData taskData = new SingleTaskData()
            {
                TaskData = await _taskManage.MyManageTask(user, Id_Task),
                Users_task = await _taskManage.GetUsersTask(user, Id_Task),
                Users_group = await _taskManage.GetUsersGroupForTask(user, Id_Task),
            };


            return View(taskData);
        }



        [HttpGet]
        public async Task<IActionResult> ManageTasks(int Id_Group)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            ViewBag.Id_Group = Id_Group;

            List<SingleTaskData> tasks = new List<SingleTaskData>();

            List<TaskRecord> taskRecords = await _taskManage.MyManageTasks(user, Id_Group);

            foreach (TaskRecord taskRecord in taskRecords)
            {
                SingleTaskData taskData = new SingleTaskData()
                {
                    TaskData = taskRecord,
                };
                tasks.Add(taskData);
            }

            return View(tasks);
        }




        [HttpGet]
        public async Task<IActionResult> MyTasks()
        {
            UserModel user = await _userManager.GetUserAsync(User);
            List<TaskRecord> tasks = await _taskActions.MyTasks(user);

            return View(tasks);
        }

        [HttpGet]
        public async Task<IActionResult> MyTasksGroup(int Id_Group)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            List<TaskRecord> tasks = await _taskActions.MyTasksGroup(user, Id_Group);
            return View(tasks);
        }

        [HttpGet]
        public async Task<IActionResult> WorkTask(int Id)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            TaskWorkModel task = await _taskActions.WorkTask(user, Id);

            ViewBag.Id_Record = Id;

            return View(task);
        }













        [HttpPost]
        public async Task<IActionResult> CreateTask(int Id_Group, string Name, string Description, string AnswerType, DateTime end)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskManage.CreateTask(user, Id_Group, Name, Description, AnswerType, end);
            return RedirectToAction("ManageTasks", "Task", new { Id_Group = Id_Group });

        }
        [HttpPost]
        public async Task<IActionResult> UpdateTask(int Id, int Id_Group, string Name, string Description, string AnswerType, DateTime end)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskManage.UpdateTask(user, Id, Id_Group, Name, Description, AnswerType, end);
            return RedirectToAction("ManageTasks", "Task", new { Id_Group = Id_Group });

        }
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int Id, int Id_Group)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskManage.DeleteTask(user, Id, Id_Group);
            return RedirectToAction("ManageTasks", "Task", new { Id_Group = Id_Group });

        }




        [HttpPost]
        public async Task<IActionResult> AddTaskUser(int Id_Task, string Id_user)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskManage.AddTaskUser(user, Id_Task, Id_user);
            return RedirectToAction("ManageUsers", "Task", new { Id_Task = Id_Task });

        }
        [HttpPost]
        public async Task<IActionResult> DeleteTaskUser(int Id_Task, string Id_user)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskManage.DeleteTaskUser(user, Id_Task, Id_user);
            return RedirectToAction("ManageUsers", "Task", new { Id_Task = Id_Task });

        }




        [HttpPost]
        public async Task<IActionResult> ChangeState(int Id, string State, int Id_Record)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            await _taskActions.ChangeState(user, Id, State);

            return RedirectToAction("WorkTask", "Task", new { Id = Id_Record });
        }
        [HttpPost]
        public async Task<IActionResult> AddAnswer(int Id, string? answer, IFormFile? file, int Id_Record)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            byte[]? fileanswer = null;
            string? extensionfile = null;

            if (file != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileanswer = memoryStream.ToArray();
                }

                extensionfile = Path.GetExtension(file.FileName);
            }

            await _taskActions.AddAnswer(user, Id, answer, fileanswer, extensionfile);

            return RedirectToAction("WorkTask", "Task", new { Id = Id_Record });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int Id)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            var fileResult = await _taskActions.DownloadAnswerFile(user, Id);

            if (fileResult == null)
            {
                return NotFound("File not found or you do not have permission to download it.");
            }

            return fileResult;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int Id, int Id_Record)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            await _taskActions.DeleteAnswerFile(user, Id);

            return RedirectToAction("WorkTask", "Task", new { Id = Id_Record });
        }



        [HttpPost]
        public async Task<IActionResult> Complete(int Id, int Id_Record)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            await _taskActions.Complete(user, Id);

            return RedirectToAction("WorkTask", "Task", new { Id = Id_Record });
        }










        [HttpGet]
        public async Task<IActionResult> UserProgress(int Id_TaskAnswer)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            UserTaskResultData data = await _taskResult.GetUserTaskResult(user, Id_TaskAnswer);

            return View(data);
        }


        [HttpPost]
        public async Task<IActionResult> InCompleted(int Id, string State)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskResult.InCompleted(user, Id, State);

            return RedirectToAction("UserProgress", "Task", new { Id_TaskAnswer = Id });
        }


        [HttpPost]
        public async Task<IActionResult> AddComment(int Id, string Comment)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskResult.AddComment(user, Id, Comment);

            return RedirectToAction("UserProgress", "Task", new { Id_TaskAnswer = Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int Id)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskResult.DeleteComment(user, Id);

            return RedirectToAction("UserProgress", "Task", new { Id_TaskAnswer = Id });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStateAdm(int Id, string State)
        {
            UserModel user = await _userManager.GetUserAsync(User);
            await _taskResult.ChangeStateAdm(user, Id, State);

            return RedirectToAction("UserProgress", "Task", new { Id_TaskAnswer = Id });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFileResult(int Id, string Firstname, string Lastname)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            var fileResult = await _taskResult.DownloadFileResult(user, Id, Firstname, Lastname);

            if (fileResult == null)
            {
                return NotFound("File not found or you do not have permission to download it.");
            }

            return fileResult;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadTextResult(int Id, string Firstname, string Lastname)
        {
            UserModel user = await _userManager.GetUserAsync(User);

            var fileResult = await _taskResult.DownloadTextResult(user, Id, Firstname, Lastname);

            if (fileResult == null)
            {
                return NotFound("File not found or you do not have permission to download it.");
            }

            return fileResult;
        }

    }
}
