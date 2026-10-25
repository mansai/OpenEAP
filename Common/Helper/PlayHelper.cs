using NAudio.Wave;
using NAudio.Wave.Alsa;
using NAudio.Wave.SampleProviders;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Helper
{
    public class PlayHelper
    {
        public static async Task PlayWaveFileAsync(string filePath)
        {
            var tcs = new TaskCompletionSource();
            var audioFileReader = new AudioFileReader(filePath);
            var waveOutEvent = new WaveOutEvent();

            waveOutEvent.PlaybackStopped += (s, e) =>
            {
                if (e.Exception != null)
                    tcs.TrySetException(e.Exception);
                else
                    tcs.TrySetResult();
            };

            try
            {
                waveOutEvent.Init(audioFileReader);
                waveOutEvent.Play();
                await tcs.Task;
            }
            finally
            {
                waveOutEvent.Dispose();
                audioFileReader.Dispose();
            }
        }
    }
    public static class AudioPlayerFactory
    {
        public static IWavePlayer CreatePlayer()
        {
            if (OperatingSystem.IsWindows())
                return new WaveOut(); // 或 NAudio 3 的 WaveOut
            if (OperatingSystem.IsLinux())
                return new AlsaOut();
            //if (OperatingSystem.IsMacOS())
            //    return new CoreAudioPlayer();
            throw new PlatformNotSupportedException();
        }
    }
    public sealed class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer _outputDevice;
        private readonly MixingSampleProvider _mixer;
        private readonly Dictionary<string, CachedSound> _cache = new();
        private readonly object _lock = new();

        // 统一混音格式，所有音效都按此格式播放
        public static readonly WaveFormat MixerFormat =
            WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        public AudioPlaybackEngine()
        {
            // ReadFully = true：没有声音时输出静音而不是停止，设备保持常开
            _mixer = new MixingSampleProvider(MixerFormat) { ReadFully = true };

            _outputDevice = AudioPlayerFactory.CreatePlayer();
            _outputDevice.Init(_mixer);
            _outputDevice.Play();   // 启动后一直运行
        }

        /// <summary>预加载并缓存音效，返回可播放对象</summary>
        public CachedSound Load(string key, string filePath)
        {
            lock (_lock)
            {
                if (!_cache.TryGetValue(key, out var sound))
                {
                    sound = new CachedSound(filePath);
                    // 格式不匹配时这里需要重采样/声道转换，见下方说明
                    _cache[key] = sound;
                }
                return sound;
            }
        }

        /// <summary>触发播放，立即返回，不阻塞、不需要 await</summary>
        public void Play(string key)
        {
            _cache.TryGetValue(key, out var sound);
            if (sound != null)
            {
                Play(sound);
            }
        }
        public void Play(CachedSound sound)
        {
            ISampleProvider provider = new CachedSoundSampleProvider(sound);
            //声道数匹配：如果音效是单声道，转为立体声
            if (provider.WaveFormat.Channels == 1 && MixerFormat.Channels == 2)
            {
                provider = new MonoToStereoSampleProvider(provider);
            }
            // 采样率匹配：如果采样率不同，使用 WDL 重采样器
            if (provider.WaveFormat.SampleRate != MixerFormat.SampleRate)
            {
                provider = new WdlResamplingSampleProvider(provider, MixerFormat.SampleRate);
            }
            lock (_lock) _mixer.AddMixerInput(provider); // AddMixerInput 非线程安全，必须加锁
        }

        public void Dispose()
        {
            _outputDevice.Stop();
            _outputDevice.Dispose();
        }
    }
    public sealed class CachedSound
    {
        public float[] AudioData { get; }
        public WaveFormat WaveFormat { get; }

        public CachedSound(string filePath)
        {
            using var reader = new AudioFileReader(filePath);
            WaveFormat = reader.WaveFormat;

            var samples = new List<float>((int)(reader.Length / 4));
            var readBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
            int samplesRead;

            // NAudio 3: Read 现在接收 Span<float>
            while ((samplesRead = reader.Read(readBuffer.AsSpan())) > 0)
            {
                samples.AddRange(readBuffer.AsSpan(0, samplesRead).ToArray());
            }

            AudioData = samples.ToArray();
        }
    }
    public sealed class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound _sound;
        private long _position;

        public CachedSoundSampleProvider(CachedSound sound) => _sound = sound;

        public WaveFormat WaveFormat => _sound.WaveFormat;

        // NAudio 3: 签名改为 Span<float>
        public int Read(Span<float> buffer)
        {
            var available = _sound.AudioData.Length - _position;
            var toCopy = (int)Math.Min(available, buffer.Length);
            if (toCopy <= 0) return 0;

            // 源数据是 float[]，目标 buffer 是 Span<float>
            _sound.AudioData.AsSpan((int)_position, toCopy).CopyTo(buffer);
            _position += toCopy;
            return toCopy;
        }
    }
}
