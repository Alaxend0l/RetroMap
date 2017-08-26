using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        Texture2D square;
        bool DebugMode;
        string Root;
        string RootConfig;
        string RootEmulators;
        string RootRoms;
        string RootConfigSystems;
        string[] Systems;
        string[] Emulators;
        string[] EmulatorExes;
        int[] SystemEmulator;
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
        bool MenuReset;
        string[] MenuEntriesDraw; //Menu strings to be drawn on-screen
        string[] MenuEntriesAccess; //Menu strings to be manipulated before being drawn
        string[] MenuEntriesProgram; //Strings for program entries to use to open programs
        string[] MenuEntriesFile; //Strings for program entries to use to open files within programs
        string[] MenuEntriesCommand; //Strings for program entries to use to as commands
        int[] MenuEntriesType; //Types of entries. 0: Link, 1: Option, 2: Open Program
        int[] MenuEntriesDestinationSection; //Entry destination when clicked (Section)
        int[] MenuEntriesDestinationMenu; //Entry destination when clicked (Menu)
        int[] MenuEntriesIntOptionSegment;
        Vector3[] MenuEntriesIntPosition;
        int[] MenuEntriesIntMinimum;
        int[] MenuEntriesIntMaximum;
        bool[] MenuEntriesLocked; //Whether or not the menu entry is currently restricted
        bool[] MenuEntriesShowFuture; //Whether or not the entry shows its destination
        bool[] MenuEntriesShowModel; //Whether or not the entry shows a model

        int[,,] MenuStorageIntStatic;
        string[,,] MenuStorageString;

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
            RootEmulators = Root + @"Emulators\";
            RootRoms = Root + @"Roms\";
            RootConfigSystems = RootConfig + @"Systems\";
            Systems = Directory.GetDirectories(RootRoms);
            Emulators = Directory.GetDirectories(RootEmulators);
            EmulatorExes = new string[Emulators.Length];
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(RootConfig);
            Directory.CreateDirectory(RootEmulators);
            Directory.CreateDirectory(RootRoms);
            Directory.CreateDirectory(RootConfigSystems);
            MenuStorageIntStatic = new int[10, 255, 255];
            MenuSystemLoad = new List<string>[Systems.Length];
            for (CX = 0; CX < Systems.Length; CX++)
            {
                MenuSystemLoad[CX] = new List<string>();
            }
            for (CX = 0; CX < Emulators.Length; CX++)
            {
                EmulatorExes[CX] = Directory.GetFiles(Emulators[CX], "*.exe")[0];
            }
            List<string> UnaccountedSystemFolders = new List<string>();
            for (CX = 0; CX < Systems.Length; CX++)
            {
                UnaccountedSystemFolders.Add(Systems[CX].Replace(RootRoms, ""));
            }
            WriteToFile(RootConfig + "Test.txt", UnaccountedSystemFolders);
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
            if (DebugMode) MenuSelection = 0; else MenuSelection = 1;
            HoldThreshold = 0.3f;
            HoldReducePerClick = 0.05f;
            EntrySelection = 0;
            MaxSections = 16;
            MaxMenus = 64;
            MaxEntries = 256;
            MaxStorage = 10000;
            ChangePrepared = true;
            SectionPast = new int[10000];
            MenuPast = new int[10000];
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
            MenuEntriesType = new int[MaxEntries];
            MenuEntriesDestinationSection = new int[MaxEntries];
            MenuEntriesDestinationMenu = new int[MaxEntries];
            MenuEntriesLocked = new bool[MaxEntries];
            MenuEntriesShowFuture = new bool[MaxEntries];
            MenuEntriesShowModel = new bool[MaxEntries];
            MenuEntriesIntOptionSegment = new int[MaxEntries];
            MenuEntriesIntPosition = new Vector3[MaxEntries];
            MenuEntriesIntMinimum = new int[MaxEntries];
            MenuEntriesIntMaximum = new int[MaxEntries];
            MenuOptionsLocations = new List<int>();
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
                if (MenuEntriesType[EntrySelection] == 5)
                {
                    if (MenuEntriesIntOptionSegment[EntrySelection] == 0)
                    {

                    }
                    else if (MenuEntriesIntOptionSegment[EntrySelection] == 1)
                    {
                        Vector3 IntPosition = MenuEntriesIntPosition[EntrySelection];
                        MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z]--;
                        if (MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] < MenuEntriesIntMinimum[EntrySelection])
                        {
                            MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] = MenuEntriesIntMaximum[EntrySelection];
                        }
                    }
                    MenuOptionsSave[SectionSelection, MenuSelection] = true;
                }
            }
            if (RightPressed == 1)
            {
                if (MenuEntriesType[EntrySelection] == 5)
                {
                    if (MenuEntriesIntOptionSegment[EntrySelection] == 0)
                    {

                    }
                    else if (MenuEntriesIntOptionSegment[EntrySelection] == 1)
                    {
                        Vector3 IntPosition = MenuEntriesIntPosition[EntrySelection];
                        MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z]++;
                        if (MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] > MenuEntriesIntMaximum[EntrySelection])
                        {
                            MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z] = MenuEntriesIntMinimum[EntrySelection];
                        }
                    }
                    MenuOptionsSave[SectionSelection, MenuSelection] = true;
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
                    int SectionLoad = SectionSelection;
                    int MenuLoad = MenuSelection;
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
            var fps = 1 / gameTime.ElapsedGameTime.TotalSeconds;
            Window.Title = fps.ToString();
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
                    if (SectionSelection == 0 && MenuSelection == 3)
                    {
                        Vector3 IntPosition = MenuEntriesIntPosition[i];
                        int IntStatic = MenuStorageIntStatic[(int)IntPosition.X, (int)IntPosition.Y, (int)IntPosition.Z];
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
                }
                spriteBatch.DrawString(font, MenuEntriesDraw[i], new Vector2(0, GraphicsDevice.Viewport.Height / 2 + i * 18 - EntrySelection * 18), Color.White);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

        int KeyFunction(Microsoft.Xna.Framework.Input.Keys Key, int Current)
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
        int PadFunction(Microsoft.Xna.Framework.Input.Buttons Button, int Current)
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
        void RebuildMenu(int X, int Y)
        {
            CX = X;
            CY = Y;
            MenuOptionsLocations.Clear();
            if (CX == 0)
            {
                if (CY == 0) MenuSetup("Debug Menu", "");
                else if (CY == 1) MenuSetup("Exit?", "");
                else if (CY == 2) MenuSetup("Systems", "");
                else if (CY == 3) MenuSetup("Emulators", "");
                else if (CY == 4) MenuSetup("Testing", "");
                else MenuSetup("Placeholder", "");
                for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++)
                {
                    if (CY == 0)
                    {
                        if (CZ == 0) EntrySetup("Console Select", 0, 2, false, true);
                        else if (CZ == 1) EntrySetup("Emulator Select", 0, 3, false, true);
                        else if (CZ == 2) EntrySetup("Settings", 0, 5, false, true);
                        else if (CZ == 3) EntryExit("Exit");
                        else MenuEnd();
                    }
                    else if (CY == 2)
                    {
                        if (CZ < Systems.Length) EntrySetup(Systems[CZ].Replace(RootRoms, ""), 1, CZ, false, true);
                        else MenuEnd();
                    }
                    else if (CY == 3)
                    {
                        if (CZ < Systems.Length) EntryOption(Systems[CZ].Replace(RootRoms, ""), 1, new Vector3(0, 0, CZ), 0, Emulators.Length - 1);
                        else MenuEnd();
                    }
                    else if (CY == 4)
                    {
                        if (CZ == 0) EntrySetup("Link Test", 0, 0, false, true);
                        else if (CZ == 1) EntryProgram("Open Fusion With Sonic 1.gen", RootEmulators + @"Fusion364\Fusion.exe", RootRoms + @"GEN\Sonic1.gen", "<FILE>");
                        else if (CZ == 2) EntryProgram("Open FCEUX With Super Mario Bros.nes", RootEmulators + @"fceux-2.2.3-win32\fceux.exe", RootRoms + @"NES\SuperMarioBros.nes", "<FILE>");
                        else if (CZ == 3) EntryProgram("Open Notepad With Test.txt", @"C:\Windows\notepad.exe", @"C:\Users\Alec Jakopin\Downloads\Test.txt", "<PROG> <FILE>");
                        else if (CZ == 4) EntryProgram("Open Notepad With Test.txt", @"C:\Windows\notepad.exe", @"C:\Users\Alec Jakopin\Downloads\Test.txt", "<PROG> <FILE>");
                        else MenuEnd();
                    }
                    else if (CY == 5)
                    {
                        if (CZ == 0) EntrySetup("Graphics Settings", 0, 6, false, true);
                        else if (CZ == 1) EntrySetup("Audio Settings", 0, 7, false, true);
                        else if (CZ == 2) EntrySetup("Control Settings", 0, 8, false, true);
                        else MenuEnd();
                    }
                    else MenuEnd();
                }
            }
            else if (CX == 1)
            {
                MenuSetup(Systems[CY], "");
                string[] SystemsGames = Directory.GetFiles(Systems[CY]);
                int SystemsGamesAmount = SystemsGames.Length;
                for (CZ = 0; CZ < MenuTotalEntries[CX, CY]; CZ++)
                {
                    if (CZ < SystemsGamesAmount) EntryProgram(SystemsGames[CZ].Replace((Systems[CY] + @"\"), ""), EmulatorExes[MenuStorageIntStatic[0, 0, CY]], SystemsGames[CZ], "<FILE>");
                    else MenuEnd();
                }
            }
            base.Initialize();
        }
        void MenuSetup(string MenuTitle, string OptionsString)
        {
            MenuTitles[CX, CY] = MenuTitle;
            MenuTotalEntries[CX, CY] = MaxEntries;
            MenuOptionsString[CX, CY] = OptionsString;
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
        void EntrySetup(string EntryString, int SectionDestination, int MenuDestination, bool Locked, bool Future)
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
        void EntryOption(string EntryString, int OptionSegment, Vector3 VariableLocation, int MinimumIntAllowed, int MaximumIntAllowed)
        {
            MenuEntriesAccess[CZ] = EntryString;
            MenuEntriesLocked[CZ] = false;
            MenuEntriesShowFuture[CZ] = false;
            MenuEntriesType[CZ] = 5;
            MenuEntriesIntOptionSegment[CZ] = OptionSegment;
            MenuOptions[CX, CY] = true;
            MenuOptionsSave[CX, CY] = false;
            MenuOptionsLocations.Add(CZ);
            if (MinimumIntAllowed > MaximumIntAllowed)
            {
                MaximumIntAllowed = MinimumIntAllowed;
            }
            if (OptionSegment == 0)
            {
                //MenuEntriesIntArraySelect[CX, CY, CZ] = ArraySelect;
                MenuEntriesIntPosition[CZ] = VariableLocation;
                MenuEntriesIntMinimum[CZ] = MinimumIntAllowed;
                MenuEntriesIntMaximum[CZ] = MaximumIntAllowed;
            }
            else if (OptionSegment == 1)
            {
                MenuEntriesIntPosition[CZ] = VariableLocation;
                MenuEntriesIntMinimum[CZ] = MinimumIntAllowed;
                MenuEntriesIntMaximum[CZ] = MaximumIntAllowed;
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
    }
}