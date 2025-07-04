using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Utils
{
    public static class MetadataUtils
    {
        public static JsonDocument ExtractMetadataFromFileStream(Stream fileStream)
        {
            //To be implemented: Extract metadata from the file stream
            return JsonDocument.Parse("{}");
        }

        public static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                // Documents
                "txt" => "text/plain",
                "pdf" => "application/pdf",
                "json" => "application/json",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "csv" => "text/csv",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "rtf" => "application/rtf",
                "html" => "text/html",
                "xml" => "application/xml",
                "md" => "text/markdown",
                "odt" => "application/vnd.oasis.opendocument.text",
                "ods" => "application/vnd.oasis.opendocument.spreadsheet",
                "odp" => "application/vnd.oasis.opendocument.presentation",
                "doc" => "application/msword",
                "xls" => "application/vnd.ms-excel",
                "ppt" => "application/vnd.ms-powerpoint",
                "log" => "text/plain",
                "yaml" => "application/x-yaml",
                "ini" => "text/plain",

                // Raster images
                "webp" => "image/webp",
                "png" => "image/png",
                "jpeg" => "image/jpeg",
                "jpg" => "image/jpeg",
                "tiff" => "image/tiff",
                "bmp" => "image/bmp",
                "gif" => "image/gif",
                "ico" => "image/x-icon",
                "heic" => "image/heic",
                "heif" => "image/heif",
                "psd" => "image/vnd.adobe.photoshop",
                "exr" => "image/aces",
                "tga" => "image/x-targa",
                "jp2" => "image/jp2",
                "pbm" => "image/x-portable-bitmap",
                "pgm" => "image/x-portable-graymap",
                "ppm" => "image/x-portable-pixmap",
                "xbm" => "image/x-xbitmap",
                "xpm" => "image/x-xpixmap",
                "dib" => "image/bmp",

                // Vector images
                "svg" => "image/svg+xml",
                "eps" => "application/postscript",
                "ai" => "application/postscript",
                "wmf" => "image/wmf",
                "emf" => "image/emf",
                "cdr" => "application/cdr",
                "cgm" => "image/cgm",
                "dxf" => "image/vnd.dxf",
                "dwg" => "image/vnd.dwg",
                "sketch" => "application/octet-stream",
                "fig" => "application/x-xfig",
                "drw" => "application/octet-stream",
                "vsd" => "application/vnd.visio",
                "fla" => "application/octet-stream",
                "swf" => "application/x-shockwave-flash",
                "sai" => "application/octet-stream",
                "svgz" => "image/svg+xml",
                "hpgl" => "application/vnd.hp-hpgl",
                "plt" => "application/plt",

                // Audio
                "wav" => "audio/wav",
                "flac" => "audio/flac",
                "mp3" => "audio/mpeg",
                "aac" => "audio/aac",
                "ogg" => "audio/ogg",
                "m4a" => "audio/mp4",
                "wma" => "audio/x-ms-wma",
                "aiff" => "audio/aiff",
                "opus" => "audio/opus",
                "amr" => "audio/amr",
                "ape" => "audio/ape",
                "mpc" => "audio/mpc",
                "wv" => "audio/wavpack",
                "tta" => "audio/tta",
                "dsf" => "audio/dsf",
                "ra" => "audio/x-pn-realaudio",
                "gsm" => "audio/gsm",
                "au" => "audio/basic",
                "vox" => "audio/voxware",

                // Video
                "webm" => "video/webm",
                "avi" => "video/x-msvideo",
                "mp4" => "video/mp4",
                "ogv" => "video/ogg",
                "mkv" => "video/x-matroska",
                "mov" => "video/quicktime",
                "wmv" => "video/x-ms-wmv",
                "flv" => "video/x-flv",
                "m4v" => "video/x-m4v",
                "mpg" => "video/mpeg",
                "mpeg" => "video/mpeg",
                "3gp" => "video/3gpp",
                "3g2" => "video/3gpp2",
                "asf" => "video/x-ms-asf",
                "rm" => "application/vnd.rn-realmedia",
                "vob" => "video/dvd",
                "ts" => "video/mp2t",
                "mts" => "video/mp2t",
                "m2ts" => "video/mp2t",
                "f4v" => "video/x-f4v",

                _ => "application/octet-stream"
            };
        }
    }
}
