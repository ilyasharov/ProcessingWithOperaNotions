using static System.Console;

namespace OperaNotions
{
    class Program
    {
        static void Main()
        {
            while (true)
            {
                WriteLine("Введите 1 для чтения заметок в базу или 2 - для записи в файлы из базы данных (или 'q' для выхода):");
                WriteLine("=====================================");
                string input = ReadLine();

                if (input == "q")
                {
                    WriteLine("Выход из программы...");
                    break;
                }

                switch (input)
                {
                    case "1":
                        ReadTextFileLines.readFunction();
                        break;

                    case "2":
                        WriteInMDFile.Write();
                        break;

                    default:
                        WriteLine("Некорректный ввод. Попробуйте снова.");
                        break;
                }
            }
        }
    }
}

    

