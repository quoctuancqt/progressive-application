namespace AccentMSAddins.Services
{
    using AccentMSAddins.Services.Common;
    using AccentMSAddins.Services.Enum;
    using AccentMSAddins.Services.Models;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Presentation;
    using System;
    using System.IO;
    using System.Linq;

    public class TagManagerService : ITagManagerService
    {
        private const string PL_TAG = "PL_TAG";
        private readonly IFilesService _fileService;

        public TagManagerService(IFilesService fileService)
        {
            _fileService = fileService;
        }

        public SlideTagExCollection GetSlideTags(PresentationPart presentationPart)
        {
            TagList tagList = null;
            SlideTagExCollection slideTagList = new SlideTagExCollection();

            foreach (var sldPart in presentationPart.SlideParts)
            {
                var slide = presentationPart.Presentation.SlideIdList.Where(s => ((SlideId)s).RelationshipId == presentationPart.GetIdOfPart(sldPart)).FirstOrDefault();

                int index = presentationPart.Presentation.SlideIdList.ToList().IndexOf(slide);

                foreach (UserDefinedTagsPart tagPart in sldPart.UserDefinedTagsParts)
                {
                    tagList = tagPart.TagList;

                    foreach (Tag tag in tagList.ChildElements)
                    {
                        if (tag.Name.Value.Equals(PL_TAG))
                        {
                            SlideTagEx slideTagEx = null;
                            slideTagEx = XmlUtils.Deserialize<SlideTagEx>(tag.Val.ToString());
                            slideTagEx.PptSlideIndex = ++index;
                            slideTagEx.Title = sldPart.Slide.InnerText;
                            if (slideTagEx != null)
                            {
                                slideTagList.Add(slideTagEx);
                            }
                        }
                    }
                }
            }

            return slideTagList;
        }

        public FileTag GetFileTag(PresentationPart presentationPart)
        {
            FileTag fileTag = null;

            if (presentationPart.UserDefinedTagsPart != null)
            {
                var tagList = presentationPart.UserDefinedTagsPart.TagList;

                foreach (Tag tag in tagList.ChildElements)
                {
                    if (tag.Name.Value.Equals(PL_TAG))
                    {
                        fileTag = XmlUtils.Deserialize<FileTag>(tag.Val.ToString());
                    }
                }
            }

            return fileTag;
        }

        public bool SetTags(CheckUpdateDto data, bool overwriteExistingTags)
        {
            bool setTags = false;

            using (Stream stream = new FileStream(data.FileUrl, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (PresentationDocument openXmlDoc = PresentationDocument.Open(stream, true))
                {
                    var presentationPart = openXmlDoc.PresentationPart;
                    SlideTagExCollection slideTags = GetSlideTags(presentationPart);
                    var libraryFileSlides = _fileService.GetFileSlidesInfo(data);

                    if (overwriteExistingTags || !slideTags.Any())
                    {
                        var fileInfo = _fileService.GetFileInfo(data);

                        FileTag fileTag = new FileTag();
                        fileTag.Library.Environment = data.ServerInfo.EnvironmentName;
                        fileTag.Library.ShortName = data.ServerInfo.ShortLibraryName;
                        fileTag.File.Id = int.Parse(data.FileId);
                        fileTag.File.ContentType = ContentType.File;
                        fileTag.File.LastModDate = fileInfo.LastModDate.Value.ToLocalTime();

                        TagList tagList = new TagList();
                        tagList.Append(new Tag()
                        {
                            Name = PL_TAG,
                            Val = XmlUtils.Serialize(fileTag)
                        });

                        if (presentationPart.UserDefinedTagsPart == null)
                        {
                            presentationPart.AddNewPart<UserDefinedTagsPart>();
                            presentationPart.UserDefinedTagsPart.TagList = tagList;
                        }

                        foreach (var slidePart in presentationPart.SlideParts)
                        {
                            var slideId = presentationPart.Presentation.SlideIdList.Where(s => ((SlideId)s).RelationshipId == presentationPart.GetIdOfPart(slidePart)).Select(s => (SlideId)s).FirstOrDefault();

                            var slideInfo = libraryFileSlides.Slides.Where(x => x.SlidePptId == slideId.Id).FirstOrDefault();

                            SlideTag slideTag = new SlideTag(fileTag);
                            slideTag.Slide.Id = slideInfo.SlideId;
                            slideTag.Slide.LastModDate = ((DateTime)slideInfo.LastModDate).ToLocalTime();

                            Tag slideObjectTag = new Tag() { Name = PL_TAG, Val = XmlUtils.Serialize(slideTag) };
                            UserDefinedTagsPart userDefinedTagsPart1 = slidePart.AddNewPart<UserDefinedTagsPart>();
                            if (userDefinedTagsPart1.TagList == null)
                                userDefinedTagsPart1.TagList = new TagList();

                            userDefinedTagsPart1.TagList.Append(slideObjectTag);

                            var id = slidePart.GetIdOfPart(userDefinedTagsPart1);

                            if (slidePart.Slide.CommonSlideData == null)
                            {
                                slidePart.Slide.CommonSlideData = new CommonSlideData();
                            }
                            if (slidePart.Slide.CommonSlideData.CustomerDataList == null)
                                slidePart.Slide.CommonSlideData.CustomerDataList = new CustomerDataList();
                            CustomerDataTags tags = new CustomerDataTags
                            {
                                Id = id
                            };
                            slidePart.Slide.CommonSlideData.CustomerDataList.AppendChild(tags);

                            slidePart.Slide.Save();
                        }
                    }
                    else if (presentationPart.UserDefinedTagsPart == null)
                    {
                        FileTag fileTag = new FileTag();
                        fileTag.Library.Environment = data.ServerInfo.EnvironmentName;
                        fileTag.Library.ShortName = data.ServerInfo.ShortLibraryName;
                        fileTag.File.Id = -1;
                        fileTag.File.ContentType = ContentType.Undefined;
                        fileTag.File.LastModDate = DateTime.Now;

                        TagList tagList = new TagList();
                        tagList.Append(new Tag()
                        {
                            Name = PL_TAG,
                            Val = XmlUtils.Serialize(fileTag)
                        });

                        if (presentationPart.UserDefinedTagsPart == null)
                        {
                            presentationPart.AddNewPart<UserDefinedTagsPart>();
                            presentationPart.UserDefinedTagsPart.TagList = tagList;
                        }

                        foreach (var slidePart in presentationPart.SlideParts)
                        {
                            var slideId = presentationPart.Presentation.SlideIdList.Where(s => ((SlideId)s).RelationshipId == presentationPart.GetIdOfPart(slidePart)).Select(s => (SlideId)s).FirstOrDefault();

                            var slideInfo = libraryFileSlides.Slides.Where(x => x.SlidePptId == slideId.Id).FirstOrDefault();

                            var index = libraryFileSlides.Slides.IndexOf(slideInfo);

                            SlideTag slideTag = new SlideTag(data.ServerInfo.EnvironmentName, data.ServerInfo.ShortLibraryName, libraryFileSlides.Slides[index], -1);

                            Tag slideObjectTag = new Tag() { Name = PL_TAG, Val = XmlUtils.Serialize(slideTag) };

                            if (slidePart.UserDefinedTagsParts.Count() > 0)
                            {
                                var customerDataPart = slidePart.UserDefinedTagsParts.FirstOrDefault().TagList;

                                customerDataPart.ReplaceChild(slideObjectTag, customerDataPart.FirstChild);
 
                                slidePart.Slide.Save();
                            }
                        }
                    }

                    setTags = true;
                }
            }

            return setTags;
        }
    }
}
