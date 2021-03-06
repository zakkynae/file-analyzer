using System.Reflection;
using Newtonsoft.Json;
//using System.Text.Json;

namespace FileAnalyzer
{
    public class Program
    {
        static void Main()
        {
            int action;
            do
            {
                PrintMenu();
                Console.Write("Выберите действие: ");
                action = int.Parse(Console.ReadLine());
                if((Menu)action == Menu.GetNewData)
                {
                    Console.Clear();
                    Console.Write("Введите директорию, которую хотите добавить в БД: ");
                    var path = Console.ReadLine();
                    WriteDb(GetFiles(path));
                    Console.WriteLine("База данных обновлена.\n Для продолжения нажмите любуюу клавишу...");
                    Console.ReadKey();
                }
                if((Menu)action == Menu.GetData)
                {
                    Console.Clear();
                    Console.Write("Введите директорию, информацию о которой хотите получить: ");
                    var path = Console.ReadLine();
                    var files = SearchInDb(path);
                    if(files.count > 0) PrintData(files.fileData);
                    else PrintData(GetFiles(path));
                    Console.WriteLine("Для продолжения нажмите любуюу клавишу...");
                    Console.ReadKey();
                }
                if((Menu)action == Menu.GetLength)
                {
                    Console.Clear();
                    Console.Write("Введите директорию: ");
                    var path = Console.ReadLine();
                    Console.WriteLine("Топ-10 самых больших файлов директории");
                    var files = SearchInDb(path);
                    if(files.count > 0) PrintData(GetLength(files.fileData));
                    else PrintData(GetLength(GetFiles(path)));
                    Console.WriteLine("Для продолжения нажмите любуюу клавишу...");
                    Console.ReadKey();
                }
                if((Menu)action == Menu.GetBiggestDirs)
                {
                    Console.Clear();
                    Console.Write("Введите директорию: ");
                    var path = Console.ReadLine();
                    Console.WriteLine("Топ-10 самых больших директорий");
                    var files = SearchInDb(path);
                    if (files.count > 0) PrintData(GetBiggestDirs(files.fileData));
                    else PrintData(GetBiggestDirs(GetFiles(path)));
                    Console.WriteLine("Для продолжения нажмите любуюу клавишу...");
                    Console.ReadKey();
                }
                if((Menu)action == Menu.GetExtensions)
                {
                    Console.Clear();
                    Console.Write("Введите директорию: ");
                    var path = Console.ReadLine();
                    Console.WriteLine("Топ-10 самых популярных расширений директории");
                    var files = SearchInDb(path);
                    if (files.count > 0) PrintData(GetExtension(files.fileData));
                    else PrintData(GetExtension(GetFiles(path)));
                    Console.WriteLine("Для продолжения нажмите любуюу клавишу...");
                    Console.ReadKey();
                }
                if((Menu)action == Menu.GetBiggestExtensions)
                {
                    Console.Clear();
                    Console.Write("Введите директорию: ");
                    var path = Console.ReadLine();
                    Console.WriteLine("Топ-10 самых больших расширений директории");
                    var files = SearchInDb(path);
                    if(files.count > 0) PrintData(GetBiggestExtensions(files.fileData));
                    else PrintData(GetBiggestExtensions(GetFiles(path)));
                    Console.WriteLine("Для продолжения нажмите любуюу клавишу...");
                    Console.ReadKey();
                }

            }
            while ((Menu)action != Menu.Quit);
        }

