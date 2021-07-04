public class AppSettings
    {
        public string Secret { get; set; } = "a8424df6d555c85b07ea2c746289f9eba649ad2b327819d2aa0b035ea0a9159e";
        public string Hostname {get; set;} = "localhost";

        public SqlConnectionSettings SqlConnection {get; set;}
    }

public class SqlConnectionSettings{

        public string MySqlConnectionString {get; set;}
        public string PostgresConnectionString {get; set;}
}