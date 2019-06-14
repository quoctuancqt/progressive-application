namespace AccentMSAddins.Services
{
    using AccentMSAddins.Services.Common;
    using AccentMSAddins.Services.Enum;
    using AccentMSAddins.Services.Models;
    using DocumentFormat.OpenXml.Packaging;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    public class FilesService : IFilesService
    {
        public void InsertCustomXml(CheckUpdateDto data)
        {
            var stringData = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{data.ServerInfo.WebClientUrl}{data.ServerInfo.ShortLibraryName}/Default.ashx?m=getfi&fid={data.FileId}&useweb=True&out=json");
            request.Method = "GET";
            request.Headers.Add("Authorization", data.ServerInfo.AuthHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                stringData = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }

            var dataObject = JObject.Parse(stringData);
            var lastModDate = dataObject.SelectToken("LastModDate").Value<string>();

            var librarianContentManifest = new LibrarianContentManifest
            {
                CatType = CategoryType.Generic,
                ContentId = data.FileId,
                ContentType = ContentType.File,
                DateCreatedUTC = DateTime.UtcNow.ToString("o"),
                Filename = data.FileName,
                LastModDate = DateTime.Parse(lastModDate).ToString("o"),
                LibraryName = data.ServerInfo.ShortLibraryName,
                LiveDb = true,
                Server = data.ServerInfo.EnvironmentName.Replace(" ", ""),
                PageCount = dataObject.SelectToken("PageCount").Value<int>(),
                Items = new List<ContentItem>()
                {
                    new ContentItem {
                        ContentType = ContentType.File,
                        FileId = data.FileId,
                        LastModDate = DateTime.Parse(lastModDate).ToString("o"),
                        PageCount =  dataObject.SelectToken("PageCount").Value<int>(),
                        ParentContentId = data.FileId,
                        ParentContentType = ContentType.File,
                        ParentItemIndex = -1,
                        ParentLastModDate = DateTime.Parse(lastModDate).ToString("o"),
                        SlideId = -1,
                        SlideNumber = -1
                    }
                }
            };

            var result = Utils.InsertCustomXml(data.FileUrl, XmlUtils.Serialize(librarianContentManifest));
        }

        public LibraryFileInfoEx GetFileInfo(CheckUpdateDto data)
        {
            LibraryFileInfoEx libraryFileInfoEx = new LibraryFileInfoEx();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{data.ServerInfo.WebClientUrl}{data.ServerInfo.ShortLibraryName}/Default.ashx?m=getfi&fid={data.FileId}&useweb=true?out=xml");
            request.Method = "GET";
            request.Headers.Add("Authorization", data.ServerInfo.AuthHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    var responseStream = Helper.ReadResponseAsStream(response);
                    var responseXMLString = Helper.ReadResponseAsString(responseStream);
                    libraryFileInfoEx = Helper.DeserializeXmlResponse<LibraryFileInfoEx>(responseXMLString);
                }
            }
            catch(Exception ex)
            {
                return null;
            }
            

            return libraryFileInfoEx;
        }

        public LibraryFileSlides GetFileSlidesInfo(CheckUpdateDto data)
        {
            LibraryFileSlides libraryFileSlides = new LibraryFileSlides();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{data.ServerInfo.WebClientUrl}{data.ServerInfo.ShortLibraryName}/Default.ashx?m=getslides&fid={data.FileId}&useweb=true?out=xml");
            request.Method = "GET";
            request.Headers.Add("Authorization", data.ServerInfo.AuthHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = Helper.ReadResponseAsStream(response);
                var responseXMLString = Helper.ReadResponseAsString(responseStream);
                libraryFileSlides = Helper.DeserializeXmlResponse<LibraryFileSlides>(responseXMLString);
            }

            return libraryFileSlides;
        }

        public string GetLatestFile(CheckUpdateDto data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{data.ServerInfo.WebClientUrl}{data.ServerInfo.ShortLibraryName}/Download.ashx?mode=dnld&fid={data.FileId}&useweb=false&out=xml");
            request.Method = "GET";
            request.Headers.Add("Accept-Encoding: gzip, deflate");
            request.Headers.Add("Accept-Language: en-US");
            request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            request.Headers.Add("Authorization", data.ServerInfo.AuthHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                return SaveFile(response, $"{data.FileName}");
            }
        }

        public string GetCustomXmlPart(PresentationPart presentationPart)
        {
            var result = string.Empty;

            var customXMLPart = presentationPart.CustomXmlParts.LastOrDefault();
            if (customXMLPart != null)
            {
                var data = customXMLPart.GetStream();
                result = Helper.ReadResponseAsString(data);
            }
            
            return result;
        }

        public Stream OpenFile(string webClientUrl, string shortLibraryName, string authHash, string fileId)
        {
            MemoryStream streamResult = new MemoryStream();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{webClientUrl}{shortLibraryName}/Download.ashx?mode=dnld&fid={fileId}&useweb=true&out=json");
            request.Method = "GET";
            request.Headers.Add("Accept-Encoding: gzip, deflate");
            request.Headers.Add("Accept-Language: en-US");
            request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            request.Headers.Add("Authorization", authHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    stream.CopyTo(streamResult);
                    stream.Close();
                    streamResult.Position = 0;
                }
            }

            return streamResult;
        }

        private string SaveFile(HttpWebResponse response, string fileName)
        {
            string location = string.Empty;
            string fileUrl = string.Empty;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        stream.CopyTo(ms);
                        ms.Position = 0;
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    Helper.CreateFolder(ref location, ref fileUrl);

                    fileUrl = Path.Combine(fileUrl, fileName);

                    Helper.DeleteFile(fileUrl);

                    using (FileStream fs = new FileStream(fileUrl, FileMode.OpenOrCreate))
                    {
                        ms.CopyTo(fs);
                        fs.Flush();
                    }
                }

                return fileUrl;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
