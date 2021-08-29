using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

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



    static public void setupDB(MySqlConnection connection){
        connection.Open();
            try
            {
            // new MySqlCommand("drop table IF EXISTS Items",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS UserRoleRelation",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS ItemRoleRelation",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS Roles",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS Users",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS Content",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS TymelineObjects",connection).ExecuteNonQuery();
            
            
            // new MySqlCommand("create table Content ( id varchar(255) PRIMARY KEY , text varchar(255)); ",connection).ExecuteNonQuery();
            // new MySqlCommand("create table TymelineObjects ( id varchar(255) PRIMARY KEY, length int, start int, canChangeLength bool, canMove bool, ContentId varchar(255) ,constraint fk_content foreign key (ContentId) references Content(id) on update restrict on Delete Cascade); ",connection).ExecuteNonQuery();

            
            new MySqlCommand("create table TymelineObjects ( id varchar(255) PRIMARY KEY, length int, start int, canChangeLength bool, canMove bool, ContentId varchar(255), INDEX idx_ContentId(ContentId) ); ",connection).ExecuteNonQuery();
            new MySqlCommand("create table Content ( fk_tymeline varchar(255) , text varchar(255),constraint fk foreign key (fk_tymeline) references TymelineObjects(ContentId) on update restrict on Delete Cascade); ",connection).ExecuteNonQuery();
            
            new MySqlCommand("create table if not exists Users (user_id varchar(255) primary key, email varchar(255) not null, password varchar(255) not null, created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP)",connection).ExecuteNonQuery();
            new MySqlCommand("create index email on Users(email)",connection).ExecuteNonQuery();

            new MySqlCommand("create table if not exists Roles(role_id int primary key, role_name varchar(255) not null, role_value varchar(255) not null)",connection).ExecuteNonQuery();

            new MySqlCommand(@"create table if not exists UserRoleRelation(user_fk varchar(255) not null, role_fk int not null,
            foreign key (user_fk) references Users (user_id) on update restrict on delete cascade,
            foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
            unique(user_fk, role_fk) )",connection).ExecuteNonQuery();


            new MySqlCommand(@"create table if not exists ItemRoleRelation(item_fk varchar(64) not null, role_fk int not null,
            foreign key (item_fk) references TymelineObjects (id) on update restrict on delete cascade,
            foreign key (role_fk) references Roles (role_id) on update restrict on delete cascade,
            unique(item_fk, role_fk) )",connection).ExecuteNonQuery();
            }
            catch (System.Exception)
            {
                
                throw;
            }
            finally{
                connection.Close();
            }
    }


    public static void prepopulateTymelineObjects(MySqlConnection connection){
            try
            {
            connection.Open();

            var command = new MySqlCommand("insert into Content values (@id,@text)",connection);
            var commandtymeline = new MySqlCommand("insert into TymelineObjects (id, canChangeLength, canMove, start, length, ContentId) values(@id,true,false,FLOOR(RAND()*10000),FLOOR(RAND()*1000000),@guid); ",connection);
            var initialContentId = Guid.NewGuid().ToString();
            var initialTymelineId= Guid.NewGuid().ToString();


          

            commandtymeline.Parameters.AddWithValue("@id",initialTymelineId);
            commandtymeline.Parameters.AddWithValue("@guid",initialContentId);

            commandtymeline.ExecuteNonQuery();


            command.Parameters.AddWithValue("@id",initialContentId);
            command.Parameters.AddWithValue("@text",TestUtil.RandomString(50)); 
            command.ExecuteNonQuery();


            for (int i = 0; i < 50; i++)
            {
                var idContent = Guid.NewGuid().ToString();
                var idTymeline = Guid.NewGuid().ToString();
                commandtymeline.Parameters.Clear();
                command.Parameters.Clear();
                commandtymeline.Parameters.AddWithValue("@id",idTymeline);
                commandtymeline.Parameters.AddWithValue("@guid",idContent);
                commandtymeline.ExecuteNonQuery();
                command.Parameters.AddWithValue("@id",idContent);
                command.Parameters.AddWithValue("@text",TestUtil.RandomString(50));
                command.ExecuteNonQuery();
                
            }
       
            }
            catch (System.Exception)
            {
                throw;
            }
            finally{
                connection.Close();
            }
        }
}