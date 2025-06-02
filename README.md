# Mini-Hardware-Monitor
This Repository contains the Source/release files for the FHD Mini Hardware Monitor project. A miniature LibreHardwareMonitor based program intended for providing hardware info via Serial to external devices and Arduino projects.
Designed to be as small and simple as i could make it.


# Working:
The program is a tiny C# program that uses the LiberHardwareMonitor library DLL to retrieve various sensor values from the system, convert them into a set of bytes and then pushes them out a COM port.
To keep its footprint to a minimum the program lacks window, instead existing solely as a system tray icon.

It has a footprint of about 8 megabyte of ram and negligable CPU use.

# Protocol
The program sends out a packet of 7 bytes every second via serial.
It uses the standard UART settings as found on an arduino with a Baud rate of 115200.

* Byte 1-2: Static 0x03 & 0xF4 (Pre-amble)
* Byte 3: CPU use 
* Byte 4: RAM use
* Byte 5: GPU use
* Byte 6: CPU temp
* Byte 7: GPU temp

Value is stored in halves (0x01 = 0.5). e.g. a byte with as value 150 is either 75% or 75 degrees.

# Configuring for automatic startup
If your project is nice and and it got its own static com-port. You can configure the monitor program to automatically try to connect to a specific COM port on start.
To set this up you have to open the .Settings file within the folder with a text editor. There you will find a set of field for automatic connecting.

I went for this approach as the alternative would have the program create itself a local appdata folder and i rather avoid such clutter.

# CPU Temperature is 0?
CPU Temperature is the only value that requires Admin rights to run. If you want that value you will want to run as administrator

# My hardware isn't detected!!
The Librehardwaremonitor is subject to constant updates for new hardware and it is almost guaranteed that this project will often lag behind.
To fix it yourself you can go to the LibrehardwareMonitor github, download the latest version of the lib DLL file and replace the one that is bundled within this project.
This is often enough for the program to start detecting boards again. May take a few weeks if you got like a cutting edge new board

# License:
The LibreHardwareMonitor project is subject to the Mozilla Public License 2.0. Please observe their own github pages for specifics.
This program is Fully Open Source. Fork, copy, re-use bits however you please. It is nothing special honestly.
