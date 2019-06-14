namespace AccentMSAddins.Services.Controllers
{
    using AccentMSAddins.Services.Common;
    using AccentMSAddins.Services.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IO;
    using System.Net;

    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private readonly IFilesService _fileServices;
        private readonly ISlideUpdateManagerService _slideUpdateManagerService;
        private readonly ITagManagerService _tagManagerService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public FilesController(IFilesService fileService, ISlideUpdateManagerService slideUpdateManagerService, IHostingEnvironment hostingEnvironment, ITagManagerService tagManagerService)
        {
            _fileServices = fileService;
            _slideUpdateManagerService = slideUpdateManagerService;
            _hostingEnvironment = hostingEnvironment;
            _tagManagerService = tagManagerService;
        }

        [HttpPost]
        [Route("getfile")]
        public IActionResult GetFile([FromBody] CheckUpdateDto data)
        {
            var result = _fileServices.GetLatestFile(data);

            if (string.IsNullOrEmpty(result))
            {
                return NotFound();
            }

            data.FileUrl = result;

            if (!data.IsOverwrite)
            {
                _fileServices.InsertCustomXml(data);

            }
            else
            {
                _tagManagerService.SetTags(data, false);

            }

            return Ok(Path.Combine(_hostingEnvironment.ContentRootPath, result));
        }

        [HttpGet]
        [Route("openfile")]
        public IActionResult OpenFile(string webClientUrl, string shortLibraryName, string authHash, string fileId, string fileName)
        {
            Stream streamResult = _fileServices.OpenFile(webClientUrl, shortLibraryName, authHash, fileId);

            Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);

            return File(streamResult, MimeTypes.GetMimeType(fileName));
        }

        [HttpPost]
        [Route("checkupdate")]
        public IActionResult CheckUpdate([FromBody] CheckUpdateDto data)
        {
            var result = _slideUpdateManagerService.CheckForUpdates(data);

            return Ok(result);
        }

        [HttpPost]
        [Route("verifyfile")]
        public IActionResult VerifyFile([FromBody] CheckUpdateDto data)
        {
            return Ok(_slideUpdateManagerService.VerifyFile(data));
        }

        [HttpGet]
        [Route("getthumbnail")]
        public string GetThumbnail(string webClienUrl, string shortLibraryName, string authHash, string sizeMode, string fileId, string slideNumber)
        {
            Stream reader = null;
            byte[] bytedata;
            var contentType = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{webClienUrl}{shortLibraryName}/Download.ashx?mode=thmb&tname={sizeMode}&fid={fileId}&snum={slideNumber}&useweb=True?out=xml");
            request.Method = "GET";
            request.Headers.Add("Accept-Encoding: gzip, deflate");
            request.Headers.Add("Accept-Language: en-US");
            request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            request.Headers.Add("Authorization", authHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;


            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                contentType = response.ContentType;

                reader = Helper.ReadResponseAsStream(response);

                bytedata = Helper.ReadStream(reader);

                if (reader != null) reader.Close();
            }

            return $"data:{contentType};base64,{Convert.ToBase64String(bytedata)}";
        }
    }
}
