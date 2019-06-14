namespace AccentMSAddins.Services.Models
{
    using AccentMSAddins.Services.Enum;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class LibraryCategoryInfo : IContentInfo
    {
        public int CatId { get; set; }
        public int ParentCatId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CatToWeb { get; set; }
        public CategoryType Type { get; set; }
        public ExtraFields ExtraFields { get; set; }

        public int ContentId { get { return CatId; } }
        public ContentType ContentType { get { return ContentType.Category; } }

        public override string ToString()
        {
            return string.Format("({0}) '{1}'", CatId, Name);
        }

        public string[] Keywords { get; set; }
    }

    public class LibraryCategoryInfoEx : LibraryCategoryInfo
    {
        public int FileCount { get; set; }
    }

    public class LibraryCategoryHeaderInfo : LibraryCategoryInfo
    {
        public int FileCount { get; set; }
        public int CatCount { get; set; }

        public static LibraryCategoryHeaderInfo FromCategoryInfo(LibraryCategoryInfo info, int catCount, int fileCount)
        {
            return new LibraryCategoryHeaderInfo
            {
                Name = info.Name,
                Description = info.Description,
                Type = CategoryType.Generic,
                ParentCatId = info.ParentCatId,
                CatCount = catCount,
                FileCount = fileCount,
            };
        }
    }

    public class LibraryCategoryInfoDescriptor : CollectionDescriptor
    {
        public LibraryCategoryInfoDescriptor()
        {

        }

        public LibraryCategoryInfoDescriptor(IList collection, int index)
            : base(collection, index)
        {
        }

        public override string DisplayName
        {
            get
            {
                LibraryCategoryInfo info = _collection[_index] as LibraryCategoryInfo;
                return info.Name;
            }
        }

        public override object GetValue(object component)
        {
            LibraryCategoryInfo info = _collection[_index] as LibraryCategoryInfo;
            return info.CatId;
        }
    }

    public class LibrarySpecialCategoriesInfo
    {
        public LibrarySpecialCategoriesInfo()
        {
            UserUploads = UserApproved = UserInProgress = UserRejected = UserProposals = -1;
        }

        public LibraryCategoryInfo Uploads { get; set; }
        public LibraryCategoryInfo Downloads { get; set; }
        public LibraryCategoryInfo GlobalFavorites { get; set; }
        public LibraryCategoryInfo Proposals { get; set; }
        public LibraryCategoryInfo System { get; set; }
        public LibraryCategoryInfo EventGroups { get; set; }
        public LibraryCategoryInfo Workflow { get; set; }
        public LibraryCategoryInfo Articles { get; set; }
        public LibraryCategoryInfo DataSource { get; set; }

        public int UserUploads { get; set; }
        public int UserApproved { get; set; }
        public int UserInProgress { get; set; }
        public int UserRejected { get; set; }
        public int UserProposals { get; set; }
    }

    public class LibraryPathInfo : List<LibraryCategoryInfo>
    {
    }

    public class LibraryPathInfoCollection : List<LibraryPathInfo>
    {
        public LibraryCategoryInfo GetCatInfo(int catId)
        {
            foreach (LibraryPathInfo path in this)
            {
                LibraryCategoryInfo catInfo = path.LastOrDefault();
                if (catInfo != null && catId == catInfo.CatId)
                {
                    return catInfo;
                }
            }

            return null;
        }
    }
}
