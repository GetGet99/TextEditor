using Get.EasyCSharp;
using Get.XAMLTools;
using System;
using System.ComponentModel;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Get.TextEditor;
using Platform.UI.Xaml.Media.Imaging;

[DependencyProperty<double>("MinImageWidth", DefaultValueExpression = "100", GenerateLocalOnPropertyChangedMethod = true, LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(EnsureScale))]
[DependencyProperty<double>("MinImageHeight", DefaultValueExpression = "100", GenerateLocalOnPropertyChangedMethod = true, LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(EnsureScale))]
[DependencyProperty<double>("MaxImageWidth", DefaultValueExpression = "500", GenerateLocalOnPropertyChangedMethod = true, LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(EnsureScale))]
[DependencyProperty<double>("MaxImageHeight", DefaultValueExpression = "500", GenerateLocalOnPropertyChangedMethod = true, LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(EnsureScale))]
[DependencyProperty<BitmapImage>("ImageSource", GenerateLocalOnPropertyChangedMethod = true, LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(EnsureScale))]
[DependencyProperty<double>("ImageScale", DefaultValueExpression = "1", LocalOnPropertyChangedMethodWithParameter = false, LocalOnPropertyChangedMethodName = nameof(EnsureScale))]
public sealed partial class ImageDisplay : Grid, INotifyPropertyChanged
{
    public ImageDisplay()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    [AutoNotifyProperty(Visibility = GeneratorVisibility.Private)]
    int _imageWidth;
    [AutoNotifyProperty(Visibility = GeneratorVisibility.Private)]
    int _imageHeight;
    double Multiply(int @base, double scale) => @base * scale;
    void EnsureScale()
    {
        var source = ImageSource;
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var scale = ImageScale;
        scale = Math.Clamp(scale, MinImageWidth / width, MaxImageWidth / width);
        scale = Math.Clamp(scale, MinImageHeight / height, MaxImageHeight / height);
        ImageWidth = width;
        ImageHeight = height;
        ImageScale = scale;
    }
}
