﻿namespace InfoBase
{
    internal enum Access { User, Teacher, Admin }
    internal class User
    {
        public string login;
        public string password;
        public Access access;
        public string name;
        public List<Note> participating;
        public User(string user, string password, string access, string name, DataBase db)
        {
            login = user;
            this.password = password;
            this.participating = new();
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
                    db.LogState($"Уровень доступа {user} не получен");
                    break;
            }
            this.name = name;
        }
        public User()
        {
            participating = new();
        }
    }
}
