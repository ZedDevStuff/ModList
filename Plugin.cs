using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ModList;
using Mono.Cecil;
using MonoMod.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace ModList
{
    [BepInPlugin("com.zeddevstuff.modlist", "ModList", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static List<ModEntry> Mods = new();
        private static AssetBundle? Bundle;
        internal static GameObject? ModmenuPrefab, ModCardPrefab, ModsButton;
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        private void Start()
        {
            if(Mods.Count != 0) return;
            var plugins = gameObject.GetComponents<BaseUnityPlugin>();
            Bundle = AssetBundle.LoadFromMemory(Resource1.modlist);
            ModmenuPrefab = Bundle.LoadAsset<GameObject>("ModMenu");
            ModCardPrefab = Bundle.LoadAsset<GameObject>("ModCard");
            ModsButton = Bundle.LoadAsset<GameObject>("ModsButton");
            foreach(var plugin in plugins)
            {
                AssemblyDescriptionAttribute desc = plugin.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                ModEntry entry = new();
                entry.Name = plugin.Info.Metadata.Name;
                entry.BepInExGuid = plugin.Info.Metadata.GUID;
                entry.BepInExVersion = plugin.Info.Metadata.Version.ToString();
                string[] deconstructedGuid = plugin.Info.Metadata.GUID.Split('.');
                entry.Author = deconstructedGuid.Length >= 2 ? deconstructedGuid[1] : "Unknown Author";
                entry.Description = IsStringWhiteSpaceOrEmpty(desc.Description) ? "No Description" : desc.Description;
                DirectoryInfo? dir = new FileInfo(plugin.GetType().Assembly.Location).Directory;
                if(dir != null)
                {
                    if(!dir.Name.Contains("plugins"))
                    {
                        dir = RecurseUntilParentIs(dir, "plugins") ?? dir;
                        FileInfo? manifestFile = dir.GetFiles("manifest.json").FirstOrDefault();
                        if(manifestFile != null)
                        {
                            JObject? manifest = null;
                            try
                            {
                                manifest = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(manifestFile.FullName));
                            }
                            catch
                            {
                                Logger.LogWarning($"Failed to parse manifest.json for {entry.Name}");
                            }
                            if(manifest != null)
                            {
                                FileInfo? icon = manifestFile.Directory.GetFiles("icon.png").FirstOrDefault();
                                Texture2D tex = new(256, 256);
                                if (icon != null)
                                {
                                    tex.LoadImage(File.ReadAllBytes(icon.FullName));
                                }
                                else tex.LoadImage(Resource1.noicon);
                                entry.Icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new(0.5f, 0.5f));
                                entry.Icon!.name = icon != null ? icon.Name : "NoIcon";
                                entry.IsFromThunderstore = true;
                                if(manifest.GetValue("description") != null)
                                {
                                    entry.Description = manifest.GetValue("description")!.ToString();
                                }
                                string modName = manifest.GetValue("name")?.ToString() ?? "";
                                if(modName != "") entry.Name = modName;
                                entry.ThunderstoreVersion = manifest.GetValue("version_number")?.ToString() ?? "";
                                string url = manifest.GetValue("website_url")?.ToString() ?? "";
                                entry.AuthorWebsite = url != "" ? url : "";
                                string author = manifestFile.Directory.Name.Replace("-" + modName, "");
                                entry.ThunderstoreUrl = $"https://thunderstore.io/c/ultrakill/p/{author}/{modName}";
                                entry.ThunderstoreGuid = manifestFile.Directory.Name;
                                entry.Author = author;
                            }
                        }
                    }
                    else
                    {
                        Texture2D tex = new(256, 256);
                        tex.LoadImage(Resource1.noicon);
                        entry.Icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new(0.5f, 0.5f));
                        entry.Icon.name = "NoIcon";
                    }
                }
                Mods.Add(entry);
            }
            Mods = Mods.OrderBy(m => m.Name).ToList();
            SceneManager.sceneLoaded += AddButton;
        }

        private void AddButton(Scene s, LoadSceneMode mode)
        {
            if(SceneHelper.CurrentScene.Contains("Menu"))
            {
                GameObject mainMenu = s.GetRootGameObjects().Where(c => c.name == "Canvas").First().transform.Find("Main Menu (1)").gameObject;
                if(mainMenu.transform.Find("ModsButton") == null)
                {
                    GameObject? button = Instantiate(ModsButton, mainMenu.GetComponent<RectTransform>());
                    button?.GetComponentInChildren<Button>().onClick.AddListener(() => OpenModMenu());
                    RectTransform? rect = button?.GetComponent<RectTransform>();
                    if(rect != null)
                    {
                        rect.SetPivot(PivotPresets.BottomLeft);
                        rect.SetAnchor(AnchorPresets.BottomLeft);
                        rect.anchoredPosition = new Vector2(5, 5);
                    }
                }
            }
        }
        private DirectoryInfo? RecurseUntilParentIs(DirectoryInfo dir, string target)
        {
            DirectoryInfo final = dir;
            while(final.Parent.Name != target)
            {
                if(final.Parent == null) return null;
                final = final.Parent;
            }
            return final;
        }
        private void OpenModMenu()
        {
            RectTransform canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
            Instantiate(ModmenuPrefab, canvas)!.AddComponent<ModMenu>();
        }
        public static T AorB<T>(T a, T b, Func<bool> condition)
        {
            return condition() ? a : b;
        }
        private static bool IsStringWhiteSpaceOrEmpty(string str)
        {
            return str.Length == 0 || str.All(c => char.IsWhiteSpace(c));
        }
       
    }
    public struct ModEntry
    {
        public Sprite? Icon;
        public string Name = "";
        public string Author = "";
        public string BepInExGuid = "";
        public string ThunderstoreGuid = "";
        public string BepInExVersion = "";
        public string ThunderstoreVersion = "";
        public string Description = "";
        public string AuthorWebsite = "";
        public string ThunderstoreUrl = "";
        public bool IsFromThunderstore = false;

        public ModEntry()
        {

        }
    }
    
}
