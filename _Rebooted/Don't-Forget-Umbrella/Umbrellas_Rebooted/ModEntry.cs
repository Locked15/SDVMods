using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbrellas_Rebooted.Logic;
using Umbrellas_Rebooted.Logic.APIs;

namespace Umbrellas_Rebooted
{
    public class ModEntry : Mod, IAssetEditor
    {
        public static UmbrellaConfig Config { get; private set; } = null;
        public static ModEntry Instance { get; private set; }

        internal static IJsonAssetsApi JsonAssets;

        private Item lastItem;
        public static bool drawUmbrella = false;
        public static bool drawRegularFarmer = false;
        public static bool isMaleFarmer;
        public static bool isBaldFarmer;
        public static bool fullRedraw;
        public static bool justDrewFarmer = true;
        public static bool changingAppearance = false;

        public static Texture2D umbrellaTexture;
        public static Texture2D regularFarmerTexture;
        public static Texture2D umbrellaOverlayTextureBack;
        public static Texture2D umbrellaOverlayTextureSide;

        //add new umbrellas here
        public static List<string> umbrellaNames = new() { "Tattered Umbrella", "Red Umbrella", "Orange Umbrella", "Yellow Umbrella", "Green Umbrella", "Blue Umbrella", "Purple Umbrella", "Black Umbrella" };
        public static List<Texture2D> umbrellaPlayerTextures = new();
        public static List<Texture2D> umbrellaTextureBack = new();
        public static List<Texture2D> umbrellaTextureSide = new();

        public static List<string> bestHatNames = new() { "Sombrero", "Living Hat", "Mushroom Cap" };
        public static List<string> goodHatNames = new() { "Garbage Hat", "Frog Hat", "Sailor's Cap", "Straw Hat", "Sou'wester", "Fishing Hat", "Copper Pan", "Hard Hat", "Golden Helmet" };

        public static List<string> customBestHats = new() { };
        public static List<string> customGoodHats = new()
        {
            "Rain Hood", // IllogicalMoodSwing's Rainy Day Clothing
            "Yellow Rain Hood", "White Rain Hood", "Red Rain Hood", "Purple Rain Hood", "Pink Rain Hood", "Green Rain Hood", "Blue Rain Hood", "Black Rain Hood"  // Hope's Hats - Rain Hoods
        };

        public static List<string> shirtsNames = new() { "Rain Coat" };
        public static List<string> customShirtNames = new();

        public static List<string> exceptionLocations = new();

        public static int wetBuffIndex = 35;
        public static int ticksInRain = 0;
        public static float staminaDrainRatePerTick;
        public static float staminaDrainShield;

        //private const string cofModID = "KoihimeNakamura.ClimatesOfFerngill";

        private Harmony harmony;

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            try
            {
                Config = Helper.ReadConfig<UmbrellaConfig>();
            }

            catch (Exception ex)
            {
                Monitor.Log($"Encountered an error while loading the config.json file. Default settings will be used instead. Full error message:\n-----\n{ex.ToString()}", LogLevel.Error);
                Config = new UmbrellaConfig();
            }

            Helper.WriteConfig(Config);
            string assetsPath = Config.NudityCompatibility ? "assets/nude" : "assets/common";

            FarmerRendererPatches.Initialize(Monitor);
            UmbrellaPatch.Initialize(Monitor);

            //add new umbrellas here
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/tattered/umbrella_overlay_back.png"));
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/red/umbrella_overlay_back.png"));
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/orange/umbrella_overlay_back.png"));
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/yellow/umbrella_overlay_back.png"));
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/green/umbrella_overlay_back.png"));
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/blue/umbrella_overlay_back.png"));
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/purple/umbrella_overlay_back.png"));
            umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/black/umbrella_overlay_back.png"));
            //umbrellaTextureBack.Add(this.Helper.ModContent.Load<Texture2D>($"{assetsPath}/pink/umbrella_overlay_back.png"));

