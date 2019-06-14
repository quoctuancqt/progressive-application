namespace AccentMSAddins.Services.Models
{
    using AccentMSAddins.Services.Common;
    using AccentMSAddins.Services.Enum;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    [Flags]
    public enum FileUpdateFlags
    {
        None = 0,
        Keyword = 1,
        Description = 2,
        Category = 4,
        Slides = 8,
        MetaData = 16,
        FileName = 32,
        NewFile = 64,
    }

    public enum FileSubType
    {
        Undefined = 0,
        Document = 1,
        Image = 2,
        Media = 3,
        Movie = 4,
        Page = 5,
        Presentation = 6,
        Slide = 7,
        Sound = 8,
    }

    [Flags]
    public enum FileFlags
    {
        [XmlEnum("0")]
        None = 0,
        [XmlEnum("1")]
        PasswordProtected = 1,
        [XmlEnum("2")]
        TemplateExists = 2, // dynamic data manager template
    }

    public class LibraryFileInfo : IContentInfo
    {
        public LibraryFileInfo()
        {
        }

        public LibraryFileInfo(LibraryFileInfo other)
        {
            FileId = other.FileId;
            Name = other.Name;
            Title = other.Title;
            Description = other.Description;
            FileSize = other.FileSize;
            LastModDate = other.LastModDate;
            ImportDate = other.ImportDate;
            RecordDate = other.RecordDate;
            UpdateFlags = other.UpdateFlags;
            //Permissions = other.Permissions;
            Flags = other.Flags;
            ImportStatus = other.ImportStatus;
            FileToWeb = other.FileToWeb;
            FileType = other.FileType;

            //if (other.LockInfo != null)
            //{
            //    LockInfo = other.LockInfo.Clone();
            //}
        }

        public int FileId { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; } // User files are prefixed with email address
        public string Title { get; set; }
        public string Description { get; set; }
        public int FileSize { get; set; }
        public DateTime? LastModDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? RecordDate { get; set; }
        public FileUpdateFlags UpdateFlags { get; set; }
        //public Permission Permissions { get; set; }
        public FileFlags Flags { get; set; }
        public int PageCount { get; set; }
        //public ContentLockInfo LockInfo { get; set; }
        public CategoryType CatType { get; set; }
        public int OwnerId { get; set; }

        public int ImportStatus { get; set; }
        public bool FileToWeb { get; set; }
        public int FileType { get; set; }

        public int ContentId { get { return FileId; } }
        public ContentType ContentType { get { return ContentType.File; } }

        public string ThumbnailId { get; set; }

        /// <summary>
        /// Contains information about the file's location(s). Is null unless intentionally populated
        /// </summary>
        //public LibraryPathInfoCollection PathInfo;

        public bool IsUserFile { get { return CatType.IsUserCategory(); } }
        //public bool IsWorkflowFile { get { return CatType.IsWorkflowCategory(); } }
        //public bool IsOnStagedLibOnly { get { return CatType.IsOnStagedLibOnly(); } }

        public string FileTypeExtension
        {
            get { return GetExtension(); }
        }

        public void SetName(string name) { Name = name; }
        public void SetTitle(string title) { Title = title; }

        //public bool CanRead { get { return Permissions.CanRead(); } }
        //public bool CanWrite { get { return Permissions.CanWrite(); } }
        //public bool IsOwner { get { return Permissions.IsOwner(); } }
        //public LibraryFileAttributeCollection Attributes { get; set; }
        //public SlideInfoCollection Slides { get; set; }
        public ExtraFields ExtraFields { get; set; }
        public string[] Keywords { get; set; }

        /// <summary>
        /// Little-Endian byte array of file checksum hash. Used by Little Endians in Admin
        /// </summary>
        public byte[] ChecksumHash { get { return BitConverter.GetBytes(FileHash); } set { FileHash = BitConverter.ToInt64(value, 0); } }

        /// <summary>
        /// The same thing as ChecksumHash only in normal format
        /// </summary>
        public long FileHash { get; set; }

        /// <summary>
        /// Returns file extension without '.'.
        /// </summary>
        /// <returns></returns>
        public string GetExtension()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                string extension = Path.GetExtension(Name);
                if (!string.IsNullOrEmpty(extension))
                {
                    return extension.Trim('.');
                }
            }
            return string.Empty;
        }

        public static LibraryFileInfo GetFromFile(string file, string fileName)
        {
            LibraryFileInfo info = new LibraryFileInfo
            {
                Name = fileName,
                Title = Path.GetFileNameWithoutExtension(fileName)
            };

            if (File.Exists(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                info.FileSize = Math.Max(1, (int)fileInfo.Length / 1024);
                info.LastModDate = fileInfo.LastWriteTime;
                info.ImportDate = DateTime.Now;
            }
            info.FileToWeb = true;
            info.ImportStatus = 1;
            return info;
        }
    }

    public class LibraryFileInfoEx : LibraryFileInfo
    {
        public string FileTypeName { get; set; }
        public FileSubType FileSubType { get; set; }
        public new string FileTypeExtension { get; set; }
    }

    public class LibraryFileResultInfo : LibraryFileInfo
    {
        public const int MAX_FILE_FIELD_COUNT = 18;

        public LibraryFileResultInfo()
        {
            ExtraFields = new ExtraFields();
            ParentCategories = new List<int>();
        }

        public LibraryFileResultInfo(LibraryFileInfo other)
            : base(other)
        {
            ExtraFields = new ExtraFields();
            ParentCategories = new List<int>();
        }

        public LibraryFileResultInfo(LibraryFileResultInfo other)
            : base(other)
        {
            ExtraFields = new ExtraFields(other.ExtraFields);
            ParentCategories = new List<int>(other.ParentCategories);
        }

        public List<int> ParentCategories { get; set; }
    }
}
