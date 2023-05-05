using OfficeOpenXml;
using System.Text;

namespace InfoBase
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //настройка для EPPlus 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //задаём кодировку
            Console.OutputEncoding = Encoding.Unicode;

            //задаём неизменные параметры
            const int consoleX = 150;
            const int consoleY = 50;
            const bool isDebug = false;

            //для верхней панели
            const string version = "v0.7.1233 alpha";
            const string info = "Auditions" + " " + version;

            //прописываем настройки консоли
            Console.SetWindowSize(consoleX, consoleY);
            if (isDebug)
            {
                int countBuferScreens = 5;
                Console.SetBufferSize(consoleX, (consoleY + 1) * countBuferScreens);
            }
            else
            {
                Console.SetBufferSize(consoleX, consoleY);
            }

            Console.CursorVisible = false;
            Console.Title = info;

            //константы для файлов 
            string workDir = Directory.GetCurrentDirectory() + @"\data\";
            string daysDir = workDir + @"days\";
            string logsDir = workDir + @"logs\";

            //подготовка датабазы 
            DataBase db = new(logsDir, true);
            if (!File.Exists(workDir + "Data.xlsx"))
            {
                _ = db.CreateDataList(workDir + "Data.xlsx");
            }

            if (!File.Exists(workDir + "Users.xlsx"))
            {
                _ = db.CreateUserList(workDir + "Users.xlsx");
            }
            /*if (Directory.GetFiles(daysDir, "*.txt").Length == 0) db.CreateDayList("01.01.0001");*/

            //заполнение данных и проверка на подлинность (непустоту) 
            if (!db.FillUsers(workDir + "Users.xlsx"))
            {
                db.LogState("Проблема со списком пользователей или ошибка FillUsers()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now:dd-MM-yyyy}.log");
                _ = Console.ReadKey();
            }
            else if (!db.FillData(workDir + "Data.xlsx"))
            {
                db.LogState("Проблема с базовыми данными или ошибка FillData()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now:dd-MM-yyyy}.log");
                _ = Console.ReadKey();
            }
            else if (!db.FillDays(daysDir))
            {
                db.LogState("Проблема с данными расписания или ошибка FillDays()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now:dd-MM-yyyy}.log");
                _ = Console.ReadKey();
            }

            //работа программы (писать свой код ТОЛЬКО здесь или внутри классов и функций в них
            //всё что выше трогать категорически запрещено для правильной работы программы
            else
            {
                while (true)
                {
                    ///тестовый набор функций работы программы, можно удалить после окончательния тестов
                    ////////////////////////////////////////////////////////////////////////////////////
                    User user1 = db.GetUser("login1", true);
                    User user2 = db.GetUser("ИмяАдмина1", false);
                    User user3 = new(user1);
                    Note note1 = db.GetNote(DataBase.Date("01.01.2000 9:30"));
                    Note note2 = db.GetNote(DataBase.Date("02.01.2000 9:30"));
                    Note note3 = new(note1);
                    Auditorium aud1 = new(db.auditoriums[0])
                    {
                        endTime = "23:30"
                    };

                    note3.name = note3.teacher.name;
                    user3.login = "log1n1";

                    _ = db.SetNote(note1, note3);
                    _ = db.SetAuditorium(db.auditoriums[0], aud1);
                    _ = db.SetUser(user1, user3);

                    Console.WriteLine("Жмакай любую клавишу"); _ = Console.ReadKey();
                    ////////////////////////////////////////////////////////////////////////////////////
                    ///тестовый набор функций работы программы, можно удалить после окончательния тестов
                }
            }
        }
    }
}