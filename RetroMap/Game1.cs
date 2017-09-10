using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Win32;
using System.Collections.Generic;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace RetroMap
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D background;
        bool DebugMode;
        string Root;
        string RootConfig;
        string RootEmulators;
        string RootRoms;
        string RootConfigSystems;
        string[] Systems;
        string[] Emulators;
        string[] EmulatorExes;
        List<string> BackgroundImages;
        int[] SystemEmulator;
        int CurrentWidth;
        int CurrentHeight;
        int MaxSections;
        int MaxMenus;
        int MaxEntries;
        int MaxStorage;
        int MenuDepth;
        int SectionSelection;
        int MenuSelection;
        int EntrySelection;
        int UpPressed;
        int DownPressed;
        int LeftPressed;
        int RightPressed;
        int ConfirmPressed;
        int BackPressed;
        bool UpPrepared;
        bool DownPrepared;
        bool ChangePrepared;
        float HoldThreshold;
        float HoldReducePerClick;
        float HoldCurrent;
        Vector2 TextScale;
        float TextSpread;
        float TextOffset;
        string TimeString;
        bool DrawSquare;
        int[] SectionPast; //Previously entered sections.
        int[] MenuPast; //Previously entered menu screens.
        int SectionLast;
        int MenuLast;
        int[] EntryAccess; //Menu item selected in past menu screens.
        int[,] MenuTotalEntries; //Total amount of entries per menu.
        string[,] MenuTitles; //Titles of each menu
        bool[,] MenuOptions;
        bool[,] MenuOptionsSave;
        string[,] MenuOptionsString;
        List<int> MenuOptionsLocations;
        List<string>[] MenuSystemLoad;
        List<string> MenuResolutionDisplay;
        List<string> MenuWindowMode;
        List<DisplayMode> MenuDisplayMode;
        bool MenuReset;
        int ExplorerPurpose;
        string ExplorerString;
        int ExplorerLastEntry;
        Vector3 ExplorerLocation;
        string[] MenuEntriesDraw; //Menu strings to be drawn on-screen
        string[] MenuEntriesAccess; //Menu strings to be manipulated before being drawn
        string[] MenuEntriesProgram; //Strings for program entries to use to open programs
        string[] MenuEntriesFile; //Strings for program entries to use to open files within programs
        string[] MenuEntriesCommand; //Strings for program entries to use to as commands
        int[] MenuEntriesPurpose;
        int[] MenuEntriesType; //Types of entries. 0: Link, 1: Option, 2: Open Program
        int[] MenuEntriesDestinationSection; //Entry destination when clicked (Section)
        int[] MenuEntriesDestinationMenu; //Entry destination when clicked (Menu)
        int[] MenuEntriesIntOptionSegment;
        Vector3[] MenuEntriesIntPosition;
        Vector3[] MenuEntriesStringPosition;
        int[] MenuEntriesIntMinimum;
        int[] MenuEntriesIntMaximum;
        bool[] MenuEntriesRebuildOnChange;
        bool[] MenuEntriesLocked; //Whether or not the menu entry is currently restricted
        bool[] MenuEntriesShowFuture; //Whether or not the entry shows its destination
        bool[] MenuEntriesShowModel; //Whether or not the entry shows a model

        int[,,] MenuStorageIntStatic;
        string[,,] MenuStorageStringStatic;

        int CX;
        int CY;
        int CZ;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.IsFixedTimeStep = false;
            this.graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            DebugMode = true;
            Root = @"C:\RetroMap\";
            RootConfig = Root + @"Config\";
            RootConfigSystems = RootConfig + @"Systems\";
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(RootConfig);
            Directory.CreateDirectory(RootConfigSystems);
            RootEmulators = Root + @"Emulators\";
            RootRoms = Root + @"Roms\";
            Directory.CreateDirectory(RootEmulators);
            Directory.CreateDirectory(RootRoms);
            Systems = Directory.GetDirectories(RootRoms);
            Emulators = Directory.GetDirectories(RootEmulators);
            EmulatorExes = new string[Emulators.Length];
            BackgroundImages = new List<string>();
            MaxSections = 16;
            MaxMenus = 64;
            MaxEntries = 65535;
            MaxStorage = 10000;
            MenuStorageIntStatic = new int[1, 255, 255];
            MenuStorageStringStatic = new string[1, 255, 255];
            MenuSystemLoad = new List<string>[Systems.Length];
            MenuResolutionDisplay = new List<string>();
            MenuDisplayMode = new List<DisplayMode>();
            MenuWindowMode = new List<string> { "Window", "Fullscreen", "Borderless" };
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                MenuResolutionDisplay.Add(mode.ToString());
                MenuDisplayMode.Add(mode);
            }
            for (CX = 0; CX < Systems.Length; CX++)
            {
                MenuSystemLoad[CX] = new List<string>();
            }
            for (CX = 0; CX < Emulators.Length; CX++)
            {
                EmulatorExes[CX] = Directory.GetFiles(Emulators[CX], "*.exe")[0];
            }
            HoldThreshold = 0.3f;
            HoldReducePerClick = 0.05f;
            EntrySelection = 0;
            ChangePrepared = true;
            SectionPast = new int[100];
            MenuPast = new int[100];
            EntryAccess = new int[MaxEntries];
            MenuTotalEntries = new int[MaxSections, MaxMenus];
            MenuTitles = new string[MaxSections, MaxMenus];
            MenuOptions = new bool[MaxSections, MaxMenus];
            MenuOptionsSave = new bool[MaxSections, MaxMenus];
            MenuOptionsString = new string[MaxSections, MaxMenus];
            MenuEntriesDraw = new string[MaxEntries];
            MenuEntriesAccess = new string[MaxEntries];
            MenuEntriesProgram = new string[MaxEntries];
            MenuEntriesFile = new string[MaxEntries];
            MenuEntriesCommand = new string[MaxEntries];
            MenuEntriesPurpose = new int[MaxEntries];
            MenuEntriesType = new int[MaxEntries];
            MenuEntriesDestinationSection = new int[MaxEntries];
            MenuEntriesDestinationMenu = new int[MaxEntries];
            MenuEntriesRebuildOnChange = new bool[MaxEntries];
            MenuEntriesLocked = new bool[MaxEntries];
            MenuEntriesShowFuture = new bool[MaxEntries];
            MenuEntriesShowModel = new bool[MaxEntries];
            MenuEntriesIntOptionSegment = new int[MaxEntries];
            MenuEntriesIntPosition = new Vector3[MaxEntries];
            MenuEntriesIntMinimum = new int[MaxEntries];
            MenuEntriesIntMaximum = new int[MaxEntries];
            MenuEntriesStringPosition = new Vector3[MaxEntries];
            MenuOptionsLocations = new List<int>();
            DataManagement("System", true);
            DataManagement("Backgrounds", true);
            DataManagement("Video", true);
            DataManagement("Clock", true);
            DataManagement("Slide", true);
            RebuildMenu(SectionSelection, MenuSelection);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("DFONT");

            // TODO: use this.Content to load your game content here
        }
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        protected override void Update(GameTime gameTime)
        {
            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            TextScale = new Vector2((float)MenuStorageIntStatic[0, 1, 3] / 72, (float)MenuStorageIntStatic[0, 1, 3] / 72);
            TextSpread = (MenuStorageIntStatic[0, 1, 3] / 2 * 2.75f);
            if (MenuStorageIntStatic[0, 2, 0] == 1)
            {
                if (MenuStorageIntStatic[0, 2, 1] == 0)
                {
                    if (MenuStorageIntStatic[0, 2, 2] == 0)
                    {
                        TimeString = DateTime.Now.ToString("hh:mm tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        TimeString = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    }
                }
                else
                {
                    if (MenuStorageIntStatic[0, 2, 2] == 0)
                    {
                        TimeString = DateTime.Now.ToString("HH:mm", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        TimeString = DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    }
                }
                string TimeStringSeconds = DateTime.Now.ToString("ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                int TimeSeconds = Int32.Parse(TimeStringSeconds);
                if (TimeSeconds % 2 == 0 && MenuStorageIntStatic[0, 2, 2] == 0)
                {
                    TimeString = TimeString.Replace(":", " ");
                }
            }
            else
            {
                TimeString = " ";
            }
            if (MenuStorageIntStatic[0, 3, 0] == 1)
            {
                TextOffset = TextOffset - TextOffset * MenuStorageIntStatic[0, 3, 1] * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                TextOffset = 0;
            }
            if (capabilities.IsConnected)
            {
                UpPressed = PadFunction(Buttons.DPadUp, UpPressed);
                DownPressed = PadFunction(Buttons.DPadDown, DownPressed);
                LeftPressed = PadFunction(Buttons.DPadLeft, LeftPressed);
                RightPressed = PadFunction(Buttons.DPadRight, RightPressed);
                ConfirmPressed = PadFunction(Buttons.A, ConfirmPressed);
                BackPressed = PadFunction(Buttons.B, BackPressed);
            }
            else
            {
                UpPressed = KeyFunction(Keys.W, UpPressed);
                DownPressed = KeyFunction(Keys.S, DownPressed);
                LeftPressed = KeyFunction(Keys.A, LeftPressed);
                RightPressed = KeyFunction(Keys.D, RightPressed);
                ConfirmPressed = KeyFunction(Keys.Enter, ConfirmPressed);
                BackPressed = KeyFunction(Keys.Escape, BackPressed);
            }
            if (UpPressed == 1)
            {
                UpPrepared = true;
            }
            if (DownPressed == 1)
            {
                DownPrepared = true;
            }
            if (LeftPressed == 1)
            {
                if (MenuEntriesType[EntrySelection] == 5 || MenuEntriesType[EntrySelection] == 6 || MenuEntriesType[EntrySelection] == 7)
                {
                    Vector3 IntPosition = MenuEntriesIntPosition[EntrySelection];
                    MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z]--;
                    if (MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] < MenuEntriesIntMinimum[EntrySelection])
                    {
                        MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] = MenuEntriesIntMaximum[EntrySelection];
                    }
                    MenuOptionsSave[SectionSelection, MenuSelection] = true;
                    if (MenuEntriesRebuildOnChange[EntrySelection]) RebuildMenu(SectionSelection, MenuSelection);
                }
                if (MenuEntriesType[EntrySelection] == 9 || MenuEntriesType[EntrySelection] == 10)
                {
                    if (ExplorerString != "")
                    {
                        if (ParentDirectoryExists(ExplorerString))
                        {
                            string[] ParentDirectories = Directory.GetDirectories(Directory.GetParent(ExplorerString).FullName);
                            for (int i = 0; i < ParentDirectories.Length; i++)
                            {
                                if (ParentDirectories[i] == ExplorerString)
                                {
                                    EntrySelection = i;
                                }
                            }
                            ExplorerString = Directory.GetParent(ExplorerString).FullName;
                            ChangePrepared = true;
                        }
                        else
                        {
                            ExplorerString = "";
                            EntrySelection = 0;
                            ChangePrepared = true;
                        }
                    }
                    else
                    {
                        SectionLast = SectionSelection;
                        MenuLast = MenuSelection;
                        MenuDepth--;
                        SectionSelection = SectionPast[MenuDepth];
                        MenuSelection = MenuPast[MenuDepth];
                        EntrySelection = EntryAccess[MenuDepth];
                        ChangePrepared = true;
                    }
                }
            }
            if (RightPressed == 1)
            {
                if (MenuEntriesType[EntrySelection] == 5 || MenuEntriesType[EntrySelection] == 6 || MenuEntriesType[EntrySelection] == 7)
                {
                    Vector3 IntPosition = MenuEntriesIntPosition[EntrySelection];
                    MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z]++;
                    if (MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] > MenuEntriesIntMaximum[EntrySelection])
                    {
                        MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] = MenuEntriesIntMinimum[EntrySelection];
                    }
                    MenuOptionsSave[SectionSelection, MenuSelection] = true;
                    if (MenuEntriesRebuildOnChange[EntrySelection]) RebuildMenu(SectionSelection, MenuSelection);
                }
                if (MenuEntriesType[EntrySelection] == 9)
                {
                    ExplorerString = MenuEntriesAccess[EntrySelection];
                    ExplorerLastEntry = EntrySelection;
                    EntrySelection = 0;
                    ChangePrepared = true;
                }
            }
            if (ConfirmPressed == 1)
            {
                if (MenuEntriesType[EntrySelection] == 0)
                {
                    SectionPast[MenuDepth] = SectionSelection;
                    MenuPast[MenuDepth] = MenuSelection;
                    SectionLast = SectionSelection;
                    MenuLast = MenuSelection;
                    EntryAccess[MenuDepth] = EntrySelection;
                    MenuDepth++;
                    SectionSelection = MenuEntriesDestinationSection[EntrySelection];
                    MenuSelection = MenuEntriesDestinationMenu[EntrySelection];
                    EntrySelection = 0;
                    ChangePrepared = true;
                }
                else if (MenuEntriesType[EntrySelection] == 2)
                {
                    if (File.Exists(MenuEntriesProgram[EntrySelection]) || Directory.Exists(MenuEntriesProgram[EntrySelection]))
                    {
                        if (File.Exists(MenuEntriesProgram[EntrySelection]))
                        {
                            //MenuEntriesAccess[EntrySelection] = "File Found! File: " + MenuEntriesProgram[EntrySelection];
                            ExecuteCommand(MenuEntriesCommand[EntrySelection]);
                        }
                        if (Directory.Exists(MenuEntriesProgram[EntrySelection]))
                        {
                            MenuEntriesAccess[EntrySelection] = "Directory Found! Directory: " + MenuEntriesProgram[EntrySelection];
                        }
                    }
                    else
                    {
                        MenuEntriesAccess[EntrySelection] = "File/Directory Not Found! " + MenuEntriesProgram[EntrySelection];
                    }
                }
                else if (MenuEntriesType[EntrySelection] == 3)
                {
                    Exit();
                }
                else if (MenuEntriesType[EntrySelection] == 5 || MenuEntriesType[EntrySelection] == 6 || MenuEntriesType[EntrySelection] == 7)
                {
                    if (SectionSelection == 0 && MenuSelection == 4)
                    {
                        Vector3 IntPosition = MenuEntriesIntPosition[0];
                        int IntStatic = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
                        IntPosition = MenuEntriesIntPosition[1];
                        int IntStatic2 = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
                        ApplyGraphics(MenuDisplayMode[IntStatic].Width, MenuDisplayMode[IntStatic].Height, IntStatic2);
                        IntPosition = MenuEntriesIntPosition[2];
                        int IntStatic3 = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
                        IntPosition = MenuEntriesIntPosition[3];
                        int IntStatic4 = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
                        List<string> VideoStorage = new List<string>();
                        VideoStorage.Add(IntStatic.ToString());
                        VideoStorage.Add(IntStatic2.ToString());
                        VideoStorage.Add(IntStatic3.ToString());
                        VideoStorage.Add(IntStatic4.ToString());
                        WriteToFile(RootConfig + @"\Video.txt", VideoStorage);
                    }
                }
                else if (MenuEntriesType[EntrySelection] == 8)
                {
                    SectionPast[MenuDepth] = SectionSelection;
                    MenuPast[MenuDepth] = MenuSelection;
                    SectionLast = SectionSelection;
                    MenuLast = MenuSelection;
                    EntryAccess[MenuDepth] = EntrySelection;
                    ExplorerPurpose = MenuEntriesPurpose[EntrySelection];
                    ExplorerString = MenuEntriesFile[EntrySelection];
                    ExplorerLocation = MenuEntriesStringPosition[EntrySelection];
                    MenuDepth++;
                    SectionSelection = 2;
                    MenuSelection = 0;
                    EntrySelection = 0;
                    if (ExplorerString != "")
                    {
                        EntrySelection = 1;
                    }
                    ChangePrepared = true;
                }
                else if (MenuEntriesType[EntrySelection] == 9 && ExplorerPurpose == 0 || MenuEntriesType[EntrySelection] == 10 && ExplorerPurpose == 1)
                {
                    MenuStorageStringStatic[(int)ExplorerLocation.X, (int)ExplorerLocation.Y, (int)ExplorerLocation.Z] = MenuEntriesAccess[EntrySelection];
                    SectionLast = SectionSelection;
                    MenuLast = MenuSelection;
                    MenuDepth--;
                    SectionSelection = SectionPast[MenuDepth];
                    MenuSelection = MenuPast[MenuDepth];
                    EntrySelection = EntryAccess[MenuDepth];
                    ChangePrepared = true;
                    MenuOptionsSave[SectionSelection, MenuSelection] = true;
                }
            }
            if (BackPressed == 1)
            {
                if (MenuDepth != 0)
                {
                    SectionLast = SectionSelection;
                    MenuLast = MenuSelection;
                    MenuDepth--;
                    SectionSelection = SectionPast[MenuDepth];
                    MenuSelection = MenuPast[MenuDepth];
                    EntrySelection = EntryAccess[MenuDepth];
                    ChangePrepared = true;
                }
                else
                {
                    Exit();
                }
            }
            if (UpPressed == 2 || DownPressed == 2)
            {
                HoldCurrent += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (HoldCurrent >= HoldThreshold)
                {
                    if (DownPressed == 2) DownPrepared = true; else UpPrepared = true;
                    HoldCurrent -= HoldReducePerClick;
                }
            }
            else HoldCurrent = 0;
            if (UpPrepared)
            {
                EntrySelection--;
                if (EntrySelection < 0)
                {
                    EntrySelection = MenuTotalEntries[SectionSelection, MenuSelection] - 1;
                    TextOffset += TextSpread * (MenuTotalEntries[SectionSelection, MenuSelection] - 1);
                }
                else
                {
                    TextOffset -= TextSpread;
                }
                while (MenuEntriesLocked[EntrySelection])
                {
                    EntrySelection--;
                    if (EntrySelection < 0)
                    {
                        EntrySelection = MenuTotalEntries[SectionSelection, MenuSelection] - 1;
                    }
                }
                UpPrepared = false;
            }
            if (DownPrepared)
            {
                EntrySelection++;
                if (EntrySelection == MenuTotalEntries[SectionSelection, MenuSelection])
                {
                    EntrySelection = 0;
                    TextOffset -= TextSpread * (MenuTotalEntries[SectionSelection, MenuSelection] - 1);
                }
                else
                {
                    TextOffset += TextSpread;
                }
                while (MenuEntriesLocked[EntrySelection])
                {
                    EntrySelection++;
                    if (EntrySelection == MenuTotalEntries[SectionSelection, MenuSelection])
                    {
                        EntrySelection = 0;
                    }
                }
                DownPrepared = false;
            }
            if (ChangePrepared)
            {
                if (MenuOptionsSave[SectionLast, MenuLast])
                {
                    if (SectionLast == 0 && MenuLast == 3)
                    {
                        for (CX = 0; CX < Systems.Length; CX++)
                        {
                            MenuSystemLoad[CX].Clear();
                            MenuSystemLoad[CX].Add(MenuStorageIntStatic[0, 0, CX].ToString());
                            WriteToFile(RootConfigSystems + Systems[CX].Replace(RootRoms, "") + @"\System.txt", MenuSystemLoad[CX]);
                        }
                    }
                    if (SectionLast == 0 && MenuLast == 8)
                    {
                        List<string> SaveList = new List<string>();
                        for (CX = 0; CX < MenuTotalEntries[SectionLast, MenuLast]; CX++)
                        {
                            SaveList.Add(MenuStorageIntStatic[0, 2, CX].ToString());
                        }
                        WriteToFile(RootConfig + @"\Clock.txt", SaveList);
                    }
                    if (SectionLast == 0 && MenuLast == 9)
                    {
                        List<string> SaveList = new List<string>();
                        for (CX = 0; CX < MenuTotalEntries[SectionLast, MenuLast]; CX++)
                        {
                            SaveList.Add(MenuStorageIntStatic[0, 3, CX].ToString());
                        }
                        WriteToFile(RootConfig + @"\Slide.txt", SaveList);
                    }
                }
                if (SectionLast == 0 && MenuLast == 14 && SectionSelection != 2)
                {
                    List<string> SaveList = new List<string>();
                    for (CX = 0; CX < MenuTotalEntries[SectionLast, MenuLast]; CX++)
                    {
                        if (CX == 0)
                        {
                            SaveList.Add(MenuStorageIntStatic[0, 4, CX].ToString());
                        }
                        else
                        {
                            if (MenuStorageStringStatic[0, 4, CX] != null)
                            {
                                SaveList.Add(MenuStorageStringStatic[0, 4, CX].ToString());
                            }
                        }
                    }
                    WriteToFile(RootConfig + @"\Backgrounds.txt", SaveList);
                }
                RebuildMenu(SectionSelection, MenuSelection);
                ChangePrepared = false;
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0f, 0f, 0f, 1f));
            spriteBatch.Begin();
            spriteBatch.Draw(background, new Rectangle(0, 0, CurrentWidth, CurrentHeight), Color.White);
            var fps = 1 / gameTime.ElapsedGameTime.TotalSeconds;
            Window.Title = fps.ToString();
            spriteBatch.End();
            if (false)
            {
                Matrix matrix = Matrix.CreateRotationX(MathHelper.ToRadians(70)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(0)) *
                Matrix.CreateScale(1, 1, 0);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, matrix);
                spriteBatch.Draw(background, new Rectangle(0, 0, CurrentWidth * 1, CurrentHeight * 1), Color.White);
                spriteBatch.End();
            }
            spriteBatch.Begin();
            for (int i = 0; i < MenuTotalEntries[SectionSelection, MenuSelection]; i++)
            {
                MenuEntriesDraw[i] = "";
                if (i == EntrySelection)
                {
                    MenuEntriesDraw[i] += ">>> ";
                }
                MenuEntriesDraw[i] += MenuEntriesAccess[i];
                if (MenuEntriesType[i] == 5)
                {
                    Vector3 IntPosition = MenuEntriesIntPosition[i];
                    int IntStatic = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
                    MenuEntriesDraw[i] += "" + IntStatic;
                }
                if (MenuEntriesType[i] == 6)
                {
                    Vector3 IntPosition = MenuEntriesIntPosition[i];
                    int IntStatic = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
                    string[] ToggleString = new string[2];
                    ToggleString[0] = "Off";
                    ToggleString[1] = "On";
                    MenuEntriesDraw[i] += "" + ToggleString[IntStatic];
                }
                if (MenuEntriesType[i] == 7)
                {
                    Vector3 IntPosition = MenuEntriesIntPosition[i];
                    int IntStatic = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
                    if (SectionSelection == 0 && MenuSelection == 3)
                    {
                        if (IntStatic < Emulators.Length)
                        {
                            MenuEntriesDraw[i] += " | " + Emulators[IntStatic].Replace(RootEmulators, "");
                        }
                        else
                        {
                            if (Emulators.Length == 0)
                            {
                                MenuEntriesDraw[i] += " | Error: No Emulators Found";
                            }
                            else
                            {
                                MenuEntriesDraw[i] += " | Error: Emulator # is too high!";
                            }
                        }
                    }
                    else if (SectionSelection == 0 && MenuSelection == 4 && i == 0)
                    {
                        MenuEntriesDraw[i] += "" + MenuDisplayMode[IntStatic].Width + "x" + MenuDisplayMode[IntStatic].Height;
                    }
                    else if (SectionSelection == 0 && MenuSelection == 4 && i == 1)
                    {
                        MenuEntriesDraw[i] += "" + MenuWindowMode[IntStatic];
                    }
                    else if (SectionSelection == 0 && MenuSelection == 4 && i == 2)
                    {
                        if (IntStatic == 0)
                        {
                            MenuEntriesDraw[i] += "" + "Empty Background";
                        }
                        else if (IntStatic == 1)
                        {
                            MenuEntriesDraw[i] += "" + "Current Desktop Background";
                        }
                        else
                        {
                            MenuEntriesDraw[i] += "" + BackgroundImages[IntStatic - 2];
                        }
                    }
                    else
                    {
                        MenuEntriesDraw[i] += "" + IntStatic;
                    }
                }
                if (MenuEntriesType[i] == 8 || MenuEntriesType[i] == 11)
                {
                    Vector3 StrPosition = MenuEntriesStringPosition[i];
                    string StrStatic = MenuStorageStringStatic[(int)StrPosition.X, (int)StrPosition.Y, (int)StrPosition.Z];
                    MenuEntriesDraw[i] += "" + StrStatic;
                }
                float TextOrigin = CurrentHeight / 2;
                float TextListY = i * TextSpread;
                float TextEntryY = EntrySelection * TextSpread;
                float TextFinalY = TextListY - TextEntryY;
                float TextX = 0;
                float TextY = TextOrigin + TextFinalY + TextOffset;
                if (TextY > (-TextSpread) && TextY < CurrentHeight + TextSpread)
                {
                    Vector2 TextPosition = new Vector2(TextX, TextY);
                    DrawString(font, MenuEntriesDraw[i], TextPosition, 0, 0, Color.White);
                }
            }
            DrawString(font, TimeString, new Vector2(CurrentWidth, 0), 2, 0, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        int KeyFunction(Keys Key, int Current)
        {
            if (Keyboard.GetState().IsKeyDown(Key))
            {
                if (Current == 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (Current == 2)
                {
                    return 3;
                }
                else
                {
                    return 0;
                }
            }
        }
        int PadFunction(Buttons Button, int Current)
        {
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            if (state.IsButtonDown(Button))
            {
                if (Current == 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (Current == 2)
                {
                    return 3;
                }
                else
                {
                    return 0;
                }
            }
        }
        void ApplyGraphics(int Width, int Height, int WindowMode)
        {
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            CurrentWidth = Width;
            CurrentHeight = Height;
            if (WindowMode == 1)
            {
                graphics.IsFullScreen = true;
            }
            else
            {
                graphics.IsFullScreen = false;
                if (WindowMode == 0)
                {
                    Window.IsBorderless = false;
                }
                else
                {
                    Window.IsBorderless = true;
                }
            }
            graphics.ApplyChanges();
            CreateBackground(MenuStorageIntStatic[0, 1, 2]);
        }
        void CreateBackground(int BackgroundID)
        {
            if (BackgroundID == 0)
            {
                background = new Texture2D(GraphicsDevice, CurrentWidth, CurrentHeight);
                Color[] data = new Color[CurrentWidth * CurrentHeight];
                for (int pixel = 0; pixel < data.Length; pixel++)
                {
                    data[pixel] = Color.Black;
                }
                background.SetData(data);
            }
            else if (BackgroundID == 1)
            {
                string pathWallpaper = "";
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
                if (regKey != null)
                {
                    pathWallpaper = regKey.GetValue("WallPaper").ToString();
                    regKey.Close();
                }
                FileStream fileStream = new FileStream(pathWallpaper, FileMode.Open);
                background = Texture2D.FromStream(GraphicsDevice, fileStream);
                fileStream.Dispose();
            }
            else if (BackgroundImages.Count + 1 >= BackgroundID)
            {
                FileStream fileStream = new FileStream(BackgroundImages[BackgroundID - 2], FileMode.Open);
                background = Texture2D.FromStream(GraphicsDevice, fileStream);
                fileStream.Dispose();
            }
        }

        void RebuildMenu(int X, int Y)
        {
            CX = X;
            CY = Y;
            CZ = 0;
            MenuOptionsLocations.Clear();
            if (CX == 0)
            {
                switch (CY)
                {
                    case 0:
                        MenuSetup("Debug Menu");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryLink("Console Select", 0, 2, false, true); break;
                                case 1: EntryLink("Emulator Select", 0, 3, false, true); break;
                                case 2: EntryLink("Settings", 0, 1, false, true); break;
                                case 3: EntryExit("Exit"); break;
                            }
                        break;
                    case 1:
                        MenuSetup("Settings Menu");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryLink("Graphics Settings", 0, 4, false, true); break;
                                case 1: EntryLink("Menu Settings", 0, 7, false, true); break;
                                case 2: EntryLink("Path Settings", 0, 11, false, true); break;
                            }
                        break;
                    case 2:
                        MenuSetup("Systems Menu");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: if (CZ < Systems.Length) EntryLink(Systems[CZ].Replace(RootRoms, ""), 1, CZ, false, true); else MenuEnd(); break;
                            }
                        break;
                    case 3:
                        MenuSetup("Emulators Menu");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: if (CZ < Systems.Length) EntryOption(Systems[CZ].Replace(RootRoms, ""), new Vector3(0, 0, CZ), 2, 0, Emulators.Length - 1, false); else MenuEnd(); break;
                            }
                        break;
                    case 4:
                        MenuSetup("Settings"); DataManagement("Backgrounds", false);
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryOption("Resolution: ", new Vector3(0, 1, 0), 2, 0, MenuResolutionDisplay.Count - 1, false); break;
                                case 1: EntryOption("Screen Mode: ", new Vector3(0, 1, 1), 2, 0, 2, false); break;
                                case 2: EntryOption("Current Background: ", new Vector3(0, 1, 2), 2, 0, BackgroundImages.Count + 1, false); break;
                                case 3: EntryOption("Font Size: ", new Vector3(0, 1, 3), 0, 12, 72, false); break;
                            }
                        break;
                    case 5:
                        MenuSetup("Audio");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                            }
                        break;
                    case 6:
                        MenuSetup("Controls");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                            }
                        break;
                    case 7:
                        MenuSetup("Options");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryLink("Clock Settings", 0, 8, false, true); break;
                                case 1: EntryLink("Slide Settings", 0, 9, false, true); break;
                                case 2: EntryLink("Audio Settings", 0, 5, false, true); break;
                                case 3: EntryLink("Control Settings", 0, 6, false, true); break;
                                case 4: EntryLink("Developer Debug Stuff", 0, 10, false, true); break;
                            }
                        break;
                    case 8:
                        MenuSetup("Clock");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryOption("Clock Toggle: ", new Vector3(0, 2, 0), 1, 0, 1, false); break;
                                case 1: EntryOption("Clock Mode: ", new Vector3(0, 2, 1), 2, 0, 1, false); break;
                                case 2: EntryOption("Clock Seconds: ", new Vector3(0, 2, 2), 1, 0, 1, false); break;
                            }
                        break;
                    case 9:
                        MenuSetup("Slide");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryOption("Slide Toggle: ", new Vector3(0, 3, 0), 1, 0, 1, false); break;
                                case 1: EntryOption("Slide Speed: ", new Vector3(0, 3, 1), 0, 0, 30, false); break;
                            }
                        break;
                    case 10:
                        MenuSetup("Debug Functions");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryExplorer("File Explorer Test, Folder. Result: ", "", 0, new Vector3(0, 0, 0)); break;
                                case 1: EntryExplorer("File Explorer Test, File. Result: ", "", 1, new Vector3(0, 0, 1)); break;
                            }
                        break;
                    case 11:
                        MenuSetup("Path Hub");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                                case 0: EntryLink("Rom Folder Paths", 0, 12, false, true); break;
                                case 1: EntryLink("Emulator Executable Paths", 0, 13, false, true); break;
                                case 2: EntryLink("Background Folder Paths", 0, 14, false, true); break;
                            }
                        break;
                    case 14:
                        MenuSetup("Background Paths");
                        for (CZ = 0; CZ < MenuStorageIntStatic[0,4,0] + 1; CZ++)
                        {
                            if (CZ == 0)
                            {
                                EntryOption("Background Directories: ", new Vector3(0, 4, 0), 0, 0, 250, true);
                            }
                            else
                            {
                                EntryExplorer("BG" + CZ + ": ", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 0, new Vector3(0, 4, CZ));
                            }
                        }
                        MenuEnd();
                        break;
                    default:
                        MenuSetup("Placeholder");
                        for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++) switch (CZ)
                            {
                                default: MenuEnd(); break;
                            }
                        break;
                }
            }
            else if (CX == 1)
            {
                MenuSetup(Systems[CY]);
                string[] SystemsGames = Directory.GetFiles(Systems[CY]);
                int SystemsGamesAmount = SystemsGames.Length;
                for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++)
                {
                    if (CZ < SystemsGamesAmount) EntryProgram(SystemsGames[CZ].Replace((Systems[CY] + @"\"), ""), EmulatorExes[MenuStorageIntStatic[0, 0, CY]], SystemsGames[CZ], "<FILE>");
                    else MenuEnd();
                }
            }
            else if (CX == 2)
            {
                if (ExplorerString == "")
                {
                    MenuSetup("File Explorer");
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo d in allDrives)
                    {
                        if (d.IsReady == true)
                        {
                            string ExplorerEntryTemp = d.Name;
                            EntryExplorerFolder(ExplorerEntryTemp);
                            CZ++;
                        }
                    }
                    MenuEnd();
                }
                else
                {
                    string[] ExplorerDirectories = Directory.GetDirectories(ExplorerString);
                    string[] ExplorerFiles = Directory.GetFiles(ExplorerString);
                    int TotalSlots = ExplorerDirectories.Length + ExplorerFiles.Length;
                    MenuSetup("Directory Explorer");
                    if (TotalSlots == 0)
                    {
                        MenuEnd();
                    }
                    else
                    {
                        for (CZ = 0; CZ < TotalSlots; CZ++)
                        {
                            if (CZ < ExplorerDirectories.Length)
                            {
                                EntryExplorerFolder(ExplorerDirectories[CZ]);
                            }
                            else
                            {
                                EntryExplorerFile(ExplorerFiles[CZ - ExplorerDirectories.Length]);
                            }
                        }
                        MenuEnd();
                    }
                }
            }
            base.Initialize();
        }

        void MenuSetup(string MenuTitle)
        {
            MenuTitles[CX, CY] = MenuTitle;
            MenuTotalEntries[CX, CY] = MaxEntries;
        }
        void MenuEnd()
        {
            if (CZ == 0)
            {
                MenuTotalEntries[CX, CY] = 1;
                EntryText("Nothing here!");
            }
            else
            {
                MenuTotalEntries[CX, CY] = CZ;
            }
        }

        void EntryLink(string EntryString, int SectionDestination, int MenuDestination, bool Locked, bool Future)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesDestinationMenu[CZ] = MenuDestination;
            MenuEntriesDestinationSection[CZ] = SectionDestination;
            MenuEntriesLocked[CZ] = Locked;
            MenuEntriesShowFuture[CZ] = Future;
            MenuEntriesType[CZ] = 0;
        }
        void EntryText(string EntryString)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesType[CZ] = 1;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
        }
        void EntryProgram(string EntryString, string ProgramString, string FileString, string CommandString)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesProgram[CZ] = ProgramString;
            MenuEntriesFile[CZ] = FileString;
            MenuEntriesCommand[CZ] = CommandString.Replace("<PROG>", ProgramString).Replace("<FILE>", FileString);
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
            MenuEntriesType[CZ] = 2;
        }
        void EntryExit(string EntryString)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesType[CZ] = 3;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
        }
        void EntryBack(string EntryString)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesType[CZ] = 4;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
        }
        void EntryOption(string EntryString, Vector3 VariableLocation, int OptionType, int MinimumIntAllowed, int MaximumIntAllowed, bool RebuildOnChange)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
            MenuEntriesRebuildOnChange[CZ] = RebuildOnChange;
            switch (OptionType)
            {
                case 0:
                    MenuEntriesType[CZ] = 5;
                    break;
                case 1:
                    MenuEntriesType[CZ] = 6;
                    break;
                case 2:
                    MenuEntriesType[CZ] = 7;
                    break;
                default:
                    MenuEntriesType[CZ] = 5;
                    break;
            }

            MenuEntriesIntOptionSegment[CZ] = 1;
            MenuOptions[CX, CY] = true;
            MenuOptionsSave[CX, CY] = false;
            MenuOptionsLocations.Add(CZ);
            if (MinimumIntAllowed > MaximumIntAllowed)
            {
                MaximumIntAllowed = MinimumIntAllowed;
            }
            MenuEntriesIntPosition[CZ] = VariableLocation;
            MenuEntriesIntMinimum[CZ] = MinimumIntAllowed;
            MenuEntriesIntMaximum[CZ] = MaximumIntAllowed;
        }
        void EntryExplorer(string EntryString, string StartingDirectory, int Purpose, Vector3 Location)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
            MenuEntriesType[CZ] = 8;
            MenuEntriesFile[CZ] = StartingDirectory;
            MenuEntriesPurpose[CZ] = Purpose;
            MenuEntriesStringPosition[CZ] = Location;
        }
        void EntryExplorerFolder(string EntryString)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
            MenuEntriesType[CZ] = 9;
        }
        void EntryExplorerFile(string EntryString)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
            MenuEntriesType[CZ] = 10;
        }
        void EntryString(string EntryString, Vector3 Location)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
            MenuEntriesType[CZ] = 11;
            MenuEntriesStringPosition[CZ] = Location;
        }

        void DataManagement(string Element, bool OnBoot)
        {
            switch (Element)
            {
                case "System":
                    List<string> UnaccountedSystemFolders = new List<string>();
                    for (CX = 0; CX < Systems.Length; CX++)
                    {
                        UnaccountedSystemFolders.Add(Systems[CX].Replace(RootRoms, ""));
                    }
                    CX = 0;
                    while (UnaccountedSystemFolders.Count > 0)
                    {
                        bool FoundSystem = false;
                        for (int a = 0; a < Systems.Length && !FoundSystem; a++)
                        {
                            if (UnaccountedSystemFolders[0] == Systems[a].Replace(RootRoms, "") && Directory.Exists(RootConfigSystems + Systems[a].Replace(RootRoms, "")) && File.Exists(RootConfigSystems + Systems[a].Replace(RootRoms, "") + @"\System.txt"))
                            {
                                FoundSystem = true;
                                CZ = a;
                            }
                        }
                        if (FoundSystem)
                        {
                            MenuSystemLoad[CX] = ReadFromFile(RootConfigSystems + Systems[CX].Replace(RootRoms, "") + @"\System.txt");
                            MenuStorageIntStatic[0, 0, CX] = Int32.Parse(MenuSystemLoad[CX][0]);
                            UnaccountedSystemFolders.RemoveAt(0);
                        }
                        else
                        {
                            Directory.CreateDirectory(RootConfigSystems + Systems[CX].Replace(RootRoms, ""));
                            List<string> TestList = new List<string>();
                            TestList.Add(0.ToString());
                            MenuSystemLoad[CX].Add(0.ToString());
                            WriteToFile(RootConfigSystems + Systems[CX].Replace(RootRoms, "") + @"\System.txt", TestList);
                            TestList.Clear();
                            string[] TestArray = Directory.GetFiles(Systems[CX]);
                            for (CY = 0; CY < TestArray.Length; CY++)
                            {
                                List<string> TestList2 = new List<string>();
                                TestList2.Add(0.ToString());
                                string UpcomingPath = RootConfigSystems + TestArray[CY].Replace(RootConfigSystems + Systems[CX].Replace(RootRoms, ""), "").Replace(RootRoms, "") + @".txt";
                                WriteToFile(UpcomingPath, TestList2);
                            }
                            MenuStorageIntStatic[0, 0, CX] = 0;
                            UnaccountedSystemFolders.RemoveAt(0);
                        }
                        CX++;
                    }
                    break;
                case "Backgrounds":
                    if (File.Exists(RootConfig + @"\Backgrounds.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        BackgroundImages.Clear();
                        if (OnBoot)
                        {
                            VideoStorage = ReadFromFile(RootConfig + @"\Backgrounds.txt");
                            int c = 0;
                            if (VideoStorage.Count > c) MenuStorageIntStatic[0, 4, 0] = Int32.Parse(VideoStorage[0]); else MenuStorageIntStatic[0, 4, 0] = 0; c++;
                            while (c < VideoStorage.Count)
                            {
                                MenuStorageStringStatic[0, 4, c] = VideoStorage[c];
                                c++;
                            }
                        }
                        else
                        {
                            VideoStorage.Add(MenuStorageIntStatic[0, 4, 0].ToString());
                            for (int i = 0; i < MenuStorageIntStatic[0, 4, 0]; i++)
                            {
                                VideoStorage.Add(MenuStorageStringStatic[0, 4, i + 1]);
                            }
                        }
                        for (int i = 0; i < MenuStorageIntStatic[0, 4, 0]; i++)
                        {
                            if (VideoStorage.Count > i+1)
                            {
                                if (VideoStorage[i + 1] != null)
                                {
                                    string[] BackgroundsGet = Directory.GetFiles(VideoStorage[i + 1]);
                                    for (int j = 0; j < BackgroundsGet.Length; j++)
                                    {
                                        string extension = Path.GetExtension(BackgroundsGet[j]);
                                        if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                                        {
                                            BackgroundImages.Add(BackgroundsGet[j]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage.Add(0.ToString());
                        MenuStorageIntStatic[0, 4, 0] = 0;
                        WriteToFile(RootConfig + @"\Backgrounds.txt", VideoStorage);
                    }
                    break;
                case "Video":
                    if (File.Exists(RootConfig + @"\Video.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\Video.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 1, 0] = Int32.Parse(VideoStorage[0]); else MenuStorageIntStatic[0, 1, 0] = 0; c++;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 1, 1] = Int32.Parse(VideoStorage[1]); else MenuStorageIntStatic[0, 1, 1] = 2; c++;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 1, 2] = Int32.Parse(VideoStorage[2]); else MenuStorageIntStatic[0, 1, 2] = 0; c++;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 1, 3] = Int32.Parse(VideoStorage[3]); else MenuStorageIntStatic[0, 1, 3] = 12; c++;
                        if (MenuStorageIntStatic[0, 1, 2] >= BackgroundImages.Count) MenuStorageIntStatic[0, 1, 2] = 0;
                        ApplyGraphics(MenuDisplayMode[MenuStorageIntStatic[0, 1, 0]].Width, MenuDisplayMode[MenuStorageIntStatic[0, 1, 0]].Height, MenuStorageIntStatic[0, 1, 1]);
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>();
                        int b = 0;
                        while (MenuDisplayMode[b].Width != 1280 && MenuDisplayMode[b].Height != 720 && MenuDisplayMode.Count != b) b++;
                        b++;
                        VideoStorage.Add(b.ToString());
                        VideoStorage.Add(2.ToString());
                        VideoStorage.Add(0.ToString());
                        MenuStorageIntStatic[0, 1, 0] = b;
                        MenuStorageIntStatic[0, 1, 1] = 2;
                        MenuStorageIntStatic[0, 1, 2] = 0;
                        MenuStorageIntStatic[0, 1, 3] = 12;
                        ApplyGraphics(MenuDisplayMode[b].Width, MenuDisplayMode[b].Height, 2);
                        WriteToFile(RootConfig + @"\Video.txt", VideoStorage);
                    }
                    break;
                case "Clock":
                    if (File.Exists(RootConfig + @"\Clock.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\Clock.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 2, 0] = Int32.Parse(VideoStorage[0]); else MenuStorageIntStatic[0, 1, 0] = 1; c++;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 2, 1] = Int32.Parse(VideoStorage[1]); else MenuStorageIntStatic[0, 1, 1] = 0; c++;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 2, 2] = Int32.Parse(VideoStorage[2]); else MenuStorageIntStatic[0, 1, 2] = 1; c++;
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage.Add(1.ToString());
                        VideoStorage.Add(0.ToString());
                        VideoStorage.Add(1.ToString());
                        MenuStorageIntStatic[0, 2, 0] = 1; //Controls Whether The Clock Will Be Shown
                        MenuStorageIntStatic[0, 2, 1] = 0; //Controls Whether The Clock Will Show 12 Hour Time or 24 Hour Time
                        MenuStorageIntStatic[0, 2, 2] = 1; //Controls Whether The Clock Will Show Seconds
                        WriteToFile(RootConfig + @"\Clock.txt", VideoStorage);
                    }
                    break;
                case "Slide":
                    if (File.Exists(RootConfig + @"\Slide.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\Slide.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 3, 0] = Int32.Parse(VideoStorage[0]); else MenuStorageIntStatic[0, 3, 0] = 1; c++;
                        if (VideoStorage.Count > c) MenuStorageIntStatic[0, 3, 1] = Int32.Parse(VideoStorage[1]); else MenuStorageIntStatic[0, 3, 1] = 16; c++;
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage.Add(1.ToString());
                        VideoStorage.Add(16.ToString());
                        MenuStorageIntStatic[0, 3, 0] = 1; //Controls Whether Entries Will Slide When Entry Is Changed
                        MenuStorageIntStatic[0, 3, 1] = 16; //Controls How Fast Entries Slide
                        WriteToFile(RootConfig + @"\Slide.txt", VideoStorage);
                    }
                    break;
            }
        }

        public void ExecuteCommand(string Command)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "\"" + MenuEntriesProgram[EntrySelection] + "\"";
            startInfo.Arguments = "\"" + Command + "\"";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
        public void DrawString(SpriteFont font, string text, Vector2 pos, int AlignmentX, int AlignmentY, Color color)
        {
            Vector2 size = font.MeasureString(text);
            Vector2 origin = size * 0.5f;

            if (AlignmentX == 0)
                origin.X -= size.X / 2;

            if (AlignmentX == 2)
                origin.X += size.X / 2;

            if (AlignmentY == 0)
                origin.Y -= size.Y / 2;

            if (AlignmentX == 0)
                origin.Y += size.Y / 2;

            spriteBatch.DrawString(font, text, pos, color, 0, origin, TextScale, SpriteEffects.None, 0);
        }
        public void WriteToFile(string Path, List<string> Contents)
        {
            using (StreamWriter writetext = new StreamWriter(Path))
            {
                for (int i = 0; i < Contents.Count; i++)
                {
                    writetext.WriteLine(Contents[i]);
                }
            }
        }
        public List<string> ReadFromFile(string Path)
        {
            List<string> ReadList = new List<string>();
            using (StreamReader readtext = new StreamReader(Path))
            {
                string readMeText = readtext.ReadLine();
                while (readMeText != null)
                {
                    ReadList.Add(readMeText);
                    readMeText = readtext.ReadLine();
                }
            }
            return ReadList;
        }

        public static bool ParentDirectoryExists(string dir)
        {
            DirectoryInfo dirInfo = Directory.GetParent(dir);
            if ((dirInfo != null) && dirInfo.Exists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}