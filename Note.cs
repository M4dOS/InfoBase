namespace InfoBase
{
    internal class Note
    {
        public string name; //имя предмета 
        public DateTime startTime; //начало 
        public DateTime endTime; //конец 
        public User teacher; //учитель 
        public string subname; //доп информация 
        public Auditorium auditorium; //аудитория 
        public List<User> participators; //записавшиеся 
        
        public Note(Note note)
        {
            if (note != null)
            {
                var note1 = note;
                this.name = note1.name;
                this.startTime = note1.startTime;
                this.endTime = note1.endTime;
                this.teacher = note1.teacher;
                this.subname = note1.subname;
                this.auditorium = note1.auditorium;
                this.participators = note1.participators;
            }
            else return;
        }
        public Note(string txtString, string day, DataBase db)
        {
            participators = new();
            string[] parametrs = txtString.Split("|");
            name = parametrs[0];
            startTime = DataBase.Date(day + ' ' + parametrs[1]);
            endTime = DataBase.Date(day + ' ' + parametrs[2]);

            if (db.GetUser(parametrs[3], false) != null) this.teacher = db.GetUser(parametrs[3], false);

            subname = parametrs[4];
            auditorium = db.GetAuditorium(parametrs[5]);
            if (auditorium == null) db.LogState($"Такой аудитории не существует: \"{parametrs[5]}\"");
            /*else capacity = auditorium.capacity;*/
        }
        public Note() 
        {
            teacher = new();
            auditorium = new();
            participators = new();
        }
    }
}
