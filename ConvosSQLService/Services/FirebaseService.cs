using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

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

        public async Task<string> UploadQuizAsync(Stream imageStream, string fileName)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string contentType = GetContentType(fileName);

            try
            {
                var storageObject = await _storageClient.UploadObjectAsync(
                    BucketName,
                    $"Quiz/{uniqueFileName}",
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


                return $"https://storage.googleapis.com/{BucketName}/Quiz/{uniqueFileName}";
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
                ".mp3" => "audio/mpeg",
                ".m4a" => "audio/x-m4a",

                ".json" => "application/json",

                _ => "application/octet-stream",
            };
        }
        public async Task<string> UploadAudioAsync(Stream audioStream, string fileName)
        {
            if (!IsSupportedAudioFile(fileName))
            {
                throw new InvalidOperationException("Only MP3 and M4A files are allowed.");
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string contentType = GetContentType(fileName);  

            try
            {
                var storageObject = await _storageClient.UploadObjectAsync(
                    BucketName,
                    $"audio/{uniqueFileName}",
                    contentType,
                    audioStream,
                    new UploadObjectOptions
                    {
                        PredefinedAcl = PredefinedObjectAcl.PublicRead
                    }
                );

                return $"https://storage.googleapis.com/{BucketName}/audio/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error uploading MP3 file to Firebase Storage.", ex);
            }
        }

        private bool IsSupportedAudioFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension == ".mp3" || extension == ".m4a";

        }
    }
}