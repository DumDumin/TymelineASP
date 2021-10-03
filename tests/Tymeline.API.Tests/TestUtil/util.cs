using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using MySql.Data.MySqlClient;




public class TestUtil
{


    public static Dictionary<IRole, List<string>> setupUserManagement(Dictionary<string, IUser> userDict, Dictionary<string, List<IRole>> userRoles)
    {
        var retDict = new Dictionary<IRole, List<string>>();



        return retDict;
    }

    public static IUserRoles mockGetUserPermissions(string email)
    {
        var UserPermissions = new UserRoles(email, new List<IRole>());
        UserPermissions.Roles.Add(new Role("Frontend", "value"));
        return UserPermissions;
    }

    public static IUser MockPasswdCheck(string Password, IUser BaseUser)
    {
        return BaseUser.verifyPassword(Password);
    }

    public static TymelineObject mockCreateTymelineObject(TymelineObject tO)
    {
        tO.Id = Guid.NewGuid().ToString();
        return tO;
    }

    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static Roles RandomRole()
    {
        return (Roles)random.Next(Enum.GetNames(typeof(Roles)).Length);
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
            roles.Add(new Role(RandomString(12), Enum.GetName(typeof(Roles), RandomRole()))
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
                Id = Guid.NewGuid().ToString(),
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
            returnDict.Add(o.Id, new List<IRole>(roles.RandomElements(5))
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
          var user = new UserRoles(key, new List<IRole>(roles.RandomElements(5)));
          returnDict.Add(user.Email, user.Roles);
      });

        return returnDict;
    }



    static public void setupDB(MySqlConnection connection)
    {
        connection.Open();
        try
        {
            // new MySqlCommand("drop table IF EXISTS Items",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS UserRoleRelation", connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS ItemRoleRelation", connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS Roles", connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS Users", connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS Content", connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS TymelineObjects", connection).ExecuteNonQuery();


            // new MySqlCommand("create table Content ( id varchar(255) PRIMARY KEY , text varchar(255)); ",connection).ExecuteNonQuery();
            // new MySqlCommand("create table TymelineObjects ( id varchar(255) PRIMARY KEY, length int, start int, canChangeLength bool, canMove bool, ContentId varchar(255) ,constraint fk_content foreign key (ContentId) references Content(id) on update restrict on Delete Cascade); ",connection).ExecuteNonQuery();


            new MySqlCommand("create table TymelineObjects ( id varchar(255) PRIMARY KEY, length int, start int, canChangeLength bool, canMove bool, ContentId varchar(255), INDEX idx_ContentId(ContentId) ); ", connection).ExecuteNonQuery();
            new MySqlCommand("create table Content ( fk_tymeline varchar(255) , text varchar(255),constraint fk foreign key (fk_tymeline) references TymelineObjects(ContentId) on update restrict on Delete Cascade); ", connection).ExecuteNonQuery();

            new MySqlCommand("create table if not exists Users (user_id varchar(255) primary key, email varchar(255) not null, password varchar(255) not null, created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP)", connection).ExecuteNonQuery();
            new MySqlCommand("create index email on Users(email)", connection).ExecuteNonQuery();

            new MySqlCommand("create table if not exists Roles(role_id int primary key, role_name varchar(255) not null, role_value varchar(255) not null)", connection).ExecuteNonQuery();

            new MySqlCommand(@"create table if not exists UserRoleRelation(user_fk varchar(255) not null, role_fk int not null,
            foreign key (user_fk) references Users (user_id) on update restrict on delete cascade,
            foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
            unique(user_fk, role_fk) )", connection).ExecuteNonQuery();


            new MySqlCommand(@"create table if not exists ItemRoleRelation(item_fk varchar(64) not null, role_fk int not null,
            foreign key (item_fk) references TymelineObjects (id) on update restrict on delete cascade,
            foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
            unique(item_fk, role_fk) )", connection).ExecuteNonQuery();
        }
        catch (System.Exception)
        {

            throw;
        }
        finally
        {
            connection.Close();
        }
    }


