using DocumentFormat.OpenXml.Drawing;
using OfficeOpenXml;
using System.Diagnostics;
using System.IO;

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

        bool consoleLogging;
        int log_counter;

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
            if (!File.Exists(dirWithLogName)) using (File.Create(dirWithLogName)) { };

            using (StreamWriter writer = new StreamWriter(dirWithLogName, true))
            {
                int n = 20;
                string line = string.Empty;
                for (int i = 0; i < n; i++) { line += '-'; }
                if (log_counter == 1)
                {
                    writer.Write('\n' + line + " НОВЫЙ ЗАПУСК " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                    if (consoleLogging) Console.WriteLine('\n' + line + " ЛОГ ЗАПУСКА " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                }
                writer.Write(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message + '\n');
                if (consoleLogging) Console.Write(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message + '\n');
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
        public bool FillUsers(string excelFileName)//первоначальное заполнение всех пользователей 
        {
            //открываем файл с данными 
            string fullPath = excelFileName;
            users_path = fullPath;
            ExcelPackage excel = new ExcelPackage(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри вводимые тобой данные пользователей");
                if (consoleLogging) 
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    Console.ReadKey(); 
                }
                return false;
            }

            int index = 1;
            while (true)
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
        public bool FillData(string excelFileName)//заполнение списка предметов и учителей 
        {
            //открываем файл с данными 
            string fullPath = excelFileName;
            data_path = fullPath;
            ExcelPackage excel = new ExcelPackage(new FileInfo(fullPath));

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
                    Console.ReadKey();
                }
                return false;
            }

            int index = 1;
            while (true)
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
            while (true)
            {
                if (index == 1) foreach (var user in users.Where(usr => usr.access == Access.Teacher).ToList())
                    {
                        this.teachers.Add(user.name);
                        temp_teachs.Add(user.name);
                    }

                string? teach = teachers.Cells[$"A{index}"].Value?.ToString();
                if (teach == null)
                {
                    break;
                }
                else
                {
                    bool cond = true;
                    foreach (var user in users.Where(usr => usr.access == Access.Teacher).ToList())
                    {
                        if (teach == user.name) { cond = false; break; }
                    }
                    if (cond) this.teachers.Add(teach);
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
            while (true)
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
        public bool FillDays(string workDir)//первоначальное заполнение всех броней 
        {
            days_path = workDir;
            string[] files = Directory.GetFiles(workDir, "*.txt");
            Note? temp_note = new();
            Auditorium? temp_auitorium = new();

            bool result = true;

            foreach (var fileName in files)
            {
                string date = System.IO.Path.GetFileName(fileName).Split(".txt")[0];
                using (StreamReader reader = new StreamReader(fileName))
                {
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
                            if(parametrs.Length == 6)
                            {
                                foreach (var aud in auditoriums)
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
                        if (!cond) result = false;
                    }
                }
            }
            return result;
        }
        public User GetUser(string name, bool mode)//найти класс User по определённому параметру 
        {
            bool cond = false;
            switch (mode)
            {
                case false: //поиск по имени
                    foreach (var user in users)
                    {
                        if (user.name == name) { cond = true; return user; }
                    }
                    break;

                case true: //поиск по логину
                    foreach (var user in users)
                    {
                        if (user.login == name) { cond = true; return user; }
                    }
                    LogState($"Пользователя с логином {name} нету в базе данных");
                    break;
            }

            switch (mode)
            {
                case false:
                    if (!cond) LogState($"Пользователя с именем {name} нету в базе данных");
                    break;

                case true:
                    if (!cond) LogState($"Пользователя с логином {name} нету в базе данных");
                    break;
            }
            return null;
        }
        public User GetFullUser(string login , string name)//найти класс User по логину и имени 
        {
            bool cond = false;
            foreach (var user in users)
            {
                if (user.name == name && user.login == login) { cond = true; return user; }
            }
            if (!cond) LogState($"Пользователя с именем {name} и логином {login} нету в базе данных");
            return null;
        }
        public Note GetNote(DateTime time)//взять запись, которая находится в рамках [начальное время;конечное время) 
        {
            foreach (var auditory in auditoriums)
            {
                foreach(var note in auditory.timetable)
                {
                    if (note.startTime <= time && time < note.endTime) return note;
                }
            }
            LogState($"Нету никаких записей в данное время: {time.ToString("dd-MM-yyyy HH:mm")}");
            return null;
        }
        public Auditorium GetAuditorium(string tag)//найти аудиторию по имени 
        {
            foreach (var auditor in auditoriums)
            {
                if (auditor.tag == tag) return auditor;
            }
            LogState($"Аудитория с номером {tag} не найдена");
            return null;
        }
        public bool SetNote(Note old_note, Note new_note)
        {
            var aud = GetAuditorium(old_note.auditorium.tag);
            if (aud.timetable.Remove(old_note))
            {
                aud.AddNote(new_note, this);
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
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        // Создаем временный файл для записи
                        string tempFilePath = System.IO.Path.GetTempFileName();

                        // Открываем временный файл для записи
                        using (StreamWriter writer = new StreamWriter(tempFilePath))
                        {
                            string line;
                            string? temp_line = String.Empty;
                            bool lineFound = false;
                            bool sucess = false;

                            // Читаем файл построчно
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (lineFound)
                                {
                                    if(temp_line != String.Empty) writer.WriteLine(temp_line);
                                    lineFound = false;
                                    sucess = true;
                                }

                                if (line.Contains(searchLine))
                                {
                                    writer.WriteLine(newLine);
                                    lineFound = true;
                                    foreach(var user in new_note.participators)
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
            ExcelPackage excel = new ExcelPackage(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри данные пользователей");
                return false;
            }
            int index = 1;
            while (true)
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
                else if(user_password == old_user.password || user_login == old_user.login 
                        || user_access == old_user.access.ToString().ToLower() || user_name == old_user.name)
                {
                    users.Cells.SetCellValue(index - 1, 0, new_user.login);
                    users.Cells.SetCellValue(index - 1, 1, new_user.password);
                    users.Cells.SetCellValue(index - 1, 2, new_user.access.ToString().ToLower());
                    users.Cells.SetCellValue(index - 1, 3, new_user.name);
                    break;
                }
                else index++;
            }
            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }
        public bool SetAuditorium(Auditorium old_aud, Auditorium new_aud)
        {
            //открываем файл с данными 
            string fullPath = data_path;
            ExcelPackage excel = new ExcelPackage(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? auditoriums = excel.Workbook.Worksheets["Кабинеты"];

            if (auditoriums == null)
            {
                LogState("Пересмотри вводимые тобой данные кабинетов");
                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    Console.ReadKey();
                }
                return false;
            }

            int index = 1;
            while (true)
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
                else if(old_aud.tag == codeName && old_aud.startTime+":00" == startTime && old_aud.endTime + ":00" == endTime 
                        && old_aud.capacity == int.Parse(capacity))
                {
                    auditoriums.Cells.SetCellValue(index - 1, 0, new_aud.tag);
                    auditoriums.Cells["B1"].Value = Date("01.01.2000 "+new_aud.startTime);
                    auditoriums.Cells["B1"].Style.Numberformat.Format = "H:mm";
                    auditoriums.Cells["C1"].Value = Date("01.01.2000 " + new_aud.endTime);
                    auditoriums.Cells["C1"].Style.Numberformat.Format = "H:mm";
                    auditoriums.Cells.SetCellValue(index - 1, 3, new_aud.capacity);
                    break;
                }
                else index++;
            }

            FileInfo excelFile = new(fullPath);
            excel.SaveAs(excelFile);
            return true;
        }


        //базовые функции, не требующиеся в дальнейшем использовании
        public bool CreateDataList(string fileName)//создание макета списка данных 
        {
            //создаем новый документ 
            ExcelPackage excel = new ExcelPackage();

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
            if (!File.Exists(fullPath)) { excel.SaveAs(excelFile); return false; } else return false;
        }
        public bool CreateUserList(string fileName)//создание макета списка юзеров 
        {
            //создаем новый документ 
            ExcelPackage excel = new ExcelPackage();

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
            if (!File.Exists(fullPath)) { excel.SaveAs(excelFile); return false; } else return true;
        }
        public bool CreateDayList(string fileName)//создание макета списка дня 
        {
            //создаем новый документ 
            string currentDirectory = Directory.GetCurrentDirectory();
            string fullPath = currentDirectory + @"\data\days\" + fileName + ".txt";

            if (!File.Exists(fullPath)) { File.Create(fullPath); return false; } else return true;
        }
    }
}