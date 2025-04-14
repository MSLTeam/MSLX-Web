using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace MSLX.Core.Utils;

public class FileHelper
{
    public static bool CheckFileSha256(string filePath, string expectedSha256)
    {
        if (!File.Exists(filePath)) return false;
        using (var fileStream = File.OpenRead(filePath))
        {
            using (var sha256 = SHA256.Create())
            {
                var fileHash = sha256.ComputeHash(fileStream);
                var fileHashString = BitConverter.ToString(fileHash).Replace("-", "").ToLowerInvariant();
                return fileHashString == expectedSha256.ToLowerInvariant();
            }
        }
    }
    
    public static void ExtractZip(string archivePath, string extractPath)
    {
        using (var zipStream = new ZipInputStream(File.OpenRead(archivePath)))
        {
            ZipEntry entry;
            while ((entry = zipStream.GetNextEntry()) != null)
            {
                var entryPath = Path.Combine(extractPath, entry.Name);
                if (entry.IsDirectory)
                    Directory.CreateDirectory(entryPath);
                else
                    using (var streamWriter = File.Create(entryPath))
                    {
                        zipStream.CopyTo(streamWriter);
                    }
            }
        }

        MoveContentsIfSingleFolder(extractPath);
    }

    public static void ExtractTarGz(string archivePath, string extractPath)
    {
        using (var fs = File.OpenRead(archivePath))
        using (var gzipStream = new GZipInputStream(fs))
        using (var tarStream = new TarInputStream(gzipStream))
        {
            TarEntry entry;
            while ((entry = tarStream.GetNextEntry()) != null)
            {
                var entryPath = Path.Combine(extractPath, entry.Name);
                if (entry.IsDirectory)
                    Directory.CreateDirectory(entryPath);
                else
                    using (var streamWriter = File.Create(entryPath))
                    {
                        tarStream.CopyEntryContents(streamWriter);
                    }
            }
        }

        MoveContentsIfSingleFolder(extractPath);
    }

    // 提取文件夹
    public static void MoveContentsIfSingleFolder(string extractPath)
    {
        var directories = Directory.GetDirectories(extractPath);
        if (directories.Length == 1)
        {
            var singleFolder = directories[0];
            foreach (var file in Directory.GetFiles(singleFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = file.Substring(singleFolder.Length + 1);
                var destinationPath = Path.Combine(extractPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                File.Move(file, destinationPath);
            }

            // 清空文件夹
            Directory.Delete(singleFolder, true);
        }
        
    }
    
    // 设置可执行权限
    public static void SetFileExecutable(string filePath)
    {
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"chmod +x {filePath}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine(result);
    }
}