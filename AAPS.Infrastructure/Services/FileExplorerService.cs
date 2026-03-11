using AAPS.Application.Abstractions.Services;
using AAPS.Application.DTO;
using Microsoft.Extensions.Configuration;

namespace AAPS.Infrastructure.Services
{
    public class FileExplorerService : IFileExplorerService
    {
        private readonly string _rootPath;

        // Used by DI when registering with a specific root path (e.g. provider files)
        public FileExplorerService(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new InvalidOperationException("FileExplorerService root path cannot be empty.");
            _rootPath = rootPath;
        }

        // Used by DI when reading from appsettings (general file explorer)
        public FileExplorerService(IConfiguration config)
            : this(config["FileExplorer:RootPath"]
                   ?? throw new InvalidOperationException("FileExplorer:RootPath is not configured in appsettings.json"))
        {
        }

        public bool IsPathSafe(string relativePath)
        {
            // Prevent directory traversal attacks
            var absolute = GetAbsolutePath(relativePath);
            return absolute.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase);
        }

        public string GetAbsolutePath(string relativePath)
        {
            // Clean the path and combine with root
            var clean = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(_rootPath, clean));
        }

        public FileExplorerResult GetContents(string relativePath)
        {
            if (!IsPathSafe(relativePath))
                throw new UnauthorizedAccessException("Access denied.");

            var absolutePath = GetAbsolutePath(relativePath);

            if (!Directory.Exists(absolutePath))
                throw new DirectoryNotFoundException($"Folder not found: {relativePath}");

            var items = new List<FileExplorerItem>();

            // Folders first
            foreach (var dir in Directory.GetDirectories(absolutePath).OrderBy(d => d))
            {
                var info = new DirectoryInfo(dir);
                items.Add(new FileExplorerItem
                {
                    Name = info.Name,
                    RelativePath = GetRelativePath(dir),
                    IsFolder = true,
                    LastModified = info.LastWriteTime
                });
            }

            // Then files
            foreach (var file in Directory.GetFiles(absolutePath).OrderBy(f => f))
            {
                var info = new FileInfo(file);
                items.Add(new FileExplorerItem
                {
                    Name = info.Name,
                    RelativePath = GetRelativePath(file),
                    IsFolder = false,
                    SizeBytes = info.Length,
                    LastModified = info.LastWriteTime
                });
            }

            return new FileExplorerResult
            {
                Items = items,
                Breadcrumbs = BuildBreadcrumbs(relativePath)
            };
        }

        public async Task<string> SaveUploadedFileAsync(string relativePath, string fileName, Stream content)
        {
            if (!IsPathSafe(relativePath))
                throw new UnauthorizedAccessException("Access denied.");

            // Sanitize filename
            var safeName = Path.GetFileName(fileName);
            var destFolder = GetAbsolutePath(relativePath);
            var destPath = Path.Combine(destFolder, safeName);

            // Handle duplicates: file.txt -> file (1).txt
            var counter = 1;
            var nameWithoutExt = Path.GetFileNameWithoutExtension(safeName);
            var ext = Path.GetExtension(safeName);
            while (File.Exists(destPath))
            {
                destPath = Path.Combine(destFolder, $"{nameWithoutExt} ({counter}){ext}");
                counter++;
            }

            using var fs = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            await content.CopyToAsync(fs);

            return Path.GetFileName(destPath);
        }

        public void DeleteFile(string relativePath)
        {
            if (!IsPathSafe(relativePath))
                throw new UnauthorizedAccessException("Access denied.");

            var absolute = GetAbsolutePath(relativePath);
            if (File.Exists(absolute))
                File.Delete(absolute);
        }

        public void DeleteFolder(string relativePath)
        {
            if (!IsPathSafe(relativePath))
                throw new UnauthorizedAccessException("Access denied.");

            var absolute = GetAbsolutePath(relativePath);
            if (Directory.Exists(absolute))
                Directory.Delete(absolute, recursive: true);
        }

        private string GetRelativePath(string absolutePath)
        {
            return Path.GetRelativePath(_rootPath, absolutePath).Replace('\\', '/');
        }

        private List<BreadcrumbItem> BuildBreadcrumbs(string relativePath)
        {
            var crumbs = new List<BreadcrumbItem>
            {
                new() { Label = "Root", RelativePath = "" }
            };

            if (string.IsNullOrWhiteSpace(relativePath)) return crumbs;

            var parts = relativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var accumulated = "";
            foreach (var part in parts)
            {
                accumulated = string.IsNullOrEmpty(accumulated) ? part : $"{accumulated}/{part}";
                crumbs.Add(new() { Label = part, RelativePath = accumulated });
            }

            return crumbs;
        }
    }
}