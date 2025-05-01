using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildAssetBundles
{
	[MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = "AssetBundles";
		Directory.CreateDirectory(assetBundleDirectory);
		var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.StrictMode, EditorUserBuildSettings.activeBuildTarget);

		// Remove stuff we don't need
		File.Delete(Path.Combine(assetBundleDirectory, Path.GetFileName(assetBundleDirectory)));
		foreach (var file in Directory.GetFiles(assetBundleDirectory, "*.manifest"))
			File.Delete(file);
	}
}