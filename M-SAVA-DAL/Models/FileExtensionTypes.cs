﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public enum FileExtensionType : short // postgresql doesn't support ushort
    {
        Unknown = 0,

        // Documents
        _TXT = 1,
        _PDF = 2,
        _JSON = 3,
        _DOCX = 4,
        _CSV = 5,
        _XLSX = 6,
        _PPTX = 7,
        _RTF = 8,
        _HTML = 9,
        _XML = 10,
        _MD = 11,
        _ODT = 12,
        _ODS = 13,
        _ODP = 14,
        _DOC = 15,
        _XLS = 16,
        _PPT = 17,
        _LOG = 18,
        _YAML = 19,
        _INI = 20,

        // Raster images
        _WEBP = 1001,
        _PNG = 1002,
        _JPEG = 1003,
        _JPG = 1004,
        _TIFF = 1005,
        _BMP = 1006,
        _GIF = 1007,
        _ICO = 1008,
        _HEIC = 1009,
        _HEIF = 1010,
        _PSD = 1011,
        _EXR = 1012,
        _TGA = 1013,
        _JP2 = 1014,
        _PBM = 1015,
        _PGM = 1016,
        _PPM = 1017,
        _XBM = 1018,
        _XPM = 1019,
        _DIB = 1020,

        // Vector images
        _SVG = 2001,
        _EPS = 2002,
        _AI = 2003,
        _WMF = 2004,
        _EMF = 2005,
        _CDR = 2006,
        _CGM = 2007,
        _DXF = 2008,
        _DWG = 2009,
        _SKETCH = 2010,
        _FIG = 2011,
        _DRW = 2012,
        _VSD = 2013,
        _FLA = 2014,
        _SWF = 2015,
        _SAI = 2016,
        _SVGZ = 2017,
        _HPGL = 2018,
        _PLT = 2019,

        // Audio
        _WAV = 3001,
        _FLAC = 3002,
        _MP3 = 3003,
        _AAC = 3004,
        _OGG = 3005,
        _M4A = 3006,
        _WMA = 3007,
        _AIFF = 3008,
        _OPUS = 3009,
        _AMR = 3010,
        _APE = 3011,
        _MPC = 3012,
        _WV = 3013,
        _TTA = 3014,
        _DSF = 3015,
        _RA = 3016,
        _GSM = 3017,
        _AU = 3018,
        _VOX = 3019,

        // Video
        _WEBM = 4001,
        _AVI = 4002,
        _MP4 = 4003,
        _OGV = 4004,
        _MKV = 4005,
        _MOV = 4006,
        _WMV = 4007,
        _FLV = 4008,
        _M4V = 4009,
        _MPG = 4010,
        _MPEG = 4011,
        _3GP = 4012,
        _3G2 = 4013,
        _ASF = 4014,
        _RM = 4015,
        _VOB = 4016,
        _TS = 4017,
        _MTS = 4018,
        _M2TS = 4019,
        _F4V = 4020
    }
}