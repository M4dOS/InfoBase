using OfficeOpenXml;
using System.Text;

namespace InfoBase
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //задаём кодировку
            Console.OutputEncoding = Encoding.Unicode;

            //задаём неизменные параметры
            const int consoleX = 150; //длина консоли
            const int consoleY = 50; //высота консоли
            const bool isDebug = true; //переключатель между режимом дебага и обычным режимом (логирование работает в обоих случаях)
            const string version = "v1.1.2020 alpha"; //строка версии (смотри правила оформления ниже)

            /*///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Оформлять строку version в соответствии с правилом:
                1) Первая цифра - реализация. Она не меняется до тех пор, пока ваша часть не будет завершена полностью.
                После полного завершения своей части просто сделать +1
                2) Вторая цифра - счётчик. Перед отправкой каждого нового коммита на гит делать +1.
                После полного завершения счётчик обнуляется.
                3) Третья цифра - текущее время. Перед отправкой каждого нового коммита написать время без ':' (09:30 -> 0930)
            Также перед отправкой коммита написать в сообщении этот номер версии и прописать условный список изменений
            Это всё необходимо для отслеживания версий и в критическом случае возвращения на определённую версию
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////код ниже трогать запрещено//////////////////////////////////////////////////////

            //настройка для EPPlus 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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
            DataBase db = new(logsDir, isDebug);
            if (!File.Exists(workDir + "Data.xlsx"))
            {
                db.CreateDataList(workDir + "Data.xlsx");
            }

            if (!File.Exists(workDir + "Users.xlsx"))
            {
                db.CreateUserList(workDir + "Users.xlsx");
            }

            //заполнение данных и проверка на подлинность (непустоту) 
            if (!db.FillUsers(workDir + "Users.xlsx"))
            {
                db.LogState("Проблема со списком пользователей или ошибка FillUsers()");
                if (isDebug)
                {
                    db.LogState($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                    Console.ReadKey();
                }
            }
            else if (!db.FillData(workDir + "Data.xlsx"))
            {
                db.LogState("Проблема с базовыми данными или ошибка FillData()");
                if (isDebug)
                {
                    db.LogState($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                    Console.ReadKey();
                }
            }
            else if (!db.FillDays(daysDir))
            {
                db.LogState("Проблема с данными расписания или ошибка FillDays()");
                if (isDebug)
                {
                    db.LogState($"Возникла ошибка, проверьте лог {DateTime.Now:yyyy-MM-dd}.log");
                    Console.ReadKey();
                }
            }
            /////////////////////////////////////////код выше трогать запрещено//////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            /*///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            1) Работа программы (писать свой код ТОЛЬКО внутри while(true){ } или внутри классов и функций в них
            2) Всё что выше трогать категорически запрещено для правильной работы программы
            3) Переключатель деббагера isDebug (по надобности) и строку версии version (в соответствии с правилами) можно изменять 
            4) Обязательным условием является использование LogState(string message) вместо Console.Write
            5) Старайтесь как можно меньше умещать код в while(true){ }
            6) Работать с Database.Date(string date) с осторожностью:
            - Первым делом она пробует формат "dd.MM.yyyy hh:mm"
            - При неудаче форматирует как "yyyy.MM.dd hh:mm"
            - В остальных случаях может выдать ошибку с последующим выходом из программы
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

            else
            {
                db.LogState($"Начало работы программы: {DateTime.Now.ToString("F")}{DateTime.Now.ToString(".fff")}"); //просто нужно
                while (true)
                {
 


                    Console.ReadKey();
                    /*db.Update();*/
                }
            }
        }
    }
}