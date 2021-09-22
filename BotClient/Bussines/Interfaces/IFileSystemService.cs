using BotClient.Models.FileSystem;
using BotClient.Models.FileSystem.Enumerators;
using System;
using System.Collections.Generic;

namespace BotClient.Bussines.Interfaces
{
    public interface IFileSystemService
    {
        DirectoryModel GetBasicDirectoriesPath(EnumBasicDirectoryType BasicDirectoryType);
        DirectoryModel GetDiretory(string DirectoryPath);
        FileModel GetBasicFilePath(EnumBasicDirectoryType BasicDirectoryType, string FilePath);
        FileModel GetFile(string FilePath);
        bool AddTextToFile(string FilePath, string Text);
        void DeleteFile(string FilePath);
        void DeleteDirectory(string DirectoryPath);
        List<FileModel> GetFiles(string DirectoryPath, string fileExtension, DateTime? MinCreationDate = null, DateTime? MaxCreationDate = null);
        List<DirectoryModel> GetDirectories(string DirectoryPath, DateTime? MinCreationDate = null, DateTime? MaxCreationDate = null);
        DirectoryModel CreateDirectory(string DirectoryPath);
        FileModel CreateFile(string FilePath, string Text);
    }
}