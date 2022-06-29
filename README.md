# UsmToolkit


Tool to convert USM video files into user-friendly formats.

## Getting started

Download ffmpeg and put `ffmpeg` binaries in ffmpeg folder, and `vgmstream` binaries in vgmstream folder (make sure it includes "test" binary)

### Extracting
```
UsmToolkit extract <file/folder> [-o=OUTPUT_DIRECTORY]
```

### Converting
```
UsmToolkit convert <file/folder> [-o=OUTPUT_DIRECTORY] [-c]
```
-c will clean temporary files

For more informations run `UsmToolkit extract -h` and `UsmToolkit convert -h`.

## Custom conversion parameter

You should find `config.json` in the folder of the executable. With it, you can completly customize how the extracted file is processed by ffmpeg.
The default configuration ships as follows:

* Video: Re-encoded ad FMP4 with best quality settings
* Audio: Re-encoded as WAV files.
* Output is an AVI file for video, WAV files for audio and TXT file for subtitles. They are converted to be compatible with Scaleform Video Encoder.

You can change these settings to your likings, it's standard ffmpeg syntax.

## TODO

This program was intended to extract .usm files from Alien Isolation, which works with a single subtitle track containing subtitle IDs. I do not know the behaviour of this program with a .usm file built with multiple subtitle tracks.

## License

UsmToolkit follows the MIT License. It uses code from [VGMToolbox](https://sourceforge.net/projects/vgmtoolbox/).
