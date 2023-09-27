using System.Diagnostics;
using NAudio.Lame;
using NAudio.Wave;
using VideoLibrary;

if (args.Length == 0)
{
    Console.WriteLine("Usage: yt2audio youtube_url [artist]");
    return 1; 
}

var youtube = YouTube.Default;
var video = youtube.GetVideo(args[0]); // fetches the video info

Console.WriteLine($"Getting {args[0]} ...");

var sw = new Stopwatch();

sw.Start();
var audioBytes = video.GetBytes(); // downloads the video bytes
sw.Stop();

Console.WriteLine($"Received {(audioBytes.Length/1024):N0} kB in {sw.Elapsed.TotalSeconds:N2} seconds. {(audioBytes.Length/1024)/sw.Elapsed.TotalSeconds:N0} kBps.");

var tempVideoPath = Path.GetTempFileName();
//var tempAudioPath = Path.ChangeExtension(tempVideoPath, ".mp3");
var audioPath = $"{video.Title}.mp3";
if (args.Length > 1 && !video.Title.Contains(args[1], StringComparison.OrdinalIgnoreCase))
    audioPath = $"{args[1]} - {audioPath}";

File.WriteAllBytes(tempVideoPath, audioBytes);

var tag = new ID3TagData
{
    Title = video.Title,
};

if (args.Length > 1)
    tag.Artist = args[1];

ConvertToMp3(tempVideoPath, audioPath, video.AudioBitrate, tag);
//ConvertToFlac(tempVideoPath, tempAudioPath);

// Cleanup
File.Delete(tempVideoPath);

Console.WriteLine("Done!");
return 0;

static void ConvertToMp3(string inputPath, string outputPath, int bitrate, ID3TagData tag)
{
    using var reader = new MediaFoundationReader(inputPath);
    //using var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, LAMEPreset.ABR_256);
    using var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, bitrate, tag);
    
    Console.WriteLine($"Encoding to \"{outputPath}\" ...");
    reader.CopyTo(writer);
}

static void ConvertToFlac(string inputPath, string outputPath)
{
    var process = new Process();
    process.StartInfo.FileName = "ffmpeg";
    process.StartInfo.Arguments = $"-i \"{inputPath}\" \"{outputPath}\"";
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();
    process.WaitForExit();
}
