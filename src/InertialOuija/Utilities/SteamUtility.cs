extern alias GameScripts;

using Steamworks;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace InertialOuija.Utilities;

internal static class SteamUtility
{
	// Everything about these APIs is horrid

	public static Task<T> AsTask<T>(this SteamAPICall_t call)
	{
		call.Validate();

		// If the async call is already completed before creating the CallResult<T>, it will never signal completion
		// Is this a bug in the Steamworks C# wrapper? No idea how to use these APIs safely and without race conditions
		// Checking SteamUtils.IsAPICallCompleted can have false positives

		var callResult = new CallResult<T>();
		var taskSource = new TaskCompletionSource<T>(callResult, TaskCreationOptions.RunContinuationsAsynchronously);
		callResult.Set(call, (result, failure) =>
		{
			if (failure)
				taskSource.SetException(new SteamApiException($"Steamworks API call error: {SteamUtils.GetAPICallFailureReason(call)}"));
			else
				taskSource.SetResult(result);
			callResult.Dispose();
		});

		return taskSource.Task;
	}

	public static SteamAPICall_t Validate(this SteamAPICall_t call, [CallerArgumentExpression(nameof(call))] string expression = null)
	{
		if (call == SteamAPICall_t.Invalid)
			throw new SteamApiException($"Steamworks API call failed.\nExpression: {expression}");
		return call;
	}

	public static void Validate(this EResult result)
	{
		if (result is not (EResult.k_EResultOK or EResult.k_EResultAdministratorOK))
			throw new SteamApiException($"Steamworks API error: {result}");
	}

	public static async Task<byte[]> ReadFileAsync(string file)
	{
		if (!SteamRemoteStorage.FileExists(file))
			return null;

		int size = SteamRemoteStorage.GetFileSize(file);
		if (size == 0)
			return []; // API docs say reading 0 bytes will cause an error

		var read = await SteamRemoteStorage.FileReadAsync(file, 0, checked((uint)size))
			.Validate()
			.AsTask<RemoteStorageFileReadAsyncComplete_t>()
			.ConfigureAwait(false);
		read.m_eResult.Validate();

		var buffer = new byte[read.m_cubRead];
		if (!SteamRemoteStorage.FileReadAsyncComplete(read.m_hFileReadAsync, buffer, read.m_cubRead))
			throw new SteamApiException("File read could not be completed");
		return buffer;
	}

	public static byte[] ReadFile(string file)
	{
		if (!SteamRemoteStorage.FileExists(file))
			return null;

		int size = SteamRemoteStorage.GetFileSize(file);
		if (size == 0)
			return []; // API docs say reading 0 bytes will cause an error

		var buffer = new byte[size];
		var read = SteamRemoteStorage.FileRead(file, buffer, size);

		if (read == 0)
			throw new SteamApiException($"File read failed\nFile: {file}");
		if (read < size)
			throw new SteamApiException($"Read less data than expected ({read} < {size})\\nFile: {file}\"");

		return buffer;
	}
}

public class SteamApiException(string message) : Exception(message)
{ }
