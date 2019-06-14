namespace AccentMSAddins.Services.Common
{
    using AccentMSAddins.Services.Enum;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public static class ClientUtils
    {
        public static int[] GetIntArray(string value)
        {
            List<int> list = new List<int>();
            if (!string.IsNullOrEmpty(value))
            {
                string[] values = value.Split(',');
                foreach (string s in values)
                {
                    list.Add(GetInt(s, -1));
                }
            }
            return list.ToArray();
        }

        public static string GetString(object value, string defaultValue)
        {
            if (value != null && value != DBNull.Value)
            {
                return value.ToString();
            }
            return defaultValue;
        }

        public static string GetString(object value)
        {
            return GetString(value, string.Empty);
        }

        public static int GetInt(string value, int defaultValue)
        {
            int i = 0;
            if (int.TryParse(value, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static T GetEnum<T>(string value, T defaultValue) where T : struct
        {
            if (System.Enum.TryParse(value, out T result))
                return result;

            return defaultValue;
        }

        public static int GetInt(object value, int defaultValue)
        {
            return GetInt(GetString(value), defaultValue);
        }

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

        public static string GetValidFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, ' ');
            }
            while (fileName.IndexOf("  ") != -1)
            {
                fileName = fileName.Replace("  ", " ");
            }
            return fileName;
        }

        public static string GetUrlFileName(string fileName, string userAgent)
        {
            if (userAgent != null)
            {
                string ua = userAgent.ToLowerInvariant();
                if (!ua.Contains("firefox") && !ua.Contains("opera") && !ua.Contains("applewebkit"))
                    fileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8).Replace('+', ' ');
            }
            return fileName;
        }

        public static string UndecorateFileName(string name)
        {
            // Remove user email address in front of file name
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            int pos = name.IndexOf('@');
            if (pos < 0)
                return name;
            pos = name.IndexOf('_', pos);
            if (pos < 0)
                return name;
            return name.Substring(pos + 1);
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

        public static string QuoteName(string name)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -_]");
            return string.Format("[{0}]", rgx.Replace(name, ""));
        }

        public static string NormalizeCharacters(string value, Dictionary<string, string> replacements = null)
        {
            if (replacements == null)
            {
                replacements = new Dictionary<string, string>
                {
                    // Non-breaking space
                    { "\u00A0", " " },
                    
                    // Dashes (e.g. em, en)
                    { @"\p{Pd}", "-" },

                    // Curly Quotes
                    { "[\u2018-\u201b]", "\'" },
                    { "[\u201c-\u201f]", "\"" }
                };
            }

            foreach (var replacement in replacements)
            {
                Regex regex = new Regex(replacement.Key);
                value = regex.Replace(value, replacement.Value);
            }

            return value;
        }
    }
}
