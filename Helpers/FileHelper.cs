using Microsoft.AspNetCore.Http;

namespace AnilUniversity.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveFileAsync(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0)
                return "";

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Convert physical folder to web path
            string folderName = new DirectoryInfo(folderPath).Name;

            return $"/Uploads/{folderName}/{fileName}";
        }
    }
}