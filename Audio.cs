using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BabySmash
{
    public static class Win32Audio
    {
        /// <summary>
        /// Collection of soundname->WAV bytes mappings
        /// </summary>
        private static readonly Dictionary<string, byte[]> cachedWavs = new();

        /// <summary>
        /// Lock this object to protect against concurrent writes to the cachedWavs collection.
        /// </summary>
        private static readonly object cachedWavsLock = new();

        #region NativeAPI
        private const UInt32 SND_ASYNC = 0x0001;
        private const UInt32 SND_MEMORY = 0x004;
        private const UInt32 SND_NOSTOP = 0x0010;

        [DllImport("winmm.dll")]
        private static extern bool PlaySound(byte[] data, IntPtr hMod, UInt32 dwFlags);

        #endregion NativeAPI

        public static void PlayWavResource(string wav)
        {
            byte[] arrWav = GetWavResource(wav);
            PlaySound(arrWav, IntPtr.Zero, SND_ASYNC | SND_MEMORY);
        }

        public static void PlayWavResourceYield(string wav)
        {
            byte[] arrWav = GetWavResource(wav);
            PlaySound(arrWav, IntPtr.Zero, SND_ASYNC | SND_NOSTOP | SND_MEMORY);
        }

        private static byte[] GetWavResource(string wav)
        {
            wav = ".Resources.Sounds." + wav;

            if (cachedWavs.ContainsKey(wav))
            {
                return cachedWavs[wav];
            }

            lock (cachedWavsLock)
            {
                // Recheck inside the lock.
                if (cachedWavs.ContainsKey(wav))
                {
                    return cachedWavs[wav];
                }

                string strName = Assembly.GetExecutingAssembly().GetName().Name + wav;

                // get the resource into a stream
                using Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(strName);
                var arrWav = new Byte[strm.Length];
                strm.Read(arrWav, 0, (int)strm.Length);
                cachedWavs.Add(wav, arrWav);
                return arrWav;
            }
        }
    }
}