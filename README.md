<p align="center">
    <img align="right" src="https://raw.githubusercontent.com/warrengalyen/GifMotion/master/GifMotion/Icon.ico" alt="GifMotion Icon" height=100>
    <h1 align="left">GifMotion</h1>
</p>

A .NET library for **reading** and **creating animated GIFs**, inspired by the lack of features from the *System.Windows.Media.GifBitmapEncoder*

# How to use

### Add to your Project
* Via NuGet:

Type `Install-Package AnimatedGif` in Package Manager Console or download manually [on NuGet](http://www.nuget.org/packages/GifMotion/)

* Manually: 

Download or clone this Project and compile on your own and import `GifMotion/bin/Release/GifMotion.dll`


### Creating a GIF
```cs
// Create new Animated GIF Creator with Path to C:\samplegif.gif and 33 ms delay between frames (~30 fps)
using (GifCreator gifCreator = AnimatedGif.Create("C:\\awesomegif.gif", 33)) 
{
    // Enumerate through a List<System.Drawing.Image> or List<System.Drawing.Bitmap> for example
    foreach (Image img in MyImagesList) 
    {
        using (img) 
        {
            // Add the image to gifEncoder with default Quality
            gifCreator.AddFrame(img, GIFQuality.Default);
        } // Image disposed
    }
} // gifCreator.Finish and gifCreator.Dispose is called here
```

### Reading a GIF
```cs
var animatedGif = new AnimatedGif(workingPath + "/samplegif.gif");
for (int i = 0; i < animatedGif.FrameCount; i++)
{
    Image gifImage = animatedGif.GetFrame(i);
}
```