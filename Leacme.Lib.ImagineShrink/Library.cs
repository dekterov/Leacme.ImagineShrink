// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using nQuant;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Leacme.Lib.ImagineShrink {
	public class Library {

		/// <summary>
		///	Optimized and processes the .png image.
		/// /// </summary>
		/// <param name="path">Absolute path to image file.</param>
		/// <returns>The processed image stream.</returns>
		public Stream ShrinkPNG(string path) {
			using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(path)) {
				const int alphaTransparency = 0;
				const int alphaFader = 0;
				var quantizer = new WuQuantizer();
				var retMS = new MemoryStream();
				using (var quantized = quantizer.QuantizeImage(bitmap, alphaTransparency, alphaFader)) {
					quantized.Save(retMS, ImageFormat.Png);
				}
				return retMS;
			}
		}

		/// <summary>
		/// Optimized and processes the .jpeg/.jpg image.
		/// /// </summary>
		/// <param name="path">Absolute path to image file.</param>
		/// <returns>The processed image stream.</returns>
		public Stream ShrinkJpeg(string path) {
			using (var image = SixLabors.ImageSharp.Image.Load(path)) {
				var ms = new MemoryStream();
				image.SaveAsJpeg(ms, new JpegEncoder() { Quality = 70 });
				return ms;
			}
		}
	}
}