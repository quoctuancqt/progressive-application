namespace AccentMSAddins.Services
{
    using AccentMSAddins.Services.Common;
    using AccentMSAddins.Services.Constants;
    using AccentMSAddins.Services.Enum;
    using AccentMSAddins.Services.Models;
    using DocumentFormat.OpenXml.Packaging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    public class SlideUpdateManagerService : ISlideUpdateManagerService
    {
        private readonly ITagManagerService _tagManagerService;
        private readonly IFilesService _filesService;
        private readonly ICategoryService _categoryService;
        private bool _downloadLatestVersion = false;
        private bool isExcuted = true;
        private bool isLibraryFile = false;

        public SlideUpdateManagerService(ITagManagerService tagManagerService, IFilesService filesService, ICategoryService categoryService)
        {
            _tagManagerService = tagManagerService;
            _filesService = filesService;
            _categoryService = categoryService;
        }

        public object CheckForUpdates(CheckUpdateDto data)
        {
            bool updatesExist = false;
            bool showConfirmation = true;
            FileTag fileTags = new FileTag();
            SlideTagExCollection slideTags = new SlideTagExCollection();
            LibrarianContentManifest libraryContentManifest = new LibrarianContentManifest();

            using (Stream stream = new FileStream(data.FileUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (PresentationDocument openXmlDoc = PresentationDocument.Open(stream, false))
                {
                    var presentationPart = openXmlDoc.PresentationPart;
                    fileTags = _tagManagerService.GetFileTag(presentationPart);
                    slideTags = _tagManagerService.GetSlideTags(presentationPart);

                    if (fileTags != null)
                    {
                        data.FileId = fileTags.File.Id.ToString();

                        if (data.AskFirst)
                        {
                            if (fileTags != null && fileTags.File.ContentType == ContentType.File)
                            {
                                return new CheckUpdateResponseDto()
                                {
                                    FileId = data.FileId,
                                    StatusCode = (HttpStatusCode)CheckUpdateStatusCode.HasNewVersion,
                                    StatusText = System.Enum.GetName(typeof(CheckUpdateStatusCode), CheckUpdateStatusCode.HasNewVersion)
                                };
                            }
                            else
                            {
                                return new CheckUpdateResponseDto()
                                {
                                    FileId = data.FileId,
                                    StatusCode = (HttpStatusCode)CheckUpdateStatusCode.CheckUpdate,
                                    StatusText = System.Enum.GetName(typeof(CheckUpdateStatusCode), CheckUpdateStatusCode.CheckUpdate)
                                };
                            }
                        }

                        if (data.IsConfirm)
                        {
                            updatesExist = CheckFileForUpdates(fileTags, slideTags, data);

                            if (!isExcuted)
                            {
                                return new CheckUpdateResponseDto()
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    StatusText = Message.ExcuteFailed
                                };
                            }

                            if (slideTags.Count > 0)
                            {
                                _downloadLatestVersion = true;
                                return new
                                {
                                    FileId = data.FileId,
                                    StatusCode = 215,
                                    Value = UpdateListOfSlides(slideTags)
                                };
                            }

                            updatesExist = slideTags.Count > 0;
                            showConfirmation = updatesExist;
                        }
                        else
                        {
                            showConfirmation = false;
                        }
                    }
                    else if (slideTags != null && slideTags.Count > 0)
                    {
                        data.FileId = slideTags.FirstOrDefault().File.Id.ToString();

                        if (data.AskFirst)
                        {
                            return new CheckUpdateResponseDto()
                            {
                                FileId = data.FileId,
                                StatusCode = (HttpStatusCode)CheckUpdateStatusCode.CheckUpdate,
                                StatusText = System.Enum.GetName(typeof(CheckUpdateStatusCode), CheckUpdateStatusCode.CheckUpdate)
                            };
                        }

                        if (data.IsConfirm)
                        {
                            updatesExist = CheckSlidesForUpdates(slideTags, false, data);

                            if (!isExcuted)
                            {
                                return new CheckUpdateResponseDto()
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    StatusText = Message.ExcuteFailed
                                };
                            }

                            if (slideTags.Count > 0)
                            {
                                _downloadLatestVersion = true;
                                return new
                                {
                                    FileId = data.FileId,
                                    StatusCode = 215,
                                    Value = UpdateListOfSlides(slideTags)
                                };
                            }

                            updatesExist = slideTags.Count > 0;
                            showConfirmation = updatesExist;
                        }
                        else
                        {
                            showConfirmation = false;
                        }
                    }
                    else
                    {
                        libraryContentManifest = XmlUtils.Deserialize<LibrarianContentManifest>(_filesService.GetCustomXmlPart(presentationPart));

                        if (libraryContentManifest != null)
                        {
                            data.FileName = libraryContentManifest.Filename;
                            data.FileId = libraryContentManifest.ContentId;

                            if (data.AskFirst)
                            {
                                return new CheckUpdateResponseDto()
                                {
                                    FileName = data.FileName,
                                    FileId = data.FileId,
                                    StatusCode = (HttpStatusCode)CheckUpdateStatusCode.HasNewVersion,
                                    StatusText = System.Enum.GetName(typeof(CheckUpdateStatusCode), CheckUpdateStatusCode.HasNewVersion)
                                };
                            }

                            updatesExist = CheckManifestForUpdates(libraryContentManifest, data);

                            if (!isExcuted)
                            {
                                return new CheckUpdateResponseDto()
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    StatusText = Message.ExcuteFailed
                                };
                            }
                        }
                        else
                        {
                            showConfirmation = false;
                        }
                    }
                }
            }

            if (showConfirmation && !updatesExist)
            {
                return new CheckUpdateResponseDto()
                {
                    FileId = data.FileId,
                    StatusCode = (HttpStatusCode)CheckUpdateStatusCode.UptoDate,
                    StatusText = System.Enum.GetName(typeof(CheckUpdateStatusCode), CheckUpdateStatusCode.UptoDate)
                };
            }

            if (updatesExist)
            {
                if (fileTags != null && _downloadLatestVersion)
                {
                    _filesService.GetLatestFile(data);
                }
                else if (slideTags != null && slideTags.Count > 0)
                {
                    return new
                    {
                        FileId = data.FileId,
                        StatusCode = 215,
                        Value = UpdateListOfSlides(slideTags)
                    };
                }
                else if (libraryContentManifest != null)
                {
                    return new CheckUpdateResponseDto()
                    {
                        FileId = libraryContentManifest.ContentId,
                        StatusCode = (HttpStatusCode)CheckUpdateStatusCode.UpdatedAvailable,
                        StatusText = System.Enum.GetName(typeof(CheckUpdateStatusCode), CheckUpdateStatusCode.UpdatedAvailable)
                    };
                }
            }

            return updatesExist;
        }

        public bool VerifyFile(CheckUpdateDto data)
        {
            FileTag fileTags = new FileTag();
            SlideTagExCollection slideTags = new SlideTagExCollection();
            LibrarianContentManifest libraryContentManifest = new LibrarianContentManifest();

            using (Stream stream = new FileStream(data.FileUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (PresentationDocument openXmlDoc = PresentationDocument.Open(stream, false))
                {
                    var presentationPart = openXmlDoc.PresentationPart;
                    fileTags = _tagManagerService.GetFileTag(presentationPart);
                    slideTags = _tagManagerService.GetSlideTags(presentationPart);
                    libraryContentManifest = XmlUtils.Deserialize<LibrarianContentManifest>(_filesService.GetCustomXmlPart(presentationPart));

                    if (fileTags != null)
                    {
                        return true;
                    }

                    if (slideTags.Count > 0)
                    {
                        return true;
                    }

                    if (libraryContentManifest != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //PRIVATE METHOD
        private bool CheckFileForUpdates(FileTag fileTag, SlideTagExCollection slideTags, CheckUpdateDto fileInfo)
        {
            bool updatesExist = false;

            if (fileTag != null)
            {
                updatesExist = CheckContentForUpdates(fileTag.File.ContentType, fileTag.File.LastModDate.ToUniversalTime(), fileInfo);
            }

            if (slideTags != null)
            {
                updatesExist |= CheckSlidesForUpdates(slideTags, isLibraryFile, fileInfo);
            }

            return updatesExist;
        }

        private bool CheckSlidesForUpdates(SlideTagExCollection slideTags, bool isLibraryFile, CheckUpdateDto fileInfo)
        {
            bool updatesExist = false;

            for (int x = 0; x < slideTags.Count; x++)
            {
                SlideTagEx slideTagEx = slideTags[x];
                if (slideTagEx.SlideStatus == TagUpdateStatus.Unknown)
                {
                    LibraryFileSlides libraryFileSlides = null;
                    try
                    {
                        if (fileInfo.FileId == "-1")
                        {
                            fileInfo.FileId = slideTagEx.File.Id.ToString();
                        }

                        libraryFileSlides = _filesService.GetFileSlidesInfo(fileInfo);
                    }
                    catch (Exception ex)
                    {
                        isExcuted = false;
                        return updatesExist;
                    }

                    if (libraryFileSlides != null)
                    {
                        for (int i = 0; i < libraryFileSlides.Slides.Count; i++)
                        {
                            SlideInfo librarySlideInfo = libraryFileSlides.Slides[i];
                            SlideTagExCollection findSlideTags;
                            findSlideTags = slideTags.FindBySlideId(fileInfo.ServerInfo.EnvironmentName, fileInfo.ServerInfo.ShortLibraryName, librarySlideInfo.SlideId);

                            if (findSlideTags.Any())
                            {
                                int movedCount = 0;
                                for (int z = 0; z < findSlideTags.Count; z++)
                                {
                                    SlideTagEx foundSlideTag = findSlideTags[z];
                                    if (isLibraryFile && foundSlideTag.PptSlideIndex != librarySlideInfo.SlideNumber)
                                    {
                                        if (z == 0 && movedCount == 0)
                                        {
                                            foundSlideTag.SlideStatus = TagUpdateStatus.Moved;
                                            ++movedCount;
                                        }
                                        else
                                        {
                                            foundSlideTag.SlideStatus = TagUpdateStatus.Added;
                                        }
                                    }
                                    else if (librarySlideInfo.LastModDate <= foundSlideTag.Slide.LastModDate.ToUniversalTime())
                                    {
                                        foundSlideTag.SlideStatus = TagUpdateStatus.NoChange;
                                    }
                                    else
                                    {
                                        foundSlideTag.SlideStatus = TagUpdateStatus.Updated;
                                        updatesExist = true;
                                    }
                                    foundSlideTag.SourceFilename = librarySlideInfo.FileName;
                                    foundSlideTag.SourceSlideNum = librarySlideInfo.SlideNumber;
                                    foundSlideTag.SourceLastModDate = (DateTime)librarySlideInfo.LastModDate;
                                }
                            }
                            else if (isLibraryFile)
                            {
                                SlideTagEx newSlideTag = new SlideTagEx(fileInfo.ServerInfo.EnvironmentName, fileInfo.ServerInfo.ShortLibraryName, librarySlideInfo, -1)
                                {
                                    SlideStatus = TagUpdateStatus.New,
                                    Title = librarySlideInfo.Title,
                                    SourceFilename = librarySlideInfo.FileName,
                                    SourceSlideNum = librarySlideInfo.SlideNumber,
                                    SourceLastModDate = (DateTime)librarySlideInfo.LastModDate
                                };
                                slideTags.Insert(i, newSlideTag);
                                updatesExist = true;
                            }
                        }
                    }

                    foreach (SlideTagEx nextSlideTagEx in slideTags)
                    {
                        if (nextSlideTagEx.SlideStatus == TagUpdateStatus.Unknown)
                        {
                            if (nextSlideTagEx.File.Id == slideTagEx.File.Id &&
                                nextSlideTagEx.Library.ShortName == slideTagEx.Library.ShortName &&
                                nextSlideTagEx.Library.Environment == slideTagEx.Library.Environment)
                            {
                                if (isLibraryFile)
                                {
                                    nextSlideTagEx.SlideStatus = TagUpdateStatus.Added;
                                }
                                else
                                {
                                    updatesExist = true;
                                    nextSlideTagEx.SlideStatus = TagUpdateStatus.MissingOrDeleted;
                                    if (libraryFileSlides != null && libraryFileSlides.Slides.Any())
                                    {
                                        nextSlideTagEx.SourceFilename = libraryFileSlides.Slides[0].FileName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return updatesExist;
        }

        private bool CheckManifestForUpdates(LibrarianContentManifest librarianContentManifest, CheckUpdateDto fileInfo)
        {
            bool updatesExist = CheckContentForUpdates(librarianContentManifest.ContentType, DateTime.Parse(librarianContentManifest.LastModDate), fileInfo);

            if (!updatesExist && (librarianContentManifest.ContentType == ContentType.GlobalFavorite || librarianContentManifest.ContentType == ContentType.MyFavorite))
            {
                foreach (var contentItem in librarianContentManifest.Items)
                {
                    LibraryFileInfoEx libraryFileInfo = _filesService.GetFileInfo(fileInfo);
                    updatesExist = libraryFileInfo.LastModDate > DateTime.Parse(contentItem.LastModDate);

                    //if (!updatesExist && contentItem.EditItem != null)
                    //{
                    //    SlidePreviewRequestEx slidePreviewRequestEx = session.LibrarianRpc.GetVirtualPresentationItemEdits(contentItem.EditItem.LinkId, contentItem.SlideId);
                    //    updatesExist = slidePreviewRequestEx.LastModDate > contentItem.EditItem.LastModDate;
                    //}

                    if (updatesExist) break; // exit loop
                }
            }

            return updatesExist;
        }

        private bool CheckContentForUpdates(ContentType contentType, DateTime lastModDateUtc, CheckUpdateDto fileInfo)
        {
            bool updatesExist = false;

            if (contentType == ContentType.File)
            {
                LibraryFileInfoEx libraryFileInfoEx = _filesService.GetFileInfo(fileInfo);
                if (libraryFileInfoEx == null)
                {
                    isExcuted = false;
                    return updatesExist;
                }
                updatesExist = libraryFileInfoEx.LastModDate > lastModDateUtc;

                LibraryCategoryInfo libraryCategoryInfo = Utils.GetCategoryInfoForFile(_categoryService, fileInfo, libraryFileInfoEx.FileId, !libraryFileInfoEx.IsUserFile);
                isLibraryFile = !libraryCategoryInfo.Type.IsUserCategory();
            }
            else
            {
                if (contentType == ContentType.GlobalFavorite)
                {
                    //ContentGroupInfo contentGroupInfo = da.LibrarianRpc.GetContentGroupInfo(contentId);
                    //updatesExist = contentGroupInfo.LastModDate > lastModDateUtc;
                }
                else if (contentType == ContentType.MyFavorite)
                {
                    //VirtualPresentation virtualPresentation = session.LibrarianRpc.GetVirtualPresentation(contentId);
                    //updatesExist = virtualPresentation.CreatedDate > lastModDateUtc;
                }
            }

            return updatesExist;
        }

        private List<SlideUpdatedResultDto> UpdateListOfSlides(SlideTagExCollection slideTags)
        {
            var result = new List<SlideUpdatedResultDto>();

            if (slideTags != null)
            {
                for (int i = 0; i < slideTags.Count; i++)
                {
                    SlideTagEx slideTagEx = slideTags[i];
                    string sourceFilename = ClientUtils.UndecorateFileName(slideTagEx.SourceFilename);
                    string status = string.Empty;
                    string lastModDate = slideTagEx.SourceLastModDate.ToString(Message.DateFormat);
                    string source = string.Format("{0} - Slide {1}", sourceFilename, slideTagEx.SourceSlideNum);
                    switch (slideTagEx.SlideStatus)
                    {
                        case TagUpdateStatus.Updated:
                            status = "Updated";
                            break;
                        case TagUpdateStatus.New:
                            status = "New";
                            break;
                        case TagUpdateStatus.MissingOrDeleted:
                            status = "Deleted";
                            lastModDate = string.Empty;
                            source = sourceFilename;
                            break;
                        case TagUpdateStatus.NoChange:
                            status = "No change";
                            break;
                        case TagUpdateStatus.Added:
                            status = "Added";
                            lastModDate = string.Empty;
                            source = string.Empty;
                            break;
                        case TagUpdateStatus.Moved:
                            status = "Moved";
                            break;
                        case TagUpdateStatus.Local:
                            lastModDate = string.Empty;
                            source = string.Empty;
                            break;
                    }


                    bool isUpdated = Helper.IsSlideUpdated(slideTagEx.SlideStatus);

                    result.Add(new SlideUpdatedResultDto
                    {
                        Tag = slideTagEx,
                        PptSlideIndex = slideTagEx.PptSlideIndex == 0 ? slideTagEx.SourceSlideNum : slideTagEx.PptSlideIndex,
                        Status = status,
                        LastModDate = lastModDate,
                        Source = source,
                        Id = slideTagEx.Slide.Id.ToString(),
                        IsUpdated = isUpdated
                    });
                }
            }

            return result.OrderBy(x => x.PptSlideIndex).ToList();
        }
    }
}
