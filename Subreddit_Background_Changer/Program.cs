using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;

namespace Subreddit_Background_Changer
{
    public class Wallpaper
    {
        public Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(Uri uri, Style style)
        {
            System.IO.Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            bool networkConnection = false;
            int count = 0;
            networkConnection = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            
            while (!networkConnection)
            {
                count++;
                int milliseconds = 2000;
                Thread.Sleep(milliseconds);
                networkConnection = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if(networkConnection == false)
                {
                    Console.WriteLine("No network connection, trying again in " + milliseconds/1000 + " seconds");
                }
                else
                {
                    Console.WriteLine("Network connection found, running script.");
                }
                

                if(count >= 10)
                {
                    Environment.Exit(0);
                }

            }

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\RedditBackgrounds\";
            string todaysFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\RedditBackgrounds\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\";
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".jpg";
            string tempPath = folder + fileName;


            //if (!File.Exists(@"C:\asodmioasmdioamsdiomasiodmasiomdioasm"))
            if (!File.Exists(tempPath))
            {



                int numScreens = Screen.AllScreens.Length;
                int lastWidth = 0;


                int maxX = 0;
                int minX = 0;

                int maxY = 0;
                int minY = 0;

                foreach (var s in Screen.AllScreens)
                {
                    Rectangle res = s.Bounds;

                    if((res.X + res.Width) > maxX)
                    {
                        maxX = res.X + res.Width;
                    }

                    if(res.X < minX)
                    {
                        minX = res.X;
                    }
                    
                    if((res.Y + res.Height) > maxY)
                    {
                        maxY = res.Y + res.Height;
                    }

                    if(res.Y < minY)
                    {
                        minY = res.Y;
                    }

                    Console.WriteLine("X: " + res.X + ", Y: " + res.Y);
                    Console.WriteLine("Width: " + res.Width + ", Y: " + res.Height);
                }



                int totalx = maxX+Math.Abs(minX);
                int totaly = maxY+Math.Abs(minY);


                Console.WriteLine("X: " + totalx + ", Y: " + totaly);


                Console.WriteLine("Getting Data from Reddit");
                var response = new WebClient().DownloadString("https://www.reddit.com/r/earthPorn/.json");
                //var response = new WebClient().DownloadString("http://192.168.88.15/redditCache.json");
                var results = JsonConvert.DeserializeObject<dynamic>(response);




                //If the backgrounds folder does not exist create it
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                //If today's folder does not exist create it
                if (!System.IO.Directory.Exists(todaysFolder))
                    System.IO.Directory.CreateDirectory(todaysFolder);

                Bitmap outputImage = new Bitmap(totalx, totaly);
                Graphics g = Graphics.FromImage(outputImage);
                g.Clear(Color.Black);

                Image[] bgImages = new Image[numScreens];
                string[] imageURLs = new string[numScreens];

                int i = 0;

                //for (int i = 0; i <= (numScreens - 1); i++)

                bool hasStickey = false;


                foreach (var s in Screen.AllScreens)
                {
                    Rectangle res = s.Bounds;

                    int mwidth = res.Width;
                    int mheight = res.Height;

                  


                    //imageURLs[i] = getImageFromURL((String)results["data"]["children"][i]["data"]["url"]);
                    //Console.WriteLine("Saving: " + imageURLs[i]);
                    //new WebClient().DownloadFile(imageURLs[i], @"C:\Users\Ryan\Desktop\TempWallapapers\" + (i + 1) + ".jpg");
                    //String jpg_tmp = @"C:\Users\Ryan\Desktop\TempWallapapers\1.jpg";
                    //bgImages[i] = Image.FromFile(jpg_tmp);

                    Console.WriteLine(results["data"]["children"][i]["data"]["title"]);
                    Console.WriteLine(results["data"]["children"][i]["data"]["stickied"]);
                    
                    

                    //If the post is sticked skip it
                    if (results["data"]["children"][i]["data"]["stickied"]=="true")
                    {
                        Console.WriteLine("The post is stickied");
                        hasStickey = true;
                        //continue;
                    }

                    if (hasStickey)
                    {
                        imageURLs[i] = getImageFromURL((String)results["data"]["children"][i+1]["data"]["url"]);
                    } else {
                        imageURLs[i] = getImageFromURL((String)results["data"]["children"][i]["data"]["url"]);
                    }

                    //Uncomment if you want to see the URL that is being saved.
                    Console.WriteLine("Saving: " + imageURLs[i]);



                    WebRequest req = WebRequest.Create(imageURLs[i]);
                    WebResponse tmp_response = req.GetResponse();
                    Stream stream = tmp_response.GetResponseStream();
                    Image tmp = Image.FromStream(stream);


                    //int xPos = (lastWidth * i);
                    int xPos = res.X + Math.Abs(minX);
                    int yPos = res.Y + Math.Abs(minY);
      

                    tmp.Save(todaysFolder + i + ".jpg");

                    if (tmp.Width > mwidth || tmp.Height > mheight)
                    {
                        Image imgStream = resizeImage(tmp, mheight, mwidth);

                        g.DrawImage(imgStream, new Point(xPos, yPos));

                        //imgStream.Save(@"C:\Users\Ryan\Desktop\TempWallapapers\" + i + ".jpg");
                    }
                    else
                    {
                        double scale = 1;


                        //If the DPI is larger then the ouput DPI (Default: 96) then scale the image down
                        if (tmp.HorizontalResolution > 96 || tmp.VerticalResolution > 96)
                        {
                            scale = 96 / Math.Max(tmp.HorizontalResolution, tmp.VerticalResolution);
                        }
                        else
                        {
                            //Otherwise the input DPI is smaller than the outpu to scale UP
                            scale = Math.Min(tmp.HorizontalResolution, tmp.VerticalResolution) / 96;
                        }



                        //Console.WriteLine("Scale: " + scale);

                        //Console.WriteLine("xPos: " + (int)(xPos + ((mwidth - (tmp.Width * scale)) / 2)));


                        g.DrawImage(tmp, new Point((int)(xPos + ((mwidth - (tmp.Width * scale)) / 2)), (int)(yPos + (mheight - (tmp.Height * scale)) / 2)));
                    }

                    stream.Close();
                    tmp.Dispose();
                    lastWidth = mwidth;

                    //Console.WriteLine("X: " + res.Width + ", Y: " + res.Height);
                    i++;
                }



                g.Dispose();

                outputImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                outputImage.Dispose();


                //Set the Wallpaper
                Console.WriteLine("Setting Wallpaper");
                Wallpaper.Set(new System.Uri(tempPath), Wallpaper.Style.Tiled);




                //Top 1680x1050 on /r/wallpapers
                //var response = new WebClient().DownloadString("https://www.reddit.com/r/wallpapers/search.json?q=1680x1050&restrict_sr=on&sort=top");


                /* TODO: UNCOMMENT ME TO GET NEW IMAGES FROM REDDIT
                var response = new WebClient().DownloadString("https://www.reddit.com/r/earthPorn/.json");
                var results = JsonConvert.DeserializeObject<dynamic>(response);


                string[] imageURLs = new string[numScreens];

                for(int i = 0; i <= (numScreens-1); i++)
                {
                    imageURLs[i] = getImageFromURL((String)results["data"]["children"][i]["data"]["url"]);
                    Console.WriteLine("Saving: " + imageURLs[i]);
                    new WebClient().DownloadFile(imageURLs[i], @"C:\Users\Ryan\Desktop\TempWallapapers\" + (i + 1) + ".jpg");
                }
                */

                /* Unneeded code
                //top wallpapers format
                //results["data"]["children"][0]["data"]["url"]

                //var response2 = new WebClient().DownloadString("https://www.reddit.com/r/earthPorn/.json");
                //Console.WriteLine(response2);

                //Get's the preview's source URL
                //Console.WriteLine(results["data"]["children"][0]["data"]["preview"]["images"][0]["source"]["url"]);

                string URL1 = results["data"]["children"][0]["data"]["preview"]["images"][0]["url"];
                Console.WriteLine("Saving: " + URL1);
                new WebClient().DownloadFile(URL1, @"C:\Users\Ryan\Desktop\TempWallapapers\1.jpg");


                string URL2 = results["data"]["children"][1]["data"]["preview"]["images"][0]["url"];
                Console.WriteLine("Saving: " + URL2);
                new WebClient().DownloadFile(URL2, @"C:\Users\Ryan\Desktop\TempWallapapers\2.jpg");


                string URL3 = results["data"]["children"][2]["data"]["preview"]["images"][0]["url"];
                Console.WriteLine("Saving: " + URL3);
                new WebClient().DownloadFile(URL3, @"C:\Users\Ryan\Desktop\TempWallapapers\3.jpg");
                */


                /*

                //Location of all of the temp images
                String jpg1 = @"C:\Users\Ryan\Desktop\TempWallapapers\1.jpg";
                String jpg2 = @"C:\Users\Ryan\Desktop\TempWallapapers\2.jpg";
                String jpg3 = @"C:\Users\Ryan\Desktop\TempWallapapers\3.jpg";


                //Import the images

                Image img1_temp = Image.FromFile(jpg2);
                Image img2_temp = Image.FromFile(jpg3);
                Image img3_temp = Image.FromFile(jpg1);

                for (int i = 0; i <= (numScreens - 1); i++)
                {

                }
                */



                //Scale the images to fit the resolution of the monitor
                // TODO: Make this a function and run it for each monitor (If they are different resolutions)


                //int width = 1680;
                //int height = 1050;





                //bmp.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);



                /*
                string tempPath = @"C:\Users\Ryan\Desktop\TempWallapapers\testing2555.jpg";
                Bitmap outputImage = new Bitmap(totalx, totaly);
                Graphics g = Graphics.FromImage(outputImage);
                g.Clear(Color.Black);
                g.DrawImage(resizeImage(img1_temp, width, height), new Point(0, 0));
                g.DrawImage(resizeImage(img2_temp, width, height), new Point(width, 0));
                g.DrawImage(resizeImage(img3_temp, width, height), new Point(1680 * 2, 0));

                g.Dispose();

                outputImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                outputImage.Dispose();
                */

                //Resize the new images to scale correctly for the monitor
                //This may not need to be done anymore since I am already doing this earlier
                //System.Drawing.Bitmap img1 = new System.Drawing.Bitmap(, 1680, 1050);
                //System.Drawing.Bitmap img2 = new System.Drawing.Bitmap(Image.FromFile(jpg3), 1680, 1050);
                //System.Drawing.Bitmap img3 = new System.Drawing.Bitmap(Image.FromFile(jpg1), 1680, 1050);







                /*
                //string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper2.bmp");
                string tempPath = @"C:\Users\Ryan\Desktop\TempWallapapers\output.jpg";

                //int width = img1.Width + img2.Width;
                //int height = Math.Max(img1.Height, img2.Height);

                Bitmap outputImage = new Bitmap(totalx, totaly);
                Graphics g = Graphics.FromImage(outputImage);
                g.Clear(Color.Black);
                g.DrawImage(img1, new Point(0, 0));
                g.DrawImage(img2, new Point(img1.Width, 0));
                g.DrawImage(img3, new Point(1680*2, 0));


                g.Dispose();
                img1.Dispose();
                img2.Dispose();
                img3.Dispose();

                outputImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                outputImage.Dispose();

                Console.WriteLine("Image Saved");

                Wallpaper.Set(new System.Uri(tempPath), Wallpaper.Style.Tiled);
                Console.WriteLine("Setting Wallpaper");
                */





















                // Keep the console window open in debug mode.
                //Console.WriteLine("Press any key to exit.");
                //Console.ReadKey();
            }
        }

