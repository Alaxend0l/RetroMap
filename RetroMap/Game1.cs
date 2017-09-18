using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
        List<string> Systems;
        List<string> EmulatorExes;
        List<string> BackgroundImages;
        int CurrentWidth;
        int CurrentHeight;
        int MaxSections;
        int MaxMenus;
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
        bool LeftPrepared;
        bool RightPrepared;
        bool ChangePrepared;
        float HoldThreshold;
        float HoldReducePerClick;
        float HoldCurrent;
        Vector2 TextScale;
        float TextSpread;
        float TextOffset;
        string TimeString;
        string CalendarString;
        int[] SectionPast; //Previously entered sections.
        int[] MenuPast; //Previously entered menu screens.
        int SectionLast;
        int MenuLast;
        int[] EntryAccess; //Menu item selected in past menu screens.
        string MenuTitle; //Titles of each menu
        bool[,] MenuOptions;
        bool[,] MenuOptionsSave;
        string[,] MenuOptionsString;
        List<int> MenuOptionsLocations;
        List<string>[] MenuSystemLoad;
        List<string> MenuResolutionDisplay;
        List<string> MenuWindowMode;
        List<DisplayMode> MenuDisplayMode;
        int ExplorerPurpose;
        string ExplorerString;
        int ExplorerLastEntry;
        string ExplorerLocation;
        List<Entry> MenuEntriesList;
        string MenuEntriesDraw; //Menu strings to be drawn on-screen
        float TimeSinceChange;

        Dictionary<string, int> StorageInt = new Dictionary<string, int>();
        Dictionary<string, string> StorageString = new Dictionary<string, string>();

        Dictionary<string, int> KeyboardInput = new Dictionary<string, int>();
        Dictionary<string, int> GamepadInput = new Dictionary<string, int>();
        Dictionary<string, int> MouseInput = new Dictionary<string, int>();

        int CX;
        int CY;
        int CZ;
        bool RebuildStop;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //this.IsFixedTimeStep = false;
            //this.graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            DebugMode = false;
            this.IsMouseVisible = true;
            Root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\RetroMap\";
            RootConfig = Root + @"Config\";
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(RootConfig);
            Systems = new List<string>();
            BackgroundImages = new List<string>();
            EmulatorExes = new List<string>();
            MaxSections = 16;
            MaxMenus = 64;
            StorageBuildOnBoot();
            MenuResolutionDisplay = new List<string>();
            MenuDisplayMode = new List<DisplayMode>();
            MenuWindowMode = new List<string> { "Window", "Fullscreen", "Borderless" };
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                MenuResolutionDisplay.Add(mode.ToString());
                MenuDisplayMode.Add(mode);
            }
            MenuEntriesList = new List<Entry>();
            HoldThreshold = 0.3f;
            HoldReducePerClick = 0.05f;
            EntrySelection = 0;
            ChangePrepared = true;
            SectionPast = new int[100];
            MenuPast = new int[100];
            EntryAccess = new int[100];
            MenuOptions = new bool[MaxSections, MaxMenus];
            MenuOptionsSave = new bool[MaxSections, MaxMenus];
            MenuOptionsString = new string[MaxSections, MaxMenus];
            MenuOptionsLocations = new List<int>();
            DataManagement("Roms", true);
            DataManagement("Emulators", true);
            DataManagement("System", true);
            DataManagement("BackgroundCustom", true);
            DataManagement("Backgrounds", true);
            DataManagement("Video", true);
            DataManagement("Clock", true);
            DataManagement("Slide", true);
            DataManagement("Audio", true);
            DataManagement("Font", true);
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
            if (EntrySelection >= MenuEntriesList.Count)
            {
                EntrySelection = MenuEntriesList.Count - 1;
            }
            Entry CE = MenuEntriesList[EntrySelection];
            TextScale = new Vector2((float)StorageInt["Font_Size"] / 72, (float)StorageInt["Font_Size"] / 72);
            TextSpread = (StorageInt["Font_Size"] / 2 * 2.75f);
            if (StorageInt["Clock_Toggle"] == 1)
            {
                if (StorageInt["Clock_Mode"] == 0)
                {
                    if (StorageInt["Clock_Seconds"] == 0)
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
                    if (StorageInt["Clock_Seconds"] == 0)
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
                if (TimeSeconds % 2 == 1 && StorageInt["Clock_Seconds"] == 0)
                {
                    TimeString = TimeString.Replace(":", " ");
                }
            }
            else
            {
                TimeString = " ";
            }
            if (StorageInt["Calendar_Toggle"] == 1)
            {
                string CalendarDay = DateTime.Now.Day.ToString();
                string CalendarMonth = DateTime.Now.Month.ToString();
                int CalendarYear = Int32.Parse(DateTime.Now.Year.ToString());
                string CalendarDayOfTheWeek = DateTime.Now.DayOfWeek.ToString();
                string C = "/";
                if (StorageInt["Calendar_Divider"] == 1) C = "-";
                if (StorageInt["Calendar_Divider"] == 2) C = " ";
                CalendarString = "";
                if (StorageInt["Calendar_DayOfTheWeek"] == 1) CalendarString += CalendarDayOfTheWeek + "    ";
                if (StorageInt["Calendar_ExtraDigit"] == 1) CalendarYear += 10000;
                if (StorageInt["Calendar_MonthName"] == 1)
                {
                    CalendarMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Int32.Parse(CalendarMonth));
                }
                else if (StorageInt["Calendar_MonthName"] == 2)
                {
                    CalendarMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Int32.Parse(CalendarMonth));
                }
                if (StorageInt["Calendar_NumberEnding"] == 1) CalendarDay = AddOrdinal(Int32.Parse(CalendarDay));
                switch (StorageInt["Calendar_Mode"])
                {
                    case 0:
                        //Month, Day, Year
                        if (StorageInt["Calendar_MonthName"] == 0)
                        {
                            CalendarString += CalendarMonth + C + CalendarDay + C + CalendarYear;
                        }
                        else
                        {
                            CalendarString += CalendarMonth + " " + CalendarDay + ", " + CalendarYear;
                        }
                        break;
                    case 1:
                        //Day, Month, Year
                        if (StorageInt["Calendar_MonthName"] == 0)
                        {
                            CalendarString += CalendarDay + C + CalendarMonth + C + CalendarYear;
                        }
                        else
                        {
                            CalendarString += CalendarDay + " " + CalendarMonth + " " + CalendarYear;
                        }
                        break;
                    case 2:
                        //Year, Month, Day
                        if (StorageInt["Calendar_MonthName"] == 0)
                        {
                            CalendarString += CalendarYear + C + CalendarMonth + C + CalendarDay;
                        }
                        else
                        {
                            CalendarString += CalendarYear + " " + CalendarMonth + " " + CalendarDay;
                        }
                        break;
                    case 3:
                        //Day, Month
                        if (StorageInt["Calendar_MonthName"] == 0)
                        {
                            CalendarString += CalendarDay + C + CalendarMonth;
                        }
                        else
                        {
                            CalendarString += CalendarDay + " " + CalendarMonth;
                        }
                        break;
                    case 4:
                        //Month, Day
                        if (StorageInt["Calendar_MonthName"] == 0)
                        {
                            CalendarString += CalendarMonth + C + CalendarDay;
                        }
                        else
                        {
                            CalendarString += CalendarMonth + " " + CalendarDay;
                        }
                        break;
                    case 5:
                        //Day
                        CalendarString += CalendarDay;
                        break;
                    case 6:
                        //Year
                        CalendarString += CalendarYear;
                        break;
                }
                if (StorageInt["Calendar_DayOfTheWeek"] == 2) CalendarString += "   " + CalendarDayOfTheWeek;
            }
            else
            {
                CalendarString = " ";
            }
            if (StorageInt["Slide_Toggle"] == 1)
            {
                TextOffset = TextOffset - TextOffset * StorageInt["Slide_Speed"] * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            InputHandling();
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
                LeftPrepared = true;
            }
            if (RightPressed == 1)
            {
                RightPrepared = true;
            }
            if (ConfirmPressed == 1)
            {
                if (CE.Type == 0)
                {
                    SectionPast[MenuDepth] = SectionSelection;
                    MenuPast[MenuDepth] = MenuSelection;
                    SectionLast = SectionSelection;
                    MenuLast = MenuSelection;
                    EntryAccess[MenuDepth] = EntrySelection;
                    MenuDepth++;
                    SectionSelection = CE.DestinationSection;
                    MenuSelection = CE.DestinationMenu;
                    EntrySelection = 0;
                    ChangePrepared = true;
                }
                else if (CE.Type == 2)
                {
                    if (File.Exists(CE.Program))
                    {
                        ExecuteCommand(CE.Command);
                    }
                }
                else if (CE.Type == 3)
                {
                    Exit();
                }
                else if (CE.Type == 5 || CE.Type == 6 || CE.Type == 7)
                {
                    if (SectionSelection == 0 && MenuSelection == 4)
                    {
                        ApplyGraphics(MenuDisplayMode[StorageInt["Video_ScreenResolution"]].Width, MenuDisplayMode[StorageInt["Video_ScreenResolution"]].Height, StorageInt["Video_ScreenMode"]);
                        List<string> VideoStorage = new List<string>
                        {
                            StorageInt["Video_ScreenResolution"].ToString(),
                            StorageInt["Video_ScreenMode"].ToString(),
                        };
                        WriteToFile(RootConfig + @"\Video.txt", VideoStorage);
                    }
                }
                else if (CE.Type == 8)
                {
                    SectionPast[MenuDepth] = SectionSelection;
                    MenuPast[MenuDepth] = MenuSelection;
                    SectionLast = SectionSelection;
                    MenuLast = MenuSelection;
                    EntryAccess[MenuDepth] = EntrySelection;
                    ExplorerPurpose = CE.Purpose;
                    ExplorerString = CE.File;
                    ExplorerLocation = CE.Key;
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
                else if (CE.Type == 9 && ExplorerPurpose == 0 || CE.Type == 10 && ExplorerPurpose == 1)
                {
                    StorageString[ExplorerLocation] = CE.Title;
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
            if (UpPressed == 2 || DownPressed == 2 || LeftPressed == 2 || RightPressed == 2)
            {
                HoldCurrent += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (HoldCurrent >= HoldThreshold)
                {
                    if (UpPressed == 2) UpPrepared = true;
                    if (DownPressed == 2) DownPrepared = true;
                    if (LeftPressed == 2) LeftPrepared = true;
                    if (RightPressed == 2) RightPrepared = true;
                    HoldCurrent -= HoldReducePerClick;
                }
            }
            else HoldCurrent = 0;
            if (UpPrepared)
            {
                EntrySelection--;
                if (EntrySelection < 0)
                {
                    EntrySelection = MenuEntriesList.Count - 1;
                    TextOffset += TextSpread * (MenuEntriesList.Count - 1);
                }
                else
                {
                    TextOffset -= TextSpread;
                }
                while (MenuEntriesList[EntrySelection].Locked)
                {
                    EntrySelection--;
                    if (EntrySelection < 0)
                    {
                        EntrySelection = MenuEntriesList.Count - 1;
                        TextOffset += TextSpread * (MenuEntriesList.Count - 1);
                    }
                    else
                    {
                        TextOffset -= TextSpread;
                    }
                }
                UpPrepared = false;
                TimeSinceChange = 0;
            }
            if (DownPrepared)
            {
                EntrySelection++;
                if (EntrySelection == MenuEntriesList.Count)
                {
                    EntrySelection = 0;
                    TextOffset -= TextSpread * (MenuEntriesList.Count - 1);
                }
                else
                {
                    TextOffset += TextSpread;
                }
                while (MenuEntriesList[EntrySelection].Locked)
                {
                    EntrySelection++;
                    if (EntrySelection == MenuEntriesList.Count)
                    {
                        EntrySelection = 0;
                        TextOffset -= TextSpread * (MenuEntriesList.Count - 1);
                    }
                    else
                    {
                        TextOffset += TextSpread;
                    }
                }
                DownPrepared = false;
                TimeSinceChange = 0;
            }
            if (LeftPrepared)
            {
                if (CE.Type == 5 || CE.Type == 6 || CE.Type == 7)
                {
                    StorageInt[CE.Key]--;
                    if (StorageInt[CE.Key] < CE.Minimum)
                    {
                        StorageInt[CE.Key] = CE.Maximum;
                    }
                    MenuOptionsSave[SectionSelection, MenuSelection] = true;
                    if (CE.RebuildOnChange)
                    {
                        ClearMenu();
                        RebuildMenu(SectionSelection, MenuSelection);
                    }
                }
                if (SectionSelection == 2)
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
                LeftPrepared = false;
            }
            if (RightPrepared)
            {
                if (CE.Type == 5 || CE.Type == 6 || CE.Type == 7)
                {
                    StorageInt[CE.Key]++;
                    if (StorageInt[CE.Key] > CE.Maximum)
                    {
                        StorageInt[CE.Key] = CE.Minimum;
                    }
                    MenuOptionsSave[SectionSelection, MenuSelection] = true;
                    if (CE.RebuildOnChange)
                    {
                        ClearMenu();
                        RebuildMenu(SectionSelection, MenuSelection);
                    }
                }
                if (CE.Type == 9)
                {
                    ExplorerString = CE.Title;
                    ExplorerLastEntry = EntrySelection;
                    EntrySelection = 0;
                    ChangePrepared = true;
                }
                RightPrepared = false;
            }
            if (ChangePrepared)
            {
                if (MenuOptionsSave[SectionLast, MenuLast])
                {
                    if (SectionLast == 0 && MenuLast == 3)
                    {
                        List<string> SaveList = new List<string>();
                        for (CX = 0; CX < MenuEntriesList.Count; CX++)
                        {
                            SaveList.Add(StorageInt["SystemEmulator_" + CX].ToString());
                        }
                        WriteToFile(RootConfig + @"\System.txt", SaveList);
                    }
                    if (SectionLast == 0 && MenuLast == 5)
                    {
                        List<string> SaveList = new List<string>();
                        SaveList.Add(StorageInt["Audio_Master"].ToString());
                        SaveList.Add(StorageInt["Audio_SFX"].ToString());
                        SaveList.Add(StorageInt["Audio_Music"].ToString());
                        WriteToFile(RootConfig + @"\Audio.txt", SaveList);
                    }
                    if (SectionLast == 0 && MenuLast == 8)
                    {
                        List<string> SaveList = new List<string>();
                        SaveList.Add(StorageInt["Clock_Toggle"].ToString());
                        SaveList.Add(StorageInt["Clock_Mode"].ToString());
                        SaveList.Add(StorageInt["Clock_Seconds"].ToString());
                        SaveList.Add(StorageInt["Clock_Position"].ToString());
                        SaveList.Add(StorageInt["Calendar_Toggle"].ToString());
                        SaveList.Add(StorageInt["Calendar_Mode"].ToString());
                        SaveList.Add(StorageInt["Calendar_DayOfTheWeek"].ToString());
                        SaveList.Add(StorageInt["Calendar_MonthName"].ToString());
                        SaveList.Add(StorageInt["Calendar_NumberEnding"].ToString());
                        SaveList.Add(StorageInt["Calendar_Divider"].ToString());
                        SaveList.Add(StorageInt["Calendar_ExtraDigit"].ToString());
                        WriteToFile(RootConfig + @"\Clock.txt", SaveList);
                    }
                    if (SectionLast == 0 && MenuLast == 9)
                    {
                        List<string> SaveList = new List<string>();
                        SaveList.Add(StorageInt["Slide_Toggle"].ToString());
                        SaveList.Add(StorageInt["Slide_Speed"].ToString());
                        WriteToFile(RootConfig + @"\Slide.txt", SaveList);
                    }
                    if (SectionLast == 0 && MenuLast == 16)
                    {
                        List<string> SaveList = new List<string>();
                        SaveList.Add(StorageInt["Font_Size"].ToString());
                        WriteToFile(RootConfig + @"\Fonts.txt", SaveList);
                    }
                }
                if (SectionLast == 0 && MenuLast == 15)
                {
                    List<string> SaveList = new List<string>();
                    SaveList.Add(StorageInt["Background_Number"].ToString());
                    WriteToFile(RootConfig + @"\BackgroundCustom.txt", SaveList);
                }
                if (SectionLast == 0 && MenuLast == 12 && SectionSelection != 2)
                {
                    List<string> SaveList = new List<string>();
                    SaveList.Add(StorageInt["Directory_Rom_Amount"].ToString());
                    for (CX = 1; CX < MenuEntriesList.Count; CX++)
                    {
                        if (StorageString["Directory_Rom_" + CX] != null)
                        {
                            SaveList.Add(StorageString["Directory_Rom_" + CX].ToString());
                        }
                    }
                    WriteToFile(RootConfig + @"\Roms.txt", SaveList);
                }
                if (SectionLast == 0 && MenuLast == 13 && SectionSelection != 2)
                {
                    List<string> SaveList = new List<string>();
                    SaveList.Add(StorageInt["Directory_Emulator_Amount"].ToString());
                    for (CX = 1; CX < MenuEntriesList.Count; CX++)
                    {
                        if (StorageString["Directory_Emulator_" + CX] != null)
                        {
                            SaveList.Add(StorageString["Directory_Emulator_" + CX].ToString());
                        }
                    }
                    WriteToFile(RootConfig + @"\Emulators.txt", SaveList);
                }
                if (SectionLast == 0 && MenuLast == 14 && SectionSelection != 2)
                {
                    List<string> SaveList = new List<string>();
                    SaveList.Add(StorageInt["Directory_Background_Amount"].ToString());
                    for (CX = 1; CX < MenuEntriesList.Count; CX++)
                    {
                        if (StorageString["Directory_Background_" + CX] != null)
                        {
                            SaveList.Add(StorageString["Directory_Background_" + CX].ToString());
                        }
                    }
                    WriteToFile(RootConfig + @"\Backgrounds.txt", SaveList);
                }
                ClearMenu();
                RebuildMenu(SectionSelection, MenuSelection);
                ChangePrepared = false;
                TimeSinceChange = 0;
            }
            if (StorageInt["Slide_Toggle"] == 0)
            {
                TextOffset = 0;
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0f, 0f, 0f, 1f));
            spriteBatch.Begin();
            spriteBatch.Draw(background, new Rectangle(0, 0, CurrentWidth, CurrentHeight), Color.White);
            spriteBatch.End();
            var fps = 1 / gameTime.ElapsedGameTime.TotalSeconds;
            Window.Title = fps.ToString();
            if (TimeSinceChange < 5)
            {
                TimeSinceChange += (float)gameTime.ElapsedGameTime.TotalSeconds * 30f;
            }
            else
            {
                TimeSinceChange = 5;
            }
            spriteBatch.Begin();
            for (int i = 0; i < MenuEntriesList.Count; i++)
            {
                float TextOrigin = CurrentHeight / 2;
                float TextListY = i * TextSpread;
                float TextEntryY = EntrySelection * TextSpread;
                float TextFinalY = TextListY - TextEntryY;
                float TextX = 0;
                float TextY = TextOrigin + TextFinalY + TextOffset;
                if (TextY > (-TextSpread) && TextY < CurrentHeight + TextSpread)
                {
                    Entry CE = MenuEntriesList[i];
                    MenuEntriesDraw = "";
                    if (i == EntrySelection)
                    {
                        if (DebugMode)
                        {
                            MenuEntriesDraw += "|" + SectionSelection + "|" + MenuSelection + "|" + EntrySelection + "| ";
                        }
                        else
                        {
                            switch (StorageInt["Text_Selection_Icon"])
                            {
                                default: MenuEntriesDraw += ">>> "; break;
                                case 1: MenuEntriesDraw += "> "; break;
                                case 2: MenuEntriesDraw += "||| "; break;
                                case 3:
                                    switch ((int)DateTime.Now.Ticks)
                                    {
                                        default: MenuEntriesDraw += (DateTime.Now.Ticks % 10000000) / (1000) + " "; break;
                                    }
                                    break;
                                case 4:
                                    switch ((DateTime.Now.Ticks % 10000000) / (2000000))
                                    {
                                        case 0: MenuEntriesDraw += "|||    "; break;
                                        case 1: MenuEntriesDraw += " |||   "; break;
                                        case 2: MenuEntriesDraw += "  |||  "; break;
                                        case 3: MenuEntriesDraw += "|  ||  "; break;
                                        case 4: MenuEntriesDraw += "||  |  "; break;
                                    }
                                    break;
                                case 5:
                                    switch ((DateTime.Now.Ticks % 5000000) / (1000000))
                                    {
                                        case 0: MenuEntriesDraw += "|||    "; break;
                                        case 1: MenuEntriesDraw += " |||   "; break;
                                        case 2: MenuEntriesDraw += "  |||  "; break;
                                        case 3: MenuEntriesDraw += "|  ||  "; break;
                                        case 4: MenuEntriesDraw += "||  |  "; break;
                                    }
                                    break;
                                case 6:
                                    switch ((int)Math.Floor(TimeSinceChange))
                                    {
                                        case 0: MenuEntriesDraw += "|    "; break;
                                        case 1: MenuEntriesDraw += "||   "; break;
                                        case 2: MenuEntriesDraw += "|||  "; break;
                                        case 3: MenuEntriesDraw += " ||  "; break;
                                        default: MenuEntriesDraw += "  |  "; break;
                                    }
                                    break;
                                case 7:
                                    switch ((int)Math.Floor(TimeSinceChange))
                                    {
                                        case 0: MenuEntriesDraw += "|  "; break;
                                        case 1: MenuEntriesDraw += "||  "; break;
                                        case 2: MenuEntriesDraw += "|||  "; break;
                                        case 3: MenuEntriesDraw += "||||  "; break;
                                        default: MenuEntriesDraw += "|||||  "; break;
                                    }
                                    break;
                            }
                        }
                    }
                    MenuEntriesDraw += CE.Title;
                    if (CE.Type == 5)
                    {
                        MenuEntriesDraw += "" + StorageInt[CE.Key];
                    }
                    if (CE.Type == 6)
                    {
                        string[] ToggleString = new string[2];
                        ToggleString[0] = "Off";
                        ToggleString[1] = "On";
                        MenuEntriesDraw += "" + ToggleString[StorageInt[CE.Key]];
                    }
                    if (CE.Type == 7)
                    {
                        if (MenuTitle == "Emulators Menu")
                        {
                            if (StorageInt[CE.Key] < EmulatorExes.Count)
                            {
                                MenuEntriesDraw += " | " + EmulatorExes[StorageInt[CE.Key]];
                            }
                            else
                            {
                                if (EmulatorExes.Count == 0)
                                {
                                    MenuEntriesDraw += " | Error: No Emulators Found";
                                }
                                else
                                {
                                    MenuEntriesDraw += " | Error: Emulator # is too high!";
                                }
                            }
                        }
                        switch (MenuEntriesList[i].Key)
                        {
                            case "Video_ScreenResolution":
                                MenuEntriesDraw += "" + MenuDisplayMode[StorageInt[CE.Key]].Width + "x" + MenuDisplayMode[StorageInt[CE.Key]].Height;
                                break;
                            case "Video_ScreenMode":
                                MenuEntriesDraw += "" + MenuWindowMode[StorageInt[CE.Key]];
                                break;
                            case "Background_Number":
                                if (StorageInt[CE.Key] == 0)
                                {
                                    MenuEntriesDraw += "" + "Empty Background";
                                }
                                else if (StorageInt[CE.Key] == 1)
                                {
                                    MenuEntriesDraw += "" + "Current Desktop Background";
                                }
                                else
                                {
                                    MenuEntriesDraw += "" + BackgroundImages[StorageInt[CE.Key] - 2];
                                }
                                break;
                            case "Clock_Mode":
                                switch (StorageInt[CE.Key])
                                {
                                    case 0: MenuEntriesDraw += "" + "12 Hour Time"; break;
                                    case 1: MenuEntriesDraw += "" + "24 Hour Time"; break;
                                }
                                break;
                            case "Clock_Position":
                                switch (StorageInt[CE.Key])
                                {
                                    case 0: MenuEntriesDraw += "" + "Line 1: Date, Time"; break;
                                    case 1: MenuEntriesDraw += "" + "Line 1: Time | Line 2: Date"; break;
                                    case 2: MenuEntriesDraw += "" + "Line 1: Date | Line 2: Time"; break;
                                }
                                break;
                            case "Calendar_DayOfTheWeek":
                                switch (StorageInt[CE.Key])
                                {
                                    case 0: MenuEntriesDraw += "" + "Off"; break;
                                    case 1: MenuEntriesDraw += "" + "Left"; break;
                                    case 2: MenuEntriesDraw += "" + "Right"; break;
                                }
                                break;
                            case "Calendar_Mode":
                                switch(StorageInt[CE.Key])
                                {
                                    case 0: MenuEntriesDraw += "" + "Month/Day/Year"; break;
                                    case 1: MenuEntriesDraw += "" + "Day/Month/Year"; break;
                                    case 2: MenuEntriesDraw += "" + "Year/Month/Day"; break;
                                    case 3: MenuEntriesDraw += "" + "Day/Month"; break;
                                    case 4: MenuEntriesDraw += "" + "Month/Day"; break;
                                    case 5: MenuEntriesDraw += "" + "Day"; break;
                                    case 6: MenuEntriesDraw += "" + "Year"; break;
                                }
                                break;
                            case "Calendar_MonthName":
                                switch (StorageInt[CE.Key])
                                {
                                    case 0: MenuEntriesDraw += "" + "Numbers"; break;
                                    case 1: MenuEntriesDraw += "" + "Abbreviated"; break;
                                    case 2: MenuEntriesDraw += "" + "Full"; break;
                                }
                                break;
                            case "Calendar_Divider":
                                switch (StorageInt[CE.Key])
                                {
                                    case 0: MenuEntriesDraw += "" + "Slashes (/)"; break;
                                    case 1: MenuEntriesDraw += "" + "Dashes (-)"; break;
                                    case 2: MenuEntriesDraw += "" + "Spaces ( )"; break;
                                }
                                break;
                        }
                    }
                    if (CE.Type == 8 || CE.Type == 11)
                    {
                        MenuEntriesDraw += "" + StorageString[CE.Key];
                    }
                    Vector2 TextPosition = new Vector2(TextX, TextY);
                    DrawString(font, MenuEntriesDraw, TextPosition, 0, 0, Color.White);
                }
            }
            switch (StorageInt["Clock_Position"])
            {
                case 0:
                    if (StorageInt["Clock_Toggle"] == 1) DrawString(font, CalendarString + "    " + TimeString, new Vector2(CurrentWidth, 0), 2, 0, Color.White);
                    else DrawString(font, CalendarString, new Vector2(CurrentWidth, 0), 2, 0, Color.White);
                    break;
                case 1:
                    DrawString(font, TimeString, new Vector2(CurrentWidth, 0), 2, 0, Color.White);
                    DrawString(font, CalendarString, new Vector2(CurrentWidth, TextSpread), 2, 0, Color.White);
                    break;
                case 2:
                    DrawString(font, CalendarString, new Vector2(CurrentWidth, 0), 2, 0, Color.White);
                    DrawString(font, TimeString, new Vector2(CurrentWidth, TextSpread), 2, 0, Color.White);
                    break;
            }
            DrawString(font, "RetroMap v3 Beta", new Vector2(CurrentWidth, CurrentHeight), 2, 2, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        void InputHandling()
        {
            CollectInputs();
            if (GamepadInput["Use"] == 0)
            {
                UpPressed = KeyboardInput["W"];
                DownPressed = KeyboardInput["S"];
                LeftPressed = KeyboardInput["A"];
                RightPressed = KeyboardInput["D"];
                ConfirmPressed = KeyboardInput["Enter"];
                BackPressed = KeyboardInput["Escape"];
            }
            else
            {
                if (GamepadInput["LSU"] != 0) UpPressed = GamepadInput["LSU"]; else UpPressed = GamepadInput["DU"];
                if (GamepadInput["LSD"] != 0) DownPressed = GamepadInput["LSD"]; else DownPressed = GamepadInput["DD"];
                if (GamepadInput["LSL"] != 0) LeftPressed = GamepadInput["LSL"]; else LeftPressed = GamepadInput["DL"];
                if (GamepadInput["LSR"] != 0) RightPressed = GamepadInput["LSR"]; else RightPressed = GamepadInput["DR"];
                ConfirmPressed = GamepadInput["A"];
                BackPressed = GamepadInput["B"];
            }
        }
        void CollectInputs()
        {
            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            KeyboardInput["A"] = KeyFunction(Keys.A, KeyboardInput["A"]);
            KeyboardInput["B"] = KeyFunction(Keys.B, KeyboardInput["B"]);
            KeyboardInput["C"] = KeyFunction(Keys.C, KeyboardInput["C"]);
            KeyboardInput["D"] = KeyFunction(Keys.D, KeyboardInput["D"]);
            KeyboardInput["E"] = KeyFunction(Keys.E, KeyboardInput["E"]);
            KeyboardInput["F"] = KeyFunction(Keys.F, KeyboardInput["F"]);
            KeyboardInput["G"] = KeyFunction(Keys.G, KeyboardInput["G"]);
            KeyboardInput["H"] = KeyFunction(Keys.H, KeyboardInput["H"]);
            KeyboardInput["I"] = KeyFunction(Keys.I, KeyboardInput["I"]);
            KeyboardInput["J"] = KeyFunction(Keys.J, KeyboardInput["J"]);
            KeyboardInput["K"] = KeyFunction(Keys.K, KeyboardInput["K"]);
            KeyboardInput["L"] = KeyFunction(Keys.L, KeyboardInput["L"]);
            KeyboardInput["M"] = KeyFunction(Keys.M, KeyboardInput["M"]);
            KeyboardInput["N"] = KeyFunction(Keys.N, KeyboardInput["N"]);
            KeyboardInput["O"] = KeyFunction(Keys.O, KeyboardInput["O"]);
            KeyboardInput["P"] = KeyFunction(Keys.P, KeyboardInput["P"]);
            KeyboardInput["Q"] = KeyFunction(Keys.Q, KeyboardInput["Q"]);
            KeyboardInput["R"] = KeyFunction(Keys.R, KeyboardInput["R"]);
            KeyboardInput["S"] = KeyFunction(Keys.S, KeyboardInput["S"]);
            KeyboardInput["T"] = KeyFunction(Keys.T, KeyboardInput["T"]);
            KeyboardInput["U"] = KeyFunction(Keys.U, KeyboardInput["U"]);
            KeyboardInput["V"] = KeyFunction(Keys.V, KeyboardInput["V"]);
            KeyboardInput["W"] = KeyFunction(Keys.W, KeyboardInput["W"]);
            KeyboardInput["X"] = KeyFunction(Keys.X, KeyboardInput["X"]);
            KeyboardInput["Y"] = KeyFunction(Keys.Y, KeyboardInput["Y"]);
            KeyboardInput["Z"] = KeyFunction(Keys.Z, KeyboardInput["Z"]);
            KeyboardInput["Up"] = KeyFunction(Keys.Up, KeyboardInput["W"]);
            KeyboardInput["Down"] = KeyFunction(Keys.Down, KeyboardInput["X"]);
            KeyboardInput["Left"] = KeyFunction(Keys.Left, KeyboardInput["Y"]);
            KeyboardInput["Right"] = KeyFunction(Keys.Z, KeyboardInput["Z"]);
            KeyboardInput["Enter"] = KeyFunction(Keys.Enter, KeyboardInput["Enter"]);
            KeyboardInput["Escape"] = KeyFunction(Keys.Escape, KeyboardInput["Escape"]);
            if (capabilities.IsConnected)
            {
                GamepadInput["A"] = PadFunction(Buttons.A, GamepadInput["A"]);
                GamepadInput["B"] = PadFunction(Buttons.B, GamepadInput["B"]);
                GamepadInput["X"] = PadFunction(Buttons.X, GamepadInput["X"]);
                GamepadInput["Y"] = PadFunction(Buttons.Y, GamepadInput["Y"]);
                GamepadInput["LB"] = PadFunction(Buttons.LeftShoulder, GamepadInput["LB"]);
                GamepadInput["RB"] = PadFunction(Buttons.RightShoulder, GamepadInput["RB"]);
                GamepadInput["LT"] = PadFunction(Buttons.LeftTrigger, GamepadInput["LT"]);
                GamepadInput["RT"] = PadFunction(Buttons.RightTrigger, GamepadInput["RT"]);
                GamepadInput["LS"] = PadFunction(Buttons.LeftStick, GamepadInput["LS"]);
                GamepadInput["RS"] = PadFunction(Buttons.RightStick, GamepadInput["RS"]);
                GamepadInput["DU"] = PadFunction(Buttons.DPadUp, GamepadInput["DU"]);
                GamepadInput["DD"] = PadFunction(Buttons.DPadDown, GamepadInput["DD"]);
                GamepadInput["DL"] = PadFunction(Buttons.DPadLeft, GamepadInput["DL"]);
                GamepadInput["DR"] = PadFunction(Buttons.DPadRight, GamepadInput["DR"]);
                GamepadInput["LSU"] = PadFunction(Buttons.LeftThumbstickUp, GamepadInput["LSU"]);
                GamepadInput["LSD"] = PadFunction(Buttons.LeftThumbstickDown, GamepadInput["LSD"]);
                GamepadInput["LSL"] = PadFunction(Buttons.LeftThumbstickLeft, GamepadInput["LSL"]);
                GamepadInput["LSR"] = PadFunction(Buttons.LeftThumbstickRight, GamepadInput["LSR"]);
                GamepadInput["RSU"] = PadFunction(Buttons.RightThumbstickUp, GamepadInput["RSU"]);
                GamepadInput["RSD"] = PadFunction(Buttons.RightThumbstickDown, GamepadInput["RSD"]);
                GamepadInput["RSL"] = PadFunction(Buttons.RightThumbstickLeft, GamepadInput["RSL"]);
                GamepadInput["RSR"] = PadFunction(Buttons.RightThumbstickRight, GamepadInput["RSR"]);
                GamepadInput["Start"] = PadFunction(Buttons.Start, GamepadInput["Start"]);
                GamepadInput["Select"] = PadFunction(Buttons.Back, GamepadInput["Select"]);
                if (GamepadInput.ContainsValue(1) || GamepadInput.ContainsValue(2))
                {
                    GamepadInput["Use"] = 10;
                }
                else
                {
                    GamepadInput["Use"] = 0;
                }
            }
            else
            {
                GamepadInput["Use"] = 0;
            }
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
            CreateBackground(StorageInt["Background_Number"]);
        }
        void CreateBackground(int BackgroundID)
        {
            background = new Texture2D(GraphicsDevice, CurrentWidth, CurrentHeight);
            if (BackgroundID == 0)
            {
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

        void ClearMenu()
        {
            while (MenuEntriesList.Count > 0)
            {
                MenuEntriesList[0] = null;
                MenuEntriesList.RemoveAt(0);
            }
        }
        void RebuildMenu(int X, int Y)
        {
            CX = X;
            CY = Y;
            CZ = 0;
            RebuildStop = false;
            MenuOptionsLocations.Clear();
            if (CX == 0)
            {
                switch (CY)
                {
                    case 0:
                        MenuSetup("Debug Menu");
                        DataManagement("Roms", false);
                        DataManagement("System", false);
                        while (CZ < StorageInt["Directory_Rom_Amount"]) EntryLink(new DirectoryInfo(@Systems[CZ]).Name, 1, CZ, false, true);
                        if (StorageInt["Directory_Rom_Amount"] != 0)
                        {
                            EntryLink("----------", 0, 0, true, false);
                        }
                        EntryLink("Setup", 0, 1, false, true);
                        EntryLink("Customize", 0, 7, false, true);
                        EntryExit("Exit");
                        MenuEnd();
                        break;
                    case 1:
                        MenuSetup("Settings Menu");
                        EntryLink("Emulator Matching", 0, 3, false, true);
                        EntryLink("Graphics", 0, 4, false, true);
                        EntryLink("Directories", 0, 11, false, true);
                        MenuEnd();
                        break;
                    case 2:
                        MenuSetup("Systems Menu");
                        MenuEnd();
                        break;
                    case 3:
                        DataManagement("Roms", false);
                        DataManagement("Emulators", false);
                        DataManagement("System", false);
                        MenuSetup("Emulators Menu");
                        while (CZ < Systems.Count) EntryOption(new DirectoryInfo(@Systems[CZ]).Name, "SystemEmulator_" + CZ, 2, 0, EmulatorExes.Count - 1, false);
                        MenuEnd();
                        break;
                    case 4:
                        DataManagement("Backgrounds", false);
                        MenuSetup("Settings");
                        EntryOption("Resolution: ", "Video_ScreenResolution", 2, 0, MenuResolutionDisplay.Count - 1, false);
                        EntryOption("Screen Mode: ", "Video_ScreenMode", 2, 0, 2, false);
                        MenuEnd();
                        break;
                    case 5:
                        MenuSetup("Audio");
                        EntryOption("Audio: ", "Audio_Toggle", 1, 0, 1, true);
                        if (StorageInt["Audio_Toggle"] == 1)
                        {
                            EntryOption("       Volume Master: ", "Audio_Master", 0, 0, 100, false);
                            EntryOption("       Volume SFX: ", "Audio_SFX", 0, 0, 100, false);
                            EntryOption("       Volume Music: ", "Audio_Music", 0, 0, 100, false);
                        }
                        MenuEnd();
                        break;
                    case 6:
                        MenuSetup("Controls");
                        MenuEnd();
                        break;
                    case 7:
                        MenuSetup("Customize");
                        EntryLink("Audio", 0, 5, false, true);
                        EntryLink("Background", 0, 15, false, true);
                        EntryLink("Clock", 0, 8, false, true);
                        EntryLink("Controls", 0, 6, false, true);
                        EntryLink("Font", 0, 16, false, true);
                        EntryLink("Slide", 0, 9, false, true);
                        MenuEnd();
                        break;
                    case 8:
                        MenuSetup("Clock");
                        EntryOption("Clock: ", "Clock_Toggle", 1, 0, 1, true);
                        if (StorageInt["Clock_Toggle"] == 1)
                        {
                            EntryOption("       Mode: ", "Clock_Mode", 2, 0, 1, false);
                            EntryOption("       Seconds: ", "Clock_Seconds", 1, 0, 1, false);
                        }
                        EntryOption("Calendar: ", "Calendar_Toggle", 1, 0, 1, true);
                        if (StorageInt["Calendar_Toggle"] == 1)
                        {
                            EntryOption("       Mode: ", "Calendar_Mode", 2, 0, 6, false);
                            EntryOption("       Day Of The Week: ", "Calendar_DayOfTheWeek", 2, 0, 2, false);
                            EntryOption("       Month Name: ", "Calendar_MonthName", 2, 0, 2, false);
                            EntryOption("       Number Ending: ", "Calendar_NumberEnding", 1, 0, 1, false);
                            EntryOption("       Divider Style: ", "Calendar_Divider", 2, 0, 2, false);
                            EntryOption("       Add 10000 To Year: ", "Calendar_ExtraDigit", 1, 0, 1, false);
                        }
                        if (StorageInt["Clock_Toggle"] == 1 || StorageInt["Calendar_Toggle"] == 1)
                        {
                            EntryOption("Layout: ", "Clock_Position", 2, 0, 2, false);
                        }
                        MenuEnd();
                        break;
                    case 9:
                        MenuSetup("Slide");
                        EntryOption("Slide: ", "Slide_Toggle", 1, 0, 1, true);
                        if (StorageInt["Slide_Toggle"] == 1)
                        {
                            EntryOption("       Slide Speed: ", "Slide_Speed", 0, 0, 30, false);
                        }
                        MenuEnd();
                        break;
                    case 10:
                        MenuSetup("Debug Functions");
                        MenuEnd();
                        break;
                    case 11:
                        MenuSetup("Path Hub");
                        EntryLink("Rom Folders", 0, 12, false, true);
                        EntryLink("Emulator Executables", 0, 13, false, true);
                        EntryLink("Background Folders", 0, 14, false, true);
                        MenuEnd();
                        break;
                    case 12:
                        MenuSetup("Rom Folder Paths");
                        EntryOption("Rom Folders: ", "Directory_Rom_Amount", 0, 0, 250, true);
                        while (CZ < StorageInt["Directory_Rom_Amount"] + 1) EntryExplorer("Folder " + CZ + ": ", "Directory_Rom_" + CZ, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 0);
                        MenuEnd();
                        break;
                    case 13:
                        MenuSetup("Emulator Paths");
                        EntryOption("Amount of Emulators: ", "Directory_Emulator_Amount", 0, 0, 250, true);
                        while (CZ < StorageInt["Directory_Emulator_Amount"] + 1) EntryExplorer("Emulator " + CZ + ": ", "Directory_Emulator_" + CZ, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 1);
                        MenuEnd();
                        break;
                    case 14:
                        MenuSetup("Background Paths");
                        EntryOption("Background Directories: ", "Directory_Background_Amount", 0, 0, 250, true);
                        while (CZ < StorageInt["Directory_Background_Amount"] + 1) EntryExplorer("BG" + CZ + ": ", "Directory_Background_" + CZ, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 0);
                        MenuEnd();
                        break;
                    case 15:
                        DataManagement("Backgrounds", false);
                        MenuSetup("Background Settings");
                        CreateBackground(StorageInt["Background_Number"]);
                        EntryOption("Background: ", "Background_Number", 2, 0, BackgroundImages.Count + 1, true);
                        MenuEnd();
                        break;
                    case 16:
                        MenuSetup("Font Settings");
                        EntryText("Font: Roboto");
                        EntryOption("Font Size: ", "Font_Size", 0, 12, 72, false);
                        MenuEnd();
                        break;
                    default:
                        MenuSetup("Placeholder");
                        MenuEnd();
                        break;
                }
            }
            else if (CX == 1)
            {
                MenuSetup(Systems[CY]);
                string[] SystemsGames = Directory.GetFiles(Systems[CY]);
                int SystemsGamesAmount = SystemsGames.Length;
                if (EmulatorExes.Count != 0)
                {
                    while (CZ < SystemsGamesAmount)
                    {
                        if (Path.GetExtension(SystemsGames[CZ]) != ".sav") EntryProgram(SystemsGames[CZ].Replace((Systems[CY] + @"\"), "").Replace(Path.GetExtension(SystemsGames[CZ]), ""), EmulatorExes[StorageInt["SystemEmulator_" + CY]], SystemsGames[CZ], "<FILE>"); else CZ++;
                    }
                    MenuEnd();
                }
                else
                {
                    EntryText("No Emulators Found!");
                    MenuEnd();
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
                        while (CZ < TotalSlots)
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

        void MenuSetup(string Title)
        {
            MenuTitle = Title;
        }
        void MenuEnd()
        {
            if (CZ == 0)
            {
                EntryText("Nothing here!");
            }
            RebuildStop = true;
        }

        void EntryLink(string EntryString, int SectionDestination, int MenuDestination, bool Locked, bool Future)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    DestinationMenu = MenuDestination,
                    DestinationSection = SectionDestination,
                    Locked = Locked,
                    ShowFuture = Future,
                    Type = 0
                });
            CZ++;
        }
        void EntryText(string EntryString)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    Type = 1
                });
            CZ++;
        }
        void EntryProgram(string EntryString, string ProgramString, string FileString, string CommandString)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    Program = ProgramString,
                    File = FileString,
                    Command = CommandString.Replace("<PROG>", ProgramString).Replace("<FILE>", FileString),
                    Type = 2
                });
            CZ++;
        }
        void EntryExit(string EntryString)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    Type = 3
                });
            CZ++;
        }
        void EntryBack(string EntryString)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    Type = 4
                });
            CZ++;
        }
        void EntryOption(string EntryString, string EntryKey, int OptionType, int MinimumIntAllowed, int MaximumIntAllowed, bool RebuildOnChange)
        {
            switch (OptionType)
            {
                case 0:
                    OptionType = 5;
                    break;
                case 1:
                    OptionType = 6;
                    break;
                case 2:
                    OptionType = 7;
                    break;
                default:
                    OptionType = 5;
                    break;
            }
            MenuOptions[CX, CY] = true;
            MenuOptionsSave[CX, CY] = false;
            MenuOptionsLocations.Add(CZ);
            if (MinimumIntAllowed > MaximumIntAllowed)
            {
                MaximumIntAllowed = MinimumIntAllowed;
            }
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Key = EntryKey,
                    Number = CZ,
                    Type = OptionType,
                    Minimum = MinimumIntAllowed,
                    Maximum = MaximumIntAllowed,
                    RebuildOnChange = RebuildOnChange,
                });
            CZ++;
        }
        void EntryExplorer(string EntryString, string EntryKey, string StartingDirectory, int Purpose)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    File = StartingDirectory,
                    Purpose = Purpose,
                    Key = EntryKey,
                    Type = 8
                });
            if (!StorageString.ContainsKey(MenuEntriesList[MenuEntriesList.Count - 1].Key))
            {
                StorageString.Add(MenuEntriesList[MenuEntriesList.Count - 1].Key, "");
            }
            CZ++;
        }
        void EntryExplorerFolder(string EntryString)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    Type = 9
                });
            CZ++;
        }
        void EntryExplorerFile(string EntryString)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    Type = 10
                });
            CZ++;
        }
        void EntryString(string EntryString, string EntryKey)
        {
            MenuEntriesList.Add(new Entry
                {
                    Title = EntryString,
                    Number = CZ,
                    Key = EntryKey,
                    Type = 11
                });
            if (!StorageString.ContainsKey(MenuEntriesList[MenuEntriesList.Count - 1].Key))
            {
                StorageString.Add(MenuEntriesList[MenuEntriesList.Count - 1].Key, "");
            }
            CZ++;
        }

        void DataManagement(string Element, bool OnBoot)
        {
            switch (Element)
            {
                case "Roms":
                    if (File.Exists(RootConfig + @"\Roms.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        Systems.Clear();
                        if (OnBoot)
                        {
                            VideoStorage = ReadFromFile(RootConfig + @"\Roms.txt");
                            int c = 0;
                            if (VideoStorage.Count > 0) StorageInt["Directory_Rom_Amount"] = Int32.Parse(VideoStorage[0]); else StorageInt["Directory_Rom_Amount"] = 0; c++;
                            while (c < VideoStorage.Count)
                            {
                                if (VideoStorage[c] == "")
                                {
                                    VideoStorage.RemoveAt(c);
                                    StorageInt["Directory_Rom_Amount"]--;
                                }
                                else
                                {
                                    StorageString["Directory_Rom_" + c] = VideoStorage[c];
                                    c++;
                                }
                            }
                        }
                        else
                        {
                            VideoStorage.Add(StorageInt["Directory_Rom_Amount"].ToString());
                            List<int> dfind = new List<int>();
                            for (int i = 0; i < StorageInt["Directory_Rom_Amount"]; i++)
                            {
                                if (StorageString["Directory_Rom_" + (i + 1)] == "")
                                {
                                    dfind.Add((i + 1));
                                }
                                VideoStorage.Add(StorageString["Directory_Rom_" + (i + 1)]);
                            }
                            while (dfind.Count > 0)
                            {
                                VideoStorage.RemoveAt(dfind[dfind.Count-1]);
                                dfind.RemoveAt(dfind.Count-1);
                                StorageInt["Directory_Rom_Amount"]--;
                            }
                        }
                        for (int i = 0; i < StorageInt["Directory_Rom_Amount"]; i++)
                        {
                            if (VideoStorage.Count > i + 1)
                            {
                                if (VideoStorage[i + 1] != null)
                                {
                                    Systems.Add(VideoStorage[i + 1]);
                                    StorageString["Directory_Rom_" + (i + 1)] = VideoStorage[i + 1];
                                }
                            }
                        }
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            0.ToString()
                        };
                        StorageInt["Directory_Rom_Amount"] = 0;
                        WriteToFile(RootConfig + @"\Roms.txt", VideoStorage);
                    }
                    MenuSystemLoad = new List<string>[Systems.Count];
                    for (CX = 0; CX < Systems.Count; CX++)
                    {
                        MenuSystemLoad[CX] = new List<string>();
                    }
                    break;
                case "Emulators":
                    if (File.Exists(RootConfig + @"\Emulators.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        EmulatorExes.Clear();
                        if (OnBoot)
                        {
                            VideoStorage = ReadFromFile(RootConfig + @"\Emulators.txt");
                            int c = 0;
                            if (VideoStorage.Count > c) StorageInt["Directory_Emulator_Amount"] = Int32.Parse(VideoStorage[0]); else StorageInt["Directory_Emulator_Amount"] = 0; c++;
                            while (c < VideoStorage.Count)
                            {
                                StorageString["Directory_Emulator_" + (c)] = VideoStorage[c];
                                c++;
                            }
                        }
                        else
                        {
                            VideoStorage.Add(StorageInt["Directory_Emulator_Amount"].ToString());
                            for (int i = 0; i < StorageInt["Directory_Emulator_Amount"]; i++)
                            {
                                VideoStorage.Add(StorageString["Directory_Emulator_" + (i + 1)]);
                            }
                        }
                        for (int i = 0; i < StorageInt["Directory_Emulator_Amount"]; i++)
                        {
                            if (VideoStorage.Count > i + 1)
                            {
                                if (VideoStorage[i + 1] != null)
                                {
                                    EmulatorExes.Add(VideoStorage[i + 1]);
                                }
                            }
                        }
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            0.ToString()
                        };
                        StorageInt["Directory_Emulator_Amount"] = 0;
                        WriteToFile(RootConfig + @"\Emulators.txt", VideoStorage);
                    }
                    break;
                case "System":
                    if (File.Exists(RootConfig + @"\System.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        if (OnBoot)
                        {
                            VideoStorage = ReadFromFile(RootConfig + @"\System.txt");
                            for (int c = 0; c < StorageInt["Directory_Rom_Amount"]; c++)
                            {
                                if (VideoStorage.Count > c) StorageInt["SystemEmulator_" + c] = Int32.Parse(VideoStorage[c]); else StorageInt["SystemEmulator_" + c] = 0;
                            }
                        }
                        else
                        {
                            for (int c = 0; c < StorageInt["Directory_Rom_Amount"]; c++)
                            {
                                if (!StorageInt.ContainsKey("SystemEmulator_" + c))
                                {
                                    StorageInt.Add("SystemEmulator_" + c, 0);
                                }
                            }
                        }
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>();
                        for (int c = 0; c < StorageInt["Directory_Rom_Amount"]; c++)
                        {
                            VideoStorage.Add(0.ToString());
                            StorageInt["SystemEmulator_" + c] = 0;
                        }
                        WriteToFile(RootConfig + @"\System.txt", VideoStorage);
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
                            if (VideoStorage.Count > c) StorageInt["Directory_Background_Amount"] = Int32.Parse(VideoStorage[0]); else StorageInt["Directory_Background_Amount"] = 0; c++;
                            while (c < VideoStorage.Count)
                            {
                                StorageString["Directory_Background_" + c] = VideoStorage[c];
                                c++;
                            }
                        }
                        else
                        {
                            VideoStorage.Add(StorageInt["Directory_Background_Amount"].ToString());
                            for (int i = 0; i < StorageInt["Directory_Background_Amount"]; i++)
                            {
                                VideoStorage.Add(StorageString["Directory_Background_" + (i + 1)]);
                            }
                        }
                        for (int i = 0; i < StorageInt["Directory_Background_Amount"]; i++)
                        {
                            if (VideoStorage.Count > i + 1)
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
                        if (StorageInt["Background_Number"] > BackgroundImages.Count)
                        {
                            StorageInt["Background_Number"] = 1;
                        }
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            0.ToString()
                        };
                        StorageInt["Directory_Background_Amount"] = 0;
                        WriteToFile(RootConfig + @"\Backgrounds.txt", VideoStorage);
                    }
                    break;
                case "Video":
                    if (File.Exists(RootConfig + @"\Video.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\Video.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) StorageInt["Video_ScreenResolution"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Video_ScreenMode"] = Int32.Parse(VideoStorage[c]); c++;
                        ApplyGraphics(MenuDisplayMode[StorageInt["Video_ScreenResolution"]].Width, MenuDisplayMode[StorageInt["Video_ScreenResolution"]].Height, StorageInt["Video_ScreenMode"]);
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>();
                        int b = 0;
                        while (MenuDisplayMode[b].Width != 1280 && MenuDisplayMode[b].Height != 720 && MenuDisplayMode.Count != b) b++;
                        b++;
                        VideoStorage.Add(b.ToString());
                        VideoStorage.Add(2.ToString());
                        StorageInt["Video_ScreenResolution"] = b;
                        StorageInt["Video_ScreenMode"] = 2;
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
                        if (VideoStorage.Count > c) StorageInt["Clock_Toggle"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Clock_Mode"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Clock_Seconds"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Clock_Position"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Calendar_Toggle"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Calendar_Mode"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Calendar_DayOfTheWeek"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Calendar_MonthName"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Calendar_NumberEnding"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Calendar_Divider"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Calendar_ExtraDigit"] = Int32.Parse(VideoStorage[c]); c++;
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            StorageInt["Clock_Toggle"].ToString(),
                            StorageInt["Clock_Mode"].ToString(),
                            StorageInt["Clock_Seconds"].ToString(),
                            StorageInt["Clock_Position"].ToString(),
                            StorageInt["Calendar_Toggle"].ToString(),
                            StorageInt["Calendar_Mode"].ToString(),
                            StorageInt["Calendar_DayOfTheWeek"].ToString(),
                            StorageInt["Calendar_MonthName"].ToString(),
                            StorageInt["Calendar_NumberEnding"].ToString(),
                            StorageInt["Calendar_Divider"].ToString(),
                            StorageInt["Calendar_ExtraDigit"].ToString()
                        };
                        WriteToFile(RootConfig + @"\Clock.txt", VideoStorage);
                    }
                    break;
                case "Slide":
                    if (File.Exists(RootConfig + @"\Slide.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\Slide.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) StorageInt["Slide_Toggle"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Slide_Speed"] = Int32.Parse(VideoStorage[c]); c++;
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            StorageInt["Slide_Toggle"].ToString(),
                            StorageInt["Slide_Speed"].ToString()
                        };
                        WriteToFile(RootConfig + @"\Slide.txt", VideoStorage);
                    }
                    break;
                case "Audio":
                    if (File.Exists(RootConfig + @"\Audio.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\Audio.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) StorageInt["Audio_Master"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Audio_SFX"] = Int32.Parse(VideoStorage[c]); c++;
                        if (VideoStorage.Count > c) StorageInt["Audio_Music"] = Int32.Parse(VideoStorage[c]); c++;
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            StorageInt["Audio_Master"].ToString(),
                            StorageInt["Audio_SFX"].ToString(),
                            StorageInt["Audio_Music"].ToString()
                        };
                        WriteToFile(RootConfig + @"\Audio.txt", VideoStorage);
                    }
                    break;
                case "BackgroundCustom":
                    if (File.Exists(RootConfig + @"\BackgroundCustom.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\BackgroundCustom.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) StorageInt["Background_Number"] = Int32.Parse(VideoStorage[c]); c++;
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            StorageInt["Background_Number"].ToString()
                        };
                        WriteToFile(RootConfig + @"\BackgroundCustom.txt", VideoStorage);
                    }
                    break;
                case "Font":
                    if (File.Exists(RootConfig + @"\Fonts.txt"))
                    {
                        List<string> VideoStorage = new List<string>();
                        VideoStorage = ReadFromFile(RootConfig + @"\Fonts.txt");
                        int c = 0;
                        if (VideoStorage.Count > c) StorageInt["Font_Size"] = Int32.Parse(VideoStorage[c]); c++;
                    }
                    else
                    {
                        List<string> VideoStorage = new List<string>
                        {
                            StorageInt["Font_Size"].ToString()
                        };
                        WriteToFile(RootConfig + @"\Fonts.txt", VideoStorage);
                    }
                    break;
            }
        }
        void StorageBuildOnBoot()
        {
            StorageInt.Add("Video_ScreenResolution", 0);
            StorageInt.Add("Video_ScreenWidth", 1280);
            StorageInt.Add("Video_ScreenHeight", 720);
            StorageInt.Add("Video_ScreenMode", 2);
            StorageInt.Add("Video_ScreenRefreshRate", 60);
            StorageInt.Add("Audio_Toggle", 1);
            StorageInt.Add("Audio_Master", 50);
            StorageInt.Add("Audio_SFX", 50);
            StorageInt.Add("Audio_Music", 50);
            StorageInt.Add("Background_Toggle", 1);
            StorageInt.Add("Background_Number", 1);
            StorageInt.Add("Background_Amount", 0);
            StorageInt.Add("Slide_Toggle", 1);
            StorageInt.Add("Slide_Speed", 12);
            StorageInt.Add("Clock_Toggle", 1);
            StorageInt.Add("Clock_Mode", 0);
            StorageInt.Add("Clock_Seconds", 0);
            StorageInt.Add("Clock_Position", 0);
            StorageInt.Add("Calendar_Toggle", 1);
            StorageInt.Add("Calendar_Mode", 0);
            StorageInt.Add("Calendar_DayOfTheWeek", 1);
            StorageInt.Add("Calendar_MonthName", 0);
            StorageInt.Add("Calendar_NumberEnding", 0);
            StorageInt.Add("Calendar_Divider", 0);
            StorageInt.Add("Calendar_ExtraDigit", 0);
            StorageInt.Add("Font_Size", 21);
            StorageInt.Add("Directory_Background_Amount", 0);
            StorageInt.Add("Directory_Emulator_Amount", 0);
            StorageInt.Add("Directory_Rom_Amount", 0);
            StorageInt.Add("Text_Selection_Icon", 6);

            KeyboardInput.Add("A", 0);
            KeyboardInput.Add("B", 0);
            KeyboardInput.Add("C", 0);
            KeyboardInput.Add("D", 0);
            KeyboardInput.Add("E", 0);
            KeyboardInput.Add("F", 0);
            KeyboardInput.Add("G", 0);
            KeyboardInput.Add("H", 0);
            KeyboardInput.Add("I", 0);
            KeyboardInput.Add("J", 0);
            KeyboardInput.Add("K", 0);
            KeyboardInput.Add("L", 0);
            KeyboardInput.Add("M", 0);
            KeyboardInput.Add("N", 0);
            KeyboardInput.Add("O", 0);
            KeyboardInput.Add("P", 0);
            KeyboardInput.Add("Q", 0);
            KeyboardInput.Add("R", 0);
            KeyboardInput.Add("S", 0);
            KeyboardInput.Add("T", 0);
            KeyboardInput.Add("U", 0);
            KeyboardInput.Add("V", 0);
            KeyboardInput.Add("W", 0);
            KeyboardInput.Add("X", 0);
            KeyboardInput.Add("Y", 0);
            KeyboardInput.Add("Z", 0);
            KeyboardInput.Add("Up", 0);
            KeyboardInput.Add("Down", 0);
            KeyboardInput.Add("Left", 0);
            KeyboardInput.Add("Right", 0);
            KeyboardInput.Add("Enter", 0);
            KeyboardInput.Add("Escape", 0);

            GamepadInput.Add("A", 0);
            GamepadInput.Add("B", 0);
            GamepadInput.Add("X", 0);
            GamepadInput.Add("Y", 0);
            GamepadInput.Add("LB", 0);
            GamepadInput.Add("RB", 0);
            GamepadInput.Add("LT", 0);
            GamepadInput.Add("RT", 0);
            GamepadInput.Add("LS", 0);
            GamepadInput.Add("RS", 0);
            GamepadInput.Add("DU", 0);
            GamepadInput.Add("DD", 0);
            GamepadInput.Add("DL", 0);
            GamepadInput.Add("DR", 0);
            GamepadInput.Add("LSU", 0);
            GamepadInput.Add("LSD", 0);
            GamepadInput.Add("LSL", 0);
            GamepadInput.Add("LSR", 0);
            GamepadInput.Add("RSU", 0);
            GamepadInput.Add("RSD", 0);
            GamepadInput.Add("RSL", 0);
            GamepadInput.Add("RSR", 0);
            GamepadInput.Add("Start", 0);
            GamepadInput.Add("Select", 0);
            GamepadInput.Add("Use", 0);

            MouseInput.Add("MouseX", 0);
            MouseInput.Add("MouseY", 0);
            MouseInput.Add("MouseDeltaX", 0);
            MouseInput.Add("MouseDeltaY", 0);
            MouseInput.Add("MouseLeftClick", 0);
            MouseInput.Add("MouseRightClick", 0);
            MouseInput.Add("MouseMiddleClick", 0);
            MouseInput.Add("MouseScrollUp", 0);
            MouseInput.Add("MouseScrollDown", 0);
        }

        [DllImport("user32")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);
        public void ExecuteCommand(string Command)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "\"" + MenuEntriesList[EntrySelection].Program + "\"",
                Arguments = "\"" + Command + "\""
            };
            process.StartInfo = startInfo;
            process.Start();
            SetForegroundWindow(process.MainWindowHandle);
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

            if (AlignmentY == 2)
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
        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }

        public class Entry
        {
            public string Title = "Placeholder";
            public string Key = "";
            public int Number = 0;
            public string Program = ""; //Strings for program entries to use to open programs
            public string File = ""; //Strings for program entries to use to open files within programs
            public string Command = ""; //Strings for program entries to use to as commands
            public int Purpose = 0;
            public int Type = 1; //Types of entries
            public int DestinationSection = 0; //Entry destination when clicked (Section)
            public int DestinationMenu = 0; //Entry destination when clicked (Menu)
            public int Minimum = 0;
            public int Maximum = 0;
            public bool RebuildOnChange = false;
            public bool Locked = false; //Whether or not the menu entry is currently restricted
            public bool ShowFuture = false; //Whether or not the entry shows its destination
            public bool ShowModel = false;
        }
    }
}