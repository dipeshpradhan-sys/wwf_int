

namespace wwfpp.Helpers
{
    /** 
     * Since 2026-Jul-05
     * jpg, jpeg, png, gif, bmp
     * doc, docx, 
     * xls, xlsx, 
     * ppt, pptx
     * pdf
     * image/jpeg, image/png, image/gif, image/bmp
     * application/msword,  application/vnd.openxmlformats-officedocument.wordprocessingml.document
     * application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
     * application/vnd.ms-powerpoint, application/vnd.openxmlformats-officedocument.presentationml.presentation
     * application/pdf
     * if (!FileValidator.ForPdf(file)) { return BadRequest("Only PDF files are allowed.");}
     * if (!FileValidator.ForImages(file)) { return BadRequest("Only image files are allowed."); }
     * if (!FileValidator.ForMSOffice(file)) { return BadRequest("Only MSoffice files are allowed."); }
     * ***/
    public static class FileValidator
    {
        public static bool ForCsv(IFormFile file)
        {
            if (file == null) return false;

            var ext = Path.GetExtension(file.FileName);
            if (!ext.Equals(".csv", StringComparison.OrdinalIgnoreCase)) return false;

            if (!(file.ContentType == "text/csv" || file.ContentType == "application/vnd.ms-excel"))
                return false;

            using var reader = new StreamReader(file.OpenReadStream());
            string firstLine = reader.ReadLine();
            return firstLine != null && firstLine.Contains(",");
        }

        public static bool ForPdf(IFormFile file)
        {
            if (file == null || file.Length == 0) { return false; }

            var allowedExt = new[] { ".pdf" };
            var allowedMime = new[] { "application/pdf" };
            return IsValidFile(file, allowedExt, allowedMime);
        }
        public static bool ForImages(IFormFile file)
        {
            if (file == null || file.Length == 0) { return false; }

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var allowedMime = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            return IsValidFile(file, allowedExt, allowedMime);
        }

        public static bool ForMSOffice(IFormFile file)
        {
            if (file == null || file.Length == 0) { return false; }

            var allowedExt = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
            var allowedMime = new[]
            {
                "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            };
            return IsValidFile(file, allowedExt, allowedMime);
        }
        public static bool ForImagesWithPdf(IFormFile file)
        {
            if (file == null || file.Length == 0) { return false; }

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
            var allowedMime = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "application/pdf" };
            return IsValidFile(file, allowedExt, allowedMime);
        }
        public static bool ForMSOfficeWithPdf(IFormFile file)
        {
            if (file == null || file.Length == 0) { return false; }

            var allowedExt = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".pdf" };
            var allowedMime = new[]
            {
                "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "application/pdf"
            };
            return IsValidFile(file, allowedExt, allowedMime);
        }

        public static bool IsValidFile(IFormFile file, string[] allowedExtensions, string[] allowedMimeTypes)
        {
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext)) { return false; }
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant())) { return false; }

            return HasValidSignature(file, ext);
        }

        private static bool HasValidSignature(IFormFile file, string ext)
        {
            using (var stream = file.OpenReadStream())
            {
                byte[] header = new byte[8];
                stream.Read(header, 0, header.Length);

                if (ext is ".jpg" or ".jpeg") { return header[0] == 0xFF && header[1] == 0xD8; }
                if (ext == ".png") { return header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47; }
                if (ext == ".gif") { return header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38; }
                if (ext == ".bmp") { return header[0] == 0x42 && header[1] == 0x4D; }
                if (ext == ".pdf") { return header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46; }

                if (ext is ".doc" or ".xls" or ".ppt") { return header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0; }
                if (ext is ".docx" or ".xlsx" or ".pptx") { return header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04; }

                return false;
            }
        }
    }
}
