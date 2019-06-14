namespace AccentMSAddins.Services.Models
{
    using AccentMSAddins.Services.Enum;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("presentationLibrarian")]
    public class FileTag
    {
        [XmlElement("library")]
        public LibraryTagInfo Library;
        [XmlElement("file")]
        public FileTagInfo File;

        public FileTag()
        {
            Library = new LibraryTagInfo();
            File = new FileTagInfo();
        }
    }

    [XmlRoot("presentationLibrarian")]
    public class SlideTag : FileTag
    {
        [XmlElement("slide")]
        public SlideTagInfo Slide;

        public SlideTag()
        {
            Slide = new SlideTagInfo();
        }

        public SlideTag(FileTag fileTag) : this()
        {
            Library.Environment = fileTag.Library.Environment;
            Library.ShortName = fileTag.Library.ShortName;
            File.Id = fileTag.File.Id;
            File.ContentType = fileTag.File.ContentType;
            //File.Name = fileTag.File.Name;
            File.LastModDate = fileTag.File.LastModDate;
        }

        public SlideTag(string environmentName, string libShortName, SlideInfo slideInfo, int vpEditLinkId) : this()
        {
            Library.Environment = environmentName;
            Library.ShortName = libShortName;
            File.Id = slideInfo.FileId;
            File.ContentType = ContentType.File;
            //File.Name = slideInfo.FileName;
            File.LastModDate = ((DateTime)slideInfo.LastModDate).ToLocalTime();
            Slide.Id = slideInfo.SlideId;
            //Slide.PptSlideId = slideInfo.SlidePptId;
            Slide.VpEditLinkId = vpEditLinkId;
            Slide.LastModDate = ((DateTime)slideInfo.LastModDate).ToLocalTime();
        }
    }

    [XmlRoot("presentationLibrarian")]
    public class SlideTagEx : SlideTag
    {
        [XmlIgnore]
        public TagUpdateStatus SlideStatus;
        [XmlIgnore]
        public string SourceFilename;
        [XmlIgnore]
        public int SourceSlideNum;
        [XmlIgnore]
        public DateTime SourceLastModDate;
        [XmlIgnore]
        public string Title;
        [XmlIgnore]
        public int PptSlideIndex;

        public SlideTagEx()
        {
            SlideStatus = TagUpdateStatus.Unknown;
            Library.Environment = string.Empty;
            Library.ShortName = string.Empty;
            SourceFilename = string.Empty;
            Title = string.Empty;
        }

        public SlideTagEx(string environmentName, string libShortName, SlideInfo slideInfo, int vpEditLinkId)
            : base(environmentName, libShortName, slideInfo, vpEditLinkId)
        {
            SlideStatus = TagUpdateStatus.Unknown;
        }
    }

    public enum TagUpdateStatus
    {
        Unknown,
        NoChange, // slide is unchanged in library
        New, // slide added to library
        Updated, // slide updated in library
        MissingOrDeleted, // slide deleted from library
        Added, // library slide added\copied to local copy
        Moved, // library slide moved in local copy
        Local, // local slide only, does not exist in library
    }

    public class LibraryTagInfo
    {
        [XmlAttribute("environment")]
        public string Environment;
        [XmlAttribute("shortName")]
        public string ShortName;
    }

    public class FileTagInfo
    {
        [XmlAttribute("id")]
        public int Id;
        //[XmlAttribute("name")]
        //public string Name;
        [XmlAttribute("lastModDate")]
        public DateTime LastModDate; // this is NOT utc !!!
        [XmlAttribute("contentType")]
        public ContentType ContentType;

        public FileTagInfo()
        {
            Id = -1;
            ContentType = ContentType.File;
        }
    }

    public class SlideTagInfo
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlAttribute("pptSlideId")]
        public int PptSlideId_DoNotUse;
        [XmlAttribute("lastModDate")] // this is NOT utc !!!
        public DateTime LastModDate;
        [XmlAttribute("vpEditLinkId")]
        public int VpEditLinkId;

        public SlideTagInfo()
        {
            Id = -1;
            VpEditLinkId = -1;
        }
    }

    public class SlideTagExCollection : List<SlideTagEx>
    {
        public SlideTagExCollection FindBySlideId(string envName, string libShortName, int slideId)
        {
            SlideTagExCollection slideTags = new SlideTagExCollection();
            foreach (SlideTagEx slideTagEx in this)
            {
                if (string.Compare(slideTagEx.Library.ShortName, libShortName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                    slideTagEx.Slide.Id == slideId)
                {
                    slideTags.Add(slideTagEx);
                }
            }
            return slideTags;
        }

        //public SlideTagEx FindByPptSlideId(string envName, string libShortName, int fileId, int pptSlideId)
        //{
        //    foreach (SlideTagEx slideTagEx in this)
        //    {
        //        if (string.Compare(slideTagEx.Library.Environment, envName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
        //            string.Compare(slideTagEx.Library.ShortName, libShortName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
        //            slideTagEx.File.Id == fileId && 
        //            slideTagEx.Slide.PptSlideId == pptSlideId)
        //        {
        //            return slideTagEx;
        //        }
        //    }
        //    return null;
        //}

        public List<FileTagInfo> GetUniqueFileTagInfoList()
        {
            List<FileTagInfo> fileTagList = new List<FileTagInfo>();
            List<string> fileList = new List<string>();
            foreach (SlideTagEx slideTagEx in this)
            {
                string key = string.Format("{0}.{1}.{2}", slideTagEx.Library.Environment, slideTagEx.Library.ShortName, slideTagEx.File.Id);
                if (!fileList.Contains(key))
                {
                    fileList.Add(key);
                    fileTagList.Add(slideTagEx.File);
                }
            }
            return fileTagList;
        }
    }
}
