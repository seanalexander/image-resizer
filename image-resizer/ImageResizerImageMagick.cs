using System;
using ImageMagick;
using Microsoft.Extensions.Logging;

namespace ImageResizer
{
    public class ImageResizerImageMagick
    {
        private readonly ILogger _logger;
        public static char file_separator = System.IO.Path.DirectorySeparatorChar;
        public ImageResizerImageMagick(ILogger<ImageResizerImageMagick> logger)
        {
            _logger = logger;
        }
        internal void Run()
        {

            _logger.LogInformation("Resizing Image(s)");

            string in_file_base = $@".{file_separator}Test{file_separator}ImageIn";
            string out_file_base = $@".{file_separator}Test{file_separator}ImageOut";

            _logger.LogInformation("Starting Resize of Batch:");

            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            ResizeImagesInPath(in_file_base, out_file_base);
            _logger.LogInformation("Elapsed time\t\t{0} ms", stopWatch.ElapsedMilliseconds);

            _logger.LogInformation("Completed.");
        }
        internal void ResizeImagesInPath(string in_file_base, string out_file_base)
        {
            int file_width = 100;
            int file_height = 75;

            VerifyAndCreateDirectories(in_file_base, out_file_base);
            string[] FilesFromFolder = GetFilesFromFolder(in_file_base);

            System.Threading.Tasks.Parallel.ForEach(FilesFromFolder, (in_file_name) =>
            //foreach (string in_file_name in FilesFromFolder)
            {
                string file_extension = System.IO.Path.GetExtension(in_file_name);
                string file_name_without_extension = System.IO.Path.GetFileNameWithoutExtension(in_file_name);
                string out_file_path = $@"{out_file_base}{file_separator}{file_name_without_extension}.{file_width}x{file_height}{file_extension}";

                bool OperationStatus = ResizeImageFromPath_Width_x_Height(in_file_name, out_file_path, file_width, file_height);
            //}
            });
        }

        internal void VerifyAndCreateDirectories(string in_file_base, string out_file_base)
        {
            if (!System.IO.Directory.Exists(in_file_base))
            {
                System.IO.Directory.CreateDirectory(in_file_base);
            }

            if (!System.IO.Directory.Exists(out_file_base))
            {
                System.IO.Directory.CreateDirectory(out_file_base);
            }
        }

        internal string[] GetFilesFromFolder(string in_file_base)
        {
            string[] FilesFromFolder = new string[] { };

            if (System.IO.Directory.Exists(in_file_base))
            {
                FilesFromFolder = System.IO.Directory.GetFiles(in_file_base);
            }
            return FilesFromFolder;
        }

        internal bool ResizeImageFromPath_Width_x_Height(string in_file_name, string out_file_path, int file_width, int file_height)
        {
            bool OperationStatus = false;
            // Read from file
            using (MagickImageCollection collection = new MagickImageCollection(in_file_name))
            {
                // Read from file
                collection.Coalesce();
                foreach (MagickImage image in collection)
                {
                    System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
                    MagickGeometry size = new MagickGeometry(file_width, file_height)
                    {
                        IgnoreAspectRatio = true
                    };

                    image.Resize(size);

                    // Save the result
                    try
                    {
                        image.Write(out_file_path);
                        OperationStatus = true;
                    }
                    catch (Exception _Exception)
                    {
                        _logger.LogError("Exception: {0}", _Exception.ToString());
                        OperationStatus = false;
                    }
                    string json_out =
$@"{{
    file_path: ""{out_file_path}"",
    elaspsed_time: ""{stopWatch.ElapsedMilliseconds}"",
    file_width: ""{file_width}"",
    file_height: ""{file_height}""
}},";
                    _logger.LogInformation(json_out);
                }
            }
            return OperationStatus;
        }
    }
}
