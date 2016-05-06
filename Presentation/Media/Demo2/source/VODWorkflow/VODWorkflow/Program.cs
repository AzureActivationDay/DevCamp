//****************************************************************************************
// Microsoft Azure Media Services allows you to build scalable, cost effective, end-to-end
// media distribution solutions that can stream media to Adobe Flash, Android, iOS, Windows,
// and other devices and platforms. If you dont have a Microsoft Azure subscription you can 
// get a FREE trial account here: http://go.microsoft.com/fwlink/?LinkId=330212
//
//
//TODO: 1. Provision a Media Service using the Microsoft Azure Management Portal 
//      http://go.microsoft.com/fwlink/?LinkId=324582 
//      2. Open App.Config and update the value of appSettings:
//					MediaServices-AccountName
//					MediaServices-AccountKey
//					Your-Demo-mp4 (this should point at your *.mp4 input file)
//****************************************************************************************    

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace VODWorkflow
{
	class Program
	{
		private static readonly string SingleInputMp4Path = ConfigurationManager.AppSettings["Your-Demo-mp4"];
		private static readonly string MediaServicesAccountKey = ConfigurationManager.AppSettings["MediaServices-AccountKey"];
		private static readonly string MediaServicesAccountName = ConfigurationManager.AppSettings["MediaServices-AccountName"];

		static void Main()
		{
			try
			{
				MediaServicesCredentials credentials = new MediaServicesCredentials(MediaServicesAccountName, MediaServicesAccountKey);
				CloudMediaContext context = new CloudMediaContext(credentials);

				Console.WriteLine("Creating new asset from local file...");

				// 1. Create a new asset by uploading a mezzanine file from a local path.
				IAsset inputAsset = context.Assets.CreateFromFile(SingleInputMp4Path, AssetCreationOptions.None,
					(af, p) =>
					{
						Console.WriteLine("Uploading '{0}' - Progress: {1:0.##}%", af.Name, p.Progress);
					});

				Console.WriteLine("Asset created.");

				// 2. Prepare a job with a single task to transcode the previous mezzanine asset
				// into a multi-bitrate asset.
				IJob job = context.Jobs.CreateWithSingleTask(MediaProcessorNames.WindowsAzureMediaEncoder,
																										 MediaEncoderTaskPresetStrings.H264AdaptiveBitrateMP4Set720p,
																										 inputAsset,
																										 "Sample Adaptive Bitrate MP4",
																											AssetCreationOptions.None);

				Console.WriteLine("Submitting transcoding job...");

				// 3. Submit the job and wait until it is completed.
				job.Submit();
				job = job.StartExecutionProgressTask(j =>
				{
					Console.WriteLine("Job state: {0}", j.State);
					Console.WriteLine("Job progress: {0:0.##}%", j.GetOverallProgress());
				}, CancellationToken.None).Result;

				Console.WriteLine("Transcoding job finished.");

				IAsset outputAsset = job.OutputMediaAssets[0];

				Console.WriteLine("Publishing output asset...");

				// 4. Publish the output asset by creating an Origin locator for adaptive streaming, 
				// and a SAS locator for progressive download.
				context.Locators.Create(LocatorType.OnDemandOrigin, outputAsset, AccessPermissions.Read, TimeSpan.FromDays(30));
				context.Locators.Create(LocatorType.Sas, outputAsset, AccessPermissions.Read, TimeSpan.FromDays(30));

				IEnumerable<IAssetFile> mp4AssetFiles = outputAsset.AssetFiles.ToList()
																										.Where(af => af.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));

				// 5. Generate the Smooth Streaming, HLS and MPEG-DASH URLs for adaptive streaming, 
				// and the Progressive Download URL.
				Uri smoothStreamingUri = outputAsset.GetSmoothStreamingUri();
				Uri hlsUri = outputAsset.GetHlsUri();
				Uri mpegDashUri = outputAsset.GetMpegDashUri();
				List<Uri> mp4ProgressiveDownloadUris = mp4AssetFiles.Select(af => af.GetSasUri()).ToList();

				// 6. Get the asset URLs.
				Console.WriteLine(smoothStreamingUri);
				Console.WriteLine(hlsUri);
				Console.WriteLine(mpegDashUri);
				mp4ProgressiveDownloadUris.ForEach(Console.WriteLine);

				Console.WriteLine("Output asset available for adaptive streaming and progressive download.");

				Console.WriteLine("VOD workflow finished.");
			}
			catch (Exception exception)
			{
				// Parse the XML error message in the Media Services response and create a new 
				// exception with its content.
				exception = MediaServicesExceptionParser.Parse(exception);

				Console.Error.WriteLine(exception.Message);
			}
			finally
			{
				Console.ReadLine();
			}
		}
	}
}