        public static Image resizeImage(Image i, int height, int width)
        {
            //If the image is larger than the monitor's resolution we need to scale it.
            float scale = Math.Min((float)width / i.Width, (float)height / i.Height);
            var bmp = new Bitmap(width, height);
            var g = Graphics.FromImage(bmp);
            var scaleWidth = (int)(i.Width * scale);
            var scaleHeight = (int)(i.Height * scale);

            g.DrawImage(i, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));


            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Bmp);


            return Image.FromStream(ms);
        }

        public static string getImageFromURL(string url)
        {

            //If the URL is an image, just return it and don't process anymore
            if ((url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".gif")))
            {
                return url;
            }
            else
            {
                //Regex imgurRegex = new Regex(@"http:\/\/(?:i\.imgur\.com\/(?<id>.*?)\.(?:jpg|png|gif)|imgur\.com\/(?:gallery\/)?)$");
                //Regex imgurRegex = new Regex(@"^https?:\/\/(\w+\.)?imgur.com\/(\w*\d\w*)+(\.[a-zA-Z]{3})?$");
                Regex imgurRegex = new Regex(@"((http(s?):\/\/)?imgur\.com\/[a-zA-Z0-9]{6,8})(?!\.jpg|\.gif|\.gifv|\.png)(?:[^a-zA-Z0-9]|$)");


                string imgurID = new Uri(url).Segments.Last();

                Match imgurMatch = imgurRegex.Match(url);


                //Console.WriteLine("Success: " + imgurMatch.Success + "  ID: " + url);

                if (imgurMatch.Success)
                {
                    //If the URL is an IMGUR URL

                    Regex imgRegex = new Regex(@"\.(jpe ? g | png | gif | bmp)$");
                    Match imgMatch = imgRegex.Match(imgurID);

                    //Console.WriteLine("Success: " + imgMatch.Success + "  ID: " + imgurID);

                    if (imgMatch.Success)
                    {
                        return url;
                    }
                    else
                    {
                        //TODO: Check if this code actually works


                        string apiURL = "https://api.imgur.com/3/image/" + imgurID;

                        // Uncomment for more debugging
                        //Console.WriteLine(apiURL);



                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiURL);
                        webRequest.Headers.Add("Authorization", "Client-ID 7d229c2f9a61c93");
                        Stream response = webRequest.GetResponse().GetResponseStream();
                        StreamReader reader = new StreamReader(response);
                        string responseFromServer = reader.ReadToEnd();


                        //Console.WriteLine(responseFromServer);


                        reader.Close();
                        response.Close();

                        /*
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiURL);
                        webRequest.Headers.Add("Authorization", "Client-ID 7d229c2f9a61c93");
                        webRequest.Method = "GET";

                        Stream response = webRequest.GetResponse().GetResponseStream();
                        StreamReader reader = new StreamReader(response);
                        string response2 = reader.ReadToEnd();

                        Console.WriteLine(response2);
                        */


                        var results = JsonConvert.DeserializeObject<dynamic>(responseFromServer);


                        //Console.WriteLine(Uri.UnescapeDataString((String)results["data"]["link"]));

                        if ((Boolean)results["success"])
                        {
                            return Uri.UnescapeDataString((String)results["data"]["link"]);
                        }
                        else
                        {
                            Console.WriteLine("ERROR: Not authenticated to Imgur.");
                            return null;
                        }



                    }


                    /*
                    if (imgurMatch.Groups.Count >= 3)
                    {
                        //The link is an image so return
                        return url;

                    }
                    */


                }
                else
                {
                    // The url is NOT an imgur link
                    return System.Net.WebUtility.HtmlDecode(url);

                }
            }//End IF url is image extension

        }
    }
}
