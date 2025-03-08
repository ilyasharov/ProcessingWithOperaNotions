using System.Text;
using Npgsql;
using static System.Console;

namespace OperaNotions
{
    public class ReadTextFileLines
    {
        // Количество записей в базу
        static int id = 0;

        // Путь к файлу // Замените на реальный путь к файлу
        static string filePath = @"C:\Users\user\Desktop\Notes\notes.adr";

        // Подключение к базе данных
        static string connString = "Host=localhost;Database=postgres;Username=postgres;Password=Admin123";

        // Массив для строк из файла
        static List<string> stringNotes = new List<string>(10000);

        public static void readFunction()
        {
            string folderName = "";
            string noteText = "";
            DateTime dtCreated = DateTime.Now;

            try
            {

                // Помещает строки из файла в массив stringNotes
                readFile();

                // Перебор строк
                foreach (string line in stringNotes)
                {

                    if (line.StartsWith("#FOLDER"))
                    {
                        folderName = strFolderNameSearch(line);
                    }

                    if (line.StartsWith("#NOTE"))
                    {
                        noteText = strNoteNameSearch(line);
                        dtCreated = ConvertUnixTimestampToDateTime(strNoteCreatedSearch(line));

                        // Отладка
                        //WriteLine($"Note number: {id++}, text: {noteText}, date create: {dtCreated}, category: {folderName}");

                        recordInDB(folderName, noteText, dtCreated);
                    }
                }

                WriteLine("-----------");
                WriteLine("END OF FILE");
                WriteLine("-----------");

                WriteLine($"String Number of : {id} was record in DB");
                ReadLine();

            }
            catch (Exception ex)
            {
                WriteLine(ex.Message.ToString()); ReadLine();
            }
        }

        // Чтение файла и вывод на экран строк, начинающихся с #FOLDER и #NOTE
        private static void readFile()
        {
            try
            {

                // Считывание файла построчно
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string currentLine;

                    // Чтение строк до конца файла
                    while ((currentLine = reader.ReadLine()) != null)
                    {
                        // Проверка начала строки
                        if (currentLine.StartsWith("#FOLDER") || currentLine.StartsWith("#NOTE"))
                        {
                            // Создание пустой строки для хранения блока
                            StringBuilder block = new StringBuilder();

                            // Добавление первой строки к блоку
                            block.AppendLine(currentLine);

                            // Чтение строк блока до пустой строки
                            while ((currentLine = reader.ReadLine()) != null && !currentLine.Equals(""))
                            {
                                block.AppendLine(currentLine);
                            }

                            // Добавление блока в список строк
                            stringNotes.Add(block.ToString().Trim());
                        }
                    }
                }

            }
            catch (Exception ex) { WriteLine(ex.Message.ToString()); ReadLine(); }
        }

        // Поиск и извлечение имени папки в строке
        private static string strFolderNameSearch(string inputString)
        {
            try
            {

                // Получение позиции "=" после "NAME"
                int nameValueStart = inputString.IndexOf("NAME=") + 5;

                // Получение позиции "CREATED"
                int createdPos = inputString.IndexOf("CREATED");

                // Извлечение отрезка между "=" и "CREATED"
                string nameValue = inputString.Substring(nameValueStart, createdPos - nameValueStart - 1).Trim();

                return nameValue;

            }
            catch (Exception ex) { WriteLine(ex.Message.ToString()); ReadLine(); return null; }
        }

        // Алгоритм на C# для получения отрезка между "=" и "CREATED"
        private static string strNoteNameSearch(string inputString)
        {
            try
            {

                // Получение позиции "=" после "NAME"
                int nameValueStart = inputString.IndexOf("NAME=") + 5;

                // Получение позиции "CREATED"
                int createdPos = inputString.IndexOf("CREATED");

                // Извлечение отрезка между "=" и "CREATED"
                string nameValue = inputString.Substring(nameValueStart, createdPos - nameValueStart - 1).Trim();

                return nameValue;

            }
            catch (Exception ex) { WriteLine(ex.Message.ToString()); ReadLine(); return null; }
        }

        // Функция преобразования "created" в дату
        private static DateTime ConvertUnixTimestampToDateTime(long timestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(timestamp);
            dateTime = dateTime.ToLocalTime();
            return dateTime;
        }

        // Запись отдельной строки в базу данных
        private static void recordInDB(string folderName, string noteText, DateTime dateCreated)
        {
            string content = noteText;
            string category = folderName;
            string dt = dateCreated.ToString();

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string insertQuery = "INSERT INTO NotionsTemp (id, Content, Category, DateCreated) VALUES (@id, @content, @category, @datecreated)";

                    id++;

                    using (var cmd = new NpgsqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.Parameters.AddWithValue("@content", content);
                        cmd.Parameters.AddWithValue("@category", category);
                        cmd.Parameters.AddWithValue("@dateCreated", dt);

                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex) { WriteLine(ex.Message.ToString()); ReadLine(); }
        }

        private static long strNoteCreatedSearch(string inputString)
        {
            string str = "";
            bool activeChar = false;
            int activePos = 0;
            string substring = "ACTIVE";

            try
            {
                // Получение позиции "CREATED"
                int createdPos = inputString.IndexOf("CREATED=") + 8;

                activeChar = inputString.Contains(substring);

                // Проверка наличия поля "ACTIVE"
                if (activeChar)
                {
                    // Извлечение значения "CREATED"
                    activePos = inputString.IndexOf(substring);
                    str = inputString.Substring(createdPos, activePos - createdPos - 1).Trim();
                }
                if (!activeChar)
                {
                    // Извлечение значения "CREATED" (если "ACTIVE" отсутствует)
                    str = inputString.Substring(createdPos).Trim();
                }

                // Преобразование значения "CREATED" в long
                long createdValue = (long)Convert.ToDouble(str);

                return createdValue;

            }
            catch (Exception ex) { WriteLine(ex.Message.ToString()); ReadLine(); return 0; }
        }

    }
}
