using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using System.Formats.Tar;
using System.Reflection.Metadata.Ecma335;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.System;

namespace InfoBase
{
    internal class DataBase
    {
        /*public List<string> subjects; //список предметов*/
        /*public List<string> teachers; //учителя (список имён)*/
        public List<Auditorium> auditoriums; //аудитории
        public List<User> users; //пользователи

        public string logfile_path; //путь к папке с логами
        public string data_path; //путь к таблице с данными
        public string users_path; //путь к таблице с юзерами
        public string days_path; //путь к папке с расписанием дней
        private readonly bool consoleLogging; //делать логи в консоли или нет (выключить, если нужно будет работать с визуализацией)

        private int log_counter; //для LogState
        ////////////////////Переменные, необходимые для работы всей датабазы////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////

        //некоторые вспомогательные инструменты
        public static DateTime Date(string date)//для дат формата "dd.MM.yyyy hh:mm" (при неудаче форматирует как "yyyy.MM.dd hh:mm")
        {
            string[] datenums = date.Split(' ')[0].Split('.');
            string[] timenums = date.Split(' ')[1].Split(':');
            DateTime data;
            try
            {
                data = new DateTime(int.Parse(datenums[0]), int.Parse(datenums[1]), int.Parse(datenums[2]),
                                int.Parse(timenums[0]), int.Parse(timenums[1]), 0);
                return data;
            }
            catch (Exception)
            {
                data = new DateTime(int.Parse(datenums[2]), int.Parse(datenums[1]), int.Parse(datenums[0]),
                                int.Parse(timenums[0]), int.Parse(timenums[1]), 0);
                return data;
            }
        }
        public void LogState(string message)//логирование всего 
        {
            log_counter++;
            bool newFile = false;
            string dirWithLogName = logfile_path + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            if (!File.Exists(dirWithLogName))
            {
                using (File.Create(dirWithLogName)) { }
                newFile = true;
            };

            using StreamWriter writer = new(dirWithLogName, true);
            int n = 20;
            string line = string.Empty;
            for (int i = 0; i < n; i++) { line += '-'; }
            if (log_counter == 1)
            {
                if (newFile)
                {
                    writer.Write(line + " НОВЫЙ ЗАПУСК " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                }
                else
                {
                    writer.Write('\n' + line + " НОВЫЙ ЗАПУСК " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                }

                if (consoleLogging)
                {
                    Console.WriteLine('\n' + line + " ЛОГ ЗАПУСКА " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                }
            }
            writer.Write(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message + '\n');
            if (consoleLogging)
            {
                if (message.Contains('\n'))
                {
                    Console.Write(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message);
                }
                else
                {
                    Console.Write(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message + '\n');
                }
            }
        }
        public User RandLogPass(string name, string access)
        {
            // Создание генератора случайных чисел
            Random random = new Random();

            // Создание случайного логина
            string login = "user" + random.Next(100000, 999999);

            // Создание случайного пароля
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string password = new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            LogState($"Успешно зарезервирован в памяти пользователь: {login}|{password}");
            return new(login, password, access, name, this);
        }
        public bool Update()
        {
            if (!FillUsers(users_path))
            {
                LogState("Проблема со списком пользователей или ошибка FillUsers()");
                if (consoleLogging)
                {
                    LogState($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                    Console.ReadKey();
                }
                return false;
            }
            else if (!FillData(data_path))
            {
                LogState("Проблема с базовыми данными или ошибка FillData()");
                if (consoleLogging)
                {
                    LogState($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                    Console.ReadKey();
                }
                return false;
            }
            else if (!FillDays(days_path))
            {
                LogState("Проблема с данными расписания или ошибка FillDays()");
                if (consoleLogging)
                {
                    LogState($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                    Console.ReadKey();
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        //рабочие инструменты базы данных
        public User? GetUser(string name, bool mode)//найти класс User по определённому параметру 
        {
            bool cond = false;
            switch (mode)
            {
                case false: //поиск по имени
                    foreach (User user in users)
                    {
                        if (user.name == name)
                        {
                            cond = true;
                            return user;
                        }
                    }
                    break;

                case true: //поиск по логину
                    foreach (User user in users)
                    {
                        if (user.login == name) { cond = true; return user; }
                    }
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
            LogState($"Нету никаких записей в данное время: {time:dd.MM.yyyy HH:mm}");
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

        public bool SetNote(Note old_note, Note new_note)//сменить одну запись на другую
        {
            Auditorium aud = GetAuditorium(old_note.auditorium.tag);
            if (aud.timetable.Remove(old_note))
            {
                _ = aud.AddNote(new_note, this);
                string filePath;

                try
                {
                    filePath = days_path + old_note.startTime.ToString("yyyy.MM.dd") + ".day"; // путь к файлу
                }
                catch (Exception ex) { LogState($"Возникла следующая ошибка: {ex}"); return false; }

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

                        if (lineFound)
                        {
                            if (temp_line != string.Empty)
                            {
                                writer.WriteLine(temp_line);
                            }

                            lineFound = false;

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
        public bool SetUser(User old_user, User new_user)//сменить одного юзера на другого
        {
            string fullPath = users_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            /*if (old_user.access == new_user.access && old_user.access == Access.Teacher)
            {
                SetTeacher(old_user.name, new_user.name);
            }
            else if (old_user.access != new_user.access && old_user.access == Access.Teacher)
            {
                DeleteTeacher(old_user.name);
                AddUser(new_user);
            }
            else if (old_user.access != new_user.access && new_user.access == Access.Teacher)
            {
                AddTeacher(new_user.name);
                DeleteUser(old_user);
            }*/

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
                LogState("Не получилось изменить запись (возможно, заменяемого вами пользователя не существует)");
                return false;
            }

            if (this.users.Remove(old_user))
            {
                this.users.Insert(indexToIns, new_user);
                int index = 1;
                bool cond = true;
                while (index <= users.Dimension.End.Row)
                {
                    string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                    string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                    string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                    string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                    if (user_password == null || user_login == null || user_access == null || user_name == null)
                    {
                        if (user_password == null && user_login == null && user_access == null && user_name == null)
                        {

                        }
                        else
                        {
                            LogState($"Строка данных аудиторий {index} выглядит неполной или является пустой");
                            cond = false;
                        }

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

                if (cond)
                {
                    FileInfo excelFile = new(fullPath);
                    excel.SaveAs(excelFile);
                }
                return cond;

            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами пользователя не существует)");
                return false;
            }
        }
        public bool SetAuditorium(Auditorium old_aud, Auditorium new_aud)//сменить одну аудиторию на другую
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
                LogState("Не получилось изменить запись (возможно, заменяемой вами аудитории не существует)");
                return false;
            }


            bool cond = true;
            if (this.auditoriums.Remove(old_aud))
            {
                this.auditoriums.Insert(indexToIns, new_aud);
                int index = 1;
                while (index <= auditoriums.Dimension.End.Row)
                {
                    string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                    string? startTime = auditoriums.Cells[$"B{index}"].Value?.ToString().Split(' ')[1];
                    string? endTime = auditoriums.Cells[$"C{index}"].Value?.ToString().Split(' ')[1];
                    string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();
                    if (codeName == null || startTime == null || endTime == null || capacity == null)
                    {
                        if (codeName == null && startTime == null && endTime == null && capacity == null)
                        {
                        }
                        else
                        {
                            cond = false;
                            LogState($"Строка данных пользователя {index} выглядит неполной");
                        }
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

            if (cond)
            {
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
            }
            return cond;
        }
        /*public bool SetSubject(string old_subject, string new_subject)//сменить один предмет на другой
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
            bool cond = true;

            if (this.subjects.FindIndex(x => x == old_subject) != -1)
            {
                indexToIns = this.subjects.FindIndex(x => x == old_subject);
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами предмета не существует)");
                cond = false;
            }


            if (teachers.Contains(old_subject))
            {
                int index = 1;
                while (index <= subjects.Dimension.End.Row)
                {
                    string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                    if (subj == null)
                    {
                    }
                    if (subj == old_subject)
                    {
                        subjects.Cells[$"A{index}"].Value = new_subject;
                    }
                    index++;
                }

                if (cond)
                {
                    FileInfo excelFile = new(fullPath);
                    excel.SaveAs(excelFile);

                    teachers.Remove(old_subject);
                    teachers.Insert(indexToIns, new_subject);

                    foreach (Auditorium aud in auditoriums)
                    {
                        for (int i = 0; i < aud.timetable.Count; i++)
                        {
                            if (aud.timetable[i].name == old_subject)
                            {
                                Note new_n = new(aud.timetable[i])
                                {
                                    name = new_subject
                                };
                                _ = SetNote(aud.timetable[i], new_n);
                            }
                        }
                    }
                }
                return cond;
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами предмета не существует)");
                return false;
            }
        }*/

        public bool DeleteNote(Note delete_note)//удалить запись
        {
            Auditorium aud = delete_note.auditorium;
            if (aud.timetable.Remove(delete_note))
            {
                string filePath;

                try
                {
                    filePath = days_path + delete_note.startTime.ToString("yyyy.MM.dd") + ".day"; // путь к файлу
                }
                catch (Exception ex) { LogState($"Ошибка: {ex}"); return false; }
                bool sucess = false;

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

                            }

                            if (line.Contains(searchLine))
                            {
                                lineFound = true;
                                sucess = true;
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

                        if (lineFound)
                        {
                            if (temp_line != string.Empty)
                            {
                                writer.WriteLine(temp_line);
                            }

                            lineFound = false;

                        }

                        // Если строка не была найдена
                        if (!sucess)
                        {
                            LogState($"Строка для замены \"{searchLine}\" не найдена");
                            sucess = false;
                        }
                    }

                    if (sucess)
                    {
                        // Закрываем файлы
                        reader.Close();

                        // Заменяем исходный файл временным файлом
                        File.Delete(filePath);
                        File.Move(tempFilePath, filePath);
                        sucess = true;
                    }

                }
                catch (IOException ex)
                {
                    LogState("Возникла следующая ошибка: " + ex.Message);
                    sucess = false;
                }
                return sucess;
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемой вами записи не существует)");
                return false;
            }
        }
        public bool DeleteUser(User delete_user)//удалить юзера
        {
            string fullPath = users_path;
            ExcelPackage excel = new(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри данные пользователей");
                return false;
            }

            if (this.users.Contains(delete_user))
            {
                int index = 1;
                bool cond = true;
                while (index <= users.Dimension.End.Row)
                {
                    string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                    string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                    string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                    string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                    if (user_password == null || user_login == null || user_access == null || user_name == null)
                    {
                        if (user_password == null && user_login == null && user_access == null && user_name == null)
                        {

                        }
                        else
                        {
                            LogState($"Строка данных пользователя {index} выглядит неполной");
                            cond = false;
                        }
                    }
                    else if (user_password == delete_user.password && user_login == delete_user.login
                            && user_access == delete_user.access.ToString().ToLower() && user_name == delete_user.name)
                    {
                        users.DeleteRow(index);
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }

                if (cond)
                {
                    FileInfo excelFile = new(fullPath);
                    excel.SaveAs(excelFile);
                    /*if (delete_user.access == Access.Teacher)
                    {
                        DeleteTeacher(delete_user.name);
                    }
                    else
                    {*/
                    foreach (Auditorium aud in auditoriums)
                    {
                        for (int i = 0; i < aud.timetable.Count; i++)
                        {
                            if (aud.timetable[i].participators.Contains(delete_user))
                            {
                                Note note0 = new(aud.timetable[i]);
                                note0.participators.Remove(delete_user);
                                SetNote(aud.timetable[i], note0);
                                this.users.Remove(delete_user);
                                i--;
                            }

                        }
                    }
                    /*}*/
                }

                return cond;
            }

            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами пользователя не существует)");
                return false;
            }
        }
        public bool DeleteAuditorium(Auditorium delete_aud)//удалить аудиторию
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

            bool cond = true;
            if (GetAuditorium(delete_aud.tag) != null)
            {
                int index = 1;
                while (index <= auditoriums.Dimension.End.Row)
                {
                    string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                    DateTime timeValue;

                    string? startTime;
                    ExcelRange startTime0 = auditoriums.Cells[$"B{index}"];
                    try
                    {
                        timeValue = DateTime.FromOADate(startTime0.GetValue<double>());
                        startTime = timeValue.ToString("H:mm");
                    }
                    catch (Exception)
                    {
                        timeValue = Date(startTime0.GetValue<string>());
                        startTime = timeValue.ToString("H:mm");

                    }


                    string? endTime;
                    ExcelRange endTime0 = auditoriums.Cells[$"C{index}"];
                    try
                    {
                        timeValue = DateTime.FromOADate(endTime0.GetValue<double>());
                        endTime = timeValue.ToString("H:mm");
                    }
                    catch (Exception)
                    {
                        timeValue = Date(endTime0.GetValue<string>());
                        endTime = timeValue.ToString("H:mm");

                    }
                    string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();


                    if (codeName == null || startTime == null || endTime == null || capacity == null)
                    {
                        if (codeName == null && startTime == null && endTime == null && capacity == null)
                        {
                        }
                        else
                        {
                            LogState($"Строка данных аудитории {index} выглядит неполной");
                            cond = false;
                        }
                    }
                    else if (delete_aud.tag == codeName && delete_aud.startTime == startTime && delete_aud.endTime == endTime
                            && delete_aud.capacity == int.Parse(capacity))
                    {
                        auditoriums.DeleteRow(index);
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

            if (cond)
            {
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
                delete_aud = GetAuditorium(delete_aud.tag);
                for (int i = 0; i < this.auditoriums.Count; i++)
                {
                    if (delete_aud == this.auditoriums[i])
                    {
                        for (int j = 0; j < this.auditoriums[i].timetable.Count; j++)
                        {
                            Note note = this.auditoriums[i].timetable[j];
                            if (note.auditorium.tag == delete_aud.tag)
                            {
                                DeleteNote(note);
                            }
                        }
                        this.auditoriums.RemoveAt(i);
                    }
                }
            }

            return cond;
        }
        /*public bool DeleteSubject(string delete_subject)//удалить предмет
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
                    Console.ReadKey();
                }
                return false;
            }


            if (this.subjects.Contains(delete_subject))
            {
                bool cond = false;
                int index = 1;
                while (index <= subjects.Dimension.End.Row)
                {
                    string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                    if (subj == null)
                    {
                    }
                    if (subj == delete_subject)
                    {
                        subjects.DeleteRow(index);
                        cond = true;
                    }
                    index++;
                }
                if (cond)
                {
                    for (int i = 0; i < auditoriums.Count; i++)
                    {
                        for (int j = 0; j < auditoriums[i].timetable.Count; j++)
                        {
                            Note note = auditoriums[i].timetable[j];
                            if (note.name == delete_subject)
                            {
                                DeleteNote(note);
                            }
                        }
                    }
                    this.subjects.Remove(delete_subject);
                    FileInfo excelFile = new(fullPath);
                    excel.SaveAs(excelFile);
                }
                return cond;
            }
            else
            {
                LogState($"Не получилось изменить запись (возможно, заменяемого вами предмета \"{delete_subject}\" не существует)");
                return false;
            }
        }*/

        public bool AddNote(Note new_note)//добавить запись
        {
            Auditorium aud = GetAuditorium(new_note.auditorium.tag);
            if (aud.AddNote(new_note, this))
            {
                using StreamWriter writer = new(CreateDayList(new_note.startTime.ToString("yyyy.MM.dd")), true);
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
        public bool AddUser(User new_user)//добавить юзера
        {
            string fullPath = users_path;
            ExcelPackage excel = new(new FileInfo(fullPath));

            /*if (new_user.access == Access.Teacher)
            {
                AddTeacher(new_user.name);
            }*/

            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри данные пользователей");
                return false;
            }

            int index = 1;
            bool cond = false;
            bool right = true;
            while (index <= users.Dimension.End.Row)
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
                        cond = true;
                    }
                    else
                    {
                        LogState($"Строка данных пользователя {index} выглядит неполной или является пустой");
                        right = false;
                    }
                }
                index++;
            }
            if (!cond)
            {
                users.Cells.SetCellValue(index - 1, 0, new_user.login);
                users.Cells.SetCellValue(index - 1, 1, new_user.password);
                users.Cells.SetCellValue(index - 1, 2, new_user.access.ToString().ToLower());
                users.Cells.SetCellValue(index - 1, 3, new_user.name);
            }

            if (right)
            {
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
            }
            return right;
        }
        public bool AddAuditorium(Auditorium new_aud)//добавить аудиторию
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
            bool cond = false;
            bool right = true;
            while (index <= auditoriums.Dimension.End.Row)
            {
                string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                DateTime timeValue;

                string? startTime;
                ExcelRange startTime0 = auditoriums.Cells[$"B{index}"];
                try
                {
                    timeValue = DateTime.FromOADate(startTime0.GetValue<double>());
                    startTime = timeValue.ToString("H:mm");
                }
                catch (Exception)
                {
                    timeValue = Date(startTime0.GetValue<string>());
                    startTime = timeValue.ToString("H:mm");

                }


                string? endTime;
                ExcelRange endTime0 = auditoriums.Cells[$"C{index}"];
                try
                {
                    timeValue = DateTime.FromOADate(endTime0.GetValue<double>());
                    endTime = timeValue.ToString("H:mm");
                }
                catch (Exception)
                {
                    timeValue = Date(endTime0.GetValue<string>());
                    endTime = timeValue.ToString("H:mm");

                }
                string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();
                if (codeName == null || startTime == null || endTime == null || capacity == null)
                {
                    if (codeName == null && startTime == null && endTime == null && capacity == null)
                    {
                        index++;
                        auditoriums.Cells[$"A{index}"].Value = new_aud.tag;
                        auditoriums.Cells.SetCellValue(index - 1, 1, Date("01.01.2000 " + new_aud.startTime));
                        auditoriums.Cells[$"B{index}"].Style.Numberformat.Format = "H:mm";
                        auditoriums.Cells.SetCellValue(index - 1, 2, Date("01.01.2000 " + new_aud.endTime));
                        auditoriums.Cells[$"C{index}"].Style.Numberformat.Format = "H:mm";
                        auditoriums.Cells[$"D{index}"].Value = new_aud.capacity;
                        cond = true;
                    }
                    else
                    {
                        LogState($"Строка данных аудитории {index} выглядит неполной");
                        right = false;
                    }
                }
                index++;
            }
            if (!cond)
            {
                auditoriums.Cells[$"A{index}"].Value = new_aud.tag;
                auditoriums.Cells.SetCellValue(index - 1, 1, Date("01.01.2000 " + new_aud.startTime));
                auditoriums.Cells[$"B{index}"].Style.Numberformat.Format = "H:mm";
                auditoriums.Cells.SetCellValue(index - 1, 2, Date("01.01.2000 " + new_aud.endTime));
                auditoriums.Cells[$"C{index}"].Style.Numberformat.Format = "H:mm";
                auditoriums.Cells[$"D{index}"].Value = new_aud.capacity;
            }

            if (right)
            {
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
            }
            return right;
        }
        /*public bool AddSubject(string new_subject)//добавить предмет
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
            bool cond = false;
            bool right = false;
            while (index <= subjects.Dimension.End.Row)
            {
                string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                if (subj == null && !cond)
                {
                    subjects.Cells[$"A{index}"].Value = new_subject;
                    cond = true;
                    right = true;
                }
                else if (subj == null && cond)
                {
                }
                index++;
            }

            if (!cond)
            {
                subjects.Cells[$"A{index}"].Value = new_subject;
                right = true;
            }

            if (right)
            {
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
            }

            return right;
        }*/


        ////////////////////////////////////////////////////////////////////////////////////////
        ///////////////Базовые функции, не требующиеся в дальнейшем использовании///////////////
        public DataBase(string logfile_path, bool consoleLogging)//конструктор 
        {
            log_counter = 0;
            /*subjects = new();*/
            /*teachers = new();*/
            auditoriums = new();
            users = new();
            this.logfile_path = logfile_path;
            this.consoleLogging = consoleLogging;
        }
        public bool FillUsers(string pathToFileUsers)//первоначальное заполнение всех пользователей 
        {
            //открываем файл с данными 
            string fullPath = pathToFileUsers;
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
            bool cond = true;
            while (index <= users.Dimension.End.Row)
            {
                string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                if (user_password == null || user_login == null || user_access == null || user_name == null)
                {
                    if (user_password == null && user_login == null && user_access == null && user_name == null)
                    {

                    }
                    else
                    {
                        LogState($"Строка данных пользователя {index} выглядит неполной или является пустой");
                        cond = false;
                    }
                }
                else
                {
                    this.users.Add(new(user_login, user_password, user_access, user_name, this));
                    index++;
                }
            }
            return cond;

        }
        public bool FillData(string pathToFileData)//заполнение списка предметов и учителей 
        {
            //открываем файл с данными 
            string fullPath = pathToFileData;
            data_path = fullPath;
            ExcelPackage excel = new(new FileInfo(fullPath));
            /*bool cond1 = false;
            bool cond2 = false;*/
            bool cond3 = true;

            //задаём списки 
            /*ExcelWorksheet? subjects = excel.Workbook.Worksheets["Предметы"];*/
            /*ExcelWorksheet? teachers = excel.Workbook.Worksheets["Учителя"];*/
            ExcelWorksheet? auditoriums = excel.Workbook.Worksheets["Кабинеты"];

            if (/*subjects == null ||*/ /*teachers == null ||*/ auditoriums == null)
            {
                /*if (subjects == null)
                {
                    LogState("Пересмотри вводимые тобой данные предметов");
                }*/

                /*if (teachers == null)
                {
                    LogState("Пересмотри вводимые тобой данные учителей");
                }*/

                if (auditoriums == null)
                {
                    LogState("Пересмотри вводимые тобой данные кабинетов");
                }

                if (consoleLogging)
                {
                    Console.WriteLine("Нажми кнопку для выхода");
                    Console.ReadKey();
                }
                return false;
            }

            /*int index = 1;
            while (index <= subjects.Dimension.End.Row)
            {
                string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                if (subj == null)
                {

                }
                else
                {
                    this.subjects.Add(subj);
                    cond1 = true;
                    index++;
                }
            }*/

            /*int index = 1;
            List<string> temp_teachs = new();
            while (index <= teachers.Dimension.End.Row)
            {
                if (index == 1)
                {
                    foreach (User? user in users.Where(usr => usr.access == Access.Teacher).ToList())
                    {
                        this.teachers.Add(user.name);
                        temp_teachs.Add(user.name);
                    }
                }

                //////////////////////////////////////////////////////////////////////
                /////////////////Проверка схожести списков учителей////////////////////
                string? teach = teachers.Cells[$"A{index}"].Value?.ToString();
                if (teach == null)
                {
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
                    cond2 = true;
                    index++;
                }
                /////////////////Проверка схожести списков учителей////////////////////
                //////////////////////////////////////////////////////////////////////
            }*/
            /*if (this.teachers.Except(temp_teachs).ToList().Count != 0)
            {
                string mes = "Списки учителей не совпадают со списком пользователей с доступом Teacher\nНехватает следующих учителей в списке :\n";
                foreach (string? str in this.teachers.Except(temp_teachs).ToList())
                {
                    mes += str + '\n';
                }
                LogState(mes);
                return false;
            }*/

            int index = 1;
            while (index <= auditoriums.Dimension.End.Row)
            {
                string? codeName = auditoriums.Cells[$"A{index}"].Value?.ToString();
                DateTime timeValue;

                string? startTime;
                ExcelRange startTime0 = auditoriums.Cells[$"B{index}"];
                try
                {
                    timeValue = DateTime.FromOADate(startTime0.GetValue<double>());
                    startTime = timeValue.ToString("H:mm");
                }
                catch (Exception)
                {
                    timeValue = Date(startTime0.GetValue<string>());
                    startTime = timeValue.ToString("H:mm");

                }


                string? endTime;
                ExcelRange endTime0 = auditoriums.Cells[$"C{index}"];
                try
                {
                    timeValue = DateTime.FromOADate(endTime0.GetValue<double>());
                    endTime = timeValue.ToString("H:mm");
                }
                catch (Exception)
                {
                    timeValue = Date(endTime0.GetValue<string>());
                    endTime = timeValue.ToString("H:mm");

                }

                string? capacity = auditoriums.Cells[$"D{index}"].Value?.ToString();
                if (codeName == null || startTime == null || endTime == null || capacity == null)
                {
                    if (codeName == null && startTime == null && endTime == null && capacity == null) { }
                    if (codeName == null && capacity == null && auditoriums.Cells[$"B{index}"].Value?.ToString() == null
                                                             && auditoriums.Cells[$"C{index}"].Value?.ToString() == null) { }
                    else
                    {
                        LogState($"Строка данных аудиторий {index} выглядит неполной или является пустой");
                        cond3 = false;
                    }
                }
                else
                {
                    string? start = startTime.Split(":")[0] + ':' + startTime.Split(":")[1];
                    string? end = endTime.Split(":")[0] + ':' + endTime.Split(":")[1];
                    this.auditoriums.Add(new(codeName, start, end, int.Parse(capacity)));
                    index++;
                }
            }
            /*if (!cond1)
            {
                LogState("Пересмотри вводимые тобой данные предметов (возможно, в них нету ничего)");
            }

            if (!cond2)
            {
                LogState("Пересмотри вводимые тобой данные учителей (возможно, в них нету ничего)");
            }*/

            return /*cond1 && cond2 &&*/ cond3;
        }
        public bool FillDays(string pathToDirDays)//первоначальное заполнение всех броней 
        {
            days_path = pathToDirDays;
            string[] files = Directory.GetFiles(pathToDirDays, "*.day");
            Note? temp_note = new();
            Auditorium? temp_auitorium = new();

            bool result = true;

            foreach (string fileName in files)
            {
                string date = System.IO.Path.GetFileName(fileName).Split(".day")[0];
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
                        string[]? parametrs = line.Split("|");
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
                                LogState($"Прочтение строки {line} безуспешно завершено. Проверьте информацию в файле {date + ".day"}");
                                result = false;
                            }
                            else if (temp_user == null)
                            {
                                string[] dta = data_path.Split("//");
                                LogState($"Взятие пользователя по строке {line} безуспешно завершено. Проверьте информацию в файле {date + ".day"} и {dta[dta.Length - 1]}");
                                result = false;
                            }
                            else
                            {
                                auditoriums.Find(x => x == temp_auitorium).timetable.Find(x => x == temp_note).participators.Add(temp_user);
                                users.Find(x => x == temp_user).participating.Add(temp_note);
                                temp_user.participating.Add(temp_note);
                            }
                        }
                        else if (parametrs == null || parametrs.Length < 2 || falseNote) { }
                        else
                        {
                            LogState($"Неверный формат данных в строке {line}");
                            result = false;
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
        /*bool SetTeacher(string old_teacher, string new_teacher)//сменить одного учителя на другого
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

            if (DeleteTeacher(old_teacher))
            {
                AddTeacher(new_teacher);

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
                while (index <= teachers.Dimension.End.Row)
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
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами учителя не существует)");
                return false;
            }

            return true;
        }*/
        /*bool DeleteTeacher(string delete_teacher)
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
                    Console.ReadKey();
                }
                return false;
            }
            bool cond = true;

            if (this.teachers.Contains(delete_teacher))
            {
                int index = 1;
                while (index <= teachers.Dimension.End.Row)
                {
                    string? teacher0 = teachers.Cells[$"A{index}"].Value?.ToString();
                    if (teacher0 == null)
                    {

                    }

                    if (teacher0 == delete_teacher)
                    {
                        teachers.DeleteRow(index);
                        cond = true;
                    }
                    index++;
                }

                if (cond)
                {
                    foreach (User? user in users.Where(usr => usr.access == Access.Teacher).ToList())
                    {
                        if (user.name == delete_teacher)
                        {
                            users.Remove(user);
                            break;
                        }
                    }

                    foreach (Auditorium aud in auditoriums)
                    {
                        for (int i = 0; i < aud.timetable.Count; i++)
                        {
                            if (aud.timetable[i].teacher.name == delete_teacher)
                            {
                                DeleteNote(aud.timetable[i]);
                                i--;
                            }
                        }
                    }

                    this.teachers.Remove(delete_teacher);
                }

            }
            else
            {
                LogState("Не получилось изменить запись (возможно, заменяемого вами учителя не существует)");
                return false;
            }

            if (cond)
            {
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);

            }
            return cond;
        }*/
        /*bool AddTeacher(string new_teacher)
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
            bool cond = false;
            while (index <= teachers.Dimension.End.Row)
            {
                string? teacher0 = teachers.Cells[$"A{index}"].Value?.ToString();
                if (teacher0 == null)
                {
                    teachers.Cells[$"A{index}"].Value = new_teacher;
                    cond = true;
                    break;
                }
                index++;
            }
            if (!cond)
            {
                teachers.Cells[$"A{index}"].Value = new_teacher;
                cond = true;
            }

            if (cond)
            {
                FileInfo excelFile = new(fullPath);
                excel.SaveAs(excelFile);
            }
            return cond;
        }*/
        public bool CreateDataList(string fileName)//создание макета списка данных 
        {
            //создаем новый документ 
            ExcelPackage excel = new();

            //добавляем лист 
            /*ExcelWorksheet worksheet1 = excel.Workbook.Worksheets.Add("Учителя");*/
            /*ExcelWorksheet worksheet2 = excel.Workbook.Worksheets.Add("Предметы");*/
            ExcelWorksheet worksheet3 = excel.Workbook.Worksheets.Add("Кабинеты");

            //добавляем данные 
            /* worksheet1.Cells["A1"].Value = "Учителя";
             worksheet1.Column(1).Width = 100;

             worksheet2.Cells["A1"].Value = "Предметы";
             worksheet2.Column(1).Width = 100;*/

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
            string fullPath = days_path + data + ".day";

            if (!File.Exists(fullPath)) { _ = File.Create(fullPath); return null; }
            else
            {
                return fullPath;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        /////////////   Штуки для реализации штук ниже   ///////////////////////////////////////

        public List<Note> GetNotesInSameTime(DateTime dt)
        {
            List<Note> notes = new List<Note>();
            int count = 0;

            string dateString = dt.ToString("yyyy.MM.dd") + ".day";
            string path = "data\\days\\" + dateString;
            //string item = user.login + "|" + user.name;
            var lines = File.ReadAllLines(path).ToList();

            string target = dt.ToString("HH:mm");
            for (int i = 0; i < lines.Count; i++)
            {
                bool checkTime = lines[i].Contains(target);
                if (checkTime == true)
                {
                    count++;
                    string[] words = lines[i].Split('|');

                    string name = words[0];
                    string startTimeString = dt.ToString("yyyy.MM.dd") + " " + words[1];
                    DateTime startTime = Date(startTimeString);
                    string endTimeString = dt.ToString("yyyy.MM.dd") + " " + words[2];
                    DateTime endTime = Date(endTimeString);
                    string t= words[3];
                    User teacher = null;
                    for (int j = 0; j < users.Count; j++)
                    {
                        if (users[j].name == t)
                        {
                            teacher = users[j];
                        }
                    }
                    string subname = words[4];
                    string room = words[5];
                    Auditorium auditorium = null;
                    for (int j = 0; j < auditoriums.Count; j++)
                    {
                        if (auditoriums[j].tag == room)
                        {
                            auditorium = auditoriums[j];
                        }
                    }

                    if (teacher == null)
                    {
                        LogState("teacher");
                    }
                    if (auditorium == null)
                    {
                        LogState("auditorium");
                    }
                    Note note = new Note(name, startTime, endTime, teacher, subname, auditorium);
                    notes.Add(note);
                    /*foreach(string s in words)
                    {
                        LogState(s);
                    }*/

                }
            }
/*            LogState(count.ToString());
            foreach (Note note in notes)
            {
                LogState(note.auditorium.tag);
            }*/
            return notes;
        }//ищет всех записей содержащие одинаковую дату и время
        public List<Note> GetNotesInSameRoom(DateTime dt, Auditorium aud) //ищет записи с одинаковой датой и аудиторией
        {
            List<Note> notes = new List<Note>();
            int count = 0;

            string dateString = dt.ToString("yyyy.MM.dd") + ".day";
            string path = "data\\days\\" + dateString;
            var lines = File.ReadAllLines(path).ToList();

            string target = aud.tag;
            for (int i = 0; i < lines.Count; i++)
            {
                bool checkRoom = lines[i].Contains(target);
                if (checkRoom == true)
                {
                    count++;
                    string[] words = lines[i].Split('|');

                    string name = words[0];
                    string startTimeString = dt.ToString("yyyy.MM.dd") + " " + words[1];
                    DateTime startTime = Date(startTimeString);
                    string endTimeString = dt.ToString("yyyy.MM.dd") + " " + words[2];
                    DateTime endTime = Date(endTimeString);
                    string t = words[3];
                    User teacher = null;
                    for (int j = 0; j < users.Count; j++)
                    {
                        if (users[j].name == t)
                        {
                            teacher = users[j];
                        }
                    }
                    string subname = words[4];
                    string room = words[5];
                    Auditorium auditorium = null;
                    for (int j = 0; j < auditoriums.Count; j++)
                    {
                        if (auditoriums[j].tag == room)
                        {
                            auditorium = auditoriums[j];
                        }
                    }

                    if (teacher == null)
                    {
                        LogState("teacher");
                    }
                    if (auditorium == null)
                    {
                        LogState("auditorium");
                    }
                    Note note = new Note(name, startTime, endTime, teacher, subname, auditorium);
                    notes.Add(note);
                }
            }
/*            LogState(count.ToString());
            foreach (Note note in notes)
            {
                LogState(note.auditorium.tag);
            }*/
            return notes;
        }
        public bool CheckDate(string dt)
        {
            DateTime check;
            return DateTime.TryParse(dt, out check);
        }//проверка на корректность даты (возможность преобразовать в DateTime)
        public bool ISRangeInRangeDate(DateTime start, DateTime end, List<Note> notes)
        {
            bool test = true;
            int testInt = 0;
            for (int i = 0; i < notes.Count; i++)
            {
                if (start >= notes[i].startTime && start < notes[i].endTime ||
                     end > notes[i].startTime && end <= notes[i].endTime ||
                     start <= notes[i].startTime && end >= notes[i].endTime)
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

        ////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////     для студента       //////////////////////////////////////////

        public void StudentSubscribe(User user) //запись ученика на предмет
        {
            LogState("Вводить время циферками: dd.mm.yyyy hh:mm");
            string FullDateTime = Console.ReadLine();
            LogState("Вы ввели: " + FullDateTime);
            bool check = CheckDate(FullDateTime);
            if (check == false)
            {
                LogState("Пук пук... пользователь неверно ввел дату и метод не может работать...");
            }
            else
            {
                DateTime sampleTime = DataBase.Date(FullDateTime);

                List<Note> notes = GetNotesInSameTime(sampleTime);
                if (notes.Count >= 0)
                {
                    LogState("В это время проводится:");
                    for (int i = 0; i < notes.Count; i++)
                    {
                        LogState(i + 1 + ") " + notes[i].name + "     " + notes[i].auditorium.tag + "  " + notes[i].teacher.name);
                    }
                    LogState("Введите индекс желаемой записи:");
                    int choseNote = Int32.Parse(Console.ReadLine()) - 1;
                    if (choseNote < 0 || choseNote > notes.Count)
                    {
                        LogState("Пук пук... пользователь ввел несуществующий индекс");
                    }
                    else
                    {
                        /*                for (int i = 0; i < notes.Count; i++)
                                        {
                                            if (i == choseNote)
                                            {
                                                Note note = notes[i];
                                                LogState("Вы выбрали   " + note.name + "             кабинет " + note.auditorium.tag);
                                            }
                                        }*/

                        Note noteOne = notes[choseNote];   //GetNote(sampleTime);

                        LogState("Вы выбрали   " + noteOne.name + "             кабинет " + noteOne.auditorium.tag);
                        LogState("Желаете зарегестрироваться?              1 - да        2 - нет");

                        switch (Console.ReadKey(false).Key)
                        {
                            case ConsoleKey.D1:
                                noteOne.participators.Add(user);
                                LogState("Вы зарегались");

                                string dateString = sampleTime.ToString("yyyy.MM.dd") + ".day";
                                //LogState(dateString);
                                string path = "data\\days\\" + dateString;
                                //LogState(path);
                                string item = user.login + "|" + user.name;
                                var lines = File.ReadAllLines(path).ToList();

                                string target = noteOne.name;
                                int index = lines.FindIndex(s => s.Contains(target));
                                lines.Insert(index + 1, item);
                                File.WriteAllLines(path, lines);

                                LogState("Список всез зарегавшихся:");
                                for (int i = 0; i < noteOne.participators.Count; i++)
                                {
                                    LogState(noteOne.participators[i].name);
                                }
                                break;

                            case ConsoleKey.D2:
                                LogState("Вы не зарегались");
                                break;
                            default:
                                break;
                        }
                        LogState("");
                    }
                }
                else if (notes.Count == 0)
                {
                    LogState("В данное время нет созданных записей: " + FullDateTime);
                    LogState("Проверьте корректность введенной даты");
                }
                else
                {
                    LogState("ОШИБКА   СТОП000... какой-то сбой в GetNotesInSameTime или StudentSubscribe");
                }
            }
        }
        public void StudentAllParticipation(User user)
        {

        } //отображение всех записей, где учавствует юзер
        //пусто

        ////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////     для преподователей     //////////////////////////////////////

        public void TeacherReserveTime(User user) //создание записи через: день->кабинет->время
        {
            //пусто
        }
        //пустой
        public void TeacherAllParticipation(User user)
        {

        } //отображение всех записей, которые создал юзер
        //пусто
        public void TeacherReserveRoom(User user) //создание записи через: кабинет->дата->время
        {
            LogState("Введите название кабинета");
            string roomString = Console.ReadLine();
            LogState("Вы ввели: " + roomString);
            Auditorium room = GetAuditorium(roomString);
            if (room == null)
            {
                LogState("Такой аудитории нет");
            }
            else
            {
                LogState("Рабочии часы: " + room.startTime + " - " + room.endTime);

                LogState("Вводить день для бронирования циферками: dd.mm.yyyy");
                string dateString = Console.ReadLine();
                string FullDateTime = dateString + " 00:00";
                LogState("Вы ввели: " + FullDateTime);
                bool check = CheckDate(FullDateTime);

                if (check == false)
                {
                    LogState("Проблема с датой");
                }
                else
                {
                    DateTime date = DataBase.Date(FullDateTime);
                    string datePath = date.ToString("yyyy.MM.dd") + ".day";
                    string path = "data\\days\\" + datePath;

                    //проверка на существование файла с днем (в противном случае создать)
                    if(File.Exists(path) == false)
                    {
                        //File.Create(path);
                        using (StreamWriter sw = new StreamWriter(path));

                    }

                    List<Note> notes = GetNotesInSameRoom(date, room);

                    if (notes.Count == null) { } //если никого нет
                    else
                        LogState("Рабочии часы: " + room.startTime + " - " + room.endTime);
                    {
                        if (notes.Count > 0)
                        {
                            LogState("Уже забранированное время:");
                            for (int i = 0; i < notes.Count; i++)
                            {
                                LogState($"{notes[i].startTime.ToString("t")}" + " - " + $"{notes[i].endTime.ToString("t")}");
                            }
                        }
                    } //если кто-то есть

                    LogState("");
                    LogState("Введите время начала брони (hh:mm):");
                    string startTimeString = Console.ReadLine();
                    string fullStartTimeString = dateString + " " + startTimeString;
                    bool checkI = CheckDate(fullStartTimeString);
                    if (checkI == false)
                    {
                        LogState("Проблема с датой");
                    }
                    else
                    {
                        DateTime startTime = DataBase.Date(fullStartTimeString);

                        LogState("Введите время конца брони (hh:mm):");
                        string endTimeString = Console.ReadLine();
                        string fullEndTimeString = dateString + " " + endTimeString;
                        bool checkII = CheckDate(fullEndTimeString);
                        if (checkII == false)
                        {
                            LogState("Проблема с датой");
                        }
                        else
                        {
                            DateTime endTime = DataBase.Date(fullEndTimeString);

                            //проверка на пересечение с другими датами
                            //LogState(startTime + " - " + endTime);
                            bool checkIII = ISRangeInRangeDate(startTime,endTime, notes);
                            //сделать проверку на пересечение срабочими часами

                            if (checkIII == false) LogState("В это время нельзя сделать бронь в аудиторию " + room.tag);
                            else
                            {
                                //заполнение строки
                                LogState("Введите название предмета/мероприятия:");
                                string subject = Console.ReadLine();
                                LogState("Введите комментарий:");
                                string subjectInfo = Console.ReadLine();
                                string fillInfo = subject + "|" + startTimeString + "|" + endTimeString + "|" + user.name + "|" + subjectInfo + "|" + room.tag;
                                LogState(fillInfo);

                                //редактирование текстового файла
                                var lines = File.ReadAllLines(path).ToList();

                                File.AppendAllText(path, fillInfo);
                                File.AppendAllText(path, "\n \n");
                                LogState("Запись создана");
                            }
                        }
                    }
                }
            }
            
        }

    }
}

// для реализации пользователя: сделать проверку на одинаковое имя (не должны существовать юзеры с одинак. именами)