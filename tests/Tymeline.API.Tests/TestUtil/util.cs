using System;
using System.Collections.Generic;
using System.Linq;

public class TestUtil{

    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static bool RandomBool()
    {
        return (random.Next() % 2)==1;
    }

    public static int RandomInt(){
        return random.Next();
    }

        public static int RandomIntWithMax(int maxValue){
        return random.Next(maxValue);
    }


 
    static public List<TymelineObject> setupTymelineList(){



            // this will return different objects each run! be prepared to not test for anything but existance of some attributes
            // DO NOT TEST THE VALUE OF ATTRIBUTES NOT CREATED AS A MOCK SPECIFICALLY FOR USE IN THAT TEST
            // IT WILL BREAK AND YOU WILL HATE LIFE

            List<TymelineObject> array = new List<TymelineObject>();
            for (int i = 1; i < 100; i++)
            {

                array.Add( new TymelineObject() {
                    Id=i.ToString(),
                    Length=500+(random.Next() % 5000),
                    Content=new Content(RandomString(12)),
                    Start=10000+(random.Next() % 5000),
                    CanChangeLength=RandomBool(),
                    CanMove=RandomBool()
                    }
                );
            }
            return array;
        }



    public static Dictionary<string,List<IRole>> setupRoles(List<TymelineObject> tymelineObjects){
        var returnDict = new Dictionary<string,List<IRole>>();
        tymelineObjects.ForEach(o => returnDict.Add(o.Id,new List<IRole>()));
        return returnDict;
    }
}