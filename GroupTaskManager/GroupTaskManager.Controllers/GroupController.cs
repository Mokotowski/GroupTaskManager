using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GroupTaskManager.GroupTaskManager.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly IGroupManage _groupManage;
        private readonly UserManager<UserModel> _userManager;

        public GroupController(IGroupManage groupManage, UserManager<UserModel> userManager)
        {
            _groupManage = groupManage;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> ManageGroups()
        {
            List<Group> groups = await _groupManage.MyManageGroups(await _userManager.GetUserAsync(User));

            return View(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GroupUsers(int Id_Group)
        {
            List<UserFindResult> users = await _groupManage.GetGroupUsers(await _userManager.GetUserAsync(User), Id_Group);
            ViewBag.Id_Group = Id_Group;
            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> MyGroups()
        {
            List<Group> groups = await _groupManage.MyGroups(await _userManager.GetUserAsync(User));

            return View(groups);
        }








        [HttpPost]
        public async Task<IActionResult> CreateGroup(string Name, string Description)
        {
            await _groupManage.CreateGroup(await  _userManager.GetUserAsync(User), Name, Description);

            return RedirectToAction("ManageGroups", "Group");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGroup(int Id_Group, string Name, string Description)
        {
            await _groupManage.UpdateGroup(await _userManager.GetUserAsync(User), Id_Group, Name, Description);

            return RedirectToAction("ManageGroups", "Group");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGroup(int Id_Group)
        {
            await _groupManage.DeleteGroup(await _userManager.GetUserAsync(User), Id_Group);

            return RedirectToAction("ManageGroups", "Group");
        }



        [HttpGet]
        public async Task<IActionResult> ResultUsers(int Id_Group, string Phrase, string type)
        {
            ViewBag.Id_Group = Id_Group;
            List<UserFindResult> users = await _groupManage.CheckUsers(Phrase, type);
            return View(users);
        }




        [HttpPost]
        public async Task<IActionResult> AddUser(int Id_Group, string Id_newuser)
        {
            await _groupManage.AddUser(await _userManager.GetUserAsync(User), Id_Group, Id_newuser);

            return RedirectToAction("GroupUsers", "Group", new { Id_Group = Id_Group });

        }


        [HttpPost]
        public async Task<IActionResult> DeleteUser(int Id_Group, int Id)
        {
            await _groupManage.DeleteUser(await _userManager.GetUserAsync(User), Id_Group, Id);

            return RedirectToAction("GroupUsers", "Group", new { Id_Group = Id_Group });

        }






    }
}
