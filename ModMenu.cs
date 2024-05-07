using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModList
{
    public class ModMenu : MonoBehaviour
    {
        private Image? Icon;
        private TMP_Text? ModName, Author, Version, Description;
        private Button? ExitButton, OpenWebsiteButton, OpenThunderstoreButton;
        private TMP_InputField? Searchbox;
        private ScrollRect? Mods;
        private List<KeyValuePair<ModEntry, GameObject>> AllModsObjects = new();
        private List<KeyValuePair<ModEntry, GameObject>> FilteredMods = new();
        private ModEntry CurrentMod;
        public void SetCurrentMod(ModEntry mod)
        {
            CurrentMod = mod;
            Icon!.sprite = mod.Icon;
            ModName!.text = mod.Name;
            Author!.text = "by " + mod.Author;
            Version!.text = "v" + mod.BepInExVersion;
            Description!.text = mod.Description;
            if(mod.IsFromThunderstore) OpenThunderstoreButton!.gameObject.SetActive(true);
            else OpenThunderstoreButton!.gameObject.SetActive(false);
            if(mod.AuthorWebsite != "") OpenWebsiteButton!.gameObject.SetActive(true);
            else OpenWebsiteButton!.gameObject.SetActive(false);
        }
        public void SetCurrentMod(GameObject g)
        {
            SetCurrentMod(AllModsObjects.Where(m => m.Value == g).First().Key);
        }
        void Awake()
        {
            Icon = transform.Find("Container/Mod/Viewport/Content/ModInfo/Icon").GetComponent<Image>();
            ModName = transform.Find("Container/Mod/Viewport/Content/ModInfo/Icon/ModName").GetComponent<TMP_Text>();
            Version = transform.Find("Container/Mod/Viewport/Content/ModInfo/Icon/Version").GetComponent<TMP_Text>();
            Author = transform.Find("Container/Mod/Viewport/Content/ModInfo/Icon/Author").GetComponent<TMP_Text>();
            Description = transform.Find("Container/Mod/Viewport/Content/Description").GetComponent<TMP_Text>();
            OpenWebsiteButton = transform.Find("Container/Mod/Viewport/Content/Buttons/OpenWebsite").GetComponent<Button>();
            OpenWebsiteButton.onClick.AddListener(() => Application.OpenURL(CurrentMod.AuthorWebsite));
            OpenWebsiteButton.gameObject.SetActive(false);
            OpenThunderstoreButton = transform.Find("Container/Mod/Viewport/Content/Buttons/ThunderstorePage").GetComponent<Button>();
            OpenThunderstoreButton.onClick.AddListener(() => Application.OpenURL(CurrentMod.ThunderstoreUrl));
            OpenThunderstoreButton.gameObject.SetActive(false);
            ExitButton = transform.Find("Exit").GetComponent<Button>();
            ExitButton.onClick.AddListener(() => Destroy(gameObject));
            Searchbox = transform.Find("SearchBox").GetComponent<TMP_InputField>();
            Searchbox.onValueChanged.AddListener(Search);
            Mods = transform.Find("Container/Mods").GetComponent<ScrollRect>();

            foreach(ModEntry mod in Plugin.Mods)
            {
                GameObject? card = Instantiate(Plugin.ModCardPrefab, Mods.content);
                card?.AddComponent<Button>().onClick.AddListener(() => SetCurrentMod(card));
                if(card != null)
                {
                    AllModsObjects.Add(new(mod, card));
                    card.transform.Find("Icon").GetComponent<Image>().sprite = mod.Icon;
                    card.transform.Find("Icon/ModName").GetComponent<TMP_Text>().text = mod.Name;
                    card.transform.Find("Icon/Version").GetComponent<TMP_Text>().text = "v" + mod.BepInExVersion;
                    card.transform.Find("Icon/Author").GetComponent<TMP_Text>().text = "by " + mod.Author;
                }
            }
            SetCurrentMod(AllModsObjects.First().Key);
        }
        public void Search(string search)
        {
            if(search == "")
            {
                foreach(var kvp in AllModsObjects)
                {
                    kvp.Value.SetActive(true);
                }
            }
            FilteredMods = AllModsObjects.Where(m => m.Key.Name.Contains(search)).ToList();
            foreach(var kvp in AllModsObjects)
            {
                if(!FilteredMods.Contains(kvp)) kvp.Value.SetActive(false); 
                else kvp.Value.SetActive(true);
            }
        }
    }
}