using AnglingClubShared.Enums;

namespace AnglingClubShared.DTOs
{

    public static class TmpFileExtensions
    {
        public static string UploadPath(this TmpFileType value)
        {
            var docPath = "";


            switch (value)
            {
                case TmpFileType.BookPrinting:
                    docPath = "BookPrinting/Uploads";
                    break;

                default:
                    var errMsg = $"TmpFile.UploadPath: Unsupported tmpFile type {value}";
                    var ex = new ArgumentOutOfRangeException(errMsg);
                    throw ex;
            }

            return docPath;

        }

        public static string OutputPath(this TmpFileType value)
        {
            var docPath = "";


            switch (value)
            {
                case TmpFileType.BookPrinting:
                    docPath = "BookPrinting/Output";
                    break;

                default:
                    var errMsg = $"TmpFile.UploadPath: Unsupported tmpFile type {value}";
                    var ex = new ArgumentOutOfRangeException(errMsg);
                    throw ex;
            }

            return docPath;

        }

    }
}
