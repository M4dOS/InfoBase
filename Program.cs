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
            const string version = "v0.8.2155 alpha";
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

            //заполнение данных и проверка на подлинность (непустоту) 
            if (!db.FillUsers(workDir + "Users.xlsx"))
            {
                db.LogState("Проблема со списком пользователей или ошибка FillUsers()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                _ = Console.ReadKey();
            }
            else if (!db.FillData(workDir + "Data.xlsx"))
            {
                db.LogState("Проблема с базовыми данными или ошибка FillData()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                _ = Console.ReadKey();
            }
            else if (!db.FillDays(daysDir))
            {
                db.LogState("Проблема с данными расписания или ошибка FillDays()");
                Console.WriteLine($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
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
                    User user3 = db.GetUser("log1n10", true);
                    Auditorium aud1 = new(db.auditoriums[0]);

                    db.DeleteAuditorium(aud1);
                    db.DeleteUser(user3);
                    

                    Console.WriteLine("Жмакай любую клавишу"); _ = Console.ReadKey();
                    ////////////////////////////////////////////////////////////////////////////////////
                    ///тестовый набор функций работы программы, можно удалить после окончательния тестов
                }
            }
        }
    }
}