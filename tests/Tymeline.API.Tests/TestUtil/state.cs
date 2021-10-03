using System;
using System.Collections.Generic;
using System.Linq;

public class TestState
{
    public Dictionary<string, IUser> userdict;
    public List<TymelineObject> tymelineList { get; private set; }
    public List<IRole> roleList { get; private set; }
    public Dictionary<string, List<IRole>> tymelineObjectRoles { get; private set; }
    public Dictionary<string, List<IRole>> userRoles { get; private set; }
    public Dictionary<IRole, List<string>> userManagement { get; private set; }


    public TestState()
    {
        tymelineList = TestUtil.setupTymelineList();
        roleList = TestUtil.CreateRoleList();
        userdict = TestUtil.createUserDict();
        userRoles = TestUtil.createRoleDict(userdict, roleList);

        tymelineObjectRoles = TestUtil.setupRoles(tymelineList, roleList);
        // userManagement = TestUtil.setupUserManagement(userdict, userRoles);
    }

    public IUserRoles MockGetUserRoles(string email)
    {
        return new UserRoles(email, userRoles[email]);
    }
    public ITymelineObjectRoles MockGetItemRoles(string toId)
    {
        return new TymelineObjectRoles(toId, tymelineObjectRoles[toId]);
    }
    public bool MockHasAccessToItem(string email, string itemid)
    {
        try
        {
            var ActiveRoles = new List<IRole>();
            userRoles.TryGetValue(email, out ActiveRoles);
            var ItemRoles = new List<IRole>();
            tymelineObjectRoles.TryGetValue(itemid, out ItemRoles);
            if (ActiveRoles.Intersect(ItemRoles).Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (System.Exception)
        {
            // either of the lists returned no valid result,
            // no intersection possible
            // no access!
            return false;
        }

    }


    public bool MockHasAccessToItem(string email, string itemid, Roles role)
    {
        try
        {
            var ActiveRoles = new List<IRole>();
            userRoles.TryGetValue(email, out ActiveRoles);
            var ValidRoles = ActiveRoles.Where(s => (int)Enum.Parse<Roles>(s.Value) >= (int)role).ToList();
            var ItemRoles = new List<IRole>();
            tymelineObjectRoles.TryGetValue(itemid, out ItemRoles);
            if (ValidRoles.Intersect(ItemRoles).Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (System.Exception)
        {
            // either of the lists returned no valid result,
            // no intersection possible
            // no access!
            return false;
        }

    }


    public IUserRoles MockAddRoleToUser(string email, IRole role)
    {
        MockAddRole(role);
        userRoles.TryGetValue(email, out var outUserRoles);
        outUserRoles.Add(role);
        return new UserRoles(email, outUserRoles);
    }

    public void MockAddRole(IRole role)
    {
        if (!roleList.Contains(role))
        {
            roleList.Add(role);
        }
    }

    public void MockRemoveRole(IRole role)
    {
        if (roleList.Contains(role))
        {

            var relevantTymelineObjects = tymelineObjectRoles.Where(s => s.Value.Contains(role)).ToList().Select(kvpair => kvpair.Key).ToList();
            relevantTymelineObjects.ForEach(to => MockRemoveRoleFromItem(role, to));
            var relevantUsers = userRoles.Where(s => s.Value.Contains(role)).ToList().Select(kvpair => kvpair.Key).ToList();
            relevantUsers.ForEach(user => MockRemoveUserRole(user, role));
            roleList.Remove(role);
        }


    }

    public ITymelineObjectRoles MockAddRoleToItem(IRole role, string toId)
    {
        MockAddRole(role);
        tymelineObjectRoles.TryGetValue(toId, out List<IRole> roles);
        if (!roles.Contains(role))
        {
            // only add each role once!
            roles.Add(role);
        }
        return new TymelineObjectRoles(toId, roles);

    }


    public ITymelineObjectRoles MockRemoveRoleFromItem(IRole role, string toId)
    {
        tymelineObjectRoles.TryGetValue(toId, out List<IRole> roles);
        roles.Remove(role);
        return new TymelineObjectRoles(toId, roles);
    }

    public IUserRoles MockRemoveUserRole(string email, IRole Role)
    {
        userRoles.TryGetValue(email, out var outUserRoles);
        outUserRoles.Remove(Role);
        return new UserRoles(email, outUserRoles);
    }



    public IUserRoles MockGetUserPermissions(string email)
    {
        return new UserRoles(email, userRoles[email]);
    }



    public List<IRole> MockGetRoles()
    {
        return roleList;
    }


    public List<IUser> MockGetUsers()
    {
        return userdict.Values.ToList();

    }

    public void MockSetRoles(IUserRoles userPermissions)
    {
        if (userRoles.ContainsKey(userPermissions.Email))
        {
            userRoles[userPermissions.Email] = userPermissions.Roles;
        }
        else
        {
            userRoles.Add(userPermissions.Email, userPermissions.Roles);
        }
    }

    public TymelineObject mockCreate(TymelineObject to)
    {

        if (tymelineList.Exists(x => x.Id.Equals(to.Id)))
        {
            throw new ArgumentException("you cannot create a TymelineObject with an existing id!");
        }
        else
        {
            if (to.Id == null)
            {
                to.Id = Guid.NewGuid().ToString();
            }
            tymelineList.Add(to);
            return to;
        }

    }

    public List<TymelineObject> mockGetAll(string Email, Roles minRole)
    {
        return tymelineList.Where(to => MockHasAccessToItem(Email, to.Id)).Distinct().ToList();
    }



    public TymelineObject mockGetById(string id)
    {
        try
        {

            return tymelineList.First(obj => obj.Id.Equals(id));

        }
        catch (System.Exception)
        {

            throw new ArgumentException();
        }
    }
    public TymelineObject MockUpdateById(string id, TymelineObject tymelineObject)
    {
        mockDeleteById(id);
        return mockCreate(tymelineObject);
    }
    public void mockDeleteById(string id)
    {
        var element = tymelineList.Find(element => element.Id.Equals(id));
        if (element != null)
        {
            //only delete the old one if it exists!
            tymelineList.Remove(element);
        }
    }
    public List<TymelineObject> mockDaoGetByTime(int start, int end)
    {
        var s = tymelineList.Where(element => start < element.Start + element.Length && start > element.Start + element.Length).ToList();
        s.AddRange(tymelineList.Where(element => start < element.Start && element.Start < end).ToList());
        return s.Distinct().ToList();
    }
    public IUser MockLogin(UserCredentials credentials)
    {
        if (credentials.complete())
        {
            // check if user is registered
            if (userdict.ContainsKey(credentials.Email))
            {
                return TestUtil.MockPasswdCheck(credentials.Password, userdict[credentials.Email]);
            }
            throw new ArgumentException();
        }
        else
        {
            throw new ArgumentException();
        }
    }


    public List<TymelineObject> MockTymelineReturnByTime(int start, int end)
    {

        var s = tymelineList.Where(element => start < element.Start + element.Length && start > element.Start + element.Length).ToList();
        s.AddRange(tymelineList.Where(element => start < element.Start && element.Start < end).ToList());
        return s.Distinct().ToList();
    }

    public TymelineObject MockTymelineReturnById(string identifier)

    {

        var results = tymelineList.Where(element => element.Id.Equals(identifier)).ToList();
        // var results = from obj in array where obj.Id.Equals(identifier) select obj; 

        switch (results.Count())
        {
            case 1:
                return results[0];
            case 0:
                throw new ArgumentException("key does not exist in the result");
            default:
                throw new ArgumentException("doesnt make sense!");
        }
    }

}