        public static string database = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DataBase.json"; //БД
        #region Напечатать меню
        public static void PrintMenu()
        {
            foreach (var action in Enum.GetValues(typeof(Menu)))
                Console.WriteLine($"{(int)action}.{action}");
        }
        #endregion
        #region Список данных файлов
        public static List<FileData> GetFiles(string path)
        {
            var fileData = new List<FileData>();
            var dirs = new Stack<string>();
            if(!Directory.Exists(path))
            {
                throw new ArgumentException("Такой дирректории не существует");
            } 
            dirs.Push(path);
            while(dirs.Count > 0)
            {
                var currentDir = dirs.Pop();
                string[] subdirs;
                try
                {
                    subdirs = Directory.GetDirectories(currentDir);
                } catch
                {
                    continue;
                }
                foreach(var dir in subdirs)
                {
                    dirs.Push(dir);
                }
                string[] files;
                try
                {
                    files = Directory.GetFiles(currentDir);
                } catch
                {
                    continue;
                }
                foreach(var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var data = new FileData(fileInfo.FullName, fileInfo.Name, fileInfo.Extension, fileInfo.Length, fileInfo.CreationTime);
                    fileData.Add(data);
                }
            }
            return fileData;
        }
        #endregion
        #region Топ 10 больших файлов дирректорий
        public static List<FileData> GetLength(List<FileData> files)
        {
            var sortedFiles = files.OrderByDescending(f => f.Length).ToList();
            var lengths = new List<FileData>();
            for(int i = 0; i < 10; i++)
                lengths.Add(sortedFiles[i]);
            return lengths;
        }
        #endregion
        #region Топ 10 больших директорий
        public static string[] GetBiggestDirs(List<FileData> files)
        {
            var dirs = new Dictionary<string, long>();
            foreach (var file in files)
            {
                file.FullName = file.FullName.Replace(file.Name, "");
                if (!dirs.ContainsKey(file.FullName)) dirs[file.FullName] = 0;
                dirs[file.FullName] =+ file.Length;
            }
            dirs = dirs.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            var extensionsDir = dirs.Keys.ToList();
            string[] biggestDirs;
            if(extensionsDir.Count >= 10)
            {
                biggestDirs = new string[10];
                for (int i = 0; i < 10; i++)
                    biggestDirs[i] = extensionsDir[i];
            } else
            {
                biggestDirs = new string[extensionsDir.Count];
                for (int i = 0; i < extensionsDir.Count; i++)
                    biggestDirs[i] = extensionsDir[i];
            }
            return biggestDirs;
        }
        #endregion
        #region Топ 10 расширений
        public static string[] GetExtension(List<FileData> files)
        {
            var extension = new Dictionary<string,int>();
            foreach (var file in files)
            {
                if (!extension.ContainsKey(file.Extension)) extension[file.Extension] = 0;
                extension[file.Extension]++;
            }
            extension = extension.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

            var extensionsDir= extension.Keys.ToList();
            var firstTenExtensions = new string[10];
            for (int i = 0; i < 10; i++)
                firstTenExtensions[i] = extensionsDir[i];
            return firstTenExtensions;
        }
        #endregion
        #region Топ 10 расширений по суммарному объему
        public static string[] GetBiggestExtensions(List<FileData> files)
        {
            var extension = new Dictionary<string, long>();
            foreach (var file in files)
            {
                if (!extension.ContainsKey(file.Extension)) extension[file.Extension] = 0;
                extension[file.Extension] =+ file.Length;
            }
            extension = extension.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            var extensionsDir = extension.Keys.ToList();
            var biggestExtension = new string[10];
            for (int i = 0; i < 10; i++)
                biggestExtension[i] = extensionsDir[i];
            return biggestExtension;
        }
        #endregion
        #region Запись в БД
        public static void WriteDb(List<FileData> fileData)
        {
            var db= new List<string>();
            foreach(var file in fileData)
            {
                //var json = JsonSerializer.Serialize(file);
                var json = JsonConvert.SerializeObject(file);
                db.Add(json);
            }
            File.WriteAllLines(database, db);
        }
        #endregion
        #region Поиск в БД
        public static (List<FileData> fileData, int count)  SearchInDb(string path)
        {
            var db = File.ReadAllLines(database);
            var fileData = new List<FileData>();
            foreach(var json in db)
            {
                //var file = JsonSerializer.Deserialize<FileData>(json);
                var file = JsonConvert.DeserializeObject<FileData>(json);
                file.FullName = file.FullName.Replace(file.Name, "");
                if (file.FullName == (path+ "\\"))
                    fileData.Add(file);
            }
            var result = (list: fileData, count: fileData.Count);
            return result;
        }
        #endregion
        #region Вывод данных в консоль
        public static void PrintData(List<FileData> fileData)
        {
            foreach(var file in fileData)
                Console.WriteLine(file.GetString());
        }
        public static void PrintData (string[] fileData)
        {
            foreach (var file in fileData)
                Console.WriteLine(file);
        }
        #endregion

    }
}