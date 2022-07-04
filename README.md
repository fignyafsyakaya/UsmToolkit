# UsmToolkit

Tool to convert USM video files into user-friendly formats.

### Extracting
```
UsmToolkit extract <file/folder> [-o=OUTPUT_DIRECTORY] [-t=MAX_NUMBER_OF_THREAD]
```

### Converting
```
UsmToolkit convert <file/folder> [-o=OUTPUT_DIRECTORY] [-c] [-t=MAX_NUMBER_OF_THREAD]
```
-c will clean temporary files

For more informations run `UsmToolkit extract -h` and `UsmToolkit convert -h`.

## Custom conversion parameter

You should find `config.json` in the folder of the executable. With it, you can completly customize how the extracted file is processed by ffmpeg.
The default configuration ships as follows:

* Video: Re-encoded ad FMP4 with best quality settings
* Output is an AVI file for video, WAV files for audio and TXT file for subtitles. They are converted to be compatible with Scaleform Video Encoder.

You can change these settings to your likings, it's standard ffmpeg syntax.

## TODO

This program was intended to extract .usm files from Alien Isolation, which works with a single subtitle track containing subtitle IDs. I do not know the behaviour of this program with a .usm file built with multiple subtitle tracks.

## License

UsmToolkit follows the MIT License. It uses code from [VGMToolbox](https://sourceforge.net/projects/vgmtoolbox/).
