namespace AAPS.Application.DTO
{
    public class FileExplorerItem
    {
        public string Name { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public bool IsFolder { get; set; }
        public long SizeBytes { get; set; }
        public DateTime LastModified { get; set; }
        public string Extension => IsFolder ? "" : Path.GetExtension(Name).TrimStart('.').ToLower();
        public string SizeDisplay => IsFolder ? "" : FormatSize(SizeBytes);
        public string Icon => GetIcon();

        private static string FormatSize(long bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
        };

        private string GetIcon() => IsFolder ? "folder" : Extension switch
        {
            "pdf" => "picture_as_pdf",
            "png" or "jpg" or "jpeg" or "gif" or "bmp" or "webp" => "image",
            "doc" or "docx" => "description",
            "xls" or "xlsx" or "csv" => "table_chart",
            "ppt" or "pptx" => "slideshow",
            "txt" or "md" => "article",
            "zip" or "rar" or "7z" => "folder_zip",
            "mp4" or "mov" or "avi" => "videocam",
            "mp3" or "wav" => "audio_file",
            _ => "insert_drive_file"
        };

        public bool IsPreviewable => Extension is "pdf" or "png" or "jpg" or "jpeg" or "gif" or "bmp" or "webp" or "txt" or "md";
    }

    public class FileExplorerResult
    {
        public List<FileExplorerItem> Items { get; set; } = new();
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
    }

    public class BreadcrumbItem
    {
        public string Label { get; set; } = "";
        public string RelativePath { get; set; } = "";
    }
}
