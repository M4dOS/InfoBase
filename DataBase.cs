using OfficeOpenXml;


namespace InfoBase
{
    internal class DataBase
    {
        public List<string> subjects;
        public List<string> teachers;
        public List<Auditorium> auditoriums;
        /*public List<Note> fullTimetable;*/
        public List<User> users;

        public string logfile_path;
        public string data_path;
        public string users_path;
        public string days_path;
        private readonly bool consoleLogging;
        private int log_counter;

        //некоторые вспомогательные инструменты
        public static DateTime Date(string date)//для дат формата "dd.mm.YYYY hh:mm" 
        {
            string[] datenums = date.Split(' ')[0].Split('.');
            string[] timenums = date.Split(' ')[1].Split(':');
            return new DateTime(int.Parse(datenums[2]), int.Parse(datenums[1]), int.Parse(datenums[0]),
                                int.Parse(timenums[0]), int.Parse(timenums[1]), 0);
        }
        public void LogState(string message)//логирование всего 
        {
            log_counter++;
            string dirWithLogName = logfile_path + DateTime.Now.ToString("dd-MM-yyyy") + ".log";
            if (!File.Exists(dirWithLogName))
            {
                using (File.Create(dirWithLogName)) { }
            };

            using StreamWriter writer = new(dirWithLogName, true);
            int n = 20;
            string line = string.Empty;
            for (int i = 0; i < n; i++) { line += '-'; }
            if (log_counter == 1)
            {
                writer.Write('\n' + line + " НОВЫЙ ЗАПУСК " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                if (consoleLogging)
                {
                    Console.WriteLine('\n' + line + " ЛОГ ЗАПУСКА " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                }
            }
            writer.Write(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message + '\n');
            if (consoleLogging)
            {
                Console.Write(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message + '\n');
            }
        }
        public DataBase(string logfile_path, bool consoleLogging)//конструктор 
        {
            log_counter = 0;
            subjects = new();
            teachers = new();
            auditoriums = new();
            users = new();
            this.logfile_path = logfile_path;
            this.consoleLogging = consoleLogging;
        }


        //рабочие инструменты базы данных
        public bool FillUsers(string pathToFileUsers)//первоначальное заполнение всех пользователей 
        {
            //открываем файл с данными 
            string fullPath = pathToFile;
            users_path = fullPath;
            ExcelPackage excel = new(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри вводимые тобой данные пользователей");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            int index = 1;
            while (index < users.Dimension.End.Row)
            {
                string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                if (user_password == null || user_login == null || user_access == null || user_name == null)
                {
                    if (user_password == null && user_login == null && user_access == null && user_name == null)
                    {
                        break;
                    }
                    LogState($"Строка данных аудиторий {index} выглядит неполной или является пустой");
                }
                else
                {
                    this.users.Add(new(user_login, user_password, user_access, user_name, this));
                    index++;
                }
            }
            return true;

        }
        public bool FillData(string pathToFileData)//заполнение списка предметов и учителей 
        {
            //открываем файл с данными 
            string fullPath = pathToFile;
            data_path = fullPath;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? subjects = excel.Workbook.Worksheets["Предметы"];
            ExcelWorksheet? teachers = excel.Workbook.Worksheets["Учителя"];
            ExcelWorksheet? auditoriums = excel.Workbook.Worksheets["Кабинеты"];

            if (subjects == null || teachers == null || auditoriums == null)
            {
                LogState("Пересмотри вводимые тобой данные кабинетов, учителей и предметов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            int index = 1;
            while (index < subjects.Dimension.End.Row)
            {
                string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                if (subj == null)
                {
                    break;
                }
                else
                {
                    this.subjects.Add(subj);
                    index++;
                }
            }

            index = 1;
            List<string> temp_teachs = new();
            while (index < teachers.Dimension.End.Row)
            {
                if (index == 1)
                {
                    foreach (User? user in users.Where(usr => usr.access == Access.Teacher).ToList())
                    {
                        this.teachers.Add(user.name);
                        temp_teachs.Add(user.name);
                    }
                }

                string? teach = teachers.Cells[$"A{index}"].Value?.ToString();
                if (teach == null)
                {
                    break;
                }
                else
                {
                    bool cond = true;
                    foreach (User? user in users.Where(usr => usr.access == Access.Teacher).ToList())
                    {
                        if (teach == user.name) { cond = false; break; }
                    }
                    if (cond)
                    {
                        this.teachers.Add(teach);
                    }

                    cond = true;
                    index++;
                }

                if (this.teachers.Except(temp_teachs).ToList().Count != 0)
                {
                    LogState("Списки учителей не совпадают со списком пользователей с доступом Teacher");
                    return false;
                }
            }

            index = 1;
            while (index < auditoriums.Dimension.End.Row)
            {
                string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                string? startTime = auditoriums.Cells[$"B{index}"].Value?.ToString().Split(' ')[1];
                string? endTime = auditoriums.Cells[$"C{index}"].Value?.ToString().Split(' ')[1];
                string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();
                if (codeName == null || startTime == null || endTime == null || capacity == null)
                {
                    if (codeName == null && startTime == null && endTime == null && capacity == null)
                    {
                        break;
                    }
                    LogState($"Строка данных аудиторий {index} выглядит неполной или является пустой");
                }
                else
                {
                    string? start = startTime.Split(":")[0] + ':' + startTime.Split(":")[1];
                    string? end = endTime.Split(":")[0] + ':' + endTime.Split(":")[1];
                    this.auditoriums.Add(new(codeName, start, end, int.Parse(capacity)));
                    index++;
                }
            }
            return true;
        }
        public bool FillDays(string pathToDirDays)//первоначальное заполнение всех броней 
        {
            days_path = pathToDirDays;
            string[] files = Directory.GetFiles(pathToDirDays, "*.txt");
            Note? temp_note = new();
            Auditorium? temp_auitorium = new();

            bool result = true;

            foreach (string fileName in files)
            {
                string date = System.IO.Path.GetFileName(fileName).Split(".txt")[0];
                using StreamReader reader = new(fileName);
                if (reader.EndOfStream)
                {
                    LogState($"Файл \".../data/days/{date}\" пуст");
                }
                else
                {
                    string line;
                    bool cond = false;
                    bool falseNote = true;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parametrs = line.Split("|");
                        if (parametrs.Length == 6)
                        {
                            foreach (Auditorium aud in auditoriums)
                            {
                                if (line.Split("|")[5] == aud.tag)
                                {
                                    temp_note = new(line, date, this);
                                    temp_auitorium = aud;
                                    if (aud.AddNote(temp_note, this))
                                    {
                                        cond = true;
                                        falseNote = false;
                                    }
                                    else
                                    {
                                        falseNote = true;
                                    }
                                    break;
                                }
                            }
                        }

                        else if (parametrs.Length == 2 && !falseNote)
                        {
                            User? temp_user = GetFullUser(parametrs[0], parametrs[1]);
                            if (temp_auitorium == null || temp_note == null)
                            {
                                LogState($"Прочтение строки {line} безуспешно завершено. Проверьте информацию в файле {date + ".txt"}");
                                result = false;
                            }
                            else if (temp_user == null)
                            {
                                LogState($"Взятие пользователя по строке {line} безуспешно завершено. Проверьте информацию в файле {date + ".txt"} и Data.xlsx");
                                result = false;
                            }
                            else
                            {
                                auditoriums.Find(x => x == temp_auitorium).timetable.Find(x => x == temp_note).participators.Add(temp_user);
                                users.Find(x => x == temp_user).participating.Add(temp_note);
                                temp_user.participating.Add(temp_note);
                                /*temp_auitorium.timetable.Find(x => x == temp_note).participators.Add(temp_user);*/
                            }
                        }
                    }
                    if (!cond)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
        public User? GetUser(string name, bool mode)//найти класс User по определённому параметру 
        {
            bool cond = false;
            switch (mode)
            {
                case false: //поиск по имени
                    foreach (User user in users)
                    {
                        if (user.name == name) { cond = true; return user; }
                    }
                    break;

                case true: //поиск по логину
                    foreach (User user in users)
                    {
                        if (user.login == name) { cond = true; return user; }
                    }
                    LogState($"Пользователя с логином {name} нету в базе данных");
                    break;
            }

            switch (mode)
            {
                case false:
                    if (!cond)
                    {
                        LogState($"Пользователя с именем {name} нету в базе данных");
                    }

                    break;

                case true:
                    if (!cond)
                    {
                        LogState($"Пользователя с логином {name} нету в базе данных");
                    }

                    break;
            }
            return null;
        }
        public User? GetFullUser(string login, string name)//найти класс User по логину и имени 
        {
            bool cond = false;
            foreach (User user in users)
            {
                if (user.name == name && user.login == login) { cond = true; return user; }
            }
            if (!cond)
            {
                LogState($"Пользователя с именем {name} и логином {login} нету в базе данных");
            }

            return null;
        }
        public Note? GetNote(DateTime time)//взять запись, которая находится в рамках [начальное время;конечное время) 
        {
            foreach (Auditorium auditory in auditoriums)
            {
                foreach (Note note in auditory.timetable)
                {
                    if (note.startTime <= time && time < note.endTime)
                    {
                        return note;
                    }
                }
            }
            LogState($"Нету никаких записей в данное время: {time:dd-MM-yyyy HH:mm}");
            return null;
        }
        public Auditorium? GetAuditorium(string tag)//найти аудиторию по имени 
        {
            foreach (Auditorium auditor in auditoriums)
            {
                if (auditor.tag == tag)
                {
                    return auditor;
                }
            }
            LogState($"Аудитория с номером {tag} не найдена");
            return null;
        }
        public bool SetNote(Note old_note, Note new_note)
        {
            Auditorium aud = GetAuditorium(old_note.auditorium.tag);
            if (aud.timetable.Remove(old_note))
            {
                _ = aud.AddNote(new_note, this);
                string filePath;

                try
                {
                    filePath = days_path + old_note.startTime.ToString("dd.MM.yyyy") + ".txt"; // путь к файлу
                }
                catch (Exception ex) { LogState($"Ошибка: {ex}"); return false; }

                /*Название предмета 1 | 9:00 | 10:00 | Преподаватель 1 | Доп описание для Название предмета 1 1 | a1*/
                string searchLine = old_note.name + '|' + old_note.startTime.ToString("H:mm") + '|' + old_note.endTime.ToString("H:mm") + '|'
                                    + old_note.teacher.name + '|' + old_note.subname + '|' + old_note.auditorium.tag;// строка, которую нужно заменить
                string newLine = new_note.name + '|' + new_note.startTime.ToString("H:mm") + '|' + new_note.endTime.ToString("H:mm") + '|'
                                 + new_note.teacher.name + '|' + new_note.subname + '|' + new_note.auditorium.tag; // новая строка, которой заменится найденная строка

                // Открываем файл для чтения и записи
                try
                {
                    using StreamReader reader = new(filePath);
                    // Создаем временный файл для записи
                    string tempFilePath = System.IO.Path.GetTempFileName();

                    // Открываем временный файл для записи
                    using (StreamWriter writer = new(tempFilePath))
                    {
                        string line;
                        string? temp_line = string.Empty;
                        bool lineFound = false;
                        bool sucess = false;

                        // Читаем файл построчно
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (lineFound)
                            {
                                if (temp_line != string.Empty)
                                {
                                    writer.WriteLine(temp_line);
                                }

                                lineFound = false;
                                sucess = true;
                            }

                            if (line.Contains(searchLine))
                            {
                                writer.WriteLine(newLine);
                                lineFound = true;
                                foreach (User user in new_note.participators)
                                {
                                    writer.WriteLine($"{user.login}|{user.name}");
                                }
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.Split('|').Length > 2)
                                    {
                                        temp_line = line;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }

                        // Если строка не была найдена
                        if (!sucess)
                        {
                            LogState($"Строка для замены \"{searchLine}\" не найдена");
                            return false;
                        }
                    }

                    // Закрываем файлы
                    reader.Close();

                    // Заменяем исходный файл временным файлом
                    File.Delete(filePath);
                    File.Move(tempFilePath, filePath);
                }
                catch (IOException ex)
                {
                    LogState("Возникла следующая ошибка: " + ex.Message);
                    return false;
                }

                return true;
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами записи не существует)");
                return false;
            }
        }
        public bool SetUser(User old_user, User new_user)
        {
            string fullPath = users_path;
            ExcelPackage excel = new(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри данные пользователей");
                return false;
            }

            int indexToIns = -1;
            if (this.users.FindIndex(x => x == old_user) != -1)
            {
                indexToIns = this.users.FindIndex(x => x == old_user);
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами записи не существует)");
                return false;
            }

            if (this.users.Remove(old_user))
            {
                this.users.Insert(indexToIns, new_user);
                int index = 1;
                while (index < users.Dimension.End.Row)
                {
                    string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                    string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                    string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                    string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                    if (user_password == null || user_login == null || user_access == null || user_name == null)
                    {
                        if (user_password == null && user_login == null && user_access == null && user_name == null)
                        {
                            break;
                        }
                        LogState($"Строка данных аудиторий {index} выглядит неполной или является пустой");
                    }
                    else if (user_password == old_user.password || user_login == old_user.login
                            || user_access == old_user.access.ToString().ToLower() || user_name == old_user.name)
                    {
                        users.Cells.SetCellValue(index - 1, 0, new_user.login);
                        users.Cells.SetCellValue(index - 1, 1, new_user.password);
                        users.Cells.SetCellValue(index - 1, 2, new_user.access.ToString().ToLower());
                        users.Cells.SetCellValue(index - 1, 3, new_user.name);
                        break;
                    }
                    index++;
                }
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
                return true;
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами юзера не существует)");
                return false;
            }
        }
        public bool SetAuditorium(Auditorium old_aud, Auditorium new_aud)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? auditoriums = excel.Workbook.Worksheets["Кабинеты"];

            if (auditoriums == null)
            {
                LogState("Пересмотри вводимые тобой данные кабинетов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            int indexToIns = -1;
            if (this.auditoriums.FindIndex(x => x == old_aud) != -1)
            {
                indexToIns = this.auditoriums.FindIndex(x => x == old_aud);
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами записи не существует)");
                return false;
            }

            if (this.auditoriums.Remove(old_aud))
            {
                this.auditoriums.Insert(indexToIns, new_aud);
                int index = 1;
                while (index < auditoriums.Dimension.End.Row)
                {
                    string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                    string? startTime = auditoriums.Cells[$"B{index}"].Value?.ToString().Split(' ')[1];
                    string? endTime = auditoriums.Cells[$"C{index}"].Value?.ToString().Split(' ')[1];
                    string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();
                    if (codeName == null || startTime == null || endTime == null || capacity == null)
                    {
                        if (codeName == null && startTime == null && endTime == null && capacity == null)
                        {
                            break;
                        }
                        LogState($"Строка данных пользователя {index} выглядит неполной");
                    }
                    else if (old_aud.tag == codeName && old_aud.startTime + ":00" == startTime && old_aud.endTime + ":00" == endTime
                            && old_aud.capacity == int.Parse(capacity))
                    {
                        auditoriums.Cells.SetCellValue(index - 1, 0, new_aud.tag);
                        auditoriums.Cells["B1"].Value = Date("01.01.2000 " + new_aud.startTime);
                        auditoriums.Cells["B1"].Style.Numberformat.Format = "H:mm";
                        auditoriums.Cells["C1"].Value = Date("01.01.2000 " + new_aud.endTime);
                        auditoriums.Cells["C1"].Style.Numberformat.Format = "H:mm";
                        auditoriums.Cells.SetCellValue(index - 1, 3, new_aud.capacity);
                        break;
                    }
                    index++;
                }
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами аудитории не существует)");
                return false;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool SetTeacher(string old_teacher, string new_teacher)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? teachers = excel.Workbook.Worksheets["Учителя"];

            if (teachers == null)
            {
                LogState("Пересмотри вводимые тобой данные учителей");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }


            int indexToIns = -1;

            if (this.teachers.FindIndex(x => x == old_teacher) != -1)
            {
                indexToIns = this.teachers.FindIndex(x => x == old_teacher);
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами записи не существует)");
                return false;
            }

            if (this.teachers.Remove(old_teacher))
            {
                this.teachers.Insert(indexToIns, new_teacher);

                foreach (User? user in users.Where(usr => usr.access == Access.Teacher).ToList())
                {
                    if (user.name == old_teacher)
                    {
                        User new_u = new(user)
                        {
                            name = new_teacher
                        };
                        _ = SetUser(user, new_u);
                        break;
                    }
                }

                int index = 1;
                while (index < teachers.Dimension.End.Row)
                {
                    string? teacher0 = teachers.Cells[$"A{index}"].Value?.ToString();
                    if (teacher0 == null)
                    {
                        break;
                    }

                    if (teacher0 == old_teacher)
                    {
                        teachers.Cells[$"A{index}"].Value = new_teacher;
                    }
                    index++;
                }
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами учителя не существует)");
                return false;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool SetSubject(string old_subject, string new_subject)
        {

            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? subjects = excel.Workbook.Worksheets["Предметы"];

            if (subjects == null)
            {
                LogState("Пересмотри вводимые тобой данные предметов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            int indexToIns = -1;

            if (this.subjects.FindIndex(x => x == old_subject) != -1)
            {
                indexToIns = this.subjects.FindIndex(x => x == old_subject);
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами предмета не существует)");
                return false;
            }


            if (teachers.Remove(old_subject))
            {
                teachers.Insert(indexToIns, new_subject);

                foreach (Auditorium aud in auditoriums)
                {
                    foreach (Note note in aud.timetable)
                    {
                        if (note.name == old_subject)
                        {
                            Note new_n = new(note)
                            {
                                name = new_subject
                            };
                            _ = SetNote(note, new_n);
                        }
                    }
                }

                int index = 1;
                while (index < subjects.Dimension.End.Row)
                {
                    string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                    if (subj == null)
                    {
                        break;
                    }
                    if (subj == old_subject)
                    {
                        subjects.Cells[$"A{index}"].Value = new_subject;
                    }
                    index++;
                }
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами учителя не существует)");
                return false;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool DeleteNote(Note delete_note)
        {
            Auditorium aud = GetAuditorium(delete_note.auditorium.tag);
            if (aud.timetable.Remove(delete_note))
            {
                string filePath;

                try
                {
                    filePath = days_path + delete_note.startTime.ToString("dd.MM.yyyy") + ".txt"; // путь к файлу
                }
                catch (Exception ex) { LogState($"Ошибка: {ex}"); return false; }

                /*Название предмета 1 | 9:00 | 10:00 | Преподаватель 1 | Доп описание для Название предмета 1 1 | a1*/
                string searchLine = delete_note.name + '|' + delete_note.startTime.ToString("H:mm") + '|' + delete_note.endTime.ToString("H:mm") + '|'
                                    + delete_note.teacher.name + '|' + delete_note.subname + '|' + delete_note.auditorium.tag;// строка, которую нужно заменить

                // Открываем файл для чтения и записи
                try
                {
                    using StreamReader reader = new(filePath);
                    // Создаем временный файл для записи
                    string tempFilePath = System.IO.Path.GetTempFileName();

                    // Открываем временный файл для записи
                    using (StreamWriter writer = new(tempFilePath))
                    {
                        string line;
                        string? temp_line = string.Empty;
                        bool lineFound = false;
                        bool sucess = false;

                        // Читаем файл построчно
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (lineFound)
                            {
                                if (temp_line != string.Empty)
                                {
                                    writer.WriteLine(temp_line);
                                }

                                lineFound = false;
                                sucess = true;
                            }

                            if (line.Contains(searchLine))
                            {
                                lineFound = true;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.Split('|').Length > 2)
                                    {
                                        temp_line = line;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }

                        // Если строка не была найдена
                        if (!sucess)
                        {
                            LogState($"Строка для замены \"{searchLine}\" не найдена");
                            return false;
                        }
                    }

                    // Закрываем файлы
                    reader.Close();

                    // Заменяем исходный файл временным файлом
                    File.Delete(filePath);
                    File.Move(tempFilePath, filePath);
                }
                catch (IOException ex)
                {
                    LogState("Возникла следующая ошибка: " + ex.Message);
                    return false;
                }

                return true;
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами записи не существует)");
                return false;
            }
        }
        public bool DeleteUser(User delete_user)
        {
            string fullPath = users_path;
            ExcelPackage excel = new(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри данные пользователей");
                return false;
            }

            if (this.users.Remove(delete_user))
            {
                int index = 1;
                while (index < users.Dimension.End.Row)
                {
                    string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                    string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                    string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                    string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                    if (user_password == null || user_login == null || user_access == null || user_name == null)
                    {
                        if (user_password == null && user_login == null && user_access == null && user_name == null)
                        {
                            break;
                        }
                        LogState($"Строка данных аудиторий {index} выглядит неполной или является пустой");
                    }
                    else if (user_password == delete_user.password || user_login == delete_user.login
                            || user_access == delete_user.access.ToString().ToLower() || user_name == delete_user.name)
                    {
                        ExcelRange rangeToDelete = users.Cells[index, 1, index, users.Dimension.End.Column];
                        rangeToDelete.Delete(eShiftTypeDelete.Up);
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
                return true;
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами юзера не существует)");
                return false;
            }
        }
        public bool DeleteAuditorium(Auditorium delete_aud)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? auditoriums = excel.Workbook.Worksheets["Кабинеты"];

            if (auditoriums == null)
            {
                LogState("Пересмотри вводимые тобой данные кабинетов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            if (this.auditoriums.Remove(delete_aud))
            {
                int index = 1;
                while (index < auditoriums.Dimension.End.Row)
                {
                    string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                    string? startTime = auditoriums.Cells[$"B{index}"].Value?.ToString().Split(' ')[1];
                    string? endTime = auditoriums.Cells[$"C{index}"].Value?.ToString().Split(' ')[1];
                    string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();
                    if (codeName == null || startTime == null || endTime == null || capacity == null)
                    {
                        if (codeName == null && startTime == null && endTime == null && capacity == null)
                        {
                            break;
                        }
                        LogState($"Строка данных пользователя {index} выглядит неполной");
                    }
                    else if (delete_aud.tag == codeName && delete_aud.startTime + ":00" == startTime && delete_aud.endTime + ":00" == endTime
                            && delete_aud.capacity == int.Parse(capacity))
                    {
                        ExcelRange rangeToDelete = auditoriums.Cells[index, 1, index, auditoriums.Dimension.End.Column];
                        rangeToDelete.Delete(eShiftTypeDelete.Up);
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами аудитории не существует)");
                return false;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool DeleteTeacher(string delete_teacher)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? teachers = excel.Workbook.Worksheets["Учителя"];

            if (teachers == null)
            {
                LogState("Пересмотри вводимые тобой данные учителей");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            if (this.teachers.Remove(delete_teacher))
            {

                foreach (User? user in users.Where(usr => usr.access == Access.Teacher).ToList())
                {
                    if (user.name == delete_teacher)
                    {
                        _ = users.Remove(user);
                        break;
                    }
                }

                int index = 1;
                while (index < teachers.Dimension.End.Row)
                {
                    string? teacher0 = teachers.Cells[$"A{index}"].Value?.ToString();
                    if (teacher0 == null)
                    {
                        break;
                    }

                    if (teacher0 == delete_teacher)
                    {
                        ExcelRange rangeToDelete = teachers.Cells[index, 1, index, teachers.Dimension.End.Column];
                        rangeToDelete.Delete(eShiftTypeDelete.Up);
                    }
                    index++;
                }
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами учителя не существует)");
                return false;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool DeleteSubject(string delete_subject)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? subjects = excel.Workbook.Worksheets["Предметы"];

            if (subjects == null)
            {
                LogState("Пересмотри вводимые тобой данные предметов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }


            if (teachers.Remove(delete_subject))
            {

                foreach (Auditorium aud in auditoriums)
                {
                    foreach (Note note in aud.timetable)
                    {
                        if (note.name == delete_subject)
                        {
                            _ = aud.timetable.Remove(note);
                        }
                    }
                }

                int index = 1;
                while (index < subjects.Dimension.End.Row)
                {
                    string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                    if (subj == null)
                    {
                        break;
                    }
                    if (subj == delete_subject)
                    {
                        ExcelRange rangeToDelete = subjects.Cells[index, 1, index, subjects.Dimension.End.Column];
                        rangeToDelete.Delete(eShiftTypeDelete.Up);
                    }
                    index++;
                }
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами учителя не существует)");
                return false;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool AddNote(Note new_note)
        {
            Auditorium aud = GetAuditorium(new_note.auditorium.tag);
            if (aud.AddNote(new_note, this))
            {
                using StreamWriter writer = new(CreateDayList(new_note.startTime.ToString("dd.MM.yyyy")), true);
                string newLine = new_note.name + '|' + new_note.startTime.ToString("H:mm") + '|' + new_note.endTime.ToString("H:mm") + '|'
                        + new_note.teacher.name + '|' + new_note.subname + '|' + new_note.auditorium.tag; // новая строка, которой заменится найденная строка
                writer.WriteLine(newLine);
                return true;
            }
            else
            {
                LogState($"Добавление записи \"{new_note.name}\" безуспешно завершено");
                return false;
            }
        }
        public bool AddUser(User new_user)
        {
            string fullPath = users_path;
            ExcelPackage excel = new(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри данные пользователей");
                return false;
            }

            int index = 1;
            while (index < users.Dimension.End.Row)
            {
                string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                if (user_password == null || user_login == null || user_access == null || user_name == null)
                {
                    if (user_password == null && user_login == null && user_access == null && user_name == null)
                    {
                        users.Cells.SetCellValue(index - 1, 0, new_user.login);
                        users.Cells.SetCellValue(index - 1, 1, new_user.password);
                        users.Cells.SetCellValue(index - 1, 2, new_user.access.ToString().ToLower());
                        users.Cells.SetCellValue(index - 1, 3, new_user.name);
                        break;
                    }
                    LogState($"Строка данных аудиторий {index} выглядит неполной или является пустой");
                }
                index++;
            }
            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;

        }
        public bool AddAuditorium(Auditorium new_aud)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? auditoriums = excel.Workbook.Worksheets["Кабинеты"];

            if (auditoriums == null)
            {
                LogState("Пересмотри вводимые тобой данные кабинетов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            int index = 1;
            while (index < auditoriums.Dimension.End.Row)
            {
                string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                string? startTime = auditoriums.Cells[$"B{index}"].Value?.ToString().Split(' ')[1];
                string? endTime = auditoriums.Cells[$"C{index}"].Value?.ToString().Split(' ')[1];
                string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();
                if (codeName == null || startTime == null || endTime == null || capacity == null)
                {
                    if (codeName == null && startTime == null && endTime == null && capacity == null)
                    {
                        auditoriums.Cells.SetCellValue(index - 1, 0, new_aud.tag);
                        auditoriums.Cells["B1"].Value = Date("01.01.2000 " + new_aud.startTime);
                        auditoriums.Cells["B1"].Style.Numberformat.Format = "H:mm";
                        auditoriums.Cells["C1"].Value = Date("01.01.2000 " + new_aud.endTime);
                        auditoriums.Cells["C1"].Style.Numberformat.Format = "H:mm";
                        auditoriums.Cells.SetCellValue(index - 1, 3, new_aud.capacity);
                        break;
                    }
                    LogState($"Строка данных пользователя {index} выглядит неполной");
                }
                index++;
            }


            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool AddTeacher(string new_teacher)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? teachers = excel.Workbook.Worksheets["Учителя"];

            if (teachers == null)
            {
                LogState("Пересмотри вводимые тобой данные учителей");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            int index = 1;
            while (index < teachers.Dimension.End.Row)
            {
                string? teacher0 = teachers.Cells[$"A{index}"].Value?.ToString();
                if (teacher0 == null)
                {
                    teachers.Cells[$"A{index}"].Value = new_teacher;
                    break;
                }
                index++;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool AddSubject(string new_subject)
        {

            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? subjects = excel.Workbook.Worksheets["Предметы"];

            if (subjects == null)
            {
                LogState("Пересмотри вводимые тобой данные предметов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    _ = Console.ReadKey();
                }
                return false;
            }

            int index = 1;
            while (index < subjects.Dimension.End.Row)
            {
                string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                if (subj == null)
                {
                    subjects.Cells[$"A{index}"].Value = new_subject;
                    break;
                }
                index++;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }


        //базовые функции, не требующиеся в дальнейшем использовании
        public bool CreateDataList(string fileName)//создание макета списка данных 
        {
            //создаем новый документ 
            ExcelPackage excel = new();

            //добавляем лист 
            ExcelWorksheet worksheet1 = excel.Workbook.Worksheets.Add("Учителя");
            ExcelWorksheet worksheet2 = excel.Workbook.Worksheets.Add("Предметы");
            ExcelWorksheet worksheet3 = excel.Workbook.Worksheets.Add("Кабинеты");

            //добавляем данные 
            worksheet1.Cells["A1"].Value = "Учителя";
            worksheet1.Column(1).Width = 100;

            worksheet2.Cells["A1"].Value = "Предметы";
            worksheet2.Column(1).Width = 100;

            worksheet3.Cells["A1"].Value = "Кодовый номер";
            worksheet3.Column(1).Width = 15.5;
            worksheet3.Cells["B1"].Value = "Начало бронирования";
            worksheet3.Column(2).Width = 22;
            worksheet3.Cells["C1"].Value = "Конец бронирования";
            worksheet3.Column(3).Width = 22;
            worksheet3.Cells["D1"].Value = "Вместимость";
            worksheet3.Column(4).Width = 13;

            //задаём путь 
            string fullPath = fileName;

            //сохраняем документ 
            FileInfo excelFile = new(fullPath);
            if (!File.Exists(fullPath)) { excel.SaveAs(excelFile); return false; }
            else
            {
                return false;
            }
        }
        public bool CreateUserList(string fileName)//создание макета списка юзеров 
        {
            //создаем новый документ 
            ExcelPackage excel = new();

            //добавляем лист 
            ExcelWorksheet worksheet1 = excel.Workbook.Worksheets.Add("Данные");

            //добавляем данные 
            worksheet1.Cells["A1"].Value = "Логин";
            worksheet1.Column(1).Width = 35;

            worksheet1.Cells["B1"].Value = "Пароль";
            worksheet1.Column(2).Width = 35;

            worksheet1.Cells["C1"].Value = "Уровень доступа";
            worksheet1.Column(3).Width = 75;

            //задаём путь 
            string fullPath = fileName;

            //сохраняем документ 
            FileInfo excelFile = new(fullPath);
            if (!File.Exists(fullPath)) { excel.SaveAs(excelFile); return false; }
            else
            {
                return true;
            }
        }
        public string? CreateDayList(string data)//создание макета списка дня 
        {
            //создаем новый документ 
            string fullPath = days_path + data + ".txt";

            if (!File.Exists(fullPath)) { _ = File.Create(fullPath); return null; }
            else
            {
                return fullPath;
            }
        }
    }
}