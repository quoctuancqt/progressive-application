using AccentMSAddins.Services.Common;
using AccentMSAddins.Services.Models;
using System.Net;

namespace AccentMSAddins.Services
{
    public class CategoryService : ICategoryService
    {
        public string GetFileCatPath(CheckUpdateDto data, bool useWeb)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{data.ServerInfo.WebClientUrl}{data.ServerInfo.ShortLibraryName}/Default.ashx?m=getpath&ftype=0&cid=-1&fid={data.FileId}&useweb={useWeb}&out=xml");
            request.Method = "GET";
            request.Headers.Add("Authorization", data.ServerInfo.AuthHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = Helper.ReadResponseAsStream(response);
                return Helper.DeserializeResponse<string>(responseStream);
            }
        }

        public LibraryCategoryInfo GetLibraryCategoryInfo(CheckUpdateDto data, int catId, bool useWeb, bool isIncludeFile = false)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{data.ServerInfo.WebClientUrl}{data.ServerInfo.ShortLibraryName}/Default.ashx?m=getci&cid={catId}&useweb={useWeb}&out=xml");
            request.Method = "GET";
            request.Headers.Add("Authorization", data.ServerInfo.AuthHash);
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = Helper.ReadResponseAsStream(response);
                return Helper.DeserializeResponse<LibraryCategoryInfo>(responseStream);
            }
        }
    }
}
