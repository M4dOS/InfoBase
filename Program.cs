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
            const string version = "v0.3.1710 alpha";
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

            //настройка для EPPlus 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //подготовка датабазы 
            DataBase db = new DataBase(logsDir);
            if (!File.Exists(workDir + "Data.xlsx")) db.CreateDataList(workDir + "Data.xlsx");
            if (!File.Exists(workDir + "Users.xlsx")) db.CreateUserList(workDir + "Users.xlsx");
            if (Directory.GetFiles(daysDir, "*.txt").Length == 0) db.CreateDayList("15.01.2001");

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
                    User user1 = db.GetUser("login1", "Логин");
                    User user2 = db.GetUser("ИмяАдмина1", "Имя");
                    User user3 = db.GetUser("ИмяАдмина2", "НеИмя");
                    Console.WriteLine("Жмакай любую клавишу"); Console.ReadKey();
                    /*место  для пары строчек кода*/
                }
            }
        }
    }
}