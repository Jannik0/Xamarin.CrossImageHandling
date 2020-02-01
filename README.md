# Xamarin.CrossImageHandling
A cross-platform solution for handling images with Xamarin

This project provides a relatively simple approach to handling image files with Xamarin.Forms, Xamarin.Android and Xamarin.iOS.

In order to handle and request the appropriate user permissions, jamesmontamagno's PermissionsPlugin is used (github: https://github.com/jamesmontemagno/PermissionsPlugin nuget: https://www.nuget.org/packages/Plugin.Permissions)


Functionality implemented in this project:
  - let the user pick an image from the device's photo library/gallery
  - convert an image stream into raw pixel data (RGBA format) for direct access
  - convert raw pixel data into a stream object (for displaying, resizing, saving, etc.)
  - get an image's width and height in pixels
  - resize an image
  - save an image to the device's photo library/gallery


Feel free to reuse this code for your own needs!
