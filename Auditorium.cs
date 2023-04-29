namespace InfoBase
{
    internal class Auditorium
    {
        public string tag; //номер аудитории
        public string startTime; //начало брони 
        public string endTime; //конец брони 
        public int capacity; //вместимость 
        public List<Note> timetable; //расписание 

        public Auditorium(string tag, string startTime, string endTime, int capacity)
        {
            this.tag = tag;
            this.startTime = startTime;
            this.endTime = endTime;
            this.capacity = capacity;
            this.timetable = new();
        }
        public bool AddNote(Note note, DataBase db)
        {
            var startTime = DataBase.Date($"{note.startTime.Day}.{note.startTime.Month}.{note.startTime.Year}" + " " + this.startTime);
            var endTime = DataBase.Date($"{note.endTime.Day}.{note.endTime.Month}.{note.endTime.Year}" + " " + this.endTime);

            if (note.auditorium != null && note.startTime >= startTime && note.endTime <= endTime)
            {
                bool cond = true;
                foreach (var note1 in timetable)
                {
                    if (!(note1.endTime <= note.startTime || note1.startTime >= note.endTime)) { cond = false; break; }
                }

                if (note.teacher == null) {db.LogState($"В брони \"{note.name}\" нету преподавателя (возможно, он отсутствует в базе данных)");return false;}
                if (cond) { timetable.Add(note); timetable.Sort((x, y) => x.startTime.CompareTo(y.startTime)); return true; }
                else
                {
                    db.LogState($"Бронь \"{note.name}\" несовместима с другими бронями (Пересекается с другими бронями)");
                    return false;
                }
            }

            else
            {
                db.LogState($"Бронь \"{note.name}\" неудовлетворяет условиям (Не подходит под допустимое время аудитории)");
                return false;
            }
        }
        public Auditorium() 
        {
            this.timetable = new();
        }
    }
}