    public static List<TymelineObject> prepopulateTymelineObjects(MySqlConnection connection)
    {
        try
        {
            Fixture fix = new Fixture();
            List<TymelineObject> items = fix.CreateMany<TymelineObject>(100).ToList();
            connection.Open();

            var command = "insert into Content values (@id,@text)";
            var commandtymeline = @"insert into TymelineObjects (id, canChangeLength, canMove, start, length, ContentId) 
            values(@id,@canChangeLength,@canMove,@start,@length,@guid);";
            MySqlCommand cmd = new MySqlCommand();
            items.ForEach(item =>
            {
                cmd.Connection = connection;
                cmd.CommandText = commandtymeline;
                cmd.Parameters.AddWithValue("@id", item.Id);
                cmd.Parameters.AddWithValue("@canChangeLength", item.CanChangeLength);
                cmd.Parameters.AddWithValue("@canMove", item.CanMove);
                cmd.Parameters.AddWithValue("@start", item.Start);
                cmd.Parameters.AddWithValue("@length", item.Length);
                cmd.Parameters.AddWithValue("@guid", item.Content.GetHashCode().ToString());

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.CommandText = command;

                cmd.Parameters.AddWithValue("@id", item.Content.GetHashCode().ToString());
                cmd.Parameters.AddWithValue("@text", item.Content.Text);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

            });
            return items;

        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
            connection.Close();
        }
    }



    public static List<IRole> prepopulateRoles(MySqlConnection connection)
    {
        connection.Open();
        Fixture fix = new Fixture();
        fix.Customizations.Add(
            new TypeRelay(
                typeof(IRole),
                typeof(Role)));
        ;
        List<IRole> expectRole = fix.CreateMany<IRole>(100).ToList();
        MySqlCommand cmd = new MySqlCommand();
        expectRole.ForEach(role =>
        {
            string CommandText = "INSERT into Roles(role_id,role_name,role_value) values(@role_id,@role_name,@role_value)";
            cmd.Connection = connection;
            cmd.CommandText = CommandText;

            cmd.Parameters.AddWithValue("@role_id", role.RoleId);
            cmd.Parameters.AddWithValue("@role_name", role.Type);
            cmd.Parameters.AddWithValue("@role_value", role.Value);
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

        });
        connection.Close();
        return expectRole;
    }


    public static List<IUserCredentials> prepopulateUser(MySqlConnection connection)
    {
        connection.Open();
        Fixture fix = new Fixture();
        fix.Customizations.Add(
           new TypeRelay(
               typeof(IUserCredentials),
               typeof(UserCredentials)));
        ;
        List<IUserCredentials> expectedUsercreds = fix.CreateMany<IUserCredentials>(100).ToList();
        MySqlCommand cmd = new MySqlCommand();
        expectedUsercreds.ForEach(creds =>
        {
            string CommandText = "INSERT into Users(user_id,email,password) values(@user_id,@email,@password)";
            cmd.Connection = connection;
            cmd.CommandText = CommandText;
            creds.Password = "asdf1234";
            var user = User.CredentialsToUser(creds).ToDaoUser();

            cmd.Parameters.AddWithValue("@user_id", user.user_id);
            cmd.Parameters.AddWithValue("@email", user.email);
            cmd.Parameters.AddWithValue("@password", user.password);
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

        });
        connection.Close();
        return expectedUsercreds;
    }


    public static void prepopulateUserRoles(MySqlConnection connection, List<IRole> roles, List<IUserCredentials> credentials)
    {

        connection.Open();
        var command = new MySqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT into UserRoleRelation(user_fk,role_fk) values(@user_fk,@role_fk)";
        credentials.ForEach(creds =>
        {
            var user = User.CredentialsToUser(creds).ToDaoUser();
            var userRoles = roles.RandomElements(3);
            userRoles.ForEach(role =>
            {
                command.Parameters.AddWithValue("@user_fk", user.user_id);
                command.Parameters.AddWithValue("@role_fk", role.RoleId);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });
        });
        connection.Close();
    }


    public static void EnsureSuccessfullSetup(MySqlConnection connection)
    {
        connection.Open();
        var command = new MySqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT COUNT(*) FROM ItemRoleRelation";
        int count = Convert.ToInt32(command.ExecuteScalar());
        if (!count.Equals(300))
        {
            throw new ArgumentException("not the correct count of ItemRoleRelation");
        }
        command = new MySqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT COUNT(*) FROM UserRoleRelation";
        count = Convert.ToInt32(command.ExecuteScalar());
        if (!count.Equals(300))
        {
            throw new ArgumentException("not the correct count of UserRoleRelation");
        }
        connection.Close();
    }

    public static void prepopulateItemRoles(MySqlConnection connection, List<IRole> roles, List<TymelineObject> items)
    {

        connection.Open();
        var command = new MySqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT into ItemRoleRelation(item_fk,role_fk) values(@item_fk,@role_fk)";
        items.ForEach(item =>
        {
            var userRoles = roles.RandomElements(3);
            userRoles.ForEach(role =>
            {
                command.Parameters.AddWithValue("@item_fk", item.Id);
                command.Parameters.AddWithValue("@role_fk", role.RoleId);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
            });
        });
        connection.Close();
    }
}