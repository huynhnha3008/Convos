using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Services
{
    public class FirebaseService
    {
        private readonly StorageClient _storageClient;
        private const string BucketName = "convoshub.appspot.com";

        public FirebaseService(IConfiguration configuration)
        {
            var serviceAccountKeyPath = configuration["Firebase:ServiceAccountKeyPath"];

            if (string.IsNullOrWhiteSpace(serviceAccountKeyPath) || !File.Exists(serviceAccountKeyPath))
            {
                throw new FileNotFoundException("Firebase service account key file not found.", serviceAccountKeyPath);
            }

            var credential = GoogleCredential.FromFile(serviceAccountKeyPath);
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadAvatarAsync(Stream imageStream, string fileName)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string contentType = GetContentType(fileName);

            try
            {
                var storageObject = await _storageClient.UploadObjectAsync(
                    BucketName,
                    $"avatars/{uniqueFileName}",
                    contentType,
                    imageStream
                );

                
                storageObject.Acl = new[] { new Google.Apis.Storage.v1.Data.ObjectAccessControl
                    {
                        Entity = "allUsers",
                        Role = "READER"
                    }
                };

                await _storageClient.UpdateObjectAsync(storageObject);

                
                return $"https://storage.googleapis.com/{BucketName}/avatars/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                
                throw new InvalidOperationException("Error uploading file to Firebase Storage.", ex);
            }
        }

        private string GetContentType(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                _ => "application/octet-stream",
            };
        }
    }
}
