using System;
using System.Collections.Generic;
using System.Linq;

public class TestUtil
{

    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static bool RandomBool()
    {
        return (random.Next() % 2) == 1;
    }

    public static int RandomInt()
    {
        return random.Next();
    }

    public static int RandomIntWithMax(int maxValue)
    {
        return random.Next(maxValue);
    }

    static public List<IRole> CreateRoleList()
    {
        List<IRole> roles = new List<IRole>();


        for (int i = 1; i < 100; i++)
        {
            roles.Add(new Role(RandomString(12), RandomString(12))
            );
        }
        return roles;
    }

    static public List<TymelineObject> setupTymelineList()
    {



        // this will return different objects each run! be prepared to not test for anything but existance of some attributes
        // DO NOT TEST THE VALUE OF ATTRIBUTES NOT CREATED AS A MOCK SPECIFICALLY FOR USE IN THAT TEST
        // IT WILL BREAK AND YOU WILL HATE LIFE

        List<TymelineObject> array = new List<TymelineObject>();
        for (int i = 1; i < 100; i++)
        {

            array.Add(new TymelineObject()
            {
                Id = i.ToString(),
                Length = 500 + (random.Next() % 5000),
                Content = new Content(RandomString(12)),
                Start = 10000 + (random.Next() % 5000),
                CanChangeLength = RandomBool(),
                CanMove = RandomBool()
            }
            );
        }
        return array;
    }



    public static Dictionary<string, List<IRole>> setupRoles(List<TymelineObject> tymelineObjects, List<IRole> roles)
    {
        var returnDict = new Dictionary<string, List<IRole>>();
        tymelineObjects.ForEach(o =>
            returnDict.Add(o.Id, new List<IRole>(roles)
        ));
        return returnDict;
    }


    static public Dictionary<string, IUser> createUserDict()
    {
        var passwordHasher = new PasswordHasher();
        Dictionary<string, IUser> users = new Dictionary<string, IUser>();
        for (int i = 2; i < 100; i++)
        {
            User user = new User($"test{i}@email.de", passwordHasher.Hash("hunter12"));
            users.Add(user.Email, user);
        }
        return users;
    }

    static public Dictionary<string, List<IRole>> createRoleDict(Dictionary<string, IUser> users, List<IRole> roles)
    {

        Dictionary<string, List<IRole>> returnDict = new Dictionary<string, List<IRole>>();

        users.Keys.ToList().ForEach(key =>
      {
          var user = new UserRoles(key, new List<IRole>(roles));
          returnDict.Add(user.Email, user.Roles);
      });

        return returnDict;
    }
}