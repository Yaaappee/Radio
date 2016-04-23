using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Un4seen.Bass;
using Un4seen.Bass.Misc;

namespace RadioStation
{
    public class RadioStation
    {
        private const string NoTitle = @"No title";

        private static string directoryName =
            @"D:\Music\КУЧА\";

        private readonly int recHandle;

        private readonly BroadCast broadCast;

        private int device = -1;

        private int freq = 44100;

        private int recordChanelsCount = 2;

        private string fileName;

        public RadioStation()
        {
            BassNet.Registration("tobij@gnail.pw", "2X18241914151432");
            Bass.BASS_RecordInit(-1);
            var isDeviceInitialize = Bass.BASS_Init(device, freq, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            if (isDeviceInitialize)
            {
                recHandle = Bass.BASS_StreamCreate(freq, recordChanelsCount, BASSFlag.BASS_STREAM_DECODE, null, IntPtr.Zero);
                var lame = EncoderLame();
                lame.Start(null, IntPtr.Zero, false);

                // ICEcast server = IceCast(lame);
                SHOUTcast server = ShoutCast(lame);

                // use the BroadCast class to control streaming:
                broadCast = new BroadCast(server) { AutoReconnect = true };
                broadCast.Notification += OnBroadCastNotification;
                broadCast.AutoConnect();

                while (!broadCast.IsConnected)
                {
                }

                while (true)
                {
                    WriteErrorMessage();
                    Bass.BASS_StreamFree(recHandle);
                    fileName = RandomFileName();

                    //recHandle = Bass.BASS_RecordStart(freq, recordChanelsCount, BASSFlag.BASS_STREAM_DECODE, null, IntPtr.Zero);
                    recHandle = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                    lame.ChannelHandle = recHandle;
                    Bass.BASS_ChannelUpdate(recHandle, 0);
                    server.UpdateTitle(ConvertedString(fileName), null);
                    Console.WriteLine(fileName);

                    while (Bass.BASS_ChannelIsActive(recHandle) == BASSActive.BASS_ACTIVE_PLAYING)
                    {
                        var buff = new byte[200];
                        Bass.BASS_ChannelGetData(recHandle, buff, buff.Length);
                    }
                }
            }
        }

        private static ICEcast IceCast(EncoderLAME lame)
        {
            ICEcast server = new ICEcast(lame, false)
                {
                    ServerAddress = "localhost",
                    ServerPort = 8000,
                    AdminPassword = "ibon9p",
                    AdminUsername = "Yaaappee",
                    Password = "ibon9p",
                    PublicFlag = true
                };
            return server;
        }

        private static SHOUTcast ShoutCast(EncoderLAME lame)
        {
            var server = new SHOUTcast(lame, true)
                {
                    ServerAddress = "localhost",
                    ServerPort = 8000,
                    AdminPassword = "ibon9pibon9p",
                    Password = "ibon9p",
                    PublicFlag = true,
                    SongTitle = ConvertedString(NoTitle)
                };
            return server;
        }

        private EncoderLAME EncoderLame()
        {
            var lame = new EncoderLAME(recHandle)
                {
                    LAME_Bitrate = (int)BaseEncoder.BITRATE.kbps_128,
                    LAME_Mode = EncoderLAME.LAMEMode.Mono,
                    LAME_TargetSampleRate = (int)BaseEncoder.SAMPLERATE.Hz_44100,
                };
            return lame;
        }

        private static void WriteErrorMessage()
        {
            Console.WriteLine(Bass.BASS_ErrorGetCode());
        }

        private static string RandomFileName()
        {
            var dinfo = new DirectoryInfo(directoryName);
            var files = dinfo.GetFiles().Where(p => p.Name.Contains(".mp3")).ToList();
            var r = new Random();
            var info = new FileInfo(files[r.Next(files.Count)].FullName);
            return info.FullName;
        }

        private static string ConvertedString(string text)
        {
            //return text;
            Encoding srcEncoding = Encoding.GetEncoding("windows-1251");
            Encoding trgEncoding = Encoding.Default;
            byte[] sourceByteArray = srcEncoding.GetBytes(text);
            byte[] resultByteArray = Encoding.Convert(srcEncoding, trgEncoding, sourceByteArray);
            return trgEncoding.GetString(resultByteArray);
        }

        private void OnBroadCastNotification(object sender, BroadCastEventArgs e)
        {
            // Note: this method might be called from another thread (non UI thread)!
            if (broadCast == null)
            {
                return;
            }

            Console.WriteLine(broadCast.IsConnected ? "CONNECTED" : "NOT connected");
        }
    }
}
