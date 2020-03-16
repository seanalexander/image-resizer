using ImageMagick;
using System;

namespace image_resizer
{
    class Program
    {
        public static char file_separator = System.IO.Path.DirectorySeparatorChar;
        static void Main()
        {
            Console.WriteLine("Resizing Image(s)");

            string in_file_base = $@".{file_separator}Test{file_separator}ImageIn";
            string out_file_base = $@".{file_separator}Test{file_separator}ImageOut";
            
            Console.WriteLine("Starting Resize of Batch:");
            
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            ResizeImagesInPath(in_file_base, out_file_base);
            Console.WriteLine("Elapsed time\t\t{0} ms", stopWatch.ElapsedMilliseconds);
            
            Console.WriteLine("Completed.");
            Console.ReadLine();
        }

        static void ResizeImagesInPath(string in_file_base, string out_file_base)
        {
            int file_width = 100;
            int file_height = 75;

            VerifyAndCreateDirectories(in_file_base, out_file_base);
            string[] FilesFromFolder = GetFilesFromFolder(in_file_base);

            System.Threading.Tasks.Parallel.ForEach(FilesFromFolder, (in_file_name) =>
            {
                string file_extension = System.IO.Path.GetExtension(in_file_name);
                string file_name_without_extension = System.IO.Path.GetFileNameWithoutExtension(in_file_name);
                string out_file_path = $@"{out_file_base}{file_separator}{file_name_without_extension}.{file_width}x{file_height}{file_extension}";

                bool OperationStatus = ResizeImageFromPath_Width_x_Height(in_file_name, out_file_path, file_width, file_height);
            });
        }

        private static void VerifyAndCreateDirectories(string in_file_base, string out_file_base)
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

        private static string[] GetFilesFromFolder(string in_file_base)
        {
            string[] FilesFromFolder = new string[] { };

            if (System.IO.Directory.Exists(in_file_base))
            {
                FilesFromFolder = System.IO.Directory.GetFiles(in_file_base);
            }
            return FilesFromFolder;
        }

        static bool ResizeImageFromPath_Width_x_Height(string in_file_name, string out_file_path, int file_width, int file_height)
        {
            bool OperationStatus = false;
            // Read from file
            using (MagickImageCollection collection = new MagickImageCollection(in_file_name))
            {
                // Read from file
                collection.Coalesce();
                foreach (MagickImage image in collection)
                {
                    //Console.WriteLine("\tStarting Resize:");
                    System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
                    MagickGeometry size = new MagickGeometry(file_width, file_height)
                    {
                        // This will resize the image to a fixed size without maintaining the aspect ratio.
                        // Normally an image will be resized to fit inside the specified size.
                        IgnoreAspectRatio = true
                    };

                    image.Resize(size);

                    // Save the result
                    image.Write(out_file_path);
                    string json_out = 
$@"{{
    file_path: ""{out_file_path}"",
    elaspsed_time: ""{stopWatch.ElapsedMilliseconds}"",
    file_width: ""{file_width}"",
    file_height: ""{file_height}""
}},";
                    Console.WriteLine(json_out);
                    OperationStatus = true;
                }
            }
            return OperationStatus;
        }
    }
}
