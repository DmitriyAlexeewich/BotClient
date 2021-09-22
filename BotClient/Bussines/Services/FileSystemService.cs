using BotClient.Bussines.Interfaces;
using BotClient.Models.FileSystem;
using BotClient.Models.FileSystem.Enumerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BotClient.Bussines.Services
{
    public class FileSystemService : IFileSystemService
    {

        public DirectoryModel GetBasicDirectoriesPath(EnumBasicDirectoryType BasicDirectoryType)
        {
            var result = new DirectoryModel();
            switch (BasicDirectoryType)
            {
                case EnumBasicDirectoryType.DebugLog:
                    result = GetDiretory(@"C:\BotLogs\ApplicationLog");
                    break;
                case EnumBasicDirectoryType.ElementLog:
                    result = GetDiretory(@"C:\BotLogs\ElementLog");
                    break;
                case EnumBasicDirectoryType.Screenshot:
                    result = GetDiretory(@"C:\Screenshot");
                    break;
                case EnumBasicDirectoryType.Chromedriver:
                    result = GetDiretory(@"C:\ChromeDriver");
                    break;
                case EnumBasicDirectoryType.ActionLog:
                    break;
                case EnumBasicDirectoryType.Configuration:
                    result = GetDiretory(@"C:\Configuration");
                    break;
                case EnumBasicDirectoryType.MissionLog:
                    result = GetDiretory(@"C:\BotLogs\MissionLog");
                    break;
            }
            return result;
        }

        public DirectoryModel GetDiretory(string DirectoryPath)
        {
            var result = new DirectoryModel();
            var directories = DirectoryPath.Split(Path.DirectorySeparatorChar).ToList();
            if (directories.Count > 0)
            {
                var progessPath = "";
                for (int i = 0; i < directories.Count; i++)
                {
                    progessPath += directories[i];
                    if (!Directory.Exists(progessPath))
                        Directory.CreateDirectory(progessPath);
                    progessPath += Path.DirectorySeparatorChar;
                }
                if (Directory.Exists(progessPath))
                {
                    result = new DirectoryModel()
                    {
                        isDirectoryExist = true,
                        DirectoryPath = progessPath
                    };
                }
                else
                {
                    result = new DirectoryModel()
                    {
                        isDirectoryExist = false,
                        DirectoryPath = progessPath
                    };
                }
            }
            return result;
        }

        public FileModel GetBasicFilePath(EnumBasicDirectoryType BasicDirectoryType, string FilePath)
        {
            var result = new FileModel();
            var directoryPath = GetBasicDirectoriesPath(BasicDirectoryType);
            if(directoryPath.isDirectoryExist)
                result = GetFile(directoryPath.DirectoryPath + FilePath);
            return result;
        }

        public FileModel GetFile(string FilePath)
        {
            var result = new FileModel();
            var directoryPath = GetDiretory(Path.GetDirectoryName(FilePath));
            if (directoryPath.isDirectoryExist)
            {
                directoryPath.DirectoryPath += Path.GetFileName(FilePath);
                if (File.Exists(directoryPath.DirectoryPath))
                {
                    result = new FileModel()
                    {
                        isFileExist = true,
                        FilePath = directoryPath.DirectoryPath
                    };
                }
                else
                {
                    result = new FileModel()
                    {
                        isFileExist = false,
                        FilePath = directoryPath.DirectoryPath
                    };
                }
            }
            return result;
        }

        public bool AddTextToFile(string FilePath, string Text)
        {
            var result = false;
            var file = GetFile(FilePath);
            if (file.isFileExist)
            {
                File.AppendAllTextAsync(FilePath, Text);
                result = true;
            }
            else
            {
                var createResult = CreateFile(FilePath, Text);
                result = createResult.isFileExist;
            }
            return result;
        }

        public async void DeleteFile(string FilePath)
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        public async void DeleteDirectory(string DirectoryPath)
        {
            if (Directory.Exists(DirectoryPath))
                Directory.Delete(DirectoryPath);
        }

        public List<FileModel> GetFiles(string DirectoryPath, string fileExtension, DateTime? MinCreationDate = null, DateTime? MaxCreationDate = null)
        {
            var result = new List<FileModel>();
            var directory = GetDiretory(DirectoryPath);
            if (directory.isDirectoryExist)
            {
                var directoryInfo = new DirectoryInfo(directory.DirectoryPath);
                var files = directoryInfo.GetFiles(fileExtension).ToList();
                if (MinCreationDate != null)
                    files.RemoveAll(item => item.CreationTime < MinCreationDate);
                if (MaxCreationDate != null)
                    files.RemoveAll(item => item.CreationTime > MaxCreationDate);
                result = files.Select(item => new FileModel()
                {
                    isFileExist = true,
                    FilePath = item.FullName
                }).ToList();
            }
            return result;
        }

        public List<DirectoryModel> GetDirectories(string DirectoryPath, DateTime? MinCreationDate = null, DateTime? MaxCreationDate = null)
        {
            var result = new List<DirectoryModel>();
            var directory = GetDiretory(DirectoryPath);
            if (directory.isDirectoryExist)
            {
                var directoryInfo = new DirectoryInfo(directory.DirectoryPath);
                var directories = directoryInfo.GetDirectories().ToList();
                if (MinCreationDate != null)
                    directories.RemoveAll(item => item.CreationTime < MinCreationDate);
                if (MaxCreationDate != null)
                    directories.RemoveAll(item => item.CreationTime > MaxCreationDate);
                result = directories.Select(item => new DirectoryModel()
                {
                    isDirectoryExist = true,
                    DirectoryPath = item.FullName
                }).ToList();
            }
            return result;
        }

        public DirectoryModel CreateDirectory(string DirectoryPath)
        {
            var result = new DirectoryModel();
            var directories = DirectoryPath.Split(Path.DirectorySeparatorChar).ToList();
            if (directories.Count > 0)
            {
                var progessPath = "";
                for (int i = 0; i < directories.Count; i++)
                {
                    progessPath += directories[i];
                    if (!Directory.Exists(progessPath))
                        Directory.CreateDirectory(progessPath);
                    progessPath += Path.DirectorySeparatorChar;
                }
            }
            if (Directory.Exists(DirectoryPath))
            {
                result = new DirectoryModel()
                {
                    isDirectoryExist = true,
                    DirectoryPath = DirectoryPath
                };
            }
            else
            {
                result = new DirectoryModel()
                {
                    isDirectoryExist = false,
                    DirectoryPath = DirectoryPath
                };
            }
            return result;
        }

        public FileModel CreateFile(string FilePath, string Text)
        {
            var result = new FileModel()
            {
                isFileExist = false,
                FilePath = FilePath
            };
            var directoryPath = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directoryPath))
                CreateDirectory(directoryPath);
            if (Directory.Exists(directoryPath))
            {
                File.WriteAllText(FilePath, Text);
                if (File.Exists(FilePath))
                {
                    result = new FileModel()
                    {
                        isFileExist = true,
                        FilePath = FilePath
                    };
                }
            }
            return result;
        }
    }
}
