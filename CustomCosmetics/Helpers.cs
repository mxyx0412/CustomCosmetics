using AmongUs.Data;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Reflection;
using UnityEngine;

namespace CustomCosmetics;

internal static class Helpers
{
    public static bool IsCN()
    {
        return (int)DataManager.Settings.Language.CurrentLanguage == 13;
    }

    public static string GithubUrl(this string url)
    {
        if (IsCN() && (url.Contains("github.com") || url.Contains("githubusercontent.com")) && !url.Contains("ghfast.top"))
            return "https://ghfast.top/" + url;
        return url;
    }

    public static unsafe Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(path);
            var length = stream!.Length;
            var byteTexture = new Il2CppStructArray<byte>(length);
            _ = stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
            if (ImageConversion.LoadImage(texture, byteTexture, false) && texture.width > 0 && texture.height > 0)
                return texture;
        }
        catch { }
        return null;
    }
}
