namespace InfoBase
{
    internal enum Access { User, Teacher, Admin }
    internal class User
    {
        public string user;
        public string password;
        public Access access;
        public User(string user, string password, string access)
        {
            this.user = user;
            this.password = password;
            switch (access)
            {
                case "user":
                    this.access = Access.User;
                    break;
                case "teacher":
                    this.access = Access.Teacher;
                    break;
                case "admin":
                    this.access = Access.Admin;
                    break;
                default: 
                    Console.WriteLine($"уровень доступа {user} не получен");
                    break;
            }
        }
    }
}
