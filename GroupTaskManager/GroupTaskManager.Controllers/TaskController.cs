using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
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
        private readonly DatabaseContext _databaseContext;
        public TaskController(UserManager<UserModel> userManager, ITaskManageServices taskManage)
        {
            _userManager = userManager;
            _taskManage = taskManage;
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
        public async Task<IActionResult> MyTasks(int Id_Group)
        {
            return View();
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






    }
}
