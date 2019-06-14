namespace AccentMSAddins.Services.Common
{
    using AccentMSAddins.Services.Enum;
    using AccentMSAddins.Services.Models;
    using DocumentFormat.OpenXml.CustomXmlDataProperties;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Presentation;
    using System;
    using System.IO;
    using System.Linq;

    public class Utils
    {
        public static bool InsertCustomXml(string filename, string customXML)
        {
            bool result = filename.ToLower().EndsWith("x");

            if (result)
            {
                switch (Helper.GetFileType(filename))
                {
                    case LibraryFileType.PowerPoint:
                        InsertCustomXmlForPpt(filename, customXML);
                        break;
                    default:
                        result = false;
                        break;
                }
            }
            return result;
        }

        public static void InsertCustomXmlForPpt(string pptFileName, string customXML)
        {
            using (PresentationDocument openXmlDoc = PresentationDocument.Open(pptFileName, true))
            {
                // Create a new custom XML part
                int count = openXmlDoc.PresentationPart.Parts.Count();
                string customXmlId = string.Format("rId{0}", count + 1);
                CustomXmlPart customXmlPart = openXmlDoc.PresentationPart.AddCustomXmlPart(CustomXmlPartType.CustomXml, customXmlId);
                using (Stream outputStream = customXmlPart.GetStream())
                {
                    using (StreamWriter ts = new StreamWriter(outputStream))
                    {
                        ts.Write(customXML);
                    }
                }

                // Add datastore so that the xml will persist after the document is modified
                count = customXmlPart.Parts.Count();
                CustomXmlPropertiesPart customXmlPropertiesPart = customXmlPart.AddNewPart<CustomXmlPropertiesPart>(string.Format("rId{0}", count + 1));
                DataStoreItem dataStoreItem = new DataStoreItem() { ItemId = string.Format("{0}", Guid.NewGuid()) };
                dataStoreItem.AddNamespaceDeclaration("ds", "http://schemas.openxmlformats.org/officeDocument/2006/customXml");

                SchemaReferences schemaReferences = new SchemaReferences();
                SchemaReference schemaReference = new SchemaReference() { Uri = "http://www.w3.org/2001/XMLSchema" };

                schemaReferences.Append(schemaReference);
                dataStoreItem.Append(schemaReferences);
                customXmlPropertiesPart.DataStoreItem = dataStoreItem;

                // Add the xml to the customer data section of the document
                CustomerData customerData = new CustomerData() { Id = customXmlId };
                if (openXmlDoc.PresentationPart.Presentation.CustomerDataList != null)
                {
                    // Sequence matters: http://www.schemacentral.com/sc/ooxml/e-p_custDataLst-1.html
                    if (openXmlDoc.PresentationPart.Presentation.CustomerDataList.Count() > 0)
                    {
                        openXmlDoc.PresentationPart.Presentation.CustomerDataList.InsertBefore(customerData, openXmlDoc.PresentationPart.Presentation.CustomerDataList.FirstChild);
                    }
                    else
                    {
                        openXmlDoc.PresentationPart.Presentation.CustomerDataList.Append(customerData);
                    }
                }
                else
                {
                    CustomerDataList customerDataList = new CustomerDataList();
                    customerDataList.Append(customerData);

                    // Sequence matters: http://www.schemacentral.com/sc/ooxml/e-p_presentation.html
                    if (openXmlDoc.PresentationPart.Presentation.Kinsoku != null)
                        openXmlDoc.PresentationPart.Presentation.InsertBefore(customerDataList, openXmlDoc.PresentationPart.Presentation.Kinsoku);
                    else if (openXmlDoc.PresentationPart.Presentation.DefaultTextStyle != null)
                        openXmlDoc.PresentationPart.Presentation.InsertBefore(customerDataList, openXmlDoc.PresentationPart.Presentation.DefaultTextStyle);
                    else if (openXmlDoc.PresentationPart.Presentation.ModificationVerifier != null)
                        openXmlDoc.PresentationPart.Presentation.InsertBefore(customerDataList, openXmlDoc.PresentationPart.Presentation.ModificationVerifier);
                    else if (openXmlDoc.PresentationPart.Presentation.PresentationExtensionList != null)
                        openXmlDoc.PresentationPart.Presentation.InsertBefore(customerDataList, openXmlDoc.PresentationPart.Presentation.PresentationExtensionList);
                    else
                        openXmlDoc.PresentationPart.Presentation.Append(customerDataList);
                }
            }
        }

        public static LibraryCategoryInfo GetCategoryInfoForFile(ICategoryService categoryService, CheckUpdateDto data, int fileId, bool useWeb)
        {
            int catId = GetCategoryIdForFile(categoryService, data, fileId, useWeb);
            LibraryCategoryInfo libraryCategoryInfo = categoryService.GetLibraryCategoryInfo(data, catId, useWeb, false);
            return libraryCategoryInfo;
        }

        public static int GetCategoryIdForFile(ICategoryService categoryService, CheckUpdateDto data, int fileId, bool useWeb)
        {
            int catId = -1;
            string catPath = categoryService.GetFileCatPath(data, useWeb);
            string[] slashes = catPath.Split('/');
            if (slashes.Length >= 2)
            {
                catId = Convert.ToInt32(slashes[slashes.Length - 2].Substring(1));
            }
            return catId;
        }
    }
}
