using interpreter_2;

string inputFile = "input.txt";
string outputDir = Directory.GetCurrentDirectory();
try
{
    if (!File.Exists(inputFile))
    {
        throw new FileNotFoundException($"Файл {inputFile} не найден");
    }

    var interpreter = new FileProcessor(inputFile, outputDir);
    interpreter.ProcessFile();
    Console.WriteLine($"Решение сохранено в {outputDir}");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}