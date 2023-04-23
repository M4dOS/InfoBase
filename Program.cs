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
            const string version = "v0.2.2051 alpha";
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

            //настройка для EPPlus 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //подготовка датабазы 
            DataBase db = new DataBase();
            if (!File.Exists(workDir + "Data.xlsx")) db.CreateDataList(workDir + "Data.xlsx");
            if (!File.Exists(workDir + "Users.xlsx")) db.CreateUserList(workDir + "Users.xlsx");
            if (Directory.GetFiles(daysDir, "*.txt").Length == 0) db.CreateDayList("15.01.2001");

            //заполнение данных и проверка на подлинность (непустоту) 
            if (!db.FillData(workDir + "Data.xlsx")) Console.WriteLine("У тебя ошибка в FillData :>");
            else if (!db.FillUsers(workDir + "Users.xlsx")) Console.WriteLine("У тебя ошибка в FillUsers :>");
            else if (!db.FillDays(daysDir)) Console.WriteLine("У тебя ошибка в FillDays :>");
            else
            {
                Console.WriteLine("Жмакай любую клавишу"); Console.ReadKey();

                while (true)
                {
                    /*место  для пары строчек кода*/
                }
            }
        }
    }
}