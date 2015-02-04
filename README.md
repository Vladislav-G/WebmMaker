## WebM Maker and Optimizer

[Download latest version here.](http://a.pomf.se/ojmkdh.zip) (04/02/2015 21:24)

![screenshot](https://i.imgur.com/eZvkxc0.png)

### What is it?

A simple tool that makes the highest quality webms given a filesize limit. Uses ffmpeg for conversion.

In a bit more detail, it makes use of variable bitrates to maintain a fixed level of quality, steadily dropping it up to a threshold. When that quality limit is reached and the filesize is still too high, the resolution is scaled down and the process repeats until the filesize limit has been reached.

### How to use it

Download the latest version, extract anywhere and run `Webm Maker.exe`, the rest should be obvious.

### Known issues

Some problems with unusual characters in file/path name. If you think it crashed, check that ffmpeg.exe is still running in Task Manager, if not then restart the program.
