using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.Json;

namespace File_manager
{
    class Program
    {
        public static int MaxDepth; // Глубина пейджинга. Задается файлом config.json
        public static int PrintPages; // Постраничный вывод информации, число строк на странице. Задается файлом config.json
        public static List<string> fsList = new List<string>();

        [Serializable]
        public class Config
        {
            public int MaxDepth { get; set; }
            public int PrintPages { get; set; }
        }
        public static Config GetConfig()
        {
            string path = @"config.json";
            Config BackupConfig = new Config() { MaxDepth = 3, PrintPages = 25 };
            try
            {
                if (!File.Exists(path))
                {
                    string json = JsonSerializer.Serialize(BackupConfig);
                    File.WriteAllText(path, json);
                }

                string newConfig = File.ReadAllText(path);
                Config config = JsonSerializer.Deserialize<Config>(newConfig);
                return config;
            }
            catch (Exception)
            {
                Console.WriteLine("Произошла ошибка чтения файла config.json");
                return BackupConfig;
            }
        }
        public static string ReadLastUsePath()
        {
            string startPath = @"startDirectory.cfg";
            string defaultPath = @"C:\";
            string path;
            try
            {
                if (File.Exists(startPath))
                {
                    path = File.ReadAllText(startPath);
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                    else
                    {
                        return defaultPath;
                    }
                }
                return defaultPath;
            }
            catch (Exception)
            {
                Console.WriteLine("Стартовый путь задан стандартным");
                return defaultPath;
            }
        }
        public static void LastUsePath(string path)
        {
            try
            {
                string startPath = @"startDirectory.cfg";
                File.WriteAllText(startPath, $"{path}");
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка при создании или записи файла со строкой стартовой позиции");
            }
        }
        public static void GetHelp()
        {
            string helpPath = @"help.txt";
            Console.Clear();
            Console.WriteLine(File.ReadAllText(helpPath));
        }
        public static void PrintPaging()
        {
            int counter = 0;
            int page = 0;
            Console.Clear();
            for (int i = 0; i < fsList.Count; i++)
            {
                Console.WriteLine(fsList[i]);
                counter++;
                if (counter % PrintPages == 0)
                {
                    page++;
                    Console.WriteLine();
                    Console.WriteLine("Распечатана " + page + " стр. из " + Math.Ceiling((double)(fsList.Count) / PrintPages) + " страниц");
                    Console.WriteLine("Press Enter to continue, or type \"q\" to stop printing.");
                    string answer = Console.ReadLine();
                    if (answer == "q")
                    {
                        break;
                    }
                    Console.Clear();
                }
            }
            fsList.Clear();
        }

        public static void OperationInfo()
        {
            Console.WriteLine("Для выполнения операции воспользуйтесь справкой --help");
        }
        public static string CutEndFilePath(string path)
        {
            path = path.Substring(0, path.Length - 1);
            return path;
        }
        public static string CheckEndPath(string path)
        {
            if (path.Substring(path.Length - 1) != "\\")
            {
                path += @"\";
            }
            return path;
        }
        public static void OperationDone()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("*** Операция завершена ***");
            Console.WriteLine(Environment.NewLine);
        }
        public static void DirectoryAndFileCopy(string sourcePath, string destinationPath)
        {
            string filePathSource = CutEndFilePath(sourcePath);
            string filePathDestination = CutEndFilePath(destinationPath);
            if (Directory.Exists(sourcePath))
            {
                DirectoryInfo dirs = new DirectoryInfo(sourcePath);
                foreach (DirectoryInfo dir in dirs.GetDirectories())
                {
                    try
                    {
                        if (Directory.Exists(destinationPath + dir.Name) != true)
                        {
                            Directory.CreateDirectory(destinationPath + dir.Name);
                        }
                        DirectoryAndFileCopy(dir.FullName, destinationPath + dir.Name);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Ошибка копирования папок");
                    }
                }
                foreach (string fileName in Directory.GetFiles(sourcePath))
                {
                    try
                    {
                        string fLink = fileName.Substring(fileName.LastIndexOf('\\'), fileName.Length - fileName.LastIndexOf('\\'));
                        File.Copy(fileName, destinationPath + fLink, true);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Ошибка копирования файлов");
                    }
                }
            }
            else if (File.Exists(filePathSource))
            {
                try
                {
                    File.Copy(filePathSource, filePathDestination, true);
                    Console.WriteLine($"Указанный файл {filePathSource} скопирован");
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка копирования файлов");
                }
            }
            else
            {
                Console.WriteLine("Указанный путь не существует");
            }
        }

