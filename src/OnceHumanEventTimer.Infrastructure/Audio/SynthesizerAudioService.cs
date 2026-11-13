// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Media;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Infrastructure.Audio;

/// <summary>
/// Audio service providing synthesized procedural sci-fi chimes and custom WAV file playback.
/// Uses pure C# in-memory PCM WAV synthesis for zero external dependency footprint.
/// </summary>
public sealed class SynthesizerAudioService : IAudioService
{
    private readonly object _lock = new();

    public void PlayWarning(double volume, string? customFilePath = null)
    {
        Task.Run(() =>
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(customFilePath) && File.Exists(customFilePath))
                {
                    PlayWavFile(customFilePath);
                    return;
                }

                // Procedural Sci-Fi Alert Chime (Two-tone pulsing chime)
                using var stream = GenerateChimeWav(
                    frequencies: new[] { 660.0, 880.0 },
                    durations: new[] { 0.15, 0.25 },
                    volume: Math.Clamp(volume, 0.0, 1.0));

                using var player = new SoundPlayer(stream);
                player.PlaySync();
            }
            catch
            {
                // Non-critical audio failure fallback
            }
        });
    }

    public void PlayEventStart(double volume)
    {
        Task.Run(() =>
        {
            try
            {
                // Major arpeggio celebration chime: C5 -> E5 -> G5 -> C6
                using var stream = GenerateChimeWav(
                    frequencies: new[] { 523.25, 659.25, 783.99, 1046.50 },
                    durations: new[] { 0.12, 0.12, 0.14, 0.40 },
                    volume: Math.Clamp(volume, 0.0, 1.0));

                using var player = new SoundPlayer(stream);
                player.PlaySync();
            }
            catch
            {
                // Non-critical audio failure fallback
            }
        });
    }

    public void PlayEventEnding(double volume)
    {
        Task.Run(() =>
        {
            try
            {
                // Descending warning tones: A5 -> F5 -> D5
                using var stream = GenerateChimeWav(
                    frequencies: new[] { 880.0, 698.46, 587.33 },
                    durations: new[] { 0.15, 0.15, 0.30 },
                    volume: Math.Clamp(volume, 0.0, 1.0));

                using var player = new SoundPlayer(stream);
                player.PlaySync();
            }
            catch
            {
                // Non-critical audio failure fallback
            }
        });
    }

    public void PlayTestSound(double volume)
    {
        PlayWarning(volume);
    }

    private static void PlayWavFile(string path)
    {
        using var player = new SoundPlayer(path);
        player.PlaySync();
    }

    /// <summary>
    /// Generates a valid 16-bit mono 44.1kHz PCM WAV stream with an exponential decay envelope.
    /// </summary>
    private static MemoryStream GenerateChimeWav(double[] frequencies, double[] durations, double volume)
    {
        const int sampleRate = 44100;
        int totalSamples = 0;
        for (int i = 0; i < durations.Length; i++)
        {
            totalSamples += (int)(sampleRate * durations[i]);
        }

        short[] samples = new short[totalSamples];
        int sampleIndex = 0;

        for (int note = 0; note < frequencies.Length; note++)
        {
            double freq = frequencies[note];
            int noteSamples = (int)(sampleRate * durations[note]);

            for (int i = 0; i < noteSamples; i++)
            {
                double t = (double)i / sampleRate;
                // Exponential decay envelope
                double envelope = Math.Exp(-3.5 * t / durations[note]);
                // Sine wave with slight 2nd harmonic overtone for a celestial bell timbre
                double fundamental = Math.Sin(2.0 * Math.PI * freq * t);
                double overtone = 0.25 * Math.Sin(4.0 * Math.PI * freq * t);
                double signal = (fundamental + overtone) * envelope * volume;

                samples[sampleIndex++] = (short)Math.Clamp(signal * short.MaxValue * 0.7, short.MinValue, short.MaxValue);
            }
        }

        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        int subChunk2Size = samples.Length * 2;
        int chunkSize = 36 + subChunk2Size;

        // RIFF header
        writer.Write(new[] { 'R', 'I', 'F', 'F' });
        writer.Write(chunkSize);
        writer.Write(new[] { 'W', 'A', 'V', 'E' });

        // "fmt " subchunk
        writer.Write(new[] { 'f', 'm', 't', ' ' });
        writer.Write(16); // Subchunk1Size for PCM
        writer.Write((short)1); // AudioFormat 1 = PCM
        writer.Write((short)1); // NumChannels = 1 (Mono)
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2); // ByteRate = SampleRate * NumChannels * BitsPerSample/8
        writer.Write((short)2); // BlockAlign = NumChannels * BitsPerSample/8
        writer.Write((short)16); // BitsPerSample = 16

        // "data" subchunk
        writer.Write(new[] { 'd', 'a', 't', 'a' });
        writer.Write(subChunk2Size);

        for (int i = 0; i < samples.Length; i++)
        {
            writer.Write(samples[i]);
        }

        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public void Dispose()
    {
    }
}
