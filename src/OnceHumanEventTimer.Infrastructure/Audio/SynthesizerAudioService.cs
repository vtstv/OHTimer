// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Media;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Infrastructure.Audio;

/// <summary>
/// Audio service providing synthesized procedural sci-fi chimes and custom WAV file playback.
/// Uses pure C# in-memory PCM WAV synthesis for zero external dependency footprint.
/// </summary>
public sealed class SynthesizerAudioService : IAudioService
{
    public void PlayPreset(SoundPreset preset, double volume, string? customFilePath = null)
    {
        Task.Run(() =>
        {
            try
            {
                if (preset == SoundPreset.Custom && !string.IsNullOrWhiteSpace(customFilePath) && File.Exists(customFilePath))
                {
                    PlayWavFile(customFilePath);
                    return;
                }

                var actualPreset = preset == SoundPreset.Custom ? SoundPreset.CosmicPulse : preset;
                using var stream = AudioWaveformGenerator.GeneratePresetWav(actualPreset, volume);
                using var player = new SoundPlayer(stream);
                player.PlaySync();
            }
            catch
            {
                // Non-critical audio failure fallback
            }
        });
    }

    public void PlayWarning(double volume, string? customFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(customFilePath) && File.Exists(customFilePath))
        {
            PlayPreset(SoundPreset.Custom, volume, customFilePath);
        }
        else
        {
            PlayPreset(SoundPreset.CosmicPulse, volume);
        }
    }

    public void PlayEventStart(double volume)
    {
        PlayPreset(SoundPreset.CelestialBell, volume);
    }

    public void PlayEventEnding(double volume)
    {
        PlayPreset(SoundPreset.EmergencySiren, volume);
    }

    public void PlayTestSound(double volume)
    {
        PlayPreset(SoundPreset.CosmicPulse, volume);
    }

    private static void PlayWavFile(string path)
    {
        using var player = new SoundPlayer(path);
        player.PlaySync();
    }

    public void Dispose()
    {
    }
}
