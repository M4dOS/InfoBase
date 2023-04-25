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
                if (log_counter == 1) writer.Write('\n' + line + " НОВЫЙ ЗАПУСК " + DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + '\n');
                writer.Write(DateTime.Now.ToString("HH:mm:ss") + " : " + message + '\n');
            }
        }
        public Auditorium FindAuditorium(string tag)//найти аудиторию по имени 
        {
            foreach (var auditor in auditoriums)
            {
                if (auditor.tag == tag) return auditor;
            }
            return null;
        }
        public DataBase(string logfile_path)//конструктор 
        {
            log_counter = 0;
            subjects = new();
            teachers = new();
            auditoriums = new();
            users = new();
            this.logfile_path = logfile_path;
        }

        //рабочие инструменты базы данных
        public bool FillUsers(string excelFileName)//первоначальное заполнение всех пользователей 
        {
            //открываем файл с данными 
            string fullPath = excelFileName;
            ExcelPackage excel = new ExcelPackage(new FileInfo(fullPath));
            ExcelWorksheet? users = excel.Workbook.Worksheets["Данные"];
            if (users == null)
            {
                LogState("Пересмотри вводимые тобой данные. Нажми любую клавишу для выхода");
                Console.ReadKey();
                return false;
            }

            int index = 1;
            while (true)
            {
                string? user_login = users.Cells[$"A{index}"].Value?.ToString();
                string? user_password = users.Cells[$"B{index}"].Value?.ToString();
                string? user_access = users.Cells[$"C{index}"].Value?.ToString();
                string? user_name = users.Cells[$"D{index}"].Value?.ToString();
                if (user_password == null || user_login == null || user_access == null || user_name == null) break;
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
            ExcelPackage excel = new ExcelPackage(new FileInfo(fullPath));

            //задаём списки 
            ExcelWorksheet? subjects = excel.Workbook.Worksheets["Предметы"];
            ExcelWorksheet? teachers = excel.Workbook.Worksheets["Учителя"];
            ExcelWorksheet? auditoriums = excel.Workbook.Worksheets["Кабинеты"];

            if (subjects == null || teachers == null || auditoriums == null)
            {
                LogState("Пересмотри вводимые тобой данные. Нажми любую клавишу для выхода");
                Console.ReadKey();
                return false;
            }

            int index = 1;
            while (true)
            {
                string? subj = subjects.Cells[$"A{index}"].Value?.ToString();
                if (subj == null) break;
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
                if (teach == null) break;
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
                if (codeName == null || startTime == null || endTime == null || capacity == null) break;
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
            string[] files = Directory.GetFiles(workDir, "*.txt");
            foreach (var fileName in files)
            {
                string date = Path.GetFileName(fileName).Split(".txt")[0];
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
                        while ((line = reader.ReadLine()) != null)
                        {
                            foreach (var aud in auditoriums)
                            {
                                if (line.Split("|")[5] == aud.tag) { aud.AddNote(new(line, date, this), this); cond = true; break; }
                            }
                        }
                        if (!cond) return false;
                    }
                }
            }
            return true;
        }
        public User GetUser(string name, string mode)//найти класс User по определённому параметру
        {
            bool cond = false;
            switch (mode)
            {
                case "Имя":
                    foreach (var user in users)
                    {
                        if (user.name == name) { cond = true; return user; }
                    }
                    break;

                case "Логин":
                    foreach (var user in users)
                    {
                        if (user.login == name) { cond = true; return user; }
                    }
                    LogState($"Пользователя с логином {name} нету в базе данных");
                    break;

                default:
                    LogState($"Задан неверный параметр поиска пользователя \"{mode}\"");
                    break;
            }

            switch (mode)
            {
                case "Имя":
                    if (!cond) LogState($"Пользователя с именем {name} нету в базе данных");
                    break;

                case "Логин":
                    if (!cond) LogState($"Пользователя с логином {name} нету в базе данных");
                    break;
            }
            return null;
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