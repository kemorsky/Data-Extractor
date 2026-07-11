// namespace DataExtractor.Tool.Services;

// using Mutagen.Bethesda;
// using Mutagen.Bethesda.Skyrim;
// using Mutagen.Bethesda.Plugins.Cache;
// using System;
// using System.Linq;
// using System.Collections.Generic;
// using Google.Apis.Services;
// using Google.Apis.Sheets.v4;
// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Download;
// using Google.Apis.Drive.v3;
// using DataExtractor.Dto;

// public class DownloadEsm
// {
//     public MemoryStream DownloadFromDrive()
//     {
//         try
//         {
//             var fileId = Environment.GetEnvironmentVariable("DRIVE-ID");
            
//             var credential = GoogleCredential.GetApplicationDefault();
//             credential = credential.CreateScoped(DriveService.Scope.Drive);
            
//             var driveService = new DriveService(new BaseClientService.Initializer
//             {
//                 HttpClientInitializer = credential,
//                 ApplicationName = "Drive API Snippets"
//             });

//             var fileRequest = driveService.Files.Get(fileId);
//             var stream = new MemoryStream();

//             fileRequest.MediaDownloader.ProgressChanged +=
//                 progress =>
//                 {
//                     switch (progress.Status)
//                     {
//                         case DownloadStatus.Downloading:
//                         {
//                             Console.WriteLine(progress.BytesDownloaded);
//                             break;
//                         }
//                         case DownloadStatus.Completed:
//                         {
//                             Console.WriteLine("Download complete.");
//                             break;
//                         }
//                         case DownloadStatus.Failed:
//                         {
//                             Console.WriteLine("Download failed.");
//                             break;
//                         }
//                     }
//                 };
//             fileRequest.Download(stream);

//             return stream;
//         } catch (Exception e)
//         {
//             if (e is AggregateException)
//             {
//                 Console.WriteLine("Credential Not found");
//             }
//             else
//             {
//                 throw;
//             }
//         };
//         return null;
//     }
// };