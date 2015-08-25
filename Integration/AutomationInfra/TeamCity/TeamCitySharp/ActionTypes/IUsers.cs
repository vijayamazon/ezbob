namespace TeamCity.ActionTypes
{
    using System.Collections.Generic;
    using TeamCity.DomainEntities;

    public interface IUsers
    {
        List<User> All();
        User Details(string userName);
        List<Role> AllRolesByUserName(string userName);
        List<Group> AllGroupsByUserName(string userName);
        List<Group> AllUserGroups();
        List<User> AllUsersByUserGroup(string userGroupName);
        List<Role> AllUserRolesByUserGroup(string userGroupName);
        bool Create(string username, string name, string email, string password);
        bool AddPassword(string username, string password);
    }
}