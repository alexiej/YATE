using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace YATE
{
    public enum ImageContentType
    {
        ImageBmpContentType = 0,
        ImageGifContentType = 1,
        ImageJpegContentType = 2,
        ImageTiffContentType = 3,
        ImagePngContentType = 4
    };

    internal class WpfPayload
    {
        private const string XamlPayloadDirectory = "/Xaml"; // 
        private const string XamlEntryName = "/Document.xaml"; // 
        private const string XamlContentType = "application/vnd.ms-wpf.xaml+xml";
        private const string XamlImageName = "/Image"; // 
        private const string XamlRelationshipFromPackageToEntryPart = "http://schemas.microsoft.com/wpf/2005/10/xaml/entry";
        private const string XamlRelationshipFromXamlPartToComponentPart = "http://schemas.microsoft.com/wpf/2005/10/xaml/component";

        internal static readonly string[] ImageContentTypeName = {
            "image/bmp", "image/gif", "image/jpeg", "image/tiff","image/png"};

        internal static readonly string[] ImageFileExtension = {
            ".bmp", ".gif", ".gif",  ".tiff",".png"};


        Package _package = null;

        private static BitmapEncoder GetBitmapEncoder(ImageContentType imageContentType)
        {
            BitmapEncoder bitmapEncoder;

            switch (imageContentType)
            {
                case ImageContentType.ImageBmpContentType:
                    bitmapEncoder = new BmpBitmapEncoder();
                    break;
                case ImageContentType.ImageGifContentType:
                    bitmapEncoder = new GifBitmapEncoder();
                    break;
                case ImageContentType.ImageJpegContentType:
                    bitmapEncoder = new JpegBitmapEncoder();
                    break;
                case ImageContentType.ImageTiffContentType:
                    bitmapEncoder = new TiffBitmapEncoder();
                    break;
                case ImageContentType.ImagePngContentType:
                    bitmapEncoder = new PngBitmapEncoder();
                    break;
                default:
                    bitmapEncoder = new PngBitmapEncoder();
                    break;
            }
            return bitmapEncoder;
        }

        // Returns a file extension corresponding to a given imageContentType
        private static string GetImageFileExtension(ImageContentType imageContentType)
        {
            return ImageFileExtension[(int)imageContentType];
        }

        private static string GetImageContentName(ImageContentType imageContentType)
        {
            return ImageContentTypeName[(int)imageContentType];
        }

        WpfPayload(Package p = null)
        {
            this._package = p;
        }

        private Package CreatePackage(Stream stream)
        {
            _package = Package.Open(stream, FileMode.Create, FileAccess.ReadWrite);
            return _package;
        }


        // Generates a image part Uri for the given image index
        private static string GetImageName(int imageIndex, ImageContentType imageContentType)
        {
            string imageFileExtension = GetImageFileExtension(imageContentType);
            return XamlImageName + (imageIndex + 1) + imageFileExtension;
        }

        // Generates a relative URL for using from within xaml Image tag.
        private static string GetImageReference(string imageName)
        {
            return "." + imageName; // imageName is supposed to be created by GetImageName method
        }

        private PackagePart CreateWpfEntryPart()
        {
            // Define an entry part uri
            Uri entryPartUri = new Uri(XamlPayloadDirectory + XamlEntryName, UriKind.Relative);

            // Create the main xaml part
            PackagePart part = _package.CreatePart(entryPartUri, XamlContentType, CompressionOption.Normal);
            // Compression is turned off in this mode.
            //NotCompressed = -1,
            // Compression is optimized for a resonable compromise between size and performance. 
            //Normal = 0,
            // Compression is optimized for size. 
            //Maximum = 1,
            // Compression is optimized for performance. 
            //Fast = 2 ,
            // Compression is optimized for super performance. 
            //SuperFast = 3,

            // Create the relationship referring to the entry part
            PackageRelationship entryRelationship = _package.CreateRelationship(entryPartUri, TargetMode.Internal, XamlRelationshipFromPackageToEntryPart);

            return part;
        }

        private void CreateImagePart(PackagePart sourcePart, BitmapSource imageSource, ImageContentType imageContentType, int imageIndex)
        {
            // Generate a new unique image part name
            string imagePartUriString = GetImageName(imageIndex, imageContentType);

            // Define an image part uri
            Uri imagePartUri = new Uri(XamlPayloadDirectory + imagePartUriString, UriKind.Relative);

            // Create a part for the image
            PackagePart imagePart = _package.CreatePart(imagePartUri, GetImageContentName(imageContentType), CompressionOption.NotCompressed);

            // Create the relationship referring from the enrty part to the image part
            PackageRelationship componentRelationship = sourcePart.CreateRelationship(imagePartUri, TargetMode.Internal, XamlRelationshipFromXamlPartToComponentPart);

            // Encode the image data
            BitmapEncoder bitmapEncoder = GetBitmapEncoder(imageContentType);
            bitmapEncoder.Frames.Add(BitmapFrame.Create(imageSource));

            // Save encoded image data into the image part in the package
            Stream imageStream = imagePart.GetStream();
            using (imageStream)
            {
                bitmapEncoder.Save(imageStream);
            }
        }


        internal PackagePart GetWpfEntryPart()
        {
            PackagePart wpfEntryPart = null;

            // Find a relationship to entry part
            PackageRelationshipCollection entryPartRelationships = _package.GetRelationshipsByType(XamlRelationshipFromPackageToEntryPart);
            PackageRelationship entryPartRelationship = null;
            foreach (PackageRelationship packageRelationship in entryPartRelationships)
            {
                entryPartRelationship = packageRelationship;
                break;
            }

            // Get a part referred by this relationship
            if (entryPartRelationship != null)
            {
                // Get entry part uri
                Uri entryPartUri = entryPartRelationship.TargetUri;

                // Get the enrty part
                wpfEntryPart = _package.GetPart(entryPartUri);
            }

            return wpfEntryPart;
        }

        private static int imageIndex = 0;

        [System.Security.SecurityCritical]
        internal static Stream SaveImage(BitmapSource bitmapSource, ImageContentType imageContentType)
        {
            MemoryStream stream = new MemoryStream();
            // Create the wpf package in the stream
            WpfPayload wpfPayload = new WpfPayload();
            using (wpfPayload.CreatePackage(stream))
            {
                PackagePart xamlEntryPart = wpfPayload.CreateWpfEntryPart();

                Stream xamlPartStream = xamlEntryPart.GetStream();
                using (xamlPartStream)
                {
                    // int imageIndex = 0;
                    imageIndex++;
                    string imageReference = GetImageReference(GetImageName(imageIndex, imageContentType));

                    StreamWriter xamlPartWriter = new StreamWriter(xamlPartStream);
                    using (xamlPartWriter)
                    {
                        string xamlText =
                            "<Span xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                            "<InlineUIContainer><Image " +
                            "Width=\"" +
                            bitmapSource.Width + "\" " +
                            "Height=\"" +
                            bitmapSource.Height + "\" " +
                            "><Image.Source><BitmapImage CacheOption=\"OnLoad\" UriSource=\"" +
                            imageReference +
                            "\"/></Image.Source></Image></InlineUIContainer></Span>";
                        xamlPartWriter.Write(xamlText);
                    }
                    wpfPayload.CreateImagePart(xamlEntryPart, bitmapSource, imageContentType, imageIndex);
                }
            }
            return stream;
        }

        static int _wpfPayloadCount; // used to disambiguate between all acts of loading from different WPF payloads.


        internal static object LoadElement(Stream stream)
        {
            Package package = Package.Open(stream, FileMode.Open, FileAccess.Read);
            WpfPayload wpfPayload = new WpfPayload(package);
            PackagePart xamlEntryPart = wpfPayload.GetWpfEntryPart();

            int newWpfPayoutCount = _wpfPayloadCount++;
            Uri payloadUri = new Uri("payload://wpf" + newWpfPayoutCount, UriKind.Absolute);
            Uri entryPartUri = PackUriHelper.Create(payloadUri, xamlEntryPart.Uri); // gives an absolute uri of the entry part
            Uri packageUri = PackUriHelper.GetPackageUri(entryPartUri); // extracts package uri from combined package+part uri
            PackageStore.AddPackage(packageUri, wpfPayload.Package); // Register the package
            ParserContext parserContext = new ParserContext();
            parserContext.BaseUri = entryPartUri;

            object xamlObject = XamlReader.Load(xamlEntryPart.GetStream(), parserContext);
            // Remove the temporary uri from the PackageStore
            PackageStore.RemovePackage(packageUri);

            return xamlObject;
        }

        public Package Package
        {
            get { return _package; }
        }

    };
}
