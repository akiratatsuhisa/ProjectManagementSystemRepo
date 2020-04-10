using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Helpers
{
    public static class FormFileValidation
    {
        public static bool IsValidFileSizeLimit(IFormFile file, long fileSizeLimit) => file != null && file.Length > 0 && file.Length < fileSizeLimit;

        public static string GetFileExtension(string fileName) => Path.GetExtension(fileName).ToLowerInvariant();

        #region validate file extention
        private static readonly string[] _permittedExtensions = {
            ".png", ".jpg", ".jpeg", ".gif", ".bmp",
            ".zip", ".7z", ".rar",
            ".txt", ".pdf", ".csv", ".xml",
            ".docx", ".doc",
            ".xlsx", ".xls",
            ".pptx", "ppt",
        };

        private static readonly string[] _excelPermittedExtensions = { ".xlsx", ".xls" };

        public static bool IsValidFileExtension(string fileName, out string extension)
        {
            extension = GetFileExtension(fileName);
            return IsValidFileExtension(extension);
        }

        public static bool IsValidAvatarFileExtension(this IFormFile file, out string extension)
        {
            extension = GetFileExtension(file.FileName);
            return IsValidFileExtension(extension);
        }

        public static bool IsValidFileExtension(string extension) => !string.IsNullOrEmpty(extension) && _permittedExtensions.Contains(extension);

        public static bool IsValidExcelFileExtension(string extension) => !string.IsNullOrEmpty(extension) && _excelPermittedExtensions.Contains(extension);
        #endregion

        #region validate file signature
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            {
                ".png",
                new List<byte[]> {
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                }
            },
            {
                ".jpg",
                new List<byte[]> {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            {
                ".jpeg",
                new List<byte[]> {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            {
                ".gif",
                new List<byte[]> {
                    new byte[] { 0x47, 0x49, 0x46, 0x38 },
                }
            },
            {
                ".bmp",
                new List<byte[]> {
                    new byte[] { 0x42, 0x4D },
                }
            },
        };

        public static bool IsValidFileSignature(IFormFile file, out string extension)
        {
            extension = GetFileExtension(file.FileName);
            return IsValidFileSignature(file, extension);
        }

        public static bool IsValidFileSignature(IFormFile file) => IsValidFileSignature(file, GetFileExtension(file.FileName));

        public static bool IsValidFileSignature(IFormFile file, string extension)
        {
            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                var signatures = _fileSignature[extension];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
        #endregion
    }
}
