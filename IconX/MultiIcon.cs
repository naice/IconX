using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace IconX
{
    public class MultiIcon 
    {
        // help structs
        private const int ICONHEADER_SIZE = 16;
        private struct TIconHeader 
        {
            public short res0;
            public short imgtype;
            public short imgcount;
        }
        private struct TIconData 
        {
            public byte width;
            public byte height;
            public byte clcount;
            public byte res0;
            public short clplanes;
            public short bpp;
            public int imglen;
            public int imgadr;

            public WriteableBitmap bmp;
        }

        private List<TIconData> icondata;
        private TIconHeader iconheader;

        /// <summary>
        /// Disables the Exceptionlogic at the load process.
        /// </summary>
        public bool SkipErrorImages { get; set; }
        /// <summary>
        /// Count of the Icons.
        /// </summary>
        public int Count { get { return icondata.Count; } }

        public MultiIcon()
        {
            SkipErrorImages = false;

            icondata = new List<TIconData>();

            iconheader = new TIconHeader();
            iconheader.imgcount = -1;
            iconheader.imgtype = 1;
            iconheader.res0 = 0;
        }
        /// <summary>
        /// Creates a Vista/Win7 Style MipMapped Icon from the image.
        /// (16, 24, 32, 48, 128 and 256 pixel per side)
        /// </summary>
        /// <param name="img">Image to create MipMap from. Should be 256 x 256 in size.</param>
        public void CreateMipMap6(WriteableBitmap img)
        {
            clear();

            // Why System.Drawing.Image ? 
            // simply because resizing with any wpf solution i know, does not give me the quality i want. 

            System.Drawing.Image sdImg = null;
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                stream.Position = 0;

                sdImg = System.Drawing.Image.FromStream(stream);
            }


            addImage(createHighRes2(sdImg, 16, 16));
            addImage(createHighRes2(sdImg, 24, 24));
            addImage(createHighRes2(sdImg, 32, 32));
            addImage(createHighRes2(sdImg, 48, 48));
            addImage(createHighRes2(sdImg, 128, 128));
            addImage(createHighRes2(sdImg, 256, 256));
        }
        /// <summary>
        /// Saves icon to file overwrite if exists.
        /// </summary>
        /// <param name="file">Path to the Icon to create.</param>
        public void Save(string file)
        {
            FileStream fstr = new FileStream(file, FileMode.Create, FileAccess.Write);
            writeIcons(fstr);
            fstr.Close();
            fstr.Dispose();
        }
        /// <summary>
        /// Loads the specified Iconfile.
        /// </summary>
        /// <param name="file">Path to the file to load.</param>
        public void Load(string file)
        {
            using (FileStream fstr = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                readIcons(fstr);
                fstr.Close();
            }
        }
        /// <summary>
        /// Returns the image at the specified index.
        /// </summary>
        public WriteableBitmap this[int idx]
        {
            get { return icondata[idx].bmp; }
            set { setImage(idx, value); }
        }

        private void clear()
        {
            foreach (var idat in icondata)
            {
                //idat.bmp.Dispose();
            }

            icondata.Clear();
        }
        private void setImage(int idx, WriteableBitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException("bmp");

            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;

            #region erw. eingabeprüfung
            if (w > 256 || h > 256)
            {
                throw new ArgumentException("Image to large. Width and height < 256.", "bmp");
            }
            if (w % 2 != 0 || h % 2 != 0)
            {
                throw new ArgumentException("Image width or height not power of 2.");
            }
            #endregion

            TIconData icd;
            if (idx == -1) 
                icd = new TIconData();
            else 
                icd = icondata[idx];

            icd.bmp = bmp;
            icd.bpp = 32;
            icd.clcount = 0;
            icd.clplanes = 0;
            icd.width = (byte)(w == 256 ? 0 : w);
            icd.height = (byte)(h == 256 ? 0 : h);
            icd.res0 = 0;
            icd.imgadr = -1;
            icd.imglen = -1;

            if (idx == -1) 
                icondata.Add(icd);
            else
                icondata[idx] = icd;
        }
        private void addImage(WriteableBitmap bmp)
        {
            setImage(-1, bmp);
        }
        private void writePngToStream(Stream str, BitmapSource image)
        {
            PngBitmapEncoder encoder5 = new PngBitmapEncoder();
            encoder5.Frames.Add(BitmapFrame.Create(image));
            encoder5.Save(str);
        }
        private void writeIcons(Stream str)
        {
            #region eingabeprüfung
            if (icondata.Count == 0)
                return;
            #endregion

            BinaryWriter bw = new BinaryWriter(str);

            iconheader.imgcount = (short)icondata.Count;

            bw.Write(iconheader.res0);
            bw.Write(iconheader.imgtype);
            bw.Write(iconheader.imgcount);

            // puffer/offset für die header daten
            int icnt = iconheader.imgcount;
            int offset = icnt * ICONHEADER_SIZE;
            int iconheader0offset = (int)str.Position;
            str.Position += offset;

            // bilddaten png-komprimiert in den stream schreiben.
            for (int i = 0; i < icnt; i++)
            {
                TIconData idat = icondata[i];

                // dateiposition merken, bild in den stream schreiben und
                // länge errechnen.
                idat.imgadr = (int)str.Position;
                writePngToStream(str, idat.bmp);
                idat.imglen = (int)(str.Position - idat.imgadr);

                icondata[i] = idat;
            }

            // zurück zum header offset.
            str.Position = iconheader0offset;

            // header informationen in den stream schreiben.
            for (int i = 0; i < icnt; i++)
            {
                /*
                    0 	1 	Specifies image width in pixels. Can be 0, 255 or a number between 0 to 255. Should be 0 if image width is 256 pixels.
                    1 	1 	Specifies image height in pixels. Can be 0, 255 or a number between 0 to 255. Should be 0 if image height is 256 pixels.
                    2 	1 	Specifies number of colors in the color palette. Should be 0 if the image is truecolor.
                    3 	1 	Reserved. Should be 0.[Notes 1]
                    4 	2 	In .ICO format: Specifies color planes. Should be 0 or 1.[Notes 2]
                    6 	2 	In .ICO format: Specifies bits per pixel. [Notes 3]
                    8 	4 	Specifies the size of the bitmap data in bytes
                    12 	4 	Specifies the offset of bitmap data address in the file
                */

                TIconData idat = icondata[i];
                bw.Write(idat.width);
                bw.Write(idat.height);
                bw.Write(idat.clcount);
                bw.Write(idat.res0);
                bw.Write(idat.clplanes);
                bw.Write(idat.bpp);
                bw.Write(idat.imglen);
                bw.Write(idat.imgadr);
            }
        }
        private void writeIcon(Stream str, TIconHeader ihead, TIconData idat, byte[] icon)
        {
            BinaryWriter bw = new BinaryWriter(str);

            if (icon == null)
            {
                using (MemoryStream mstr = new MemoryStream())
                {
                    writePngToStream(mstr, idat.bmp);
                    icon = mstr.ToArray();
                }
            }

            bw.Write(ihead.res0);
            bw.Write(ihead.imgtype);
            bw.Write((short)1);
            bw.Write(idat.width);
            bw.Write(idat.height);
            bw.Write(idat.clcount);
            bw.Write(idat.res0);
            bw.Write(idat.clplanes);
            bw.Write(idat.bpp);
            bw.Write(idat.imglen);
            bw.Write((int)22);
            bw.Write(icon);
        }
        private void readIcons(Stream str)
        {
            BinaryReader br = new BinaryReader(str);
            Exception ex = new Exception("Read error. Unsupported fileformat!");
            int icnt = 0;

            clear();

            iconheader.res0     = br.ReadInt16();
            iconheader.imgtype  = br.ReadInt16();
            iconheader.imgcount = br.ReadInt16();
            icnt = iconheader.imgcount;

            if (iconheader.imgtype != 1 && iconheader.imgtype != 0)
                throw ex;

            for (int i = 0; i < icnt; i++)
            {
                TIconData idat = new TIconData();

                // read icon from file
                idat.width      = br.ReadByte();
                idat.height     = br.ReadByte();
                idat.clcount    = br.ReadByte();
                idat.res0       = br.ReadByte();
                idat.clplanes   = br.ReadInt16();
                idat.bpp        = br.ReadInt16();
                idat.imglen     = br.ReadInt32();
                idat.imgadr     = br.ReadInt32();

                if (idat.imglen > str.Length || idat.imglen < 0)
                    throw ex;
                if (idat.imgadr + idat.imglen > str.Length || idat.imgadr < -1)
                    throw ex;

                long psav = str.Position;
                str.Position = idat.imgadr;

                using (MemoryStream mstr = new MemoryStream())
                {
                    writeIcon(mstr, iconheader, idat, br.ReadBytes(idat.imglen));
                    mstr.Position = 0;

                    var bmp = readImage(mstr);

                    if (bmp != null)
                        addImage(new WriteableBitmap(bmp));
                    else
                    {
                        if (!SkipErrorImages)
                        {
                            string err = string.Format(
                                "Fehler beim laden des Bildes. (i:{0}, p:{1}, l:{2})\r\n" +
                                "Information: {3}", i, idat.imgadr, idat.imglen);
                            throw new Exception(err);
                        }
                    }
                }

                str.Position = psav;                            
            }
        }
        private WriteableBitmap readImage(Stream str)
        {
            WriteableBitmap result = null;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = str;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                result = new WriteableBitmap(bmp);
            }
            catch 
            {
                result = null;
            }

            return result;
        }

        private WriteableBitmap createHighRes2(System.Drawing.Image img, int w, int h)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(w, h);

            System.Drawing.Graphics dc = System.Drawing.Graphics.FromImage(bmp);
            dc.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            dc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            dc.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            dc.DrawImage(img, 0, 0, w, h);
            dc.Dispose();


            WriteableBitmap wbmp = null;
            using (var mstr = new MemoryStream())
            {
                bmp.Save(mstr, System.Drawing.Imaging.ImageFormat.Png);
                bmp.Dispose();
                mstr.Position = 0;
                var bmpx = new BitmapImage();
                bmpx.BeginInit();
                bmpx.StreamSource = mstr;
                bmpx.CacheOption = BitmapCacheOption.OnLoad;
                bmpx.EndInit();

                wbmp = new WriteableBitmap(bmpx);
            }

            return wbmp;
        }
    }
}
