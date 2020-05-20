using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace Ydownloader
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        List<YouTubeVideo> ResolutionList = new List<YouTubeVideo>();
        YouTubeVideo video;
        Client<YouTubeVideo> service;
        string url="";

        public MainPage()
        {
            InitializeComponent();

            service = Client.For(YouTube.Default);
            btnDownload.IsVisible = false;
            Isbusy.IsVisible = false;
        }

        private const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
        private static Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
        private static string[] validAuthorities = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be", "www.m.youtube.com","m.youtube.com" };

        public string ExtractVideoIdFromUri(Uri uri)
        {
            try
            {
                string authority = new UriBuilder(uri).Uri.Authority.ToLower();

                //check if the url is a youtube url
                if (validAuthorities.Contains(authority))
                {
                    //and extract the id
                    var regRes = regexExtractId.Match(uri.ToString());
                    if (regRes.Success)
                    {
                        return regRes.Groups[1].Value;
                    }
                }
            }
            catch { }


            return null;
        }

        public static string GetDefaultFolder()
        {
            var home = DependencyService.Get<IFileSystem>().GetExternalStorage();

            return home;
        }

        private void btnDownload_Clicked(object sender, EventArgs e)
        {

            Isbusy.IsVisible = true;
            Device.BeginInvokeOnMainThread(async() =>
            {
                try
                {
                    string folder = GetDefaultFolder();

                    string path = Path.Combine(folder, video.FullName);

                    status.Text = "Saving";
                    var selectedRes = (YouTubeVideo)PickResolution.SelectedItem;
                    File.WriteAllBytes(path, selectedRes.GetBytes());

                    status.Text = "Done";
                    Isbusy.IsVisible = false;

                }
                catch (Exception)
                {

                    return;
                }
                

            });
        }

        private void btnSearch_Clicked(object sender, EventArgs e)
        {
            Search();


                
        }

        private void PickResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PickResolution.SelectedIndex > -1)
            {
                btnDownload.IsVisible = true;
            }
            else
            {
                btnDownload.IsVisible = false;
            }
        }

        private void ViewWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            Isbusy.IsVisible = true;

        }

        private void ViewWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            Isbusy.IsVisible = false;
            txtsearch.Text = e.Url.Replace("#dialog", string.Empty);
            Search();
         }

        private void Search()
        {
            try
            {
                btnDownload.IsVisible = false;
                PickResolution.IsVisible = false;
                Device.BeginInvokeOnMainThread(async () =>
                {

                    try
                    {
                        
                        Isbusy.IsVisible = true;
                        //txtsearch.Text = "https://www.youtube.com/watch?v=EYj1cM-S25s&feature=youtu.be";
                        string uri = ExtractVideoIdFromUri(new System.Uri(txtsearch.Text));
                        if (string.IsNullOrEmpty(uri))
                        {
                            Isbusy.IsVisible = false;
                        
                            return;
                        }
                        await Task.Delay(100);
                        video = service.GetVideo("https://youtube.com/watch?v=" + uri);

                        lbltitle.Text = video.Title;
                        string fileExtension = video.FileExtension;
                        string fullName = video.FullName;
                        int resolution = video.Resolution;

                        var video2 = service.GetAllVideos("https://youtube.com/watch?v=" + uri);

                        ResolutionList.Clear();


                        foreach (var item in video2)
                        {
                            if ((item.Resolution > 0 && item.AudioBitrate > 0)|| item.AudioBitrate > 0)
                            {
                                if (!ResolutionList.Contains(item))
                                {
                                    ResolutionList.Add(item);

                                }

                            }
                        }



                        PickResolution.ItemsSource = null;
                        PickResolution.ItemsSource = ResolutionList;

                        Isbusy.IsVisible = false;
                        PickResolution.IsVisible = true;
                    }
                    catch (Exception)
                    {

                        return;
                    }

                });
            }
            catch (Exception)
            {

                return;
            }
        }

 

        protected override bool OnBackButtonPressed()
        {
            if (ViewWebView.CanGoBack)
            {
                ViewWebView.GoBack();
                return true;
            }

            else
            {
                return false;
            }

        }
    }
}
