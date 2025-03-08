using NLog;
using Npgsql;
using System.Text;
using static System.Console;

namespace OperaNotions
{
    public class WriteInMDFile
    {
        // Переменная логгера
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // Чисто заметок (записей в таблице)
        static int count = 0;

        public static async Task Write()
        {
            _logger.Info("Начало работы программы.");

            string connectionString = "Host=localhost;Username=postgres;Password=Admin123;Database=postgres";
            string outputDirectory = @"C:\Users\user\Desktop\Obsidian\ObsidianNotes";

            try
            {
                if (LogManager.Configuration == null)
                {
                    WriteLine("Ошибка: Конфигурация NLog не загружена!");
                }

                using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                _logger.Info("Подключение к базе данных установлено.");

                string query = "SELECT content, category, datecreated FROM notionstemp";
                using NpgsqlCommand command = new NpgsqlCommand(query, connection);
                using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    try
                    {
                        // Чтение содержания заметки
                        string content = reader.GetString(0)
                            .Replace("\r\n", "\n")  // Убираем Windows-переводы строк
                            .Replace("\r", "\n")    // Убираем возможные CR
                            .Replace("\u0002", "\n")
                            .Trim();  // Убираем лишние пробелы и переводы в начале и конце

                        // Чтение категории заметки (папки)
                        string category = reader.GetString(1);

                        // Чтение даты заметки (хранится в String)
                        string dateString = reader.GetString(2);

                        // Замена знаков (иначе не создаётся имя файла)
                        dateString = dateString.Replace(".", "-").Replace(":", "-");

                        // Перестановка местами чисел в строке даты и времени (для сортировки по году, затем по месяцу и т.д.)
                        string[] dateParts = dateString.Split(' ');
                        string[] dateElements = dateParts[0].Split('-');
                        if (dateElements.Length == 3)
                        {
                            dateString = $"{dateElements[2]}-{dateElements[1]}-{dateElements[0]} {dateParts[1]}";
                        }

                        string categoryPath = Path.Combine(outputDirectory, category);
                        Directory.CreateDirectory(categoryPath);

                        string fileName = $"{dateString}.md";
                        string filePath = Path.Combine(categoryPath, fileName);

                        await File.WriteAllTextAsync(filePath, content, new UTF8Encoding(false));

                        count++;

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Ошибка при обработке записи из базы данных");
                    }
                }

            }
            catch (Exception ex)
            {

                _logger.Fatal(ex, "Критическая ошибка при подключении к базе данных");
                WriteLine(ex.Message);
            }

            finally
            {

                _logger.Info("Завершение работы программы.");

                WriteLine("Заметки успешно экспортированы!");
                WriteLine("-----------");
                WriteLine($"В количестве: {count} штук.");
                WriteLine("-----------");
                WriteLine("Нажмите ENTER для выхода.");
                ReadLine();
            }
        }
    }
}
