using OfficeOpenXml;
using System.Text;

namespace InfoBase
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //задаём кодировку
            Console.OutputEncoding = Encoding.Unicode;

            //задаём неизменные параметры
            const int consoleX = 150;
            const int consoleY = 50;
            const int countBuferMaps = 5;
            const bool isDebug = false;

            //для верхней панели
            const string version = "v0.5.2258 alpha";
            const string info = "Auditions" + " " + version;

            //прописываем настройки консоли
            Console.SetWindowSize(consoleX, consoleY);
            if (isDebug) Console.SetBufferSize(consoleX, (consoleY + 1) * countBuferMaps);
            else Console.SetBufferSize(consoleX, consoleY);
            Console.CursorVisible = false;
            Console.Title = info;

            //константы для файлов 
            string workDir = Directory.GetCurrentDirectory() + @"\data\";
            string daysDir = workDir + @"days\";
            string logsDir = workDir + @"logs\";
            /*string subjectDir = workDir + @"sub\";*/

            //настройка для EPPlus 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //подготовка датабазы 
            DataBase db = new DataBase(logsDir, true);
            /*if (!File.Exists(workDir + "Data.xlsx")) db.CreateDataList(workDir + "Data.xlsx");
            if (!File.Exists(workDir + "Users.xlsx")) db.CreateUserList(workDir + "Users.xlsx");
            if (Directory.GetFiles(daysDir, "*.txt").Length == 0) db.CreateDayList("15.01.2001");*/

            //заполнение данных и проверка на подлинность (непустоту) 
            if (!db.FillUsers(workDir + "Users.xlsx"))
            {
                db.LogState("Проблема со списком пользователей или ошибка FillUsers()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now.ToString("dd-MM-yyyy")}.log");
                Console.ReadKey();
            }
            else if (!db.FillData(workDir + "Data.xlsx"))
            {
                db.LogState("Проблема с базовыми данными или ошибка FillData()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now.ToString("dd-MM-yyyy")}.log");
                Console.ReadKey();
            }
            else if (!db.FillDays(daysDir))
            {
                db.LogState("Проблема с данными расписания или ошибка FillDays()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now.ToString("dd-MM-yyyy")}.log");
                Console.ReadKey();
            }

            //работа программы
            else
            {
                while (true)
                {
                    User user1 = db.GetUser("login1", true); 
                    User user2 = db.GetUser("ИмяАдмина1", false);
                    User user3 = new(user1);
                    Note note1 = db.GetNote(DataBase.Date("01.01.2000 9:30"));
                    Note note2 = db.GetNote(DataBase.Date("02.01.2000 9:30"));
                    Note note3 = new(note1);
                    Auditorium aud1 = new(db.auditoriums[0]);
                    aud1.endTime = "23:30";

                    note3.name = note3.teacher.name;
                    user3.login = "log1n1";
                    
                    db.SetNote(note1, note3);
                    db.SetAuditorium(db.auditoriums[0], aud1);
                    /*db.SetUser(user1, user3);*/

                    Console.WriteLine("Жмакай любую клавишу"); Console.ReadKey();
                    /*место  для пары строчек кода*/
                }
            }
        }
    }
}