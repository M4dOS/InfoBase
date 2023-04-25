namespace InfoBase
{
    internal class Note
    {
        public string name;
        public DateTime startTime;
        public DateTime endTime;
        public string teacher;
        public string subname;
        public Auditorium auditorium;

        public int capacity;
        public List<User> participators;
        public Note(string name, DateTime startTime, DateTime endTime, string teacher, string subname, Auditorium auditorium)
        {
            this.participators = new();
            this.name = name;
            this.startTime = startTime;
            this.endTime = endTime;
            this.teacher = teacher;
            this.subname = subname;
            this.auditorium = auditorium;
            this.capacity = auditorium.capacity;
        }
        public Note(string txtString, string day, DataBase db)
        {
            this.participators = new();
            string[] parametrs = txtString.Split("|");
            name = parametrs[0];
            startTime = DataBase.Date(day + ' ' + parametrs[1]);
            endTime = DataBase.Date(day + ' ' + parametrs[2]); ;
            teacher = parametrs[3];
            subname = parametrs[4];
            auditorium = db.FindAuditorium(parametrs[5]);
            if (auditorium == null) Console.WriteLine($"такой аудитории не существует: \"{parametrs[5]}\"");
            else this.capacity = auditorium.capacity;
        }
    }
}
