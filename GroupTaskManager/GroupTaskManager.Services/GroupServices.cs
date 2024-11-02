using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GroupTaskManager.GroupTaskManager.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GroupTaskManager.GroupTaskManager.Services
{
    public class GroupServices : IGroupManage, IGroupCheck
    {
        private readonly ILogger<GroupServices> _logger;
        private readonly DatabaseContext _databaseContext;

        public GroupServices(ILogger<GroupServices> logger, DatabaseContext databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        public async Task CreateGroup(UserModel user, string Name, string Description)
        {
            try
            {
                var group = new Group
                {
                    Id_User = user.Id,
                    Name = Name,
                    Description = Description,
                    User = user
                };

                await _databaseContext.Group.AddAsync(group);
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation($"Group '{Name}' created by User ID {user.Id}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group.");
                throw;
            }
        }

        public async Task UpdateGroup(UserModel user, int Id_Group, string Name, string Description)
        {
            try
            {
                var group = await _databaseContext.Group.FindAsync(Id_Group);
                if (group == null)
                {
                    _logger.LogWarning($"Group ID {Id_Group} not found.");
                    return;
                }

                if (group.Id_User != user.Id)
                {
                    _logger.LogWarning($"User ID {user.Id} unauthorized to update Group ID {Id_Group}.");
                    return;
                }

                group.Name = Name;
                group.Description = Description;

                _databaseContext.Group.Update(group);
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation($"Group ID {Id_Group} updated by User ID {user.Id}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group.");
                throw;
            }
        }

        public async Task DeleteGroup(UserModel user, int Id_Group)
        {
            try
            {
                var group = await _databaseContext.Group.FindAsync(Id_Group);
                if (group == null)
                {
                    _logger.LogWarning($"Group ID {Id_Group} not found.");
                    return;
                }

                if (group.Id_User != user.Id)
                {
                    _logger.LogWarning($"User ID {user.Id} unauthorized to delete Group ID {Id_Group}.");
                    return;
                }

                _databaseContext.Group.Remove(group);
                await _databaseContext.SaveChangesAsync();
                _logger.LogInformation($"Group ID {Id_Group} deleted by User ID {user.Id}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group.");
                throw;
            }
        }

        public async Task AddUser(UserModel user, int Id_Group, string Id_newuser)
        {
            try
            {
                var group = await _databaseContext.Group.FindAsync(Id_Group);
                if (group == null)
                {
                    _logger.LogWarning($"Group ID {Id_Group} not found.");
                    return;
                }

                if (group.Id_User != user.Id)
                {
                    _logger.LogWarning($"User ID {user.Id} unauthorized to add users to Group ID {Id_Group}.");
                    return;
                }

                if (!await _databaseContext.Group_User.AnyAsync(p => p.Id_User == Id_newuser && p.Id_Group == Id_Group))
                {
                    var group_User = new Group_User
                    {
                        Id_User = Id_newuser,
                        Id_Group = Id_Group,
                        Group = group
                    };

                    await _databaseContext.Group_User.AddAsync(group_User);
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation($"User ID {Id_newuser} added to Group ID {Id_Group} by User ID {user.Id}.");
                }
                else
                {
                    _logger.LogInformation($"User ID {Id_newuser} is already in Group ID {Id_Group}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user to group.");
                throw;
            }
        }

        public async Task DeleteUser(UserModel user, int Id_Group, int Id)
        {
            try
            {
                var group = await _databaseContext.Group.FindAsync(Id_Group);
                if (group == null)
                {
                    _logger.LogWarning($"Group ID {Id_Group} not found.");
                    return;
                }

                if (group.Id_User != user.Id)
                {
                    _logger.LogWarning($"User ID {user.Id} unauthorized to delete users from Group ID {Id_Group}.");
                    return;
                }

                var group_User = await _databaseContext.Group_User.FindAsync(Id);
                if (group_User != null)
                {
                    _databaseContext.Group_User.Remove(group_User);
                    await _databaseContext.SaveChangesAsync();
                    _logger.LogInformation($"Group User ID {Id} removed from Group ID {Id_Group} by User ID {user.Id}.");
                }
                else
                {
                    _logger.LogWarning($"Group User ID {Id} not found in Group ID {Id_Group}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user from group.");
                throw;
            }
        }

        public async Task<List<UserFindResult>> GetGroupUsers(UserModel user, int Id_Group)
        {
            try
            {
                var group = await _databaseContext.Group.FindAsync(Id_Group);
                if (group == null)
                {
                    _logger.LogWarning($"Group ID {Id_Group} not found.");
                    return new List<UserFindResult>();
                }

                if (group.Id_User != user.Id)
                {
                    _logger.LogWarning($"User ID {user.Id} unauthorized to view users in Group ID {Id_Group}.");
                    return new List<UserFindResult>();
                }



                var users = await _databaseContext.Group_User
                    .Where(p => p.Id_Group == Id_Group)
                    .Join(
                        _databaseContext.Users,
                        groupUser => groupUser.Id_User, 
                        user => user.Id,               
                        (groupUser, user) => new UserFindResult 
                        {
                            Id_Group_User = groupUser.Id,
                            Firstname = user.Firstname,
                            Lastname = user.Lastname,
                            Email = user.Email
                        }
                    )
                    .ToListAsync();



                _logger.LogInformation($"User ID {user.Id} retrieved users for Group ID {Id_Group}.");
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users from group.");
                throw;
            }
        }


        public async Task<List<Group>> MyManageGroups(UserModel user)
        {
            try
            {
                _logger.LogInformation($"Fetching managed groups for User ID {user.Id}.");

                var MyGroups = await _databaseContext.Group
                    .Where(g => g.Id_User == user.Id)
                    .ToListAsync();

                _logger.LogInformation($"Found {MyGroups.Count} managed groups for User ID {user.Id}.");
                return MyGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching managed groups for User ID {user.Id}.");
                throw;
            }
        }

        public async Task<List<Group>> MyGroups(UserModel user)
        {
            try
            {
                _logger.LogInformation($"Fetching groups for User ID {user.Id}.");

                var Id_Groups = await _databaseContext.Group_User
                    .Where(p => p.Id_User == user.Id)
                    .Select(p => p.Id_Group)
                    .Distinct()
                    .ToListAsync();

                if (!Id_Groups.Any())
                {
                    _logger.LogWarning($"No groups found for User ID {user.Id}.");
                    return new List<Group>();
                }

                var groups = await _databaseContext.Group
                    .Where(p => Id_Groups.Contains(p.Id))
                    .ToListAsync();

                _logger.LogInformation($"Found {groups.Count} groups for User ID {user.Id}.");
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching groups for User ID {user.Id}.");
                throw;
            }
        }

        public async Task<List<UserFindResult>> CheckUsers(string Phrase, string type)
        {
            List<UserFindResult> results = new List<UserFindResult>();

            // Valid search types
            var validTypes = new HashSet<string> { "email", "firstname", "lastname" };

            if (!validTypes.Contains(type.ToLower()))
            {
                _logger.LogWarning("Invalid search type provided: {Type}", type);
                return results;  
            }

            try
            {
                _logger.LogInformation("Starting search for users by {Type} with phrase: {Phrase}", type, Phrase);

                IQueryable<UserModel> query = _databaseContext.Users;

                if (type == "email")
                {
                    query = query.Where(u => u.Email.Contains(Phrase));
                }
                else if (type == "firstname")
                {
                    query = query.Where(u => u.Firstname.Contains(Phrase));
                }
                else if (type == "lastname")
                {
                    query = query.Where(u => u.Lastname.Contains(Phrase));
                }

                results = await query
                    .Select(u => new UserFindResult
                    {
                        Id = u.Id,
                        Firstname = u.Firstname,
                        Lastname = u.Lastname,
                        Email =  u.Email
                    })
                    .ToListAsync();

                _logger.LogInformation("Search completed successfully. {Count} results found.", results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for users with type: {Type} and phrase: {Phrase}", type, Phrase);
                return new List<UserFindResult>(); 
            }

            return results;
        }




        public async Task<bool> IsOwnerGroup(UserModel user, int Id_Group)
        {
            try
            {
                _logger.LogInformation($"Checking if User ID {user.Id} is the owner of Group ID {Id_Group}.");

                bool isOwner = await _databaseContext.Group.AnyAsync(p => p.Id == Id_Group && p.Id_User == user.Id);

                if (isOwner)
                {
                    _logger.LogInformation($"User ID {user.Id} is the owner of Group ID {Id_Group}.");
                }
                else
                {
                    _logger.LogWarning($"User ID {user.Id} is not the owner of Group ID {Id_Group}.");
                }

                return isOwner;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if User ID {user.Id} is the owner of Group ID {Id_Group}.");
                throw;
            }
        }

        public async Task<bool> IsMember(UserModel user, int Id_Group)
        {
            try
            {
                _logger.LogInformation($"Checking if User ID {user.Id} is a member of Group ID {Id_Group}.");

                bool isMember = await _databaseContext.Group_User.AnyAsync(p => p.Id_Group == Id_Group && p.Id_User == user.Id);

                if (isMember)
                {
                    _logger.LogInformation($"User ID {user.Id} is a member of Group ID {Id_Group}.");
                }
                else
                {
                    _logger.LogWarning($"User ID {user.Id} is not a member of Group ID {Id_Group}.");
                }

                return isMember;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if User ID {user.Id} is a member of Group ID {Id_Group}.");
                throw;
            }
        }

    }
}
