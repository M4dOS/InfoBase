using DocumentFormat.OpenXml.Office2016.Drawing.Command;

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
            name = note.name;
            startTime = note.startTime;
            endTime = note.endTime;
            teacher = note.teacher;
            subname = note.subname;
            auditorium = note.auditorium;
            participators = note.participators;
        }
        public Note(string txtString, string day, DataBase db)
        {
            participators = new();
            string[] parametrs = txtString.Split("|");
            name = parametrs[0];
            startTime = DataBase.Date(day + ' ' + parametrs[1]);
            endTime = DataBase.Date(day + ' ' + parametrs[2]);

            if (db.GetUser(parametrs[3], false) != null)
            {
                teacher = db.GetUser(parametrs[3], false);
            }

            subname = parametrs[4];
            auditorium = db.GetAuditorium(parametrs[5]);
            if (auditorium == null)
            {
                db.LogState($"Такой аудитории не существует: \"{parametrs[5]}\"");
            }
            /*else capacity = auditorium.capacity;*/
        }
        public Note()
        {
            teacher = new();
            auditorium = new();
            participators = new();
        }

        /////////////////////////////////////////////////////////////////////////////
        
        // найти запись по дате(деньб месяцб год), после аудиторию, после время (часы)
        // создать метод поиска записи по дате и его создания
        public string InputStringDate(DataBase db) //для ввода пользователем полной даты
        {
            db.LogState("Вводить время циферками: yyyy.mm.dd hh:mm");
            //сделать проверку ввода адекватной даты

            //сделать проверку даты
            db.LogState("день");
            string Day = Console.ReadLine();
            db.LogState("месяц");
            string Mounth = Console.ReadLine();
            db.LogState("год");
            string Year = Console.ReadLine();

            //проверку времени
            db.LogState("час");
            string Hour = Console.ReadLine();
            db.LogState("минуты");
            string Minuts = Console.ReadLine();

            string Date = Day + "." + Mounth + "." + Year;
            string Time = Hour + ":" + Minuts;
            string fullDate = Date + " " + Time;

            db.LogState("введенное время: " + fullDate);
            return fullDate;
        }
        public string InputStringDay(DataBase db)
        {
            db.LogState("Вводить время циферками: yyyy.mm.dd");

            //сделать проверку даты
            db.LogState("день");
            string Day = Console.ReadLine();
            db.LogState("месяц");
            string Mounth = Console.ReadLine();
            db.LogState("год");
            string Year = Console.ReadLine();

            string Date = Day + "." + Mounth + "." + Year;
            string Time ="00:00";
            string fullDate = Date + " " + Time;

            db.LogState("введенное время: " + Date);
            return fullDate;
        }//день, месяц, год
        public string InputStringDate(DataBase db, string date) //для ввода пользователем дат
        {
            db.LogState("Вводить время циферками: hh:mm");

            //проверку времени
            db.LogState("час");
            string Hour = Console.ReadLine();
            db.LogState("минуты");
            string Minuts = Console.ReadLine();

            string Date = date;
            string Time = Hour + ":" + Minuts;
            string fullDate = Date + " " + Time;

            db.LogState("введенное время: " + Time);
            return fullDate;
        }
        public Auditorium InputAuditorium(DataBase db) //ввод и поиск аудитории
        {
            string roomName = null;
            Auditorium chosenRoom = null;

            //сначала выбрать время, потом аудиторию


            while (roomName == null)
            {

                db.LogState("введите название аудитории");
                roomName = Console.ReadLine();
                Auditorium room = db.GetAuditorium(roomName);

                if (room != null)
                {
                    db.LogState("Выбранная аудитория:");
                    db.LogState("   " + room.tag + "         вместимость: " + room.capacity + " чебупелей            рабочее время: " + room.startTime + "-" + room.endTime);
                    db.LogState("");
                }
                else
                {
                    db.LogState("не удалось найти аудиторию, попробуйте ещё раз");
                    db.LogState("");
                }
                chosenRoom = room;
            }
            return chosenRoom;
        }
       /* public bool IsInRange(this DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck >= startDate && dateToCheck < endDate;
        }*/
        public bool ISRangeInRange(DateTime start, DateTime end, Auditorium room) //не работает
        {
            bool test = true;
            int testInt = 0;
            for (int i =0; i < room.timetable.Count; i++)
            {
                if ( start >= room.timetable[i].startTime && start < room.timetable[i].endTime ||
                     end > room.timetable[i].startTime && end <= room.timetable[i].endTime ||
                     start <= room.timetable[i].startTime && end >= room.timetable[i].endTime)
                {
                    test = false;
                    testInt++;
                    return test;
                }
                else
                {
                    test = true;
                    return test;
                }
            }
            if (test == false) return false;
            else if (testInt > 0) return false;
            else return true;
        } //проверка диапозона времени на все диапозоны из аудиторий
        public void InputTime(DataBase db, Auditorium room)//ввод и проверка времени в аудитории(резервирование)
        {
            db.LogState("");
            db.LogState("Введите время для резервирования аудитории: yyyy.mm.dd " + room.tag);
            db.LogState("Рабочии часы: " + room.startTime + " - " + room.endTime);
            //показ недоступного времени (другие учителя заняли это время) ////// норм переписать
            if (room.timetable == null) { } //если никого нет
            else
            {
                db.LogState("Уже забранированное время:");
                for (int i = 0; i < room.timetable.Count; i++)
                {
                    db.LogState($"{room.timetable[i].startTime.ToString("t")}" + " - " +  $"{room.timetable[i].endTime.ToString("t")}");
                }
            } //если кто-то есть

            db.LogState("");
            db.LogState("Введите время начала:");
            string strartTimeString = InputStringDate(db);
            DateTime strartTimeDT = DataBase.Date(strartTimeString);

            db.LogState("");
            db.LogState("Введите время конца:");
            string endTimeString = InputStringDate(db);
            DateTime endTimeDT = DataBase.Date(endTimeString);

            bool checkTime = ISRangeInRange(strartTimeDT, endTimeDT, room);
            if (checkTime = false) db.LogState("Вы опаздали, место заняли в это время");
            else db.LogState("Браво, вы умеете читать и использовать информацию выше для благих дел");

        }

        public Note(string name, DateTime startTime, DateTime endTime, User teacher, string subname, Auditorium auditorium)
        {
            participators = new();
            this.name = name;
            //db.LogState("start Time");
            this.startTime = startTime;
            //db.LogState("end Time");
            this.endTime = endTime;
            this.teacher = teacher;
            this.subname = subname;
            this.auditorium = auditorium;
        }
    }
}

