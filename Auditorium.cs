using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;

namespace InfoBase
{
    internal class Auditorium
    {
        public string tag;
        public string startTime;
        public string endTime;
        public int capacity;
        public List<Note> timetable;

        public Auditorium(string tag, string startTime, string endTime, int capacity)
        {
            this.tag = tag;
            this.startTime = startTime;
            this.endTime = endTime;
            this.capacity = capacity;
            this.timetable = new List<Note>();
        }
        public void AddNote(Note note, DataBase db)
        {
            var startTime = DataBase.Date($"{note.startTime.Day}.{note.startTime.Month}.{note.startTime.Year}" + " " + this.startTime);
            var endTime = DataBase.Date($"{note.endTime.Day}.{note.endTime.Month}.{note.endTime.Year}" + " " + this.endTime);

            if (note.auditorium!=null && note.startTime >= startTime && note.endTime <= endTime)
            {
                bool cond = true;
                foreach(var note1 in timetable)
                {
                    if(!(note1.endTime<=note.startTime || note1.startTime>=note.endTime)) { cond = false; break; }
                }
                if (cond) { timetable.Add(note); timetable.Sort((x, y) => x.startTime.CompareTo(y.startTime)); }
                else
                {
                    db.LogState("Бронь несовместима с другими бронями (Пересекается с другими бронями)");
                }
            }

            else
            {
                db.LogState("Бронь неудовлетворяет условиям (Не подходит под допустимое время аудитории)");
            }
        }
    }
}
