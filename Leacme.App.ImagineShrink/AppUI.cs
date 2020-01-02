// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Leacme.Lib.ImagineShrink;

namespace Leacme.App.ImagineShrink {

	public class AppUI {

		private class ImagePath {
			public string Images { get; set; }
		}

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library encoder = new Library();
		public TextBlock topBlurb { get; } = App.TextBlock;
		public Button LoadImgButton { get; } = App.Button;
		public Button OptImgButton { get; } = App.Button;
		public Button OutFolderButton { get; set; }
		public TextBox OutputFolderTextBox { get; set; }
		public (StackPanel holder, TextBlock label, TextBox field, Button button) OutputDirPan { get; } = App.HorizontalFieldWithButton;
		public DataGrid ToConvertDataGrid { get; } = App.DataGrid;

		public AppUI() {

			topBlurb.TextAlignment = TextAlignment.Center;
			topBlurb.Margin = new Thickness(5);
			topBlurb.Text = "Choose images to reduce their size. The optimizations are lossy";
			rootPan.Children.Add(topBlurb);

			LoadImgButton.Content = "Load Images...";
			LoadImgButton.Click += async (x, y) => {
				ToConvertDataGrid.Items = (await OpenFiles(PermutateExtensionCase(new List<string> { "png", "jpg", "jpeg" }).ToList())).Select(z => new ImagePath { Images = z }).ToList();
			};
			rootPan.Children.Add(LoadImgButton);

			OutputDirPan.holder.HorizontalAlignment = HorizontalAlignment.Center;
			OutputDirPan.holder.Children.OfType<TextBlock>().First().Text = "Output Directory:";
			OutputFolderTextBox = OutputDirPan.holder.Children.OfType<TextBox>().First();
			OutputFolderTextBox.Width = 550;
			OutputFolderTextBox.IsReadOnly = true;
			OutFolderButton = OutputDirPan.holder.Children.OfType<Button>().First();
			OutFolderButton.Content = "Choose Folder...";
			OutFolderButton.Click += async (x, y) => { OutputFolderTextBox.Text = await OpenFolder(); };
			ResetImagesList();
			ToConvertDataGrid.Height = 350;
			rootPan.Children.Add(ToConvertDataGrid);
			rootPan.Children.Add(OutputDirPan.holder);

			rootPan.Children.Add(OptImgButton);

			OptImgButton.Content = "Optimize Images";
			OptImgButton.Click += async (x, y) => {
				if (!ToConvertDataGrid.Items.Cast<ImagePath>().Where(z =>
				Path.GetExtension(z.Images).ToLower() == ".jpg" ||
				Path.GetExtension(z.Images).ToLower() == ".jpeg" ||
				Path.GetExtension(z.Images).ToLower() == ".png"
					).Any()) {
					var noInputImagesNotification = App.NotificationWindow;
					((StackPanel)noInputImagesNotification.Content).Children.OfType<TextBlock>().First().Text = "No images to optimize";
					await noInputImagesNotification.ShowDialog<Window>(Application.Current.MainWindow);
				} else if (OutputFolderTextBox.Text == null) {
					var noOutputPathNotification = App.NotificationWindow;
					((StackPanel)noOutputPathNotification.Content).Children.OfType<TextBlock>().First().Text = "Specify output directory";
					await noOutputPathNotification.ShowDialog<Window>(Application.Current.MainWindow);
				} else {
					List<string> jpegs = ToConvertDataGrid.Items.Cast<ImagePath>().Where(z =>
						Path.GetExtension(z.Images).ToLower() == ".jpg" ||
						Path.GetExtension(z.Images).ToLower() == ".jpeg").Select(a => a.Images).ToList();
					List<string> pngs = ToConvertDataGrid.Items.Cast<ImagePath>().Where(z =>
						Path.GetExtension(z.Images).ToLower() == ".png").Select(a => a.Images).ToList();

					jpegs.ForEach(b => { System.Drawing.Image.FromStream(encoder.ShrinkJpeg(b)).Save(Path.Combine(OutputFolderTextBox.Text, Path.GetFileName(b))); });
					pngs.ForEach(b => { System.Drawing.Image.FromStream(encoder.ShrinkPNG(b)).Save(Path.Combine(OutputFolderTextBox.Text, Path.GetFileName(b))); });

					ResetImagesList();
					var finishedNotification = App.NotificationWindow;
					((StackPanel)finishedNotification.Content).Children.OfType<TextBlock>().First().Text = "Finished optimizing images";
					await finishedNotification.ShowDialog<Window>(Application.Current.MainWindow);
				}
			};
		}

		private void ResetImagesList() {
			ToConvertDataGrid.Items = Enumerable.Repeat<ImagePath>(new ImagePath { Images = " " }, 15).ToList();
		}

		private IEnumerable<string> PermutateExtensionCase(List<string> extensionsToPermutate) {
			var permExtensions = new List<string>();
			foreach (var extension in extensionsToPermutate) {
				var te = extension.ToLower();
				permExtensions.AddRange(Enumerable.Range(0, 1 << te.Length).Select(x => string.Concat(te.Select((y, z) => (x & (1 << z)) == 0 ? char.ToLower(y) : char.ToUpper(y)))).ToList());
			}
			return permExtensions;
		}

		private async Task<IEnumerable<string>> OpenFiles(List<string> allowedExtensions) {
			var dialog = new OpenFileDialog() {
				Title = "Select Image File...",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				AllowMultiple = true
			};
			dialog.Filters.Add(new FileDialogFilter() { Name = "Image", Extensions = allowedExtensions });
			var res = await dialog.ShowAsync(Application.Current.MainWindow);
			return (res?.Any() == true) ? res : Enumerable.Empty<string>();
		}

		private async Task<string> OpenFolder() {
			var dialog = new OpenFolderDialog() {
				Title = "Select Output Folder...",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			};
			var res = await dialog.ShowAsync(Application.Current.MainWindow);
			return res ?? "";
		}
	}
}