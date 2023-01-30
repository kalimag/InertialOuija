extern alias GameScripts;

using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using GameScripts.Assets.Source.GhostCars;
using GameScripts.Assets.Source.SaveData;

namespace InertialOuija.Ghosts;

public class ExternalGhost
{
	private const uint FileSignature = 0x01_474449;
	private static readonly ThreadLocal<BinaryFormatter> Serializer = new(CreateSerializer);



	public ExternalGhostInfo Info { get; }
	public IGhostRecording Recording { get; }



	public ExternalGhost(ExternalGhostInfo info, IGhostRecording recording)
	{
		Info = info;
		Recording = recording;
	}



	public void Save(Stream stream)
	{
		if (Recording is null)
			throw new InvalidOperationException("Attempted to save header-only ExternalGhost.");

		using (var writer = new BinaryWriter(stream, Encoding.Default, true))
		{
			writer.Write(FileSignature);
		}

		using var compressedStream = new GZipStream(stream, CompressionMode.Compress, true);
		{
			Serializer.Value.Serialize(compressedStream, Info);
			Serializer.Value.Serialize(compressedStream, Recording);
		}
	}

	private static (ExternalGhostInfo info, IGhostRecording recording) LoadInternal(Stream stream, bool infoOnly = false)
	{
		using (var reader = new BinaryReader(stream, Encoding.Default, true))
		{
			uint signature = reader.ReadUInt32();
			if (signature != FileSignature)
				throw new InvalidDataException($"Ghost file does not have expected signature.");
		}

		ExternalGhostInfo info;
		IGhostRecording recording = null;
		using (var decompressedStream = new GZipStream(stream, CompressionMode.Decompress))
		{
			info = (ExternalGhostInfo)Serializer.Value.Deserialize(decompressedStream);
			if (!infoOnly)
				recording = (IGhostRecording)Serializer.Value.Deserialize(decompressedStream);
		}

		return (info, recording);
	}


	public static ExternalGhost Load(Stream stream)
	{
		var (info, recording) = LoadInternal(stream, false);
		return new ExternalGhost(info, recording);
	}
	public static ExternalGhost Load(string path)
	{
		using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		return Load(stream);
	}


	public static ExternalGhostInfo LoadInfo(Stream stream)
	{
		(var info, _) = LoadInternal(stream, true);
		return info;
	}
	public static ExternalGhostInfo LoadInfo(string path)
	{
		using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		return LoadInfo(stream);
	}


	private static BinaryFormatter CreateSerializer()
	{
		var serializer = new BinaryFormatter();
		serializer.Binder = new GhostSerializationBinder();
		SaveHelpers.AddUnitySerialisationSurrogates(serializer);
		return serializer;
	}
}
