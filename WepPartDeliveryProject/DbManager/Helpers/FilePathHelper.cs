using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Helpers
{
    public class FilePathHelper
    {
        public static string PathToDirWithDish(string pathToPublicClientAppDirectory, string dirWithDishImages, string categoryLink, string dishId)
        {
            var pathToDishesDir = Path.Combine(pathToPublicClientAppDirectory, dirWithDishImages);
            var pathToCategoryDir = Path.Combine(pathToDishesDir, categoryLink);
            var pathToDishDir = Path.Combine(pathToCategoryDir, dishId);

            return pathToDishDir;
        }

        public static string ConvertFromIOPathToInternetPath_DirWithDish(string pathToPublicClientAppDirectory, string pathToImage)
        {
            pathToImage = pathToImage
                .Replace(pathToPublicClientAppDirectory, "")
                .Replace('\\', '/');

            return Path.Combine("/", pathToImage);
        }
    }
}
