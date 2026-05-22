using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetUtil
{
    private static readonly string _generatedPathGuid = "6385f8da0e893d142aaaef7ed709f4bd";
    private static int _batchDepth;

    private static string GeneratedPathRoot
    {
        get
        {
            var path = AssetDatabase.GUIDToAssetPath(_generatedPathGuid);
            if (string.IsNullOrEmpty(path))
            {
                throw new DirectoryNotFoundException(
                    $"OneClickInventory generated asset folder was not found. GUID: {_generatedPathGuid}");
            }

            return path;
        }
    }

    private static void AcquireDirectory(string path)
    {
        var directoryPath = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(directoryPath))
        {
            throw new DirectoryNotFoundException($"Invalid asset path: {path}");
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            if (_batchDepth == 0)
            {
                AssetDatabase.Refresh();
            }
        }
    }

    public static void BeginBatchEdit()
    {
        if (_batchDepth == 0)
        {
            AssetDatabase.StartAssetEditing();
        }

        _batchDepth++;
    }

    public static void EndBatchEdit()
    {
        if (_batchDepth == 0) return;

        _batchDepth--;
        if (_batchDepth != 0) return;

        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static string GetPath(string key)
    {
        var assetPath = $"{GeneratedPathRoot}/{key}";
        AcquireDirectory(assetPath);
        return assetPath;
    }

    public static void CreateAsset(Object asset, string key)
    {
        var path = GetPath(key);
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(asset, path);
    }

    public static string GetEmptyPath(string key)
    {
        var path = GetPath(key);
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        return path;
    }

    public static string GetPersistentPath(string key)
    {
        var assetPath = $"Assets/Inventory/{key}";
        AcquireDirectory(assetPath);
        return assetPath;
    }

    public static void ClearGeneratedAssets()
    {
        var generatedPathRoot = GeneratedPathRoot;
        if (Directory.Exists(generatedPathRoot))
        {
            Directory.Delete(generatedPathRoot, true);
        }

        Directory.CreateDirectory(generatedPathRoot);
        File.WriteAllBytes(generatedPathRoot + "/dummy", new byte[] { });
        AssetDatabase.Refresh();
    }
}
