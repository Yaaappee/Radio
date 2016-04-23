using System;
using System.IO;
using System.Text;
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

            var isDeviceInitialize = Bass.BASS_Init(device, freq, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (isDeviceInitialize)
            {
                var song = 1;
                Bass.BASS_RecordInit(-1);

                // recHandle = Bass.BASS_RecordStart(freq, recordChanelsCount, BASSFlag.BASS_STREAM_DECODE, null, IntPtr.Zero);
                // recHandle = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_STREAM_DECODE);

                // create an encoder instance (e.g. for MP3 use EncoderLAME):
                var lame = new EncoderLAME(recHandle)
                {
                    LAME_Bitrate = (int)BaseEncoder.BITRATE.kbps_56,
                    LAME_Mode = EncoderLAME.LAMEMode.Mono,
                    LAME_TargetSampleRate = (int)BaseEncoder.SAMPLERATE.Hz_22050,
                };
                lame.Start(null, IntPtr.Zero, false);

                // create a StreamingServer instance (e.g. SHOUTcast) using the encoder:
                /*
                ICEcast icecast = new ICEcast(lame, false);
                icecast.ServerAddress = "localhost";
                icecast.ServerPort = 8000;
                icecast.AdminPassword = "ibon9p";
                icecast.AdminUsername = "Yaaappee";
                icecast.Password = "ibon9p";
                icecast.PublicFlag = true;
                icecast.SongTitle = fileName; 
                */
                var icecast = new SHOUTcast(lame, true)
                {
                    ServerAddress = "localhost",
                    ServerPort = 8000,
                    AdminPassword = "ibon9pibon9p",
                    Password = "ibon9p",
                    PublicFlag = true,
                    SongTitle = ConvertedString(NoTitle)
                };

                // use the BroadCast class to control streaming:
                broadCast = new BroadCast(icecast) { AutoReconnect = true };
                broadCast.Notification += OnBroadCastNotification;
                broadCast.AutoConnect();

                while (true)
                {
                    fileName = RandomFileName();
                    while (Bass.BASS_ChannelIsActive(recHandle) == BASSActive.BASS_ACTIVE_PLAYING)
                    {
                        var buff = new byte[200];
                        Bass.BASS_ChannelGetData(recHandle, buff, buff.Length);
                    }

                    WriteErrorMessage();
                    Bass.BASS_StreamFree(recHandle);
                    song++;

                    // recHandle = Bass.BASS_RecordStart(freq, 2, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
                    recHandle = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                    Console.WriteLine(recHandle);
                    lame.ChannelHandle = recHandle;
                    Bass.BASS_ChannelUpdate(recHandle, 0);
                    icecast.UpdateTitle(ConvertedString(song + " - " + fileName), null);
                    Console.WriteLine(icecast.SongTitle);
                }
            }
        }

        private static void WriteErrorMessage()
        {
            Console.WriteLine(Bass.BASS_ErrorGetCode());
        }

        private static string RandomFileName()
        {
            var dinfo = new DirectoryInfo(directoryName);
            var files = dinfo.GetFiles();
            var r = new Random();
            var info = new FileInfo(files[r.Next(files.Length)].FullName);
            return info.FullName;
        }

        private static string ConvertedString(string text)
        {
            byte[] sourceByteArray = Encoding.Default.GetBytes(text);
            byte[] resultByteArray = Encoding.Convert(Encoding.Default, Encoding.UTF8, sourceByteArray);
            return Encoding.UTF8.GetString(resultByteArray);
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
