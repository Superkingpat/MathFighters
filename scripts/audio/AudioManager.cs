using Godot;
using System;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    //Singleton pattern
    public static AudioManager Instance { get; private set; }

    [Export] public AudioStream BackgroundMusic { get; set; }
    [Export] public float MusicVolume { get; set; } = 0.5f;
    [Export] public bool PlayMusicOnStart { get; set; } = true;

    [Export] public AudioStream DefaultShootSound { get; set; }
    [Export] public AudioStream GeoTriangleShootSound { get; set; }
    [Export] public AudioStream PenShootSound { get; set; }
    [Export] public float SfxVolume { get; set; } = 0.8f;

    private AudioStreamPlayer musicPlayer;
    private AudioStreamPlayer sfxPlayer;
    private Dictionary<string, AudioStream> weaponSounds = new Dictionary<string, AudioStream>();

    //called when the node enters the scene tree for the first time
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupAudioPlayers();
            InitializeSoundDictionary();
            
            if (PlayMusicOnStart)
            {
                PlayBackgroundMusic();
            }
        }
        else
        {
            QueueFree();
        }
    }

    private void SetupAudioPlayers()
    {
        musicPlayer = new AudioStreamPlayer();
        musicPlayer.VolumeDb = LinearToDecibel(MusicVolume);
        AddChild(musicPlayer);

        sfxPlayer = new AudioStreamPlayer();
        sfxPlayer.VolumeDb = LinearToDecibel(SfxVolume);
        AddChild(sfxPlayer);
    }

    private void InitializeSoundDictionary()
    {
        weaponSounds["default"] = DefaultShootSound;
        weaponSounds["geotriangle"] = GeoTriangleShootSound;
        weaponSounds["pen"] = PenShootSound;
    }

    private float LinearToDecibel(float linear)
    {
        if (linear <= 0)
            return -80f; //silent
        
        return Mathf.Log(linear) * 20f;
    }

    public void PlayBackgroundMusic()
    {
        if (BackgroundMusic != null && musicPlayer != null)
        {
            musicPlayer.Stream = BackgroundMusic;
            musicPlayer.Play();
        }
        else
        {
            GD.PrintErr("Background music or audio player not assigned!");
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicPlayer != null && musicPlayer.Playing)
        {
            musicPlayer.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp(volume, 0f, 1f);
        if (musicPlayer != null)
        {
            musicPlayer.VolumeDb = LinearToDecibel(MusicVolume);
        }
    }

    public void PlayShootSound(string weaponType = "default")
    {
        string key = weaponType.ToLower();
        
        if (sfxPlayer == null)
        {
            GD.PrintErr("SFX audio player not assigned!");
            return;
        }

        if (weaponSounds.ContainsKey(key) && weaponSounds[key] != null)
        {
            sfxPlayer.Stream = weaponSounds[key];
            sfxPlayer.Play();
        }
        else
        {
            if (weaponSounds.ContainsKey("default") && weaponSounds["default"] != null)
            {
                sfxPlayer.Stream = weaponSounds["default"];
                sfxPlayer.Play();
            }
            else
            {
                GD.PrintErr($"No sound assigned for weapon type: {weaponType}");
            }
        }
    }
}