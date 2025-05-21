using Godot;
using System;
using System.Collections.Generic;

public partial class SoundController : Node
{
    public static SoundController Instance { get; private set; }

    [Export] public AudioStream WalkingSound { get; set; }
    [Export] public AudioStream LosingLifeSound { get; set; }
    [Export] public AudioStream KickingSound { get; set; }
    [Export] public AudioStream MusicTrack { get; set; }

    [Export(PropertyHint.Range, "0,1")] public float MasterVolume { get; set; } = 1.0f;
    [Export] public bool Mute { get; private set; } = false;

    private AudioStreamPlayer musicPlayer;
    private AudioStreamPlayer sfxPlayer;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;

            musicPlayer = new AudioStreamPlayer();
            AddChild(musicPlayer);

            sfxPlayer = new AudioStreamPlayer();
            AddChild(sfxPlayer);

            ApplyVolume();
            PlayMusic();
        }
        else
        {
            QueueFree();
        }
    }

    private void ApplyVolume()
    {
        float volumeDb = Mute ? -80f : LinearToDecibel(MasterVolume);
        musicPlayer.VolumeDb = volumeDb;
        sfxPlayer.VolumeDb = volumeDb;
    }

    private float LinearToDecibel(float linear)
    {
        return linear <= 0f ? -80f : Mathf.Log(linear) * 20f;
    }

    public void PlayMusic()
    {
        if (MusicTrack != null)
        {
            musicPlayer.Stream = MusicTrack;
            musicPlayer.Play();
        }
    }

    public void StopMusic() => musicPlayer.Stop();

    public void MuteToggle()
    {
        Mute = !Mute;
        ApplyVolume();
    }

    public void VolumeUp()
    {
        MasterVolume = Mathf.Clamp(MasterVolume + 0.1f, 0f, 1f);
        ApplyVolume();
    }

    public void VolumeDown()
    {
        MasterVolume = Mathf.Clamp(MasterVolume - 0.1f, 0f, 1f);
        ApplyVolume();
    }

    public void PlaySfx(string type)
    {
        AudioStream stream = type.ToLower() switch
        {
            "walk" => WalkingSound,
            "kick" => KickingSound,
            "lose" => LosingLifeSound,
            _ => null,
        };

        if (stream != null)
        {
            sfxPlayer.Stream = stream;
            sfxPlayer.Play();
        }
        else
        {
            GD.PrintErr($"No SFX found for type '{type}'");
        }
    }
}
