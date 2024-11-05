using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GroupTaskManager.GroupTaskManager.Controllers
{
    public class TaskController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly ITaskManageServices _taskManage;

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
                AddTaskUser - Listy z wszystkimi userami info czy są przypisań
                DeleteTaskUser - listy z dodanymi do taska
        */


        [HttpGet]
        public async Task<IActionResult> ManageTasks(int Id_Group)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MyTasks(int Id_Group)
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> CreateTask(int Id_Group, string Name, string Description, string AnswerType, DateTime end)
        {

        }
        [HttpPost]
        public async Task<IActionResult> UpdateTask(int Id, int Id_Group, string Name, string Description, string AnswerType, DateTime end)
        {

        }
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int Id, int Id_Group)
        {

        }




        [HttpPost]
        public async Task<IActionResult> AddTaskUser(int Id_Task, List<string> Id_users)
        {

        }
        [HttpPost]
        public async Task<IActionResult> DeleteTaskUser(int Id_Task, List<string> Id_users)
        {

        }






    }
}