        public static void DirectoryAndFileRemove(string path)
        {
            try
            {
                string filePath = CutEndFilePath(path);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    Console.WriteLine($"Указанная директория {path} удалена");
                }
                else if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Указанный файл {filePath} удален");
                }
                else
                {
                    Console.WriteLine("Указанный путь не существует");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка удаления");
            }
        }

        public static void DirectoryAndFileMove(string oldPath, string newPath)
        {
            try
            {
                string filePathOld = CutEndFilePath(oldPath);
                string filePathNew = CutEndFilePath(newPath);
                if (Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, newPath);
                    if (filePathOld.Substring(0, filePathOld.LastIndexOf('\\')) == filePathNew.Substring(0, filePathNew.LastIndexOf('\\')))
                    {
                        Console.WriteLine("Директория переименована");
                    }
                    else
                    {
                        Console.WriteLine("Директория перемещена");
                    }
                }
                else if (File.Exists(filePathOld))
                {
                    File.Move(filePathOld, filePathNew);
                    Path.GetFullPath(filePathOld);
                    if (filePathOld.Substring(0, filePathOld.LastIndexOf('\\')) == filePathNew.Substring(0, filePathNew.LastIndexOf('\\')))
                    {
                        Console.WriteLine("Файл переименован");
                    }
                    else
                    {
                        Console.WriteLine("Файл перемещен");
                    }
                }
                else
                {
                    Console.WriteLine("Указанный путь не существует");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка пути");
            }
        }
        public static long GetDirectorySize(DirectoryInfo directory, bool includeSubdir)
        {
            long totalSize = 0;
            try
            {
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                {
                    totalSize += (long)file.Length;
                }
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    totalSize += GetDirectorySize(dir, true);
                }
                return (long)totalSize;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Отказано в доступе.");
                return (long)totalSize;
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка чтения размера файлов.");
                return (long)totalSize;
            }
        }

        public static void GetFileInfo(string path)
        {
            try
            {
                string flInput = CutEndFilePath(path);
                if (File.Exists(flInput))
                {
                    FileInfo fl = new FileInfo(flInput);
                    FileVersionInfo fInfo = FileVersionInfo.GetVersionInfo(flInput);
                    fsList.Add("Информация о файле: " + fl.FullName);
                    fsList.Add("");
                    fsList.Add($"{"Имя: ",-15} {fl.Name}");
                    fsList.Add($"{"Размер: ",-15} {fl.Length:N0} байт");
                    fsList.Add($"{"Дата создания: ",-15} {fl.CreationTime}");
                    fsList.Add($"{"Тип: ",-15} {fl.Attributes}");
                    if (fl.Extension == ".exe")
                    {
                        Console.WriteLine();
                        fsList.Add("");
                        fsList.Add($"{"Product Name: ",-15} {fInfo.ProductName}");
                        fsList.Add($"{"Product Version: ",-15} {fInfo.ProductVersion}");
                        fsList.Add($"{"Company Name: ",-15} {fInfo.CompanyName}");
                        fsList.Add($"{"Product Name: ",-15} {fInfo.ProductName}");
                        fsList.Add($"{"File Version: ",-15} {fInfo.FileVersion}");
                        fsList.Add($"{"File Description: ",-15} {fInfo.FileDescription}");
                        fsList.Add($"{"Original Filename: ",-15} {fInfo.OriginalFilename}");
                        fsList.Add($"{"Legal Copyright: ",-15} {fInfo.LegalCopyright}");
                        fsList.Add($"{"Internal Name: ",-15} {fInfo.InternalName}");
                        fsList.Add($"{"IsDebug: ",-15} {fInfo.IsDebug}");
                        fsList.Add($"{"IsPatched: ",-15} {fInfo.IsPatched}");
                        fsList.Add($"{"IsPreRelease: ",-15} {fInfo.IsPreRelease}");
                        fsList.Add($"{"IsPrivateBuild: ",-15} {fInfo.IsPrivateBuild}");
                        fsList.Add($"{"IsSpecialBuild: ",-15} {fInfo.IsSpecialBuild}");
                    }
                    fsList.Add("");
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                OperationInfo();
            }
        }
        public static void GetListOfDirectory(string path)
        {
            string dirName = path;
            if (Directory.Exists(dirName))
            {
                fsList.Add("**********************************************************************************************************************");
                fsList.Add("Подкаталоги: " + dirName);
                DirectoryInfo dirs = new DirectoryInfo(dirName);
                DirectoryInfo[] getDirs = dirs.GetDirectories();
                fsList.Add($"{"Имя папки",-70} {"Размер, байт",15} {"Дата создания",20} {"Тип",-10}");
                foreach (DirectoryInfo s in getDirs)
                {
                    fsList.Add($"{s.Name,-70} {GetDirectorySize(s, true),15:N0} {s.CreationTime,20} {s.Attributes,-10} ");
                }
                fsList.Add("");
                fsList.Add("**********************************************************************************************************************");
                fsList.Add("Файлы: " + dirName);
                DirectoryInfo files = new DirectoryInfo(dirName);
                FileInfo[] getFiles = files.GetFiles();
                fsList.Add($"{"Имя файла",-70} {"Размер, байт",15} {"Дата создания",20} {"Тип",-10}");
                if (getFiles.Length != 0)
                {
                    for (int i = 0; i < getFiles.Length; i++)
                    {
                        fsList.Add($"{getFiles[i].Name,-70} {getFiles[i].Length,15:N0} {getFiles[i].CreationTime,20} {getFiles[i].Attributes,-10} ");
                    }
                }
                else if (getFiles.Length == 0)
                {
                    fsList.Add("Данная папка: " + dirName + " не содержит файлов.");
                }
                fsList.Add("");
                fsList.Add("**********************************************************************************************************************");
                fsList.Add("Информация о директории: " + dirName);
                fsList.Add("");
                fsList.Add($"{"Имя: ",-15} {dirs.Name}");
                fsList.Add($"{"Размер: ",-15} {GetDirectorySize(dirs, true):N0} байт");
                fsList.Add($"{"Дата создания: ",-15} {dirs.CreationTime}");
                fsList.Add($"{"Тип: ",-15} {dirs.Attributes}");
                fsList.Add("");
            }
        }

        public static bool IsDirectory(FileSystemInfo fsItem)
        {
            return ((fsItem.Attributes & FileAttributes.Directory) == FileAttributes.Directory);
        }

        public static void PrintTree(string startPath, string prefix = "", int depth = 0)
        {
            try
            {
                if (depth >= MaxDepth)
                {
                    return;
                }
                DirectoryInfo myDir = new DirectoryInfo(startPath);
                FileSystemInfo[] fsItems = myDir.GetFileSystemInfos();
                if (depth == 0)
                {
                    fsList.Add(Convert.ToString(myDir.Root));
                }
                foreach (FileSystemInfo fsItem in fsItems)
                {
                    FileSystemInfo lastItem = fsItems[fsItems.Length - 1];
                    if (fsItem != lastItem)
                    {
                        fsList.Add(prefix + "├───" + fsItem.Name);
                        if (IsDirectory(fsItem))
                        {
                            PrintTree(fsItem.FullName, prefix + "│   ", depth + 1);
                        }
                    }
                    if (fsItem == lastItem)
                    {
                        if (lastItem != null)
                        {
                            fsList.Add(prefix + @"└───" + lastItem.Name);
                            if (IsDirectory(lastItem))
                            {
                                PrintTree(lastItem.FullName, prefix + "    ", depth + 1);
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Отказано в доступе.");
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка.");
            }
        }

        static void Main(string[] args)
        {
            Config config = GetConfig();
            MaxDepth = config.MaxDepth;
            PrintPages = config.PrintPages;
            string input;
            string readCommandDisk;
            string myPath = CheckEndPath(ReadLastUsePath());
            bool isExit = false;
            char[] sep = { ' ' };
            char[] sep2 = { ' ', '.', '\\', '/' };
            do
            {
                try
                {
                    DirectoryInfo curDir = new DirectoryInfo(myPath);
                    Console.Write(curDir.FullName + " > ");
                    input = Console.ReadLine();
                    string[] inputId = input.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    string[] inputId_2 = input.Split(sep2, StringSplitOptions.RemoveEmptyEntries);
                    if (input.Length == 0)
                    {
                        continue;
                    }
                    if (input.Length == 2)
                    {
                        if (input.Substring(input.Length - 1) == ":")
                        {
                            inputId[0] = "disk";
                        }
                    }
                    switch (inputId[0])
                    {
                        case "disk":
                            readCommandDisk = input.Substring(0, 2);
                            int count = 0;
                            string disk = readCommandDisk.ToUpper();                            
                            disk = CheckEndPath(disk);
                            DriveInfo[] getDrives = DriveInfo.GetDrives();
                            for (int i = 0; i < getDrives.Length; i++)
                            {
                                if (disk == getDrives[i].Name & getDrives[i].IsReady)
                                {
                                    myPath = disk;
                                    count = 1;
                                    break;
                                }
                                else if (disk == getDrives[i].Name & !getDrives[i].IsReady)
                                {
                                    Console.WriteLine($"Диск {disk} не готов.");
                                    count = 1;
                                    break;
                                }
                            }
                            if (count == 0)
                            {
                                Console.WriteLine($"Диск {disk} не существует.");
                            }
                            break;
                        case "cd":
                            try
                            {
                                if (inputId.Length == 1)
                                {
                                    Console.WriteLine("Укажите место назначения. Для вывода списка команд можете вызвать справку --help.");
                                }
                                else if (inputId.Length == 2)
                                {
                                    if (inputId[1] == ".")
                                    {
                                        throw new Exception();
                                    }
                                    if (inputId[1] == "..")
                                    {
                                        if (myPath == curDir.Root.ToString())
                                        {
                                            Console.WriteLine("Ошибка. Вы находитесь в корневом каталоге.");
                                            break;
                                        }
                                        myPath = CheckEndPath(curDir.Parent.ToString());
                                        break;
                                    }
                                    if (inputId[1] == "/")
                                    {
                                        myPath = curDir.Root.ToString();
                                        break;
                                    }
                                    if (inputId[1] == "")
                                    {
                                        throw new Exception();
                                    }
                                    if (inputId_2.Length == 1)
                                    {
                                        throw new Exception();
                                    }
                                    if (Directory.Exists(inputId[1]))
                                    {
                                        myPath = CheckEndPath(inputId[1]);
                                    }
                                    else if (Directory.Exists(myPath + inputId[1]))
                                    {
                                        myPath = CheckEndPath(CheckEndPath(myPath) + inputId[1]);
                                    }
                                    else if (Directory.Exists(inputId_2[1]))
                                    {
                                        myPath = CheckEndPath(inputId_2[1]);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Указанное место: " + inputId[1] + " не существует.");
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                OperationInfo();
                            }
                            break;
                        case "mv":
                            try
                            {
                                string oldPath = CheckEndPath(inputId[1]);
                                string newPath = CheckEndPath(inputId[2]);
                                if (inputId.Length == 3)
                                {
                                    DirectoryAndFileMove(oldPath, newPath);
                                    OperationDone();
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            catch (Exception)
                            {
                                OperationInfo();
                            }

                            break;
                        case "rm":
                            try
                            {
                                string deletePath = CheckEndPath(inputId[1]);
                                DirectoryAndFileRemove(deletePath);
                                OperationDone();
                            }
                            catch (Exception)
                            {
                                OperationInfo();
                            }
                            break;
                        case "cp":
                            try
                            {
                                string fromPath = CheckEndPath(inputId[1]);
                                Console.WriteLine(fromPath);
                                string toPath = CheckEndPath(inputId[2]);
                                Console.WriteLine(toPath);
                                DirectoryAndFileCopy(fromPath, toPath);
                                OperationDone();
                            }
                            catch (Exception)
                            {
                                OperationInfo();
                            }
                            break;
                        case "tree":
                            try
                            {
                                PrintTree(CheckEndPath(curDir.FullName));
                                GetListOfDirectory(CheckEndPath(curDir.FullName));
                                PrintPaging();
                                OperationDone();
                            }
                            catch (Exception)
                            {
                                OperationInfo();
                            }

                            break;
                        case "--help":
                            GetHelp();
                            break;
                        case "exit":
                            isExit = true;
                            LastUsePath(curDir.FullName);
                            break;
                        case "file":
                            try
                            {                                
                                GetFileInfo(CheckEndPath(inputId[1]));
                                PrintPaging();
                                OperationDone();
                            }
                            catch (Exception)
                            {
                                OperationInfo();
                            }
                            break;
                        case "ls":
                            try
                            {                                
                                GetListOfDirectory(CheckEndPath(curDir.FullName));
                                PrintPaging();
                                OperationDone();
                            }
                            catch (Exception)
                            {
                                OperationInfo();
                            }
                            break;
                        default:
                            Console.WriteLine("Команда не определена. Для вывода списка команд можете вызвать справку --help.");
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка ввода. Для вывода списка команд можете вызвать справку --help.");
                }
            }
            while (!isExit);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

    }
}
