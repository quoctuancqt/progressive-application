namespace AccentMSAddins.Services.Common
{
    using AccentMSAddins.Services.Enum;
    using AccentMSAddins.Services.Models;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;

    public class Helper
    {
        public static Type GetType(string typeName)
        {
            Type type = null;
            string[] values = typeName.Split('.');
            if (values.Length > 2)
            {
                Assembly assembly = AppDomain.CurrentDomain.Load(values[0] + "." + values[1]);
                if (assembly != null)
                {
                    type = assembly.GetType(typeName, false, true);
                }
            }
            if (type == null)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly == null || string.IsNullOrEmpty(assembly.FullName) || assembly.FullName.StartsWith("System", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    type = assembly.GetType(typeName, false, true);
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            return type;
        }

        public static LibraryFileType GetFileType(string fileName)
        {
            LibraryFileType fileType = LibraryFileType.None;
            if (!string.IsNullOrEmpty(fileName))
            {
                string ext = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(ext))
                {
                    switch (ext.ToLower())
                    {
                        case ".doc":
                        case ".docx":
                            fileType = LibraryFileType.Word;
                            break;

                        case ".xls":
                        case ".xlsx":
                            fileType = LibraryFileType.Excel;
                            break;

                        case ".ppt":
                        case ".pptx":
                            fileType = LibraryFileType.PowerPoint;
                            break;

                        case ".jpg":
                        case ".png":
                        case ".jpeg":
                        case ".bmp":
                        case ".gif":
                            fileType = LibraryFileType.Image;
                            break;

                        case ".txt":
                            fileType = LibraryFileType.Text;
                            break;

                        case ".pdf":
                            fileType = LibraryFileType.Pdf;
                            break;

                        default:
                            fileType = LibraryFileType.Other;
                            break;
                    }
                }
            }
            return fileType;
        }

        public static void CreateFolder(ref string pathString, ref string fileUrl)
        {
            pathString = @"Accent Technologies";

            fileUrl = $"wwwroot/{pathString}";

            if (!Directory.Exists(fileUrl))
            {
                Directory.CreateDirectory(fileUrl);
            }
        }

        public static void DeleteFile(string pathString)
        {
            if (File.Exists(pathString))
            {
                File.Delete(pathString);
            }
        }

        public static T DeserializeResponse<T>(Stream stream, Output output = Output.Default)
        {
            string response = ReadResponseAsString(stream);
            if (output == Output.xml)
            {
                return (T)DeserializeXmlResponse<T>(response);
            }
            else
            {
                return (T)DeserializeJsonResponse<T>(response);
            }
        }

        public static T DeserializeXmlResponse<T>(string xml)
        {
            T responseType = default(T);
            responseType = (T)XmlUtils.Deserialize<T>(xml);
            return responseType;
        }

        public static T DeserializeJsonResponse<T>(string json)
        {
            T responseType = default(T);
            responseType = JsonConvert.DeserializeObject<T>(json);
            //responseType = (T)XmlUtils.Deserialize<T>(xml);
            return responseType;
        }

        public static string ReadResponseAsString(Stream stream, bool closeStream = true)
        {
            string xmlReponse = string.Empty;
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    xmlReponse = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (closeStream)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return xmlReponse;
        }

        public static Stream ReadResponseAsStream(HttpWebResponse response)
        {
            Stream reponseStream = null;
            using (Stream stream = response.GetResponseStream())
            {
                reponseStream = new MemoryStream();
                stream.CopyTo(reponseStream);
                reponseStream.Position = 0;
            }
            return reponseStream;
        }

        public static bool IsSlideUpdated(TagUpdateStatus status)
        {
            return !(status == TagUpdateStatus.NoChange || status == TagUpdateStatus.Added || status == TagUpdateStatus.Moved || status == TagUpdateStatus.Local);
        }

        public static byte[] ReadStream(Stream stream)
        {
            byte[] buffer = new byte[1024000];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static bool WriteLog(string strMessage, string strFileName="log")
        {
            try
            {
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", "wwwroot", strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
