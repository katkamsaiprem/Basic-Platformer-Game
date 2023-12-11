using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween for smooth animations
using UnityEngine.UI; // For UI elements like Toggle


public class UIManager : MonoBehaviour
{
    // Singleton instance for global access
    public static UIManager instance;

    // List of screen information objects
    [SerializeField]
    List<UIScreenInfo> screenInfos = new List<UIScreenInfo>();

    // Dictionary to map screen types to their info for quick access
    Dictionary<ScreenTypes, UIScreenInfo> screens = new Dictionary<ScreenTypes, UIScreenInfo>();

    // Enum defining different types of UI screens
    public enum ScreenTypes
    {
        MAIN_MENU,
        GAMEPLAY,
        SETTINGS,
        PAUSE,
        COMPLETED,
        GAME_OVER,
        QUIT,
        SHOP,
        INVENTORY,
        ACHIEVEMENTS,
        LEVEL_SELECTION,
        LEADERBOARD,
        RATE_US
    }

    // Keys for saving sound and music settings
    public string music_Key = "MUSIC_ON";
    public string sound_Key = "SOUND_ON";

    // Toggle references for sound and music
    [SerializeField] Toggle soundToggle, musicToggle;

    // Stack to keep track of screen navigation history
    private Stack<UIScreenInfo> screenHistory = new Stack<UIScreenInfo>();

    private void Awake()
    {
        // Singleton pattern setup
        if (instance == null)
        {
            instance = this;
        }

        // Initialize the screens dictionary with screen info from the list
        foreach (UIScreenInfo info in screenInfos)
        {
            screens[info.scrType] = info;
        }
    }

    private void Start()
    {
        // Perform initial UI updates (e.g., settings states)
        MakeUIUpdates();
    }

    void Update()
    {
        // Handle back button functionality (e.g., Android back button)
        if (Input.GetKeyDown(KeyCode.Escape))
            HandleBackFunctionality();
    }

    void HandleBackFunctionality()
    {

        // If there's a history of screens navigated, go back to the last one
        if (screenHistory.Count > 0)
        {
            UIScreenInfo lastScreen = screenHistory.Pop();
            TransitionToScreen(lastScreen);
        }
        else
        { 
            //if you are in the gameplay - (Show Pause Menu)
            //else if you are in the Main Menu - (Quit Menu)
        }
    }

    void ShowPopup(UIScreenInfo screen)
    {
        // Show a UI screen with a pop-up animation
        if (screens.TryGetValue(screen.scrType, out UIScreenInfo currtInfo))
        {
            currtInfo.screenRef.SetActive(true);
            currtInfo.screenRef.transform.
                DOScale(1, currtInfo.animDuration).
                From(0).SetEase(currtInfo.animType);
        }       
    }

    void HidePopup(UIScreenInfo screen)
    {
        // Hide a UI screen with a reverse pop-up animation
        if (screens.TryGetValue(screen.scrType, out UIScreenInfo currtInfo))
        {
            currtInfo.screenRef.transform.
                DOScale(0, currtInfo.animDuration).
                From(1).SetEase(currtInfo.animType)
                .OnComplete(()=> {
                    currtInfo.screenRef.SetActive(false);
                });
        }
    }

    void TransitionToScreen(UIScreenInfo screen)
    {
        // Transition to a specific screen, saving the current screen in history
        if (screenHistory.Count == 0 ||
            screenHistory.Peek() != screen)
        {
            screenHistory.Push(screen);//Saving current screen to history.
        }

        ShowPopup(screen);
    }

    //-------------------------------------------------------------//

    // The following methods control showing or hiding main menu, gameplay, and settings screens
    // They use the ShowPopup and HidePopup methods for transitions

    public void ShowOrHideMainMenu(bool isOpen)
    {
        UIScreenInfo info = screens[ScreenTypes.MAIN_MENU];
        if (isOpen)
        {
            TransitionToScreen(info);
        }
        else
        { 
            HidePopup(info);
        }

       
    }

    public void ShowOrHideGamePlay(bool isOpen)
    {
        UIScreenInfo info = screens[ScreenTypes.GAMEPLAY];
        if (isOpen)
        {
            TransitionToScreen(info);            
            HidePopup(screens[ScreenTypes.MAIN_MENU]);
            HidePopup(screens[ScreenTypes.SETTINGS]);
        }
        else
        {
            HidePopup(info); 
        }
       
    }

    public void ShowOrHideSettings(bool isOpen)
    {
        UIScreenInfo info = screens[ScreenTypes.SETTINGS];
        if (isOpen)                 
            TransitionToScreen(info);      
        else
            HidePopup(info);

       
    }

    public void SetSound()
    {
        if (soundToggle.isOn)
        {
            SoundManager.instance.SetSfxVolume(1);
            SaveDataManager.instance.Save(sound_Key,1);
        }
        else
        {
            SoundManager.instance.SetSfxVolume(0);
            SaveDataManager.instance.Save(sound_Key, 0);
        }

        MakeUIUpdates();
    }

    public void SetMusic()
    {
        if (musicToggle.isOn)
        {
            SoundManager.instance.SetMusicVolume(1);
            SaveDataManager.instance.Save(music_Key, 1);           
        }
        else
        {
            SoundManager.instance.SetMusicVolume(0);
            SaveDataManager.instance.Save(music_Key, 0);
        }

        MakeUIUpdates();
    }


    /// <summary>
    /// Update UI elements based on saved settings (sound and music)
    /// For example, set the toggles on or off based on saved preferences
    /// </summary>
    void MakeUIUpdates()
    {
        if (SaveDataManager.instance.GetInt(music_Key) > 0)
        {
            musicToggle.isOn = true;
            SoundManager.instance.SetMusicVolume(1);
        }
        else
        {
            musicToggle.isOn = false;
            SoundManager.instance.SetMusicVolume(0);
        }

        if (SaveDataManager.instance.GetInt(sound_Key) > 0)
        {
            soundToggle.isOn = true;
            SoundManager.instance.SetSfxVolume(1);
        }
        else
        {
            soundToggle.isOn = false;
            SoundManager.instance.SetSfxVolume(0);
        }
    }
}
