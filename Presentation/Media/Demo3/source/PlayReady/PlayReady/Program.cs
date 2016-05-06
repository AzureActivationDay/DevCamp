using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace PlayReady
{
	class Program
	{
		// Paths to support files (within the above base path). You can use 
		// the provided sample media files from the "SupportFiles" folder, or 
		// provide paths to your own media files below to run these samples.

		// Media Services account information.
		private static readonly string MediaServicesAccountName = ConfigurationManager.AppSettings["MediaServices-AccountName"];
		private static readonly string MediaServicesAccountKey = ConfigurationManager.AppSettings["MediaServices-AccountKey"];

		private static readonly string SingleMp4File = ConfigurationManager.AppSettings["Your-Demo-mp4"];

		// XML Configruation files path.
		private const string ConfigurationXmlFiles = @".\Configurations\";

		private static MediaServicesCredentials cachedCredentials;
		private static CloudMediaContext context;

		static void Main()
		{
			// Create and cache the Media Services credentials in a static class variable.
			cachedCredentials = new MediaServicesCredentials(MediaServicesAccountName, MediaServicesAccountKey);
			// Use the cached credentials to create CloudMediaContext.
			context = new CloudMediaContext(cachedCredentials);

			// Encoding and encrypting assets //////////////////////
			// Load a single MP4 file.
			IAsset asset = IngestSingleMp4File(SingleMp4File, AssetCreationOptions.None);

			// Encode an MP4 file to a set of multibitrate MP4s.
			// Then, package a set of MP4s to clear Smooth Streaming.
			IAsset clearSmoothStreamAsset = ConvertMp4ToMultibitrateMp4SToSmoothStreaming(asset);

			// Encrypt your clear Smooth Streaming to Smooth Streaming with PlayReady.
			IAsset outputAsset = CreateSmoothStreamEncryptedWithPlayReady(clearSmoothStreamAsset);

			// You can use the http://smf.cloudapp.net/healthmonitor player 
			// to test the smoothStreamURL URL.
			Console.WriteLine("Smooth Streaming URL: {0}", outputAsset.GetSmoothStreamingUri());

			// You can use the http://dashif.org/reference/players/javascript/ player 
			// to test the dashURL URL.
			Console.WriteLine("MPEG DASH URL: {0}", outputAsset.GetMpegDashUri());
		}

		/// <summary>
		/// Creates a job with 2 tasks: 
		/// 1 task - encodes a single MP4 to multibitrate MP4s,
		/// 2 task - packages MP4s to Smooth Streaming.
		/// </summary>
		/// <returns>The output asset.</returns>
		public static IAsset ConvertMp4ToMultibitrateMp4SToSmoothStreaming(IAsset asset)
		{
			// Create a new job.
			IJob job = context.Jobs.Create("Convert MP4 to Smooth Streaming.");

			// Add task 1 - Encode single MP4 into multibitrate MP4s.
			IAsset mp4SAsset = EncodeMp4IntoMultibitrateMp4STask(job, asset);
			// Add task 2 - Package a multibitrate MP4 set to Clear Smooth Stream.
			PackageMp4ToSmoothStreamingTask(job, mp4SAsset);

			// Submit the job and wait until it is completed.
			job.Submit();
			job = job.StartExecutionProgressTask(
				j =>
				{
					Console.WriteLine("Job state: {0}", j.State);
					Console.WriteLine("Job progress: {0:0.##}%", j.GetOverallProgress());
				},
				CancellationToken.None).Result;

			// Get the output asset that contains the Smooth Streaming asset.
			return job.OutputMediaAssets[1];
		}

		/// <summary>
		/// Encrypts Smooth Stream with PlayReady.
		/// Then creates a Smooth Streaming Url.
		/// </summary>
		/// <param name="clearSmoothStreamAsset">Asset that contains clear Smooth Streaming.</param>
		/// <returns>The output asset.</returns>
		public static IAsset CreateSmoothStreamEncryptedWithPlayReady(IAsset clearSmoothStreamAsset)
		{
			// Create a job.
			IJob job = context.Jobs.Create("Encrypt to PlayReady Smooth Streaming.");

			// Add task 1 - Encrypt Smooth Streaming with PlayReady 
			EncryptSmoothStreamWithPlayReadyTask(job, clearSmoothStreamAsset);

			// Submit the job and wait until it is completed.
			job.Submit();
			job = job.StartExecutionProgressTask(
				j =>
				{
					Console.WriteLine("Job state: {0}", j.State);
					Console.WriteLine("Job progress: {0:0.##}%", j.GetOverallProgress());
				},
				CancellationToken.None).Result;

			// The OutputMediaAssets[0] contains the desired asset.
			context.Locators.Create(
					LocatorType.OnDemandOrigin,
					job.OutputMediaAssets[0],
					AccessPermissions.Read,
					TimeSpan.FromDays(30));

			return job.OutputMediaAssets[0];
		}

		/// <summary>
		/// Uploads a single file.
		/// </summary>
		/// <param name="fileDir">The location of the files.</param>
		/// <param name="assetCreationOptions">
		///  You can specify the following encryption options for the AssetCreationOptions.
		///      None:  no encryption.  
		///      StorageEncrypted: storage encryption. Encrypts a clear input file 
		///        before it is uploaded to Azure storage. 
		///      CommonEncryptionProtected: for Common Encryption Protected (CENC) files. 
		///        For example, a set of files that are already PlayReady encrypted. 
		///      EnvelopeEncryptionProtected: for HLS with AES encryption files.
		///        NOTE: The files must have been encoded and encrypted by Transform Manager. 
		///     </param>
		/// <returns>Returns an asset that contains a single file.</returns>
		/// <returns></returns>
		private static IAsset IngestSingleMp4File(string fileDir, AssetCreationOptions assetCreationOptions)
		{
			// Use the SDK extension method to create a new asset by 
			// uploading a mezzanine file from a local path.
			IAsset asset = context.Assets.CreateFromFile(
					fileDir,
					assetCreationOptions,
					(af, p) =>
					{
						Console.WriteLine("Uploading '{0}' - Progress: {1:0.##}%", af.Name, p.Progress);
					});

			return asset;
		}

		/// <summary>
		/// Creates a task to encode to Adaptive Bitrate. 
		/// Adds the new task to a job.
		/// </summary>
		/// <param name="job">The job to which to add the new task.</param>
		/// <param name="asset">The input asset.</param>
		/// <returns>The output asset.</returns>
		private static IAsset EncodeMp4IntoMultibitrateMp4STask(IJob job, IAsset asset)
		{
			// Get the SDK extension method to  get a reference to the Azure Media Encoder.
			IMediaProcessor encoder = context.MediaProcessors.GetLatestMediaProcessorByName(
				MediaProcessorNames.WindowsAzureMediaEncoder);

			ITask adpativeBitrateTask = job.Tasks.AddNew(
				"MP4 to Adaptive Bitrate Task",
				encoder,
				"H264 Adaptive Bitrate MP4 Set 720p",
				TaskOptions.None);

			// Specify the input Asset
			adpativeBitrateTask.InputAssets.Add(asset);

			// Add an output asset to contain the results of the job. 
			// This output is specified as AssetCreationOptions.None, which 
			// means the output asset is in the clear (unencrypted).
			IAsset abrAsset = adpativeBitrateTask.OutputAssets.AddNew(
				"Multibitrate MP4s",
				AssetCreationOptions.None);

			return abrAsset;
		}

		/// <summary>
		/// Creates a task to convert the MP4 file(s) to a Smooth Streaming asset.
		/// Adds the new task to a job.
		/// </summary>
		/// <param name="job">The job to which to add the new task.</param>
		/// <param name="asset">The input asset.</param>
		/// <returns>The output asset.</returns>
		private static void PackageMp4ToSmoothStreamingTask(IJob job, IAsset asset)
		{
			// Get the SDK extension method to  get a reference to the Azure Media Packager.
			IMediaProcessor packager = context.MediaProcessors.GetLatestMediaProcessorByName(
					MediaProcessorNames.WindowsAzureMediaPackager);

			// Azure Media Packager does not accept string presets, so load xml configuration
			string smoothConfig = File.ReadAllText(Path.Combine(
									ConfigurationXmlFiles,
									"MediaPackager_MP4toSmooth.xml"));

			// Create a new Task to convert adaptive bitrate to Smooth Streaming.
			ITask smoothStreamingTask = job.Tasks.AddNew("MP4 to Smooth Task",
				 packager,
				 smoothConfig,
				 TaskOptions.None);

			// Specify the input Asset, which is the output Asset from the first task
			smoothStreamingTask.InputAssets.Add(asset);

			// Add an output asset to contain the results of the job. 
			// This output is specified as AssetCreationOptions.None, which 
			// means the output asset is in the clear (unencrypted).
			smoothStreamingTask.OutputAssets.AddNew(
				"Clear Smooth Stream",
				AssetCreationOptions.None);
		}

		/// <summary>
		/// Creates a task to encrypt Smooth Streaming with PlayReady.
		/// Note: To deliver DASH, make sure to set the useSencBox and adjustSubSamples 
		/// configuration properties to true. 
		/// In this example, MediaEncryptor_PlayReadyProtection.xml contains configuration.
		/// </summary>
		/// <param name="job">The job to which to add the new task.</param>
		/// <param name="asset">The input asset.</param>
		/// <returns>The output asset.</returns>
		private static void EncryptSmoothStreamWithPlayReadyTask(IJob job, IAsset asset)
		{
			// Get the SDK extension method to  get a reference to the Azure Media Encryptor.
			IMediaProcessor playreadyProcessor = context.MediaProcessors.GetLatestMediaProcessorByName(
					MediaProcessorNames.WindowsAzureMediaEncryptor);

			// Read the configuration XML.
			//
			// Note that the configuration defined in MediaEncryptor_PlayReadyProtection.xml
			// is using keySeedValue. It is recommended that you do this only for testing 
			// and not in production. For more information, see 
			// http://msdn.microsoft.com/en-us/library/windowsazure/dn189154.aspx.
			//
			string configPlayReady = File.ReadAllText(Path.Combine(ConfigurationXmlFiles,
																	@"MediaEncryptor_PlayReadyProtection.xml"));

			ITask playreadyTask = job.Tasks.AddNew("My PlayReady Task",
				 playreadyProcessor,
				 configPlayReady,
				 TaskOptions.ProtectedConfiguration);

			playreadyTask.InputAssets.Add(asset);

			// Add an output asset to contain the results of the job. 
			// This output is specified as AssetCreationOptions.CommonEncryptionProtected.
			playreadyTask.OutputAssets.AddNew(
				"PlayReady Smooth Streaming",
				AssetCreationOptions.CommonEncryptionProtected);
		}





































		///// <summary>
		///// Update your configuration .xml file dynamically.
		///// </summary>
		//public static void UpdatePlayReadyConfigurationXmlFile(string keyDeliveryServiceUriStr)
		//{
		//	Guid keyId = Guid.NewGuid();
		//	byte[] keyValue = GetRandomBuffer(16);
		//	Uri keyDeliveryServiceUri =
		//			new Uri(keyDeliveryServiceUriStr);

		//	string xmlFileName = Path.Combine(ConfigurationXmlFiles,
		//															@"MediaEncryptor_PlayReadyProtection.xml");

		//	XNamespace xmlns = "http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#";

		//	// Prepare the encryption task template
		//	XDocument doc = XDocument.Load(xmlFileName);

		//	var licenseAcquisitionUrlEl = doc
		//					.Descendants(xmlns + "property")
		//					.Where(p => p.Attribute("name").Value == "licenseAcquisitionUrl")
		//					.FirstOrDefault();
		//	var contentKeyEl = doc
		//					.Descendants(xmlns + "property")
		//					.Where(p => p.Attribute("name").Value == "contentKey")
		//					.FirstOrDefault();
		//	var keyIdEl = doc
		//					.Descendants(xmlns + "property")
		//					.Where(p => p.Attribute("name").Value == "keyId")
		//					.FirstOrDefault();

		//	// Update the "value" property for each element.
		//	if (licenseAcquisitionUrlEl != null)
		//		licenseAcquisitionUrlEl.Attribute("value").SetValue(keyDeliveryServiceUri);

		//	if (contentKeyEl != null)
		//		contentKeyEl.Attribute("value").SetValue(Convert.ToBase64String(keyValue));

		//	if (keyIdEl != null)
		//		keyIdEl.Attribute("value").SetValue(keyId);

		//	doc.Save(xmlFileName);
		//}

		//private static byte[] GetRandomBuffer(int length)
		//{
		//	var returnValue = new byte[length];

		//	using (var rng =
		//			new RNGCryptoServiceProvider())
		//	{
		//		rng.GetBytes(returnValue);
		//	}

		//	return returnValue;
		//}
	}
}