# !!----OVERHAUL IN PROGRESS----!!
This project is being overhauled for working with modern systems. Please stand by
NO working files are here yet outside of one single Release that only sends out LOAD values.

# Mini-Hardware-Monitor
This Repository contains the Source/release files for the FHD Mini Hardware Monitor project. A miniature LibreHardwareMonitor based program intended for providing hardware info via Serial to external devices and Arduino projects.
Designed to be as small and simple as i could make it.


# Working:
The program is a tiny C# program that uses the LiberHardwareMonitor library DLL to retrieve various sensor values from the system, convert them into a set of bytes and then pushes them out a COM port.
To keep its footprint to a minimum the program lacks window, instead existing solely as a system tray icon.

It has a footprint of about 7 megabyte of ram and negligable CPU use.

# Protocol
The program sends out a packet of 8 bytes ever half a second via serial.
It uses the standard UART settings as found on an arduino with a Baud rate of 115200.

* Byte 1-2: Static x & x (Pre-amble)
* Byte 3: CPU use
* Byte 4: RAM use
* Byte 5: GPU use
* Byte 6: CPU temp
* Byte 7: MB temp
* Byte 8: GPU temp

Value is stored in halves (0x01 = 0.5). e.g. a byte with as value 150 is either 75% or 75 degrees.

# Configuring for automatic startup
If your project is nice and and it got its own static com-port. You can configure the monitor program to automatically try to connect to a specific COM port on start.
To set this up you have to open the .Settings file within the folder with a text editor. There you will find a set of field for automatic connecting.

I went for this approach as the alternative would have the program create itself a local appdata folder and i rather avoid such clutter.

# My hardware isn't detected!!
The Librehardwaremonitor is subject to constant updates for new hardware and it is almost guaranteed that this project will often lag behind.
To fix it yourself you can go to the LibrehardwareMonitor github, download the latest version of the lib and replace the one that is bundled within this project.
This is often enough for the program to start detecting boards again. May take a few weeks if you got like a cutting edge new board

# License:
Both this project and the LibreHardwareMonitor project are subject to the Mozilla Public License 2.0