            //add new umbrellas here
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/tattered/umbrella_overlay_side.png"));
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/red/umbrella_overlay_side.png"));
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/orange/umbrella_overlay_side.png"));
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/yellow/umbrella_overlay_side.png"));
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/green/umbrella_overlay_side.png"));
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/blue/umbrella_overlay_side.png"));
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/purple/umbrella_overlay_side.png"));
            umbrellaTextureSide.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/black/umbrella_overlay_side.png"));
            //umbrellaTextureBack.Add(Helper.ModContent.Load<Texture2D>($"{assetsPath}/pink/umbrella_overlay_back.png"));

            //Events
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.MenuChanged += onMenuChanged;

            harmony = new Harmony(ModManifest.UniqueID);

            //Harmony Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.draw),
                new Type[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }),
                postfix: new HarmonyMethod(typeof(FarmerRendererPatches), nameof(FarmerRendererPatches.draw_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText),
                new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>) }),
                prefix: new HarmonyMethod(typeof(UmbrellaPatch), nameof(UmbrellaPatch.drawHoverTextPrefix)),
                postfix: new HarmonyMethod(typeof(UmbrellaPatch), nameof(UmbrellaPatch.drawHoverTextPostfix))
            );
        }

        private void onMenuChanged(object sender, MenuChangedEventArgs e) // Handle when the player changes their appearance at the shrine of illusions
        {
            CreateModMenu();

            if (changingAppearance)
            {
                changingAppearance = false;
                OnSaveLoaded(null, null);
                lastItem = null;
            }

            if (Game1.activeClickableMenu is CharacterCustomization)
            {
                changingAppearance = true;
                drawUmbrella = false;
                drawRegularFarmer = true;
                fullRedraw = true;
                redrawFarmer();
            }

            if (e.NewMenu is ShopMenu menu && menu != null)
            {
                // Add umbrellas to the hat mouse shop
                if (menu.potraitPersonDialogue == Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11494"), Game1.dialogueFont, Game1.tileSize * 5 - Game1.pixelZoom * 4))
                {
                    foreach (string umbrellaName in umbrellaNames)
                    {
                        if (umbrellaName != "Tattered Umbrella")
                        {
                            var o = new MeleeWeapon(spriteIndex: JsonAssets.GetWeaponId(name: umbrellaName))
                            {
                                DisplayName = Helper.Translation.Get($"umbrella.{umbrellaName[0..umbrellaName.IndexOf(' ')]}")
                            };

                            menu.itemPriceAndStock.Add(o, new[] { 1000, int.MaxValue });
                            menu.forSale.Insert(menu.forSale.Count, o);
                        }
                    }
                }
            }

            if (e.OldMenu is LetterViewerMenu letterClosed && letterClosed.isMail && e.NewMenu == null)
            {
                if (letterClosed.mailTitle == "lewis_umbrella") //thanks bbblueberry for the below snippet
                {
                    Game1.player.completelyStopAnimatingOrDoingAction();
                    DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750);

                    Game1.player.faceDirection(2);
                    Game1.player.freezePause = 4000;
                    Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[3]
                    {
                        new FarmerSprite.AnimationFrame(57, 0),
                        new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, Farmer.showHoldingItem),
                        new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false)
                    });

                    Game1.player.mostRecentlyGrabbedItem = new MeleeWeapon(spriteIndex: JsonAssets.GetWeaponId(name: "Tattered Umbrella"))
                    {
                        DisplayName = Helper.Translation.Get("umbrella.Tattered")
                    };
                    Game1.player.canMove = false;

                    AddItem(Game1.player.mostRecentlyGrabbedItem);
                }
            }
        }

        public static void AddItem(Item item)
        {
            if (Game1.player.couldInventoryAcceptThisItem(item))
                Game1.player.addItemToInventory(item);
            else
                Game1.player.addItemByMenuIfNecessaryElseHoldUp(item);
        }


        public void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            try
            {
                JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
                Helper.Content.InvalidateCache("Data/mail");
            }

            catch
            {
                Monitor.Log("Error loading JSON assets", LogLevel.Warn);
            }
        }

        private void CreateModMenu()
        {
            try
            {
                var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
                configMenu?.Register
                (
                    mod: ModManifest,
                    reset: () => Config = new(),
                    save: () => Helper.WriteConfig(Config)
                );

                if (configMenu == null)
                {
                    return;
                }

                else
                {
                    #region Basic Options.

                    configMenu.AddSectionTitle
                    (
                        mod: ModManifest,
                        text: () => Helper.Translation.Get("option.basic-options"),
                        tooltip: () => Helper.Translation.Get("option.basic-options.desc")
                    );

                    // 'Wetness Enable' Option.
                    configMenu.AddBoolOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.wetness-enabled"),
                        tooltip: () => Helper.Translation.Get("option.wetness-enabled.desc"),
                        getValue: () => Config.EnableWetness,
                        setValue: value => Config.EnableWetness = value
                    );

                    // 'Redraw Enable' Option.
                    configMenu.AddBoolOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.redraw-enabled"),
                        tooltip: () => Helper.Translation.Get("option.redraw-enabled.desc"),
                        getValue: () => Config.RedrawEnabled,
                        setValue: value => Config.RedrawEnabled = value
                    );
                    #endregion

                    #region Multiplier Options.

                    configMenu.AddSectionTitle
                    (
                        mod: ModManifest,
                        text: () => Helper.Translation.Get("option.multiplier-options"),
                        tooltip: () => Helper.Translation.Get("option.multiplier-options.desc")
                    );

                    // 'Stamina Drain' Option.
                    configMenu.AddNumberOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.stamina-drain"),
                        tooltip: () => Helper.Translation.Get("option.stamina-drain.desc"),
                        getValue: () => Config.StaminaDrainRate,
                        setValue: value => Config.StaminaDrainRate = value,
                        max: 10f,
                        min: -2.5f
                    );

                    // 'Best Hats Protection' Option.
                    configMenu.AddNumberOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.best-hats-protection"),
                        tooltip: () => Helper.Translation.Get("option.best-hats-protection.desc"),
                        getValue: () => Config.BestHatsProtection,
                        setValue: value => Config.BestHatsProtection = value,
                        max: 1f,
                        min: -1f
                    );

                    // 'Good Hats Protection' Option.
                    configMenu.AddNumberOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.good-hats-protection"),
                        tooltip: () => Helper.Translation.Get("option.good-hats-protection.desc"),
                        getValue: () => Config.GoodHatsProtection,
                        setValue: value => Config.GoodHatsProtection = value,
                        max: 1f,
                        min: -1f
                    );

                    // 'Shirt Protection' Option.
                    configMenu.AddNumberOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.shirt-protection"),
                        tooltip: () => Helper.Translation.Get("option.shirt-protection.desc"),
                        getValue: () => Config.ShirtsProtection,
                        setValue: value => Config.ShirtsProtection = value,
                        max: 1f,
                        min: -1f
                    );
                    #endregion

                    #region Text Options.

                    configMenu.AddSectionTitle
                    (
                        mod: ModManifest,
                        text: () => Helper.Translation.Get("option.advanced-options"),
                        tooltip: () => Helper.Translation.Get("option.advanced-options.desc")
                    );

                    // 'Best Hats' Option.
                    configMenu.AddTextOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.best-hats"),
                        tooltip: () => Helper.Translation.Get("option.best-hats.desc"),
                        getValue: () => Config.BestRainHats,
                        setValue: value => Config.BestRainHats = value
                    );

                    // 'Good Hats' Option.
                    configMenu.AddTextOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.good-hats"),
                        tooltip: () => Helper.Translation.Get("option.good-hats.desc"),
                        getValue: () => Config.GoodRainHats,
                        setValue: value => Config.GoodRainHats = value
                    );

                    // 'Shirt Names' Option.
                    configMenu.AddTextOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.shirt-names"),
                        tooltip: () => Helper.Translation.Get("option.shirt-names.desc"),
                        getValue: () => Config.ShirtNames,
                        setValue: value => Config.ShirtNames = value
                    );

                    // 'Exception Locations' Option.
                    configMenu.AddTextOption
                    (
                        mod: ModManifest,
                        name: () => Helper.Translation.Get("option.exception-locations"),
                        tooltip: () => Helper.Translation.Get("option.exception-locations.desc"),
                        getValue: () => Config.ExceptionLocationNames,
                        setValue: value => Config.ExceptionLocationNames = value
                    );
                    #endregion
                }
            }

            catch (Exception ex)
            {
                Monitor.Log($"An error happened while loading this mod's GMCM options menu. Its menu might be missing or fail to work. The auto-generated error message has been added to the log.", LogLevel.Warn);
                Monitor.Log($"----------", LogLevel.Trace);
                Monitor.Log($"{ex}", LogLevel.Trace);
            }
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e) // Prevent weird graphical bugs when the player returns to title screen with umbrella equipped.
        {
            drawUmbrella = false;
            drawRegularFarmer = true;
            fullRedraw = true;

            redrawFarmer();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.Name.IsEquivalentTo("Characters/Farmer/farmer_base") || asset.Name.IsEquivalentTo("Characters/Farmer/farmer_base_bald")
                   || asset.Name.IsEquivalentTo("Characters/Farmer/farmer_girl_base") || asset.Name.IsEquivalentTo("Characters/Farmer/farmer_girl_base_bald")
                   || asset.Name.IsEquivalentTo("TileSheets/BuffsIcons") || asset.Name.IsEquivalentTo("Data/hats") || asset.Name.IsEquivalentTo("Strings/StringsFromCSFiles")
                   || asset.Name.IsEquivalentTo("Data/mail");
        }

        public void Edit<T>(IAssetData asset)
        {
            string assetsPath = Config.NudityCompatibility ? "assets/nude" : "assets/common";

            if (asset.Name.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                data["ShopMenu.cs.11494"] = Helper.Translation.Get("shops.mouse");
            }
            if (asset.Name.IsEquivalentTo("Data/mail"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                data["hatter"] = Helper.Translation.Get("letters.mouse");

                if (JsonAssets != null)
                {
                    data["lewis_umbrella"] = Helper.Translation.Get("letters.lewis");
                }
            }

            if (asset.Name.IsEquivalentTo("TileSheets/BuffsIcons"))
            {
                var editor = asset.AsImage();

                Texture2D sourceImage = Helper.ModContent.Load<Texture2D>("assets/WetBuff.png");
                editor.PatchImage(sourceImage, targetArea: new Rectangle(176, 32, 16, 16));
            }

            if (Config.EnableWetness)
            {
                if (asset.Name.IsEquivalentTo("Data/hats"))
                {
                    IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
                }
            }

            if (!fullRedraw && Config.RedrawEnabled)
            {
                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_girl_base"))
                {
                    if (drawUmbrella)
                    {
                        asset.AsImage().PatchImage(umbrellaTexture);
                    }

                    if (drawRegularFarmer)
                    {
                        asset.AsImage().PatchImage(regularFarmerTexture);
                    }
                }
                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_base"))
                {
                    if (drawUmbrella)
                    {
                        asset.AsImage().PatchImage(umbrellaTexture);
                    }

                    if (drawRegularFarmer)
                    {
                        asset.AsImage().PatchImage(regularFarmerTexture);
                    }
                }
                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_base_bald"))
                {
                    if (drawUmbrella)
                    {
                        asset.AsImage().PatchImage(umbrellaTexture);
                    }

                    if (drawRegularFarmer)
                    {
                        asset.AsImage().PatchImage(regularFarmerTexture);
                    }
                }
                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_girl_base_bald"))
                {
                    if (drawUmbrella)
                    {
                        asset.AsImage().PatchImage(umbrellaTexture);
                    }

                    if (drawRegularFarmer)
                    {
                        asset.AsImage().PatchImage(regularFarmerTexture);
                    }
                }
            }

            else if (Config.RedrawEnabled)
            {
                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_girl_base"))
                {
                    asset.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>($"{assetsPath}/farmer_base_girl.png"));
                }

                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_base"))
                {
                    asset.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>($"{assetsPath}/farmer_base_boy.png"));
                }

                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_base_bald"))
                {
                    asset.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>($"{assetsPath}/farmer_base_boy_bald.png"));
                }

                if (asset.Name.IsEquivalentTo("Characters/Farmer/farmer_girl_base_bald"))
                {
                    asset.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>($"{assetsPath}/farmer_base_girl_bald.png"));
                }
            }

        }

        /// <summary>
        /// Base items data updating.
        /// I think it's not really necessary, but I need to test it.
        /// </summary>
        /// <param name="data"></param>
        private void GenerateDefaultHatsInfo(IDictionary<int, string> data)
        {
            Monitor.Log("Using default tool to generate description.", LogLevel.Debug);

            for (int i = 0; i < data.Count; i++)
            {
                int index = GetItemDescriptionSeparatorIndex(data[i]);

                // To optimize code, we will get index only on items, that needed to be expanded.
                switch (i)
                {
                    // 90% Protection:
                    case 3:
                    case 40:
                    case 42:
                        data[i] = data[i].Insert(index, Helper.Translation.Get($"{Helper.Translation.Get("ui.water-proof")}" +
                                                                               $"{Helper.Translation.Get("ui.proof-level.major")}"));

                        break;

                    // 50% Protection:
                    case 66:
                    case 78:
                    case 17:
                    case 4:
                    case 28:
                    case 55:
                    case 71:
                    case 27:
                    case 75:
                        data[i] = data[i].Insert(index, Helper.Translation.Get($"{Helper.Translation.Get("ui.water-proof")}" +
                                                                               $"{Helper.Translation.Get("ui.proof-level.moderate")}"));

                        break;
                }
            }
        }

        private static int GetItemDescriptionSeparatorIndex(string data)
        {
            int count = 0;
            var index = data.ToList().FindIndex(e =>
            {
                // This code will return index of second '/' element in item data.
                if (e.Equals('/'))
                {
                    count++;

                    if (count == 2)
                    {
                        return true;
                    }
                }

                return false;
            });

            return index;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Cause author (probably) never will add compatibility with 'Climates Of Ferngill', I removed this code.

            fullRedraw = false;
            isMaleFarmer = Game1.player.IsMale;
            isBaldFarmer = Game1.player.IsBaldHairStyle(Game1.player.getHair());
            customBestHats.AddRange(Config.BestRainHats.Split(',').ToList());
            customGoodHats.AddRange(Config.GoodRainHats.Split(',').ToList());
            customShirtNames.AddRange(Config.ShirtNames.Split(',').ToList());
            exceptionLocations.AddRange(Config.ExceptionLocationNames.Split(',').ToList());

            try
            {
                Config = Helper.ReadConfig<UmbrellaConfig>();
            }
            catch (Exception ex)
            {
                Monitor.Log($"Encountered an error while loading the config.json file. Default settings will be used instead. Full error message:\n-----\n{ex}", LogLevel.Error);
                Config = new UmbrellaConfig();
            }

            staminaDrainRatePerTick = Config.StaminaDrainRate / 420;
            Helper.Content.InvalidateCache("Data/hats");

            loadUmbrellaTextures();

            if (Config.EnableWetness & !Game1.player.mailReceived.Contains("lewis_umbrella"))
            {
                Game1.addMailForTomorrow("lewis_umbrella");
            }
        }

        public void loadUmbrellaTextures()
        {
            umbrellaPlayerTextures.Clear();
            string path = Config.NudityCompatibility ? "assets/nude" : "assets/common";

            // Add new umbrellas in here.
            if (isMaleFarmer)
            {
                if (isBaldFarmer)
                {
                    regularFarmerTexture = Helper.ModContent.Load<Texture2D>($"{path}/farmer_base_boy_bald.png");

                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/tattered/farmer_base_boy_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/red/farmer_base_boy_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/orange/farmer_base_boy_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/yellow/farmer_base_boy_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/green/farmer_base_boy_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/blue/farmer_base_boy_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/purple/farmer_base_boy_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/black/farmer_base_boy_bald.png"));
                    //umbrellaPlayerTextures.Add(this.Helper.ModContent.Load<Texture2D>($"{path}/pink/farmer_base_boy_bald.png"));
                }

                else
                {
                    regularFarmerTexture = Helper.ModContent.Load<Texture2D>($"{path}/farmer_base_boy.png");

                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/tattered/farmer_base_boy.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/red/farmer_base_boy.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/orange/farmer_base_boy.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/yellow/farmer_base_boy.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/green/farmer_base_boy.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/blue/farmer_base_boy.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/purple/farmer_base_boy.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/black/farmer_base_boy.png"));
                    //umbrellaPlayerTextures.Add(this.Helper.ModContent.Load<Texture2D>($"{path}/pink/farmer_base_boy.png"));
                }
            }

            else
            {
                if (isBaldFarmer)
                {
                    regularFarmerTexture = Helper.ModContent.Load<Texture2D>($"{path}/farmer_base_girl_bald.png");

                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/tattered/farmer_base_girl_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/red/farmer_base_girl_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/orange/farmer_base_girl_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/yellow/farmer_base_girl_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/green/farmer_base_girl_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/blue/farmer_base_girl_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/purple/farmer_base_girl_bald.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/black/farmer_base_girl_bald.png"));
                    //umbrellaPlayerTextures.Add(this.Helper.ModContent.Load<Texture2D>($"{path}/pink/farmer_base_girl_bald.png"));
                }

                else
                {
                    regularFarmerTexture = Helper.ModContent.Load<Texture2D>($"{path}/farmer_base_girl.png");

                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/tattered/farmer_base_girl.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/red/farmer_base_girl.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/orange/farmer_base_girl.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/yellow/farmer_base_girl.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/green/farmer_base_girl.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/blue/farmer_base_girl.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/purple/farmer_base_girl.png"));
                    umbrellaPlayerTextures.Add(Helper.ModContent.Load<Texture2D>($"{path}/black/farmer_base_girl.png"));
                    //umbrellaPlayerTextures.Add(this.Helper.ModContent.Load<Texture2D>($"{path}/pink/farmer_base_girl.png"));
                }
            }
        }

        public void redrawFarmer()
        {
            if (Config.RedrawEnabled)
            {
                if (isMaleFarmer) //Redraw the farmer
                {
                    if (isBaldFarmer)
                    {
                        Helper.Content.InvalidateCache("Characters/Farmer/farmer_base_bald");
                    }
                    else
                    {
                        Helper.Content.InvalidateCache("Characters/Farmer/farmer_base");
                    }
                }
                else
                {
                    if (isBaldFarmer)
                    {
                        Helper.Content.InvalidateCache("Characters/Farmer/farmer_girl_base_bald");
                    }
                    else
                    {
                        Helper.Content.InvalidateCache("Characters/Farmer/farmer_girl_base");
                    }
                }
            }
        }

        private void addWetBuff()
        {
            Buff wetnessBuff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == wetBuffIndex);

            Game1.buffsDisplay.addOtherBuff(wetnessBuff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "Rain", Helper.Translation.Get("debuff.source")));
            wetnessBuff.which = wetBuffIndex;
            wetnessBuff.sheetIndex = wetBuffIndex;
            wetnessBuff.millisecondsDuration = 10000;
            wetnessBuff.description = Game1.player.IsMale ? Helper.Translation.Get("debuff.header.male") : Helper.Translation.Get("debuff.header.female") +
                                      $"\n{Helper.Translation.Get("debuff.body")}";
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // Handle drawing umbrella
            if (lastItem != Game1.player.CurrentItem) // Selected item has changed
            {
                if (Game1.player.CurrentItem is not null)
                {
                    if (umbrellaNames.Contains(Game1.player.CurrentItem.Name))
                    {
                        drawUmbrella = true;
                        drawRegularFarmer = false;
                        justDrewFarmer = false;
                        umbrellaTexture = umbrellaPlayerTextures[umbrellaNames.IndexOf(Game1.player.CurrentItem.Name)];
                        umbrellaOverlayTextureBack = umbrellaTextureBack[umbrellaNames.IndexOf(Game1.player.CurrentItem.Name)];
                        umbrellaOverlayTextureSide = umbrellaTextureSide[umbrellaNames.IndexOf(Game1.player.CurrentItem.Name)];
                        redrawFarmer();
                    }
                    else if (!justDrewFarmer)
                    {
                        drawRegularFarmer = true;
                        drawUmbrella = false;
                        redrawFarmer();
                        justDrewFarmer = true;
                    }
                }
                else if (!justDrewFarmer)
                {
                    drawRegularFarmer = true;
                    drawUmbrella = false;
                    redrawFarmer();
                    justDrewFarmer = true;
                }
            }

            // Handle wetness buff
            if (Game1.currentLocation.IsOutdoors && Game1.IsRainingHere(Game1.player.currentLocation) &&
                Game1.player.currentLocation.Name != "Desert" && !exceptionLocations.Contains(Game1.player.currentLocation.Name))
            {
                if (Game1.player.CurrentItem is not null)
                {
                    if (!umbrellaNames.Contains(Game1.player.CurrentItem.Name))
                    {
                        ticksInRain += 1;

                        if (Game1.buffsDisplay.otherBuffs.FindIndex(p => p.which == wetBuffIndex) != -1)
                        {
                            Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == wetBuffIndex).millisecondsDuration = 10000;
                        }
                    }

                    else
                    {
                        ticksInRain = 0;
                    }
                }
                else
                {
                    ticksInRain += 1;

                    if (Game1.buffsDisplay.otherBuffs.FindIndex(p => p.which == wetBuffIndex) != -1)
                    {
                        Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == wetBuffIndex).millisecondsDuration = 10000;
                    }
                }

                if (ticksInRain > 120 & Game1.buffsDisplay.otherBuffs.FindIndex(p => p.which == wetBuffIndex) == -1 & Config.EnableWetness)
                {
                    addWetBuff();
                }
            }

            else
            {
                ticksInRain = 0;
            }

            // Handle stamina drain.
            if (Game1.buffsDisplay.otherBuffs.FindIndex(p => p.which == wetBuffIndex) != -1 & Context.IsPlayerFree)
            {
                if (Game1.player.Stamina - staminaDrainRatePerTick > 1)
                {
                    staminaDrainShield = 0f;

                    if (Game1.player.hat.Value is not null && Game1.player.hat.Value.Name is string name)
                    {
                        if (bestHatNames.Contains(name) ||
                            customBestHats.Contains(name))
                        {
                            staminaDrainShield += Config.BestHatsProtection;
                        }

                        else if (goodHatNames.Contains(name) ||
                                 customGoodHats.Contains(name))
                        {
                            staminaDrainShield += Config.GoodHatsProtection;
                        }
                    }

                    if (Game1.player.shirtItem.Value is not null)
                    {
                        if (shirtsNames.Contains(Game1.player.shirtItem.Value.Name) ||
                            customShirtNames.Contains(Game1.player.shirtItem.Value.Name))
                        {
                            staminaDrainShield += Config.ShirtsProtection;
                        }
                    }

                    staminaDrainShield = staminaDrainShield > 1f ? 1f : staminaDrainShield;
                    Game1.player.Stamina -= staminaDrainRatePerTick * (1f - staminaDrainShield);
                }
            }

            lastItem = Game1.player.CurrentItem;
        }
    }
